using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class BuildController : MonoBehaviour
{
	[SerializeField]
	private BuildPointer buildPointer;

	[SerializeField]
	private BuildLayout buildLayout;

	[SerializeField]
	private BuildModeCameraController cameraController;

	[SerializeField]
	private DockPointHint dockPointHintPrefab;

	[SerializeField]
	private Transform dockPointHintPoolParent;

	[SerializeField]
	private float gamepadCursorMinSpeed = 2f;

	[SerializeField]
	private float gamepadCursorMaxSpeed = 8f;

	[SerializeField]
	private float gamepadCursorAcceleration = 6f;

	[SerializeField]
	private float gamepadCursorMaxAcceleration = 12f;

	[SerializeField]
	private float gamepadRemoveCursorMinSpeed = 250f;

	[SerializeField]
	private float gamepadRemoveCursorMaxSpeed = 400f;

	[SerializeField]
	private float gamepadRemoveCursorAcceleration = 75f;

	[SerializeField]
	private float gamepadCursorScreenOffsetX = 50f;

	[SerializeField]
	private float gamepadCursorScreenOffsetY = 50f;

	private Vector3 curPosVisualCenter = Vector3.zero;

	private Vector3 curPosActual = Vector3.zero;

	private bool isBuildModeActive;

	private Vector3 lastCursorPos;

	private Vector3 lastSnappedCursorPos;

	private float gamepadCursorSpeed;

	private bool dpadPressedOnce;

	private Vector2Int gridStep;

	private bool isBuildModeInputLocked;

	private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

	private WorldZone currentWorldZone;

	private BuildData currentBuildData;

	private readonly List<DockPointHint> displayedDockPointHints = new List<DockPointHint>();

	private Pool dockPointHintsPool;

	private readonly List<BuildArea> fullCoverSoftBuildAreasCache = new List<BuildArea>();

	private readonly Dictionary<BuildArea, Wgo> fullCoverSoftHintWgos = new Dictionary<BuildArea, Wgo>();

	private readonly Dictionary<BuildArea, Bounds> fullCoverSoftHintBounds = new Dictionary<BuildArea, Bounds>();

	private readonly Collider[] fullCoverSoftOverlapColliders = new Collider[64];

	private bool fullCoverSoftCacheBuilt;

	private BuildArea currentFullCoverSoftHintArea;

	private static BuildController cachedInstance;

	public static BuildController Instance
	{
		get
		{
			if (cachedInstance == null)
			{
				cachedInstance = UnityEngine.Object.FindFirstObjectByType<BuildController>();
				if (cachedInstance == null)
				{
					Debug.LogError($"Cannot find instance of {typeof(BuildController)} on current scene.");
				}
			}
			return cachedInstance;
		}
		set
		{
			cachedInstance = value;
		}
	}

	public bool IsBuildModeActive
	{
		get
		{
			return isBuildModeActive;
		}
		set
		{
			isBuildModeActive = value;
			this.OnBuildModeStateChanged?.Invoke(value);
		}
	}

	public WorldZone CurrentWorldZone => currentWorldZone;

	public BuildLayout BuildLayout => buildLayout;

	public BuildArea CurrentFullCoverSoftHintArea => currentFullCoverSoftHintArea;

	public Vector3 LastCursorScreenPosition => lastCursorPos;

	private bool IsRemoveMode
	{
		get
		{
			if (currentBuildData != null)
			{
				return currentBuildData.BuildingMode == BuildingDef.BuildingMode.Remove;
			}
			return false;
		}
	}

	public event Action<bool> OnBuildModeStateChanged;

	private void Awake()
	{
		if (!(dockPointHintPrefab == null))
		{
			dockPointHintPrefab.gameObject.SetActive(value: false);
			Transform poolParent = ((dockPointHintPoolParent != null) ? dockPointHintPoolParent : base.transform);
			dockPointHintsPool = new Pool(dockPointHintPrefab, poolParent, 0);
			buildLayout.BuildGrid3D.Init();
		}
	}

	public void EnableBuildMode(BuildData buildData, WorldZone worldZone, List<NeedItemData> itemNeeds = null, MultiInventory multiInventory = null)
	{
		currentWorldZone = worldZone;
		currentBuildData = buildData;
		isBuildModeInputLocked = false;
		gridStep = ((buildData.Definition != null) ? (BuildConsts.BUILD_GRID_SIZE * buildData.Definition.customGridStep) : BuildConsts.BUILD_GRID_SIZE);
		LazyUI.Get<HUD>().SetDisableState(HudStateType.BuildController, isEnabled: false);
		BuildingHUDData data = new BuildingHUDData();
		LazyUI.Get<BuildingHUD>().Draw(data);
		IsBuildModeActive = true;
		SetIgnoredStateForChunkableObjectsInWorldZone(worldZone);
		Physics.SyncTransforms();
		Rect wholeZoneRect = worldZone.Data.wholeZoneRect;
		Vector3 buildPos = worldZone.GetBuildPos();
		buildPointer.Enable(buildData, worldZone.Id, buildPos, itemNeeds, multiInventory);
		bool useExtensions = buildPointer.PointerObject is WgoBuildPointer wgoBuildPointer && wgoBuildPointer.DrawBuffAreas;
		List<BuildElevationArea> buildElevationAreas = worldZone.GetBuildElevationAreas();
		Debug.Log($"BUILDING: elevationAreas.Count = {buildElevationAreas.Count}");
		buildLayout.EnableBuildingMode(buildPos, worldZone.Id, wholeZoneRect, buildData.Definition, useExtensions, buildElevationAreas);
		gamepadCursorSpeed = GetGamepadCursorMinSpeed();
		lastCursorPos = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
		fullCoverSoftCacheBuilt = false;
		UpdateFullCoverSoftBuildAreaHints();
		UpdateDockPointsHints(force: true);
		Debug.Log($"BUILDING: WorldZone.GetBuildPos() = {worldZone.GetBuildPos()}");
		Debug.Log("BUILDING: worldZone.Id = " + worldZone.Id);
		cameraController.Enable(buildPointer.VisualCenter, worldZone.ZoneCollider);
		StartCoroutine(CenterPointerOnScreenAfterCameraUpdate());
	}

	public void Update()
	{
		if (IsBuildModeActive && !isBuildModeInputLocked && (!(MainGame.PlayerController != null) || MainGame.PlayerController.IsControlsEnabledExcept(TakenControlType.ByBuilding)))
		{
			UpdatePointerAtPos(GetCursorPosition(Time.deltaTime));
			UpdateBuildModeInput();
			UpdateBuildModeTreeTransparencyOccluders();
		}
	}

	public void DisableBuildMode()
	{
		LazyUI.Get<BuildingHUD>().Hide();
		LazyUI.Get<HUD>().SetDisableState(HudStateType.BuildController, isEnabled: true);
		IsBuildModeActive = false;
		isBuildModeInputLocked = false;
		Object3DTransparencyOccluder.Shared.Clear();
		ClearDockPointsHints();
		ClearFullCoverSoftBuildAreaHints();
		currentBuildData = null;
		buildLayout.DisableBuildingMode();
		cameraController.Disable();
		buildPointer.Disable();
		SetNotIgnoredStateForChunkableObjectsInCurrentWorldZone();
	}

	public void UpdatePointerObjectPosition(Vector3 snappedCursorPos)
	{
		curPosVisualCenter = snappedCursorPos;
		curPosActual = curPosVisualCenter - buildPointer.ShiftToVisualCenter;
		buildPointer.UpdatePos(curPosActual);
		buildPointer.UpdateAvailability();
		UpdateFullCoverSoftBuildAreaHints();
	}

	private void SetIgnoredStateForChunkableObjectsInWorldZone(WorldZone worldZone)
	{
		foreach (Wgo wgo in worldZone.Wgos)
		{
			wgo.UpdateFlag(ChunkingIgnoreType.Building, newValue: true);
		}
		if (currentWorldZone.AllStaticObjectsInZone == null)
		{
			BoxCollider zoneCollider = worldZone.ZoneCollider;
			Bounds bounds = new Bounds(zoneCollider.transform.TransformPoint(zoneCollider.center), zoneCollider.size);
			worldZone.AllStaticObjectsInZone = LazySingleton<ChunkManager>.Instance.GetAllChunkableObjectsInBoundsForSelectedLayers(new List<ChunkManagerLayerType> { ChunkManagerLayerType.StaticObjects }, bounds);
		}
		foreach (IChunkableObject item in worldZone.AllStaticObjectsInZone)
		{
			item.UpdateFlag(ChunkingIgnoreType.Building, newValue: true);
		}
	}

	private void SetNotIgnoredStateForChunkableObjectsInCurrentWorldZone()
	{
		foreach (IChunkableObject item in currentWorldZone.AllStaticObjectsInZone)
		{
			item.UpdateFlag(ChunkingIgnoreType.Building, newValue: false);
		}
		foreach (Wgo wgo in currentWorldZone.Wgos)
		{
			wgo.UpdateFlag(ChunkingIgnoreType.Building, newValue: false);
		}
	}

	private void UpdateBuildModeTreeTransparencyOccluders()
	{
		Object3DTransparencyOccluder shared = Object3DTransparencyOccluder.Shared;
		shared.BeginFrame();
		PlayerView playerView = MainGame.PlayerController?.View;
		if (playerView != null)
		{
			Vector3 position = playerView.transform.position;
			shared.AddOccludersFromCapsule(position, position + Vector3.up * 1.4f, 0.24f, 10f);
		}
		if (buildPointer?.VisualCenter != null && buildPointer.PointerObject is BuildPointerObject buildPointerObject)
		{
			Bounds worldRoundedBounds = buildPointerObject.GetWorldRoundedBounds();
			Vector3 position2 = buildPointer.VisualCenter.position;
			float num = Mathf.Max(1.4f, worldRoundedBounds.size.y);
			float capsuleRadius = Mathf.Max(0.24f, Mathf.Max(worldRoundedBounds.extents.x, worldRoundedBounds.extents.z));
			Vector3 vector = new Vector3(position2.x, worldRoundedBounds.min.y, position2.z);
			shared.AddOccludersFromCapsule(vector, vector + Vector3.up * num, capsuleRadius, 10f);
		}
		shared.EndFrame();
	}

	private void UpdatePointerAtPos(Vector3 pos, bool forceUpdate = false)
	{
		lastCursorPos = pos;
		Ray ray = CameraSystem.ScreenPointToRay(lastCursorPos);
		Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
		if (Physics.Raycast(ray, out var hitInfo, 100f, 2048))
		{
			float z = (float)Math.Round(hitInfo.point.z / 0.0125f, MidpointRounding.AwayFromZero) * 0.0125f;
			Vector3 vector = new Vector3(hitInfo.point.x, hitInfo.point.y, z);
			if (currentWorldZone != null)
			{
				vector = VisualConsts.ProjectElevationPointToGround(vector, currentWorldZone.GroundPlaneY);
			}
			Vector3 roundedPosXZ = VisualConsts.GetRoundedPosXZ(vector - buildPointer.ShiftToVisualCenter, gridStep);
			roundedPosXZ += -Vector3.up * -0.006f;
			roundedPosXZ += buildPointer.ShiftToVisualCenter;
			lastSnappedCursorPos = roundedPosXZ;
			if (currentWorldZone != null && currentWorldZone.TryGetBuildElevationY(roundedPosXZ.x, roundedPosXZ.z, out var elevationY))
			{
				lastSnappedCursorPos = VisualConsts.ProjectGroundPointToElevation(roundedPosXZ, elevationY);
			}
			if (!lastSnappedCursorPos.x.EqualsTo(curPosVisualCenter.x) || !lastSnappedCursorPos.z.EqualsTo(curPosVisualCenter.z) || !lastSnappedCursorPos.y.EqualsTo(curPosVisualCenter.y, 0.001f) || forceUpdate)
			{
				UpdatePointerObjectPosition(lastSnappedCursorPos);
			}
		}
	}

	private void UpdateBuildModeInput()
	{
		if (isBuildModeInputLocked)
		{
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Back) || LazyInput.GetKeyDown(GameKey.RightClick))
		{
			DisableBuildMode();
			LazySingleton<BuildManager>.Instance.Disable();
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Rotate) && buildPointer.PointerObject.HasRotation())
		{
			buildPointer.Rotate();
			UpdatePointerObjectPosition(lastSnappedCursorPos);
			StartCoroutine(LockBuildInputAndDoDelayedAction(delegate
			{
				buildLayout.UpdateBuildingMode();
				buildPointer.UpdateAvailability();
				UpdateFullCoverSoftBuildAreaHints();
				UpdateDockPointsHints(force: true);
			}));
		}
		if (LazyInput.GetKeyDown(GameKey.Build) && buildPointer.TryBuildActionInput())
		{
			StartCoroutine(LockBuildInputAndDoDelayedAction(delegate
			{
				buildLayout.UpdateBuildingMode();
				buildPointer.UpdateAvailability();
				buildPointer.UpdateModulesLimitsWidget();
				UpdateFullCoverSoftBuildAreaHints();
				UpdateDockPointsHints(force: true);
			}));
		}
	}

	private Vector3 GetCursorPosition(float deltaTime)
	{
		if (LazyInput.IsGamepadActive)
		{
			Vector3 vector = default(Vector3);
			if (LazyInput.GetKey(GameKey.DpadUp))
			{
				vector += Vector3.up;
			}
			else if (LazyInput.GetKey(GameKey.DpadDown))
			{
				vector += Vector3.down;
			}
			else if (LazyInput.GetKey(GameKey.DpadLeft))
			{
				vector += Vector3.left;
			}
			else if (LazyInput.GetKey(GameKey.DpadRight))
			{
				vector += Vector3.right;
			}
			if (!vector.sqrMagnitude.EqualsTo(0f))
			{
				if (!IsRemoveMode && !dpadPressedOnce)
				{
					dpadPressedOnce = true;
					Vector2 vector2 = Vector2.Scale(BuildConsts.BUILD_GRID_SIZE_WORLD_UNIT, (Vector2)vector);
					Vector3 pos = CameraSystem.WorldToScreenPoint(curPosVisualCenter + new Vector3(vector2.x, 0f, vector2.y));
					return SnapToBounds(pos);
				}
				return ProcessCursorContiniousMoving(vector, deltaTime);
			}
			dpadPressedOnce = false;
			vector = LazyInput.GetDirection();
			if (vector.sqrMagnitude.EqualsTo(0f))
			{
				gamepadCursorSpeed = GetGamepadCursorMinSpeed();
				if (IsRemoveMode)
				{
					return SnapToBounds(lastCursorPos);
				}
				return SnapToBounds(CameraSystem.WorldToScreenPoint(lastSnappedCursorPos));
			}
			return ProcessCursorContiniousMoving(vector, deltaTime, isStick: true);
		}
		return Input.mousePosition;
	}

	private IEnumerator CenterPointerOnScreenAfterCameraUpdate()
	{
		isBuildModeInputLocked = true;
		yield return new WaitForEndOfFrame();
		if (IsBuildModeActive)
		{
			lastCursorPos = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
			Debug.Log($"BUILDING: lastCursorPos = {lastCursorPos}");
			UpdatePointerAtPos(lastCursorPos, forceUpdate: true);
			CameraSystem.Instance.ActiveCameraController.SetTargetInstant(buildPointer.VisualCenter);
			Debug.Log($"BUILDING: buildPointer.VisualCenter = {buildPointer.VisualCenter.position}");
			isBuildModeInputLocked = false;
		}
	}

	private IEnumerator LockBuildInputAndDoDelayedAction(Action delayedAction = null)
	{
		isBuildModeInputLocked = true;
		buildPointer.SetVisibleSelectionCells(isVisible: false);
		yield return waitForFixedUpdate;
		buildPointer.SetVisibleSelectionCells(isVisible: true);
		isBuildModeInputLocked = false;
		delayedAction?.Invoke();
		yield return null;
	}

	private Vector3 ProcessCursorContiniousMoving(Vector3 inputDirection, float deltaTime, bool isStick = false)
	{
		if (IsRemoveMode)
		{
			gamepadCursorSpeed += gamepadRemoveCursorAcceleration * deltaTime;
			gamepadCursorSpeed = Mathf.Clamp(gamepadCursorSpeed, gamepadRemoveCursorMinSpeed, gamepadRemoveCursorMaxSpeed);
			float num = gamepadCursorSpeed * LazyUI.ScaleFactor * deltaTime;
			return SnapToBounds(lastCursorPos + inputDirection * num);
		}
		float value = (isStick ? (gamepadCursorAcceleration / inputDirection.magnitude) : gamepadCursorAcceleration);
		value = Mathf.Clamp(value, gamepadCursorAcceleration, gamepadCursorMaxAcceleration);
		gamepadCursorSpeed += value * deltaTime;
		gamepadCursorSpeed = Mathf.Clamp(gamepadCursorSpeed, gamepadCursorMinSpeed, gamepadCursorMaxSpeed);
		Vector3 pos = lastCursorPos + inputDirection * gamepadCursorSpeed;
		return SnapToBounds(pos);
	}

	private float GetGamepadCursorMinSpeed()
	{
		if (!IsRemoveMode)
		{
			return gamepadCursorMinSpeed;
		}
		return gamepadRemoveCursorMinSpeed;
	}

	private Vector3 SnapToBounds(Vector3 pos)
	{
		pos.x = Mathf.Clamp(pos.x, gamepadCursorScreenOffsetX, (float)Screen.width - gamepadCursorScreenOffsetX);
		pos.y = Mathf.Clamp(pos.y, gamepadCursorScreenOffsetY, (float)Screen.height - gamepadCursorScreenOffsetY);
		return pos;
	}

	private void ClearDockPointsHints()
	{
		for (int i = 0; i < displayedDockPointHints.Count; i++)
		{
			DockPointHint dockPointHint = displayedDockPointHints[i];
			if (!(dockPointHint == null))
			{
				dockPointHint.Remove();
				if (dockPointHintsPool != null)
				{
					dockPointHintsPool.ReleaseObject(dockPointHint);
				}
				else
				{
					UnityEngine.Object.Destroy(dockPointHint.gameObject);
				}
			}
		}
		displayedDockPointHints.Clear();
	}

	private void ShowDockPointsHints()
	{
		if (dockPointHintPrefab == null || !(buildPointer?.PointerObject is WgoBuildPointer wgoBuildPointer))
		{
			return;
		}
		IReadOnlyList<DockPoint> readOnlyList = wgoBuildPointer.Target?.DockPoints;
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			return;
		}
		Transform parent = ((dockPointHintPoolParent != null) ? dockPointHintPoolParent : base.transform);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			DockPoint dockPoint = readOnlyList[i];
			if (!(dockPoint == null) && dockPoint.gameObject.activeInHierarchy && !dockPoint.HideInFighting && !dockPoint.IsForZombie)
			{
				DockPointHint dockPointHint = ((dockPointHintsPool != null) ? dockPointHintsPool.GetOrCreateObject<DockPointHint>() : UnityEngine.Object.Instantiate(dockPointHintPrefab));
				dockPointHint.transform.SetParent(parent);
				displayedDockPointHints.Add(dockPointHint);
				dockPointHint.Display(dockPoint);
			}
		}
	}

	private void UpdateDockPointsHints(bool force)
	{
		if (force)
		{
			ClearDockPointsHints();
			ShowDockPointsHints();
		}
	}

	private bool IsFullCoverSoftMode()
	{
		BuildingDef definition = currentBuildData.Definition;
		if (definition != null && definition.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverSoft && !string.IsNullOrEmpty(definition.customBuildAreaId) && buildPointer != null)
		{
			return buildPointer.PointerObject is WgoBuildPointer;
		}
		return false;
	}

	private static Collider GetBuildAreaCollider(BuildArea buildArea)
	{
		if (buildArea == null)
		{
			return null;
		}
		if (buildArea.Collider != null)
		{
			return buildArea.Collider;
		}
		return buildArea.GetComponent<Collider>();
	}

	private static Vector3 GetColliderCenterWorld(Collider col)
	{
		if (col == null)
		{
			return Vector3.zero;
		}
		if (!(col is BoxCollider boxCollider))
		{
			if (!(col is SphereCollider sphereCollider))
			{
				if (col is CapsuleCollider capsuleCollider)
				{
					return capsuleCollider.transform.TransformPoint(capsuleCollider.center);
				}
				return col.bounds.center;
			}
			return sphereCollider.transform.TransformPoint(sphereCollider.center);
		}
		return boxCollider.transform.TransformPoint(boxCollider.center);
	}

	private void EnsureFullCoverSoftBuildAreasCacheBuilt()
	{
		if (fullCoverSoftCacheBuilt)
		{
			return;
		}
		fullCoverSoftCacheBuilt = true;
		fullCoverSoftBuildAreasCache.Clear();
		if (!IsFullCoverSoftMode() || currentWorldZone == null || currentWorldZone.ZoneCollider == null)
		{
			return;
		}
		string customBuildAreaId = currentBuildData.Definition.customBuildAreaId;
		Bounds bounds = currentWorldZone.ZoneCollider.bounds;
		Collider[] array = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, 524288);
		if (array == null || array.Length == 0)
		{
			return;
		}
		foreach (Collider collider in array)
		{
			if (!(collider == null) && collider.TryGetComponent<BuildArea>(out var component) && !(component.Id != customBuildAreaId) && !fullCoverSoftBuildAreasCache.Contains(component))
			{
				fullCoverSoftBuildAreasCache.Add(component);
			}
		}
	}

	private void ClearFullCoverSoftBuildAreaHints()
	{
		foreach (KeyValuePair<BuildArea, Wgo> fullCoverSoftHintWgo in fullCoverSoftHintWgos)
		{
			Wgo value = fullCoverSoftHintWgo.Value;
			if (value != null)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
		fullCoverSoftHintWgos.Clear();
		fullCoverSoftHintBounds.Clear();
		fullCoverSoftBuildAreasCache.Clear();
		currentFullCoverSoftHintArea = null;
		fullCoverSoftCacheBuilt = false;
	}

	private static bool TryGetWgoBuildAreaBounds(Wgo wgo, out Bounds bounds)
	{
		bounds = default(Bounds);
		if (wgo == null)
		{
			return false;
		}
		bool flag = false;
		Collider[] componentsInChildren = wgo.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (!(collider == null) && collider.gameObject.activeInHierarchy && collider.gameObject.layer == 19)
			{
				Debug.Log($"col: {collider.gameObject.name}, bounds: {collider.bounds}");
				if (!flag)
				{
					flag = true;
					bounds = collider.bounds;
				}
				else
				{
					bounds.Encapsulate(collider.bounds);
				}
			}
		}
		return flag;
	}

	private Wgo GetOrCreateFullCoverSoftHint(BuildArea buildArea)
	{
		if (buildArea == null)
		{
			return null;
		}
		if (fullCoverSoftHintWgos.TryGetValue(buildArea, out var value) && value != null)
		{
			return value;
		}
		GameScene currentGameScene = MainGame.PlayerController.CurrentGameScene;
		if (currentGameScene == null)
		{
			return null;
		}
		string text = (string.IsNullOrEmpty(currentBuildData.Definition.customWgoPlacePreview) ? currentBuildData.WgoId : currentBuildData.Definition.customWgoPlacePreview);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		WgoData wgoData = new WgoData(text, Vector3.zero, currentGameScene.Id)
		{
			isTempObject = true
		};
		if (buildPointer.PointerObject is WgoBuildPointer wgoBuildPointer)
		{
			wgoData.MainWgoPartData.variationId = wgoBuildPointer.Target.MainWgoPart.WgoPartData.variationId;
			wgoData.MainWgoPartData.rotationIndex = (buildArea.HasRotationRequirement ? buildArea.RotationRequirement : wgoBuildPointer.Target.MainWgoPart.WgoPartData.rotationIndex);
		}
		Wgo wgo = Wgo.Spawn(wgoData, currentGameScene.transform, registerInChunkManagerIfStatic: true, ignoreChunkRegistration: true, applyDefaultWgoPartState: true);
		wgo.UpdateChunkVisibility(isVisible: true);
		if (TryGetWgoBuildAreaBounds(wgo, out var bounds))
		{
			fullCoverSoftHintBounds[buildArea] = bounds;
		}
		Collider[] componentsInChildren = wgo.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (collider != null)
			{
				collider.gameObject.SetActive(value: false);
			}
		}
		NavMeshCutBoxCustom[] componentsInChildren2 = wgo.GetComponentsInChildren<NavMeshCutBoxCustom>(includeInactive: true);
		foreach (NavMeshCutBoxCustom navMeshCutBoxCustom in componentsInChildren2)
		{
			if (navMeshCutBoxCustom != null)
			{
				navMeshCutBoxCustom.gameObject.SetActive(value: false);
			}
		}
		wgo.SetSelectionTint(Color.green, 0.35f);
		componentsInChildren = wgo.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider2 in componentsInChildren)
		{
			if (collider2 != null)
			{
				collider2.gameObject.SetActive(value: false);
			}
		}
		fullCoverSoftHintWgos[buildArea] = wgo;
		return wgo;
	}

	private bool IsFullCoverSoftBuildAreaFree(BuildArea buildArea)
	{
		if (buildArea == null)
		{
			return false;
		}
		Collider buildAreaCollider = GetBuildAreaCollider(buildArea);
		if (buildAreaCollider == null)
		{
			return false;
		}
		Wgo componentInParent = buildArea.GetComponentInParent<Wgo>();
		BuildingDef definition = currentBuildData.Definition;
		Bounds bounds = buildAreaCollider.bounds;
		Vector3 halfExtents = Vector3.Max(Vector3.zero, bounds.extents - VisualConsts.XYZ_STEP);
		int num = Physics.OverlapBoxNonAlloc(bounds.center, halfExtents, fullCoverSoftOverlapColliders, Quaternion.identity, 590080);
		for (int i = 0; i < num; i++)
		{
			Collider collider = fullCoverSoftOverlapColliders[i];
			if (collider == null || collider.TryGetComponent<BuildArea>(out var _) || collider.TryGetComponent<ModuleSlotArea>(out var _))
			{
				continue;
			}
			if (PlacementBlockingArea.TryGet(collider, out var _))
			{
				if (PlacementBlockingArea.IsBlockingFor(collider, definition))
				{
					return false;
				}
				continue;
			}
			Wgo componentInParent2 = collider.GetComponentInParent<Wgo>();
			if (componentInParent2 != null)
			{
				if (!(componentInParent2 == componentInParent) && componentInParent2.Data != null && !componentInParent2.Data.isTempObject && (definition == null || !definition.ShouldIgnoreWgoGroupAsObstacle(componentInParent2.Data.Definition.wgoGroup)))
				{
					return false;
				}
				continue;
			}
			int layer = collider.gameObject.layer;
			if (layer == 8 || layer == 16)
			{
				return false;
			}
		}
		return true;
	}

	private static bool ContainsXZ(Bounds bounds, Vector3 point)
	{
		if (point.x >= bounds.min.x && point.x <= bounds.max.x && point.z >= bounds.min.z)
		{
			return point.z <= bounds.max.z;
		}
		return false;
	}

	private static bool FullyContainsXZ(Bounds container, Bounds inner, float eps = 0.001f)
	{
		if (inner.min.x >= container.min.x - eps && inner.max.x <= container.max.x + eps && inner.min.z >= container.min.z - eps)
		{
			return inner.max.z <= container.max.z + eps;
		}
		return false;
	}

	private void UpdateFullCoverSoftBuildAreaHints()
	{
		if (!IsFullCoverSoftMode())
		{
			if (fullCoverSoftHintWgos.Count > 0)
			{
				ClearFullCoverSoftBuildAreaHints();
			}
			return;
		}
		EnsureFullCoverSoftBuildAreasCacheBuilt();
		if (fullCoverSoftBuildAreasCache.Count == 0)
		{
			return;
		}
		Bounds container = default(Bounds);
		if (buildPointer != null && buildPointer.PointerObject is BuildPointerObject buildPointerObject)
		{
			container = buildPointerObject.GetWorldRoundedBounds();
		}
		BuildArea buildArea = null;
		for (int i = 0; i < fullCoverSoftBuildAreasCache.Count; i++)
		{
			BuildArea buildArea2 = fullCoverSoftBuildAreasCache[i];
			Collider buildAreaCollider = GetBuildAreaCollider(buildArea2);
			if (!(buildAreaCollider == null) && FullyContainsXZ(container, buildAreaCollider.bounds))
			{
				buildArea = buildArea2;
				break;
			}
		}
		currentFullCoverSoftHintArea = buildArea;
		for (int j = 0; j < fullCoverSoftBuildAreasCache.Count; j++)
		{
			BuildArea buildArea3 = fullCoverSoftBuildAreasCache[j];
			Collider buildAreaCollider2 = GetBuildAreaCollider(buildArea3);
			if (buildAreaCollider2 == null)
			{
				continue;
			}
			if (!IsFullCoverSoftBuildAreaFree(buildArea3))
			{
				if (fullCoverSoftHintWgos.TryGetValue(buildArea3, out var value) && value != null)
				{
					UnityEngine.Object.Destroy(value.gameObject);
					fullCoverSoftHintWgos.Remove(buildArea3);
					fullCoverSoftHintBounds.Remove(buildArea3);
				}
				continue;
			}
			Wgo orCreateFullCoverSoftHint = GetOrCreateFullCoverSoftHint(buildArea3);
			if (!(orCreateFullCoverSoftHint == null))
			{
				Vector3 center = buildAreaCollider2.bounds.center;
				float y = buildArea3.transform.position.y;
				Debug.DrawLine(center, center + Vector3.up * 2.5f, Color.green, 5f);
				Vector3 position = Vector3.zero;
				if (fullCoverSoftHintBounds.TryGetValue(buildArea3, out var value2))
				{
					Vector3 vector = -value2.center;
					vector.y = 0f;
					position = center + vector;
					position.y = y;
				}
				orCreateFullCoverSoftHint.Data.Position = position;
				orCreateFullCoverSoftHint.transform.position = position;
				orCreateFullCoverSoftHint.gameObject.SetActive(buildArea3 != buildArea);
			}
		}
	}
}
