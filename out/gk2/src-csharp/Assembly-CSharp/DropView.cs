using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DropView : MonoBehaviour, IBubbleDrawable, IChunkableObject
{
	[Serializable]
	public class DropResCurve
	{
		public AnimationCurve curve;

		[Range(0f, 1f)]
		public float durationFactor;
	}

	private const int POSSIBLE_MASK = 16843008;

	private const float START_COL_RADIUS = 0.001f;

	private const float COL_RADIUS_STEP = 0.005f;

	private const int OVERLAP_COLLIDERS_MAX_COUNT = 50;

	private const float BigDropTopSurfaceThickness = 0.04f;

	private const float SurfaceSepEpsilonSq = 1E-06f;

	private const string DROP_PREFAB_KEY = "Assets/AddressableAssets/Drops/Drop.prefab";

	private static Collider[] overlapColliders = new Collider[50];

	private static AsyncOperationHandle<GameObject> dropPrefabHandle;

	private static GameObject cachedDropPrefab;

	[SerializeField]
	private CapsuleCollider smallItemCollider;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private BoxCollider bigDropTopSurface;

	[SerializeField]
	[Space]
	private float mergeDelayTime = 0.3f;

	[SerializeField]
	private float collectDelayTime = 0.6f;

	[SerializeField]
	private float spawnForceImpulse;

	[SerializeField]
	private float unstackForceImpulse = 5f;

	[SerializeField]
	private float unstackForceImpulseBigDrop = 100f;

	[SerializeField]
	private float spawnScaleTweenDuration = 0.25f;

	[SerializeField]
	private Ease spawnScaleTweenEase = Ease.OutBack;

	[SerializeField]
	[Space]
	private bool isPhysicBounce;

	[SerializeField]
	private float bounceForceImpulseFromPlayer;

	[SerializeField]
	private float bounceForceImpulse;

	[SerializeField]
	private float bounceTime;

	[SerializeField]
	private float bounceHeight;

	[SerializeField]
	private List<DropResCurve> bounceAnimationCurves = new List<DropResCurve>();

	[SerializeField]
	[Space]
	private DropViewAtomBase smallItem;

	[SerializeField]
	private DropViewAtomBase bigItem;

	[SerializeField]
	private Transform bigItemMeshesTransform;

	[SerializeField]
	[Space]
	private float dropRandomizerRadius = 0.8f;

	[SerializeField]
	[Range(0f, 1f)]
	private float positionRandomization;

	[SerializeField]
	private Bounds bounds;

	[SerializeField]
	private KickSettings smallDropKickSettings;

	[SerializeField]
	private KickSettings bigDropKickSettings;

	[SerializeField]
	[Space]
	private DropData data;

	private bool isKicked;

	private bool isDespawning;

	private bool isMergeDelayed;

	private bool isCollectDelayed;

	private Vector3 spawnDirection = Vector3.back;

	private DropViewAtomBase activeDropViewAtom;

	private bool isPhysicDisabled;

	private bool shouldPlayBounce;

	private float smallColliderRadius;

	private float bounceTimer;

	private DropResCurve bounceAnimationCurve;

	private Vector3 bigItemMeshesStartLocalPosition;

	private bool registeredInChunkManager;

	private WaterFloatingObject waterFloatingObject;

	private Transform waterBobRoot;

	private bool ignoresItemCollisions;

	private List<Collider> allNestedColliders;

	private IInteractionHandler interactionHandler;

	private Coroutine moveToCollectorCoroutine;

	private Coroutine moveToPositionCoroutine;

	public static float PreloadProgress { get; private set; }

	public DropData Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public IInteractionHandler InteractionHandler => interactionHandler;

	public bool IsDespawning => isDespawning;

	public bool IsCollectDelayed => isCollectDelayed;

	public bool IsPhysicDisabled => isPhysicDisabled;

	public bool IsTimedCollecting { get; private set; }

	public bool CanBeMerged
	{
		get
		{
			if (!isDespawning && !isMergeDelayed && !isPhysicDisabled)
			{
				return !IsTimedCollecting;
			}
			return false;
		}
	}

	public bool IsRiverDump { get; private set; }

	public SGuid BubbleDrawableUniqueId => data.UniqueId;

	public List<LazyWidgetDataBase> BubbleDrawableWidgets => GetWidgetData();

	public Vector3 BubbleDrawablePosition => data.Position + Vector3.up * 0.75f;

	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	private void Awake()
	{
		bigItemMeshesStartLocalPosition = bigItemMeshesTransform.localPosition;
	}

	public static async UniTask PreloadAsync()
	{
		if (cachedDropPrefab != null)
		{
			PreloadProgress = 1f;
			return;
		}
		GameShutdown.ThrowIfRequested();
		if (!dropPrefabHandle.IsValid())
		{
			dropPrefabHandle = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Drops/Drop.prefab");
		}
		try
		{
			cachedDropPrefab = await dropPrefabHandle.ToUniTask(Progress.Create(delegate(float p)
			{
				PreloadProgress = p;
			}), PlayerLoopTiming.Update, GameShutdown.Token, cancelImmediately: true, autoReleaseWhenCanceled: true);
		}
		catch (OperationCanceledException)
		{
			dropPrefabHandle = default(AsyncOperationHandle<GameObject>);
			throw;
		}
		if (cachedDropPrefab == null)
		{
			Debug.LogError("[DropView] Failed to preload drop prefab at [Assets/AddressableAssets/Drops/Drop.prefab]");
		}
		PreloadProgress = ((cachedDropPrefab != null) ? 1f : 0f);
	}

	private static GameObject GetDropPrefabSync()
	{
		if (cachedDropPrefab != null)
		{
			return cachedDropPrefab;
		}
		if (GameShutdown.IsRequested)
		{
			return null;
		}
		if (!dropPrefabHandle.IsValid())
		{
			dropPrefabHandle = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Drops/Drop.prefab");
		}
		cachedDropPrefab = dropPrefabHandle.WaitForCompletion();
		return cachedDropPrefab;
	}

	public static DropView SpawnDrop(DropData drop, Transform parent, bool skipWorldPlacement = false)
	{
		GameObject dropPrefabSync = GetDropPrefabSync();
		if (dropPrefabSync == null)
		{
			Debug.LogError("[DropView] Failed to spawn drop [" + drop?.Id + "]. Prefab is null.");
			return null;
		}
		DropView component = dropPrefabSync.GetComponent<DropView>();
		component = UnityEngine.Object.Instantiate(component, parent);
		component.Data = drop;
		component.ignoresItemCollisions = drop.DropType == DropType.Item && drop.Item?.Definition != null && drop.Item.Definition.CanNotBeDestroyed;
		component.UpdateChunkVisibility(isVisible: true);
		LazySingleton<ChunkManager>.Instance.RegisterDynamicChunkableObject(component, ChunkManagerLayerType.DropView);
		component.registeredInChunkManager = true;
		component.allNestedColliders = component.GetComponentsInChildren<Collider>(includeInactive: true).ToList();
		Transform transform = component.transform;
		component.isKicked = false;
		component.rb.linearVelocity = Vector3.zero;
		component.rb.angularVelocity = Vector3.zero;
		if (!skipWorldPlacement && !component.Data.IsDroppedFromPlayer)
		{
			if (component.Data.Size == ItemSize.Small)
			{
				component.DoKick(component.CorrectSpawnPos().normalized, 0.33f);
			}
			else
			{
				Vector3 foundDropPos = component.Data.Position;
				bool flag = false;
				List<Vector3> occupiedBigDropPositions = GetOccupiedBigDropPositions(component.Data.UniqueId);
				if (SpecialPhysicsCastUtils.GetSpawnPosForBigDropRectangular(component.data.Position, LazyConsts.BIG_DROP_COLLIDER_SIZE, 0.19999999f, 20, 0.19999999f, 20, out foundDropPos, occupiedBigDropPositions))
				{
					flag = true;
				}
				if (!flag)
				{
					foundDropPos = GetSpawnPosFromSample(component.Data.Position);
				}
				component.Data.Position = foundDropPos;
				Array.Clear(overlapColliders, 0, 50);
				Vector3 center = component.Data.Position + Vector3.up * (LazyConsts.BIG_DROP_COLLIDER_SIZE.y / 2f + 0.1f);
				Vector3 halfExtents = LazyConsts.BIG_DROP_COLLIDER_SIZE / 2f;
				Physics.OverlapBoxNonAlloc(center, halfExtents, overlapColliders, Quaternion.Euler(Vector3.zero), 65536);
				float minSeparation = Mathf.Max(LazyConsts.BIG_DROP_COLLIDER_SIZE.x, LazyConsts.BIG_DROP_COLLIDER_SIZE.z);
				Collider[] array = overlapColliders;
				foreach (Collider collider in array)
				{
					if (!(collider == null))
					{
						DropView componentInParent = collider.gameObject.GetComponentInParent<DropView>();
						if (!(componentInParent == null) && !(componentInParent == component) && !componentInParent.isPhysicDisabled)
						{
							Vector3 preferredAway = componentInParent.transform.position - component.Data.Position;
							componentInParent.SeparateAlongSurfaceFrom(component.Data.Position, preferredAway, minSeparation);
							componentInParent.StartCoroutine(componentInParent.BeKinematicDropForGivenFrames(2));
						}
					}
				}
			}
		}
		bool flag2 = drop.DropType != 0 || drop.Item.Definition.itemGroupIds.Contains("body");
		component.interactionHandler = (flag2 ? new ZombieDropInteractionHandler().Init(component) : new BigDropInteractionHandler().Init(component));
		transform.position = component.Data.Position;
		component.rb.position = component.Data.Position;
		component.Data.OnCountChanged += component.UpdateTextSprite;
		component.UpdateView();
		if (!skipWorldPlacement)
		{
			component.UpdateEffects();
			component.StartCoroutine(component.MergeDelayCoroutine());
			component.StartCoroutine(component.CollectDelayCoroutine());
		}
		component.UpdateTextSprite();
		component.PlaySpawnScaleTween();
		component.SetColliderSmall();
		if (!skipWorldPlacement)
		{
			PlayItemDropSound(component);
		}
		return component;
	}

	public static void PlayItemDropSound(DropView dropView)
	{
		List<string> itemGroupIds = dropView.Data.Item.Definition.itemGroupIds;
		if (itemGroupIds.Contains("zombie"))
		{
			LazyAudio.PlayAtGameObject("oh_zombie_drop", dropView.transform, SpatialType.sound3D);
		}
		else if (itemGroupIds.Contains("corpse"))
		{
			LazyAudio.PlayAtGameObject("oh_corpse_drop", dropView.transform, SpatialType.sound3D);
		}
		else if (dropView.Data.Item.id == "wood")
		{
			LazyAudio.PlayAtGameObject("oh_wood_drop", dropView.transform, SpatialType.sound3D);
		}
	}

	private static List<Vector3> GetOccupiedBigDropPositions(SGuid excludeUniqueId)
	{
		List<Vector3> list = new List<Vector3>();
		GameSceneData gameSceneDataById = MainGame.Instance.GameSave.WorldData.GetGameSceneDataById(MainGame.PlayerData.currentGameSceneId);
		if (gameSceneDataById == null)
		{
			return list;
		}
		foreach (DropData droppedItem in gameSceneDataById.droppedItems)
		{
			if (droppedItem != null && !droppedItem.IsRemoving && !(droppedItem.UniqueId == excludeUniqueId) && droppedItem.Size == ItemSize.Big)
			{
				list.Add(droppedItem.Position);
			}
		}
		return list;
	}

	private void SeparateAlongSurfaceFrom(Vector3 anchorWorldPos, Vector3 preferredAway, float minSeparation)
	{
		RaycastHit groundHit;
		Vector3 vector = (TryGetGroundNormal(out groundHit) ? groundHit.normal : Vector3.up);
		Vector3 vector2 = Vector3.ProjectOnPlane(base.transform.position - anchorWorldPos, vector);
		float magnitude = vector2.magnitude;
		if (!(magnitude >= minSeparation))
		{
			Vector3 vector3 = ((magnitude * magnitude > 1E-06f) ? vector2.normalized : GetSurfaceSeparationDirection(preferredAway, vector));
			float num = minSeparation - magnitude;
			Vector3 vector4 = base.transform.position + vector3 * num;
			if (TrySnapToGround(vector4, out var snapped))
			{
				vector4 = snapped;
			}
			ApplySyncedWorldPosition(VisualConsts.GetRoundedPosXZ(vector4));
		}
	}

	private void UnstackAlongSurface(Vector3 penetrationDirection, float penetrationDistance)
	{
		RaycastHit groundHit;
		Vector3 groundNormal = (TryGetGroundNormal(out groundHit) ? groundHit.normal : Vector3.up);
		Vector3 surfaceSeparationDirection = GetSurfaceSeparationDirection(penetrationDirection, groundNormal);
		float num = Mathf.Max(penetrationDistance, 0.05f);
		Vector3 vector = base.transform.position + surfaceSeparationDirection * num;
		if (TrySnapToGround(vector, out var snapped))
		{
			vector = snapped;
		}
		ApplySyncedWorldPosition(VisualConsts.GetRoundedPosXZ(vector));
	}

	private static Vector3 GetSurfaceSeparationDirection(Vector3 rawSep, Vector3 groundNormal)
	{
		Vector3 vector = Vector3.ProjectOnPlane(rawSep, groundNormal);
		if (vector.sqrMagnitude > 1E-06f)
		{
			return vector.normalized;
		}
		Vector3 vector2 = rawSep.XZ();
		vector = Vector3.ProjectOnPlane((vector2.sqrMagnitude > 1E-06f) ? vector2 : Vector3.right, groundNormal);
		if (vector.sqrMagnitude > 1E-06f)
		{
			return vector.normalized;
		}
		Vector3 vector3 = Vector3.Cross(groundNormal, Vector3.up);
		if (vector3.sqrMagnitude <= 1E-06f)
		{
			vector3 = Vector3.Cross(groundNormal, Vector3.right);
		}
		if (!(vector3.sqrMagnitude > 1E-06f))
		{
			return Vector3.right;
		}
		return vector3.normalized;
	}

	private bool TryGetGroundNormal(out RaycastHit groundHit)
	{
		return SpecialPhysicsCastUtils.TryGetTopmostGroundHit(base.transform.position, out groundHit);
	}

	private static bool TrySnapToGround(Vector3 worldPos, out Vector3 snapped)
	{
		return SpecialPhysicsCastUtils.TrySnapDropPosToTopmostGround(worldPos, out snapped);
	}

	private void SyncBigDropTopSurface(bool physicsEnabled)
	{
		if (bigDropTopSurface == null)
		{
			return;
		}
		if (data == null || data.Size != ItemSize.Big || !physicsEnabled || ignoresItemCollisions)
		{
			bigDropTopSurface.gameObject.SetActive(value: false);
			return;
		}
		DropViewAtomMesh dropViewAtomMesh = activeDropViewAtom as DropViewAtomMesh;
		Collider collider = ((dropViewAtomMesh != null && dropViewAtomMesh.MeshElement != null) ? dropViewAtomMesh.MeshElement.physicsCollider : null);
		if (collider == null)
		{
			bigDropTopSurface.gameObject.SetActive(value: false);
			return;
		}
		ApplyBigDropTopSurfaceShape(collider);
		bigDropTopSurface.gameObject.SetActive(value: true);
	}

	private void ApplyBigDropTopSurfaceShape(Collider phys)
	{
		Bounds bounds = phys.bounds;
		float num = 0.04f;
		float y = num / Mathf.Max(base.transform.lossyScale.y, 0.0001f);
		Vector3 position = new Vector3(bounds.center.x, bounds.max.y - num * 0.5f, bounds.center.z);
		Vector3 center = base.transform.InverseTransformPoint(position);
		Vector3 vector = base.transform.InverseTransformVector(new Vector3(bounds.size.x, num, bounds.size.z));
		vector = new Vector3(Mathf.Abs(vector.x), y, Mathf.Abs(vector.z));
		bigDropTopSurface.center = center;
		bigDropTopSurface.size = vector;
		if (phys.sharedMaterial != null)
		{
			bigDropTopSurface.sharedMaterial = phys.sharedMaterial;
		}
	}

	private void ApplySyncedWorldPosition(Vector3 worldPos)
	{
		data.Position = worldPos;
		base.transform.position = worldPos;
		rb.position = worldPos;
		if (!rb.isKinematic)
		{
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}

	private static Vector3 GetSpawnPosFromSample(Vector3 gridCenter)
	{
		float height = 0.2f;
		float num = 2f;
		float num2 = 2f;
		float radius = 0.6f;
		float radius2 = 2f;
		float num3 = 0.6f;
		float num4 = 0.12f;
		PoissonDiskSampler3D poissonDiskSampler3D = new PoissonDiskSampler3D(num, height, num2, radius);
		Array.Clear(overlapColliders, 0, 50);
		Physics.OverlapSphereNonAlloc(gridCenter, radius2, overlapColliders, 66880);
		List<Vector3> list = poissonDiskSampler3D.Samples();
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = new Vector3(list[i].x - num / 2f, 0f, list[i].z - num2 / 2f);
		}
		List<Vector3> list2 = new List<Vector3>();
		foreach (Vector3 item in list.ToList())
		{
			bool flag = false;
			Collider[] array = overlapColliders;
			foreach (Collider collider in array)
			{
				if (!(collider == null) && !(collider.gameObject == null) && collider.gameObject.layer != 16 && Vector3.Distance(collider.bounds.center, gridCenter + item) <= num3)
				{
					list.Remove(item);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			foreach (DropData droppedItem in MainGame.Instance.GameSave.WorldData.GetGameSceneDataById(MainGame.PlayerData.currentGameSceneId).droppedItems)
			{
				if (droppedItem.Size == ItemSize.Big)
				{
					if (Vector3.Distance(droppedItem.Position, gridCenter + item) <= num3)
					{
						list.Remove(item);
						flag = true;
						break;
					}
				}
				else if (droppedItem.Size == ItemSize.Small && Vector3.Distance(droppedItem.Position, gridCenter + item) <= num4)
				{
					list.Remove(item);
					flag = true;
					break;
				}
			}
			if (!flag && SpecialPhysicsCastUtils.TrySnapDropPosToTopmostGround(gridCenter + item, out var snapped) && SpecialPhysicsCastUtils.IsReachableDropElevation(gridCenter.y, snapped.y))
			{
				list2.Add(snapped);
			}
		}
		if (list2.Count > 0)
		{
			return list2.GetRandom();
		}
		return gridCenter;
	}

	public void DespawnView()
	{
		isDespawning = true;
		data.OnCountChanged -= UpdateTextSprite;
		UnityEngine.Object.Destroy(base.gameObject);
		if (registeredInChunkManager)
		{
			LazySingleton<ChunkManager>.Instance.UnregisterDynamicChunkableObject(this, ChunkManagerLayerType.DropView);
			registeredInChunkManager = false;
		}
	}

	private void OnDisable()
	{
		base.transform.DOKill();
		bigItemMeshesTransform?.DOKill(complete: true);
	}

	private void OnDestroy()
	{
		if (registeredInChunkManager)
		{
			LazySingleton<ChunkManager>.Instance.UnregisterDynamicChunkableObject(this, ChunkManagerLayerType.DropView);
			registeredInChunkManager = false;
		}
	}

	public void TryMoveToCollector(Transform target)
	{
		if (!isPhysicDisabled && !IsTimedCollecting)
		{
			moveToCollectorCoroutine = StartCoroutine(MoveToCollector(target));
		}
	}

	public void MoveToCollectorTimed(Transform target, float duration)
	{
		if (!isDespawning && data != null && !data.IsRemoving)
		{
			StopMoveCoroutines();
			isCollectDelayed = false;
			IsTimedCollecting = true;
			if (target == null || duration <= 0f)
			{
				CollectTimedDrop();
			}
			else
			{
				moveToCollectorCoroutine = StartCoroutine(MoveToCollectorTimedCoroutine(target, duration));
			}
		}
	}

	public void MoveToCustomPosition(Vector3 position)
	{
		if (!isPhysicDisabled)
		{
			moveToPositionCoroutine = StartCoroutine(MoveToPosition(position));
		}
	}

	public void SetInteractionVisualState(bool isUnderInteraction)
	{
		activeDropViewAtom.SetInteractionState(isUnderInteraction);
	}

	public List<LazyWidgetDataBase> GetWidgetData()
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		if (MainGame.PlayerController.PlayerInteractionComponent.BigDropUnderInteraction == this && (interactionHandler.HasInteraction() || interactionHandler.HasInteraction2()))
		{
			InteractionInfos interactionInfos = interactionHandler.GetInteractionInfos();
			List<UIInteractionHintRowWidgetData> list2 = new List<UIInteractionHintRowWidgetData>();
			foreach (InteractionInfo item in interactionInfos.list)
			{
				if (!string.IsNullOrEmpty(item.text) || !string.IsNullOrEmpty(item.customIconId))
				{
					list2.Add(new UIInteractionHintRowWidgetData(item));
				}
			}
			if (list2.Count > 0)
			{
				list.Add(new UIInteractionHintWidgetData(list2));
			}
		}
		return list;
	}

	private void Update()
	{
		if (!MainGame.IsGamePaused)
		{
			data.Position = base.transform.position;
			if (shouldPlayBounce && !isPhysicBounce)
			{
				PlayBounce();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		TryUnwaterDrop(other);
		TryMergeIfDrop(other);
	}

	private void OnTriggerStay(Collider other)
	{
		TryMergeIfDrop(other);
	}

	private void TryMergeIfDrop(Collider other)
	{
		if (CanBeMerged)
		{
			DropView componentInParent = other.GetComponentInParent<DropView>();
			if (componentInParent != null && componentInParent.CanBeMerged)
			{
				Merge(componentInParent);
			}
		}
	}

	private void TryUnwaterDrop(Collider other)
	{
		if (other.gameObject.layer == 4)
		{
			SpecialPhysicsCastUtils.GetPlayerDropPosition(MainGame.PlayerData.position.Value, MainGame.PlayerData.Direction, out var foundDropPos);
			DropView component = GetComponent<DropView>();
			if ((bool)component)
			{
				component.gameObject.transform.position = foundDropPos;
			}
		}
	}

	private void UpdateTextSprite()
	{
		if (!isDespawning && data != null)
		{
			activeDropViewAtom.GetSpriteText().SetText((data.Count <= 1) ? string.Empty : data.Count.ToString());
		}
	}

	private void UpdateEffects()
	{
		if (data.Size == ItemSize.Big)
		{
			shouldPlayBounce = true;
		}
		if (!data.IsDroppedFromPlayer && data.Size == ItemSize.Small)
		{
			rb.AddForce(spawnDirection * spawnForceImpulse, ForceMode.Impulse);
		}
		if (shouldPlayBounce && !isPhysicBounce)
		{
			bounceAnimationCurve = bounceAnimationCurves[UnityEngine.Random.Range(0, bounceAnimationCurves.Count - 1)];
			LazyTimer.AddTimer(bounceTime, StopBounce);
		}
	}

	private void UpdateView()
	{
		smallItem.Deactivate();
		bigItem.Deactivate();
		bool flag = data.Size == ItemSize.Small;
		activeDropViewAtom = (flag ? smallItem : bigItem);
		if (activeDropViewAtom is DropViewAtomMesh dropViewAtomMesh && data.Item.Definition.isLinkedToWgo)
		{
			ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(data.Item.UniqueId);
			if (zombie != null)
			{
				SkinPresetGK2 presetForWgoData = ZombieSkinHelper.GetPresetForWgoData(zombie, "zombie_worker");
				if (presetForWgoData != null)
				{
					dropViewAtomMesh.ActivateZombie(presetForWgoData.head.id.ToString("D4"), presetForWgoData.head.palette);
					SyncBigDropTopSurface(!isPhysicDisabled);
					ApplyItemCollisionFilter();
					return;
				}
			}
		}
		activeDropViewAtom.Activate(data.IconId);
		SyncBigDropTopSurface(!isPhysicDisabled);
		ApplyItemCollisionFilter();
	}

	private void PlaySpawnScaleTween()
	{
		if (data.Size != ItemSize.Big)
		{
			base.transform.DOKill();
			Vector3 localScale = base.transform.localScale;
			base.transform.localScale = Vector3.zero;
			base.transform.DOScale(localScale, spawnScaleTweenDuration).SetEase(spawnScaleTweenEase);
		}
	}

	private void DoBounceFromGround()
	{
		rb.AddForce(Vector3.up * (data.IsDroppedFromPlayer ? bounceForceImpulseFromPlayer : bounceForceImpulse), ForceMode.Impulse);
	}

	private IEnumerator MergeDelayCoroutine()
	{
		float mergeDelayTimer2 = 0f;
		isMergeDelayed = true;
		WaitForEndOfFrame wait = new WaitForEndOfFrame();
		while (mergeDelayTimer2 < mergeDelayTime)
		{
			if (!MainGame.IsGamePaused)
			{
				mergeDelayTimer2 += Time.deltaTime;
			}
			yield return wait;
		}
		isMergeDelayed = false;
		mergeDelayTimer2 = 0f;
		while (mergeDelayTimer2 < 0.1f)
		{
			if (!MainGame.IsGamePaused)
			{
				mergeDelayTimer2 += Time.deltaTime;
			}
			yield return wait;
		}
		TryGetDropsAroundAndMerge();
	}

	private IEnumerator CollectDelayCoroutine()
	{
		float timer2 = 0f;
		isCollectDelayed = true;
		bool isFishing = LazyUI.Get<UIFishingWindow>().IsShownAndTop;
		WaitForEndOfFrame wait = new WaitForEndOfFrame();
		while (timer2 < collectDelayTime)
		{
			if (!MainGame.IsGamePaused || isFishing)
			{
				timer2 += Time.deltaTime;
			}
			yield return wait;
		}
		isCollectDelayed = false;
		timer2 = 0f;
		while (timer2 < 0.1f)
		{
			if (!MainGame.IsGamePaused || isFishing)
			{
				timer2 += Time.deltaTime;
			}
			yield return wait;
		}
	}

	private void Merge(DropView dropToMergeWith)
	{
		if (data.TryAddDropItemPartial(dropToMergeWith.data))
		{
			UpdateTextSprite();
			if (dropToMergeWith.Data.Count == 0)
			{
				MainGame.Instance.dropSystem.RemoveDrop(dropToMergeWith.Data, Data.WorldId);
			}
			if (!dropToMergeWith.isDespawning)
			{
				dropToMergeWith.UpdateTextSprite();
			}
		}
	}

	private void TryGetDropsAroundAndMerge()
	{
		if (!CanBeMerged)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 1f, 65536);
		for (int i = 0; i < array.Length; i++)
		{
			DropView dropView = null;
			if (array[i].TryGetComponent<DropViewAtomSprite>(out var component))
			{
				dropView = component.ParentDropGameObject;
				if (!(dropView == this) && dropView.CanBeMerged)
				{
					Merge(dropView);
				}
			}
		}
	}

	public void StopMoving()
	{
		StopMoveCoroutines();
		SetNonPhysicState(isPhysicDisabled: false);
	}

	private void StopMoveCoroutines()
	{
		if (moveToCollectorCoroutine != null)
		{
			StopCoroutine(moveToCollectorCoroutine);
			moveToCollectorCoroutine = null;
		}
		if (moveToPositionCoroutine != null)
		{
			StopCoroutine(moveToPositionCoroutine);
			moveToPositionCoroutine = null;
		}
		IsTimedCollecting = false;
	}

	private IEnumerator MoveToCollector(Transform target)
	{
		Vector3 moveVector = target.position - base.transform.position;
		float speedK = 1f;
		SetNonPhysicState(isPhysicDisabled: true);
		bool isFishing = LazyUI.Get<UIFishingWindow>().IsShownAndTop;
		while (moveVector.magnitude > 0.1f)
		{
			if (!MainGame.IsGamePaused || isFishing)
			{
				speedK += 0.2f;
				Vector3 vector = moveVector.normalized * ((3f + speedK) * Time.deltaTime);
				if (vector.magnitude > moveVector.magnitude)
				{
					base.transform.position = target.position;
				}
				else
				{
					base.transform.position += vector;
				}
				moveVector = target.position - base.transform.position;
				if (moveVector.magnitude >= 4f)
				{
					break;
				}
			}
			yield return new WaitForEndOfFrame();
		}
		SetNonPhysicState(isPhysicDisabled: false);
		moveToCollectorCoroutine = null;
	}

	private IEnumerator MoveToCollectorTimedCoroutine(Transform target, float duration)
	{
		Vector3 startPosition = base.transform.position;
		float elapsed = 0f;
		SetNonPhysicState(isPhysicDisabled: true);
		WaitForEndOfFrame wait = new WaitForEndOfFrame();
		while (elapsed < duration)
		{
			if (isDespawning || data == null || data.IsRemoving)
			{
				IsTimedCollecting = false;
				yield break;
			}
			if (target == null)
			{
				moveToCollectorCoroutine = null;
				CollectTimedDrop();
				yield break;
			}
			if (!MainGame.IsGamePaused)
			{
				elapsed += Time.deltaTime;
				float num = Mathf.Clamp01(elapsed / duration);
				SetWorldPosition(Vector3.Lerp(startPosition, target.position, num * num));
			}
			yield return wait;
		}
		if (isDespawning || data == null || data.IsRemoving)
		{
			IsTimedCollecting = false;
			yield break;
		}
		if (target != null)
		{
			SetWorldPosition(target.position);
		}
		moveToCollectorCoroutine = null;
		CollectTimedDrop();
	}

	private void CollectTimedDrop()
	{
		IsTimedCollecting = false;
		if (!isDespawning && data != null && !data.IsRemoving)
		{
			MainGame.PlayerData.CollectDrop(this);
		}
	}

	private void SetWorldPosition(Vector3 position)
	{
		base.transform.position = position;
		if (rb != null)
		{
			rb.position = position;
		}
	}

	private IEnumerator MoveToPosition(Vector3 position)
	{
		Vector3 moveVector = position - base.transform.position;
		SetNonPhysicState(isPhysicDisabled: true);
		while (moveVector.magnitude > 0.1f)
		{
			if (!MainGame.IsGamePaused)
			{
				Vector3 vector = moveVector.normalized * (2f * Time.deltaTime);
				if (vector.magnitude > moveVector.magnitude)
				{
					base.transform.position = position;
				}
				else
				{
					base.transform.position += vector;
				}
				moveVector = position - base.transform.position;
			}
			yield return new WaitForEndOfFrame();
		}
		SetNonPhysicState(isPhysicDisabled: false);
		moveToPositionCoroutine = null;
	}

	private IEnumerator BeKinematicDropForGivenFrames(int framesCount = 1)
	{
		rb.isKinematic = true;
		for (int i = 0; i < framesCount; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		if (!isPhysicDisabled)
		{
			rb.isKinematic = false;
		}
		yield return null;
	}

	private void PlayBounce()
	{
		if (!(bounceTimer > bounceTime) && bounceAnimationCurve != null)
		{
			bounceTimer += Time.deltaTime;
			float time = bounceTimer / bounceTime;
			float num = bounceAnimationCurve.curve.Evaluate(time);
			bigItemMeshesTransform.localPosition = bigItemMeshesStartLocalPosition + Vector3.up * (num * bounceHeight);
		}
	}

	private void StopBounce()
	{
		if (bounceAnimationCurve != null && !(bigItemMeshesTransform == null))
		{
			bounceAnimationCurve = null;
			bounceTime = 0f;
			shouldPlayBounce = false;
			bigItemMeshesTransform.DOKill(complete: true);
			bigItemMeshesTransform.DOLocalMove(bigItemMeshesStartLocalPosition, 0.2f);
		}
	}

	private void ForceStopBounce()
	{
		if (!(bigItemMeshesTransform == null))
		{
			bounceAnimationCurve = null;
			bounceTime = 0f;
			shouldPlayBounce = false;
			bigItemMeshesTransform.DOKill(complete: true);
			bigItemMeshesTransform.localPosition = bigItemMeshesStartLocalPosition;
		}
	}

	private void FixedUpdate()
	{
		if (!MainGame.IsGamePaused)
		{
			UpdateDropPhysics(Time.fixedDeltaTime);
		}
	}

	public void DoKick(Transform collisionSourceTransform, bool ignoreDistanceToSource = false)
	{
		KickSettings kickSettings = ((data.Size == ItemSize.Small) ? smallDropKickSettings : bigDropKickSettings);
		if (ignoreDistanceToSource || !(Vector3.Distance(collisionSourceTransform.position, base.transform.position) > kickSettings.distanceToKick))
		{
			DoKick(base.transform.position - collisionSourceTransform.position);
		}
	}

	private void DoKick(Vector3 direction, float forceFactor = 1f)
	{
		if (!isKicked)
		{
			KickSettings kickSettings = ((data.Size == ItemSize.Small) ? smallDropKickSettings : bigDropKickSettings);
			isKicked = true;
			Vector3 force = Vector3.Scale(direction.normalized, kickSettings.kickForce * forceFactor);
			rb.AddForce(force, ForceMode.Impulse);
			LazyTimer.AddTimer(kickSettings.kickDelay, delegate
			{
				isKicked = false;
			});
			data.TryStartAutoDestroyTimer();
		}
	}

	private void UpdateDropPhysics(float deltaTime)
	{
		if (shouldPlayBounce && isPhysicBounce && rb.linearVelocity.y <= 0.001f)
		{
			shouldPlayBounce = false;
			DoBounceFromGround();
		}
		UpdateCollidersSizes();
		if (isPhysicDisabled || isMergeDelayed || (data.Size == ItemSize.Small && !smallItemCollider.isTrigger))
		{
			return;
		}
		Collider[] array = ((data.Size != ItemSize.Small) ? Physics.OverlapSphere(base.transform.position, 0.7f, 16843008) : Physics.OverlapSphere(base.transform.position, smallItemCollider.radius, 16843008));
		if (array != null && array.Length != 0 && data.Size != ItemSize.Small)
		{
			DropViewAtomMesh dropViewAtomMesh = bigItem as DropViewAtomMesh;
			foreach (Collider collider in array)
			{
				if (!(dropViewAtomMesh.MeshElement.physicsCollider == collider))
				{
					DropView componentInParent = collider.GetComponentInParent<DropView>();
					if ((!(componentInParent != null) || (!(componentInParent == this) && !componentInParent.isPhysicDisabled && !ignoresItemCollisions && !componentInParent.ignoresItemCollisions)) && Physics.ComputePenetration(dropViewAtomMesh.MeshElement.physicsCollider, dropViewAtomMesh.MeshElement.physicsCollider.transform.position, dropViewAtomMesh.MeshElement.physicsCollider.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var direction, out var distance) && !(direction.y > 0.5f))
					{
						UnstackAlongSurface(direction, distance);
						break;
					}
				}
			}
		}
		else
		{
			SetCollidersTriggerState(isTrigger: false);
		}
	}

	public void PrepareAsRiverDump()
	{
		IsRiverDump = true;
		SetNonPhysicState(isPhysicDisabled: true);
		interactionHandler = new DisabledDropInteractionHandler();
		EnsureWaterBobRoot();
		SetWaterBobEnabled(enabled: false);
		if (allNestedColliders == null)
		{
			return;
		}
		for (int i = 0; i < allNestedColliders.Count; i++)
		{
			if (allNestedColliders[i] != null)
			{
				allNestedColliders[i].enabled = false;
			}
		}
	}

	public void SetRiverWorldPosition(Vector3 worldPos)
	{
		if (data != null)
		{
			data.Position = worldPos;
		}
		base.transform.position = worldPos;
		if (rb != null)
		{
			rb.position = worldPos;
		}
	}

	public void SetWaterBobEnabled(bool enabled)
	{
		EnsureWaterBobRoot();
		if (waterFloatingObject != null)
		{
			waterFloatingObject.enabled = enabled;
		}
	}

	private void EnsureWaterBobRoot()
	{
		if (!(waterBobRoot != null) && !(bigItemMeshesTransform == null))
		{
			GameObject gameObject = new GameObject("RiverWaterBob");
			waterBobRoot = gameObject.transform;
			waterBobRoot.SetParent(base.transform, worldPositionStays: false);
			waterBobRoot.localPosition = Vector3.zero;
			waterBobRoot.localRotation = Quaternion.identity;
			waterBobRoot.localScale = Vector3.one;
			bigItemMeshesTransform.SetParent(waterBobRoot, worldPositionStays: true);
			waterFloatingObject = gameObject.AddComponent<WaterFloatingObject>();
			waterFloatingObject.enabled = false;
		}
	}

	public void SetNonPhysicState(bool isPhysicDisabled)
	{
		if (isPhysicDisabled)
		{
			ForceStopBounce();
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.isKinematic = true;
			rb.useGravity = false;
			rb.interpolation = RigidbodyInterpolation.None;
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			SetCollidersTriggerState(isTrigger: true);
		}
		else
		{
			SetCollidersTriggerState(isTrigger: false);
			rb.isKinematic = false;
			rb.useGravity = true;
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		this.isPhysicDisabled = isPhysicDisabled;
	}

	public void SetCollidersTriggerState(bool isTrigger)
	{
		smallItemCollider.isTrigger = isTrigger;
		if (data.Size == ItemSize.Big && activeDropViewAtom is DropViewAtomMesh dropViewAtomMesh && dropViewAtomMesh.MeshElement != null)
		{
			if (dropViewAtomMesh.MeshElement.colliderContainer != null)
			{
				dropViewAtomMesh.MeshElement.colliderContainer.SetActive(!isTrigger);
			}
			else if (dropViewAtomMesh.MeshElement.physicsCollider != null)
			{
				dropViewAtomMesh.MeshElement.physicsCollider.enabled = !isTrigger;
			}
		}
		SyncBigDropTopSurface(data != null && data.Size == ItemSize.Big && !isTrigger);
		ApplyItemCollisionFilter();
	}

	private void ApplyItemCollisionFilter()
	{
		if (!ignoresItemCollisions)
		{
			return;
		}
		ExcludeDropLayer(smallItemCollider);
		ExcludeDropLayer(bigDropTopSurface);
		if (!(activeDropViewAtom is DropViewAtomMesh dropViewAtomMesh) || !(dropViewAtomMesh.MeshElement != null))
		{
			return;
		}
		ExcludeDropLayer(dropViewAtomMesh.MeshElement.physicsCollider);
		if (dropViewAtomMesh.MeshElement.colliderContainer != null)
		{
			Collider[] componentsInChildren = dropViewAtomMesh.MeshElement.colliderContainer.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ExcludeDropLayer(componentsInChildren[i]);
			}
		}
	}

	private static void ExcludeDropLayer(Collider col)
	{
		if (col != null)
		{
			col.excludeLayers = (int)col.excludeLayers | 0x10000;
		}
	}

	public void SetColliderSmall()
	{
		smallColliderRadius = smallItemCollider.radius;
		smallItemCollider.radius = 0.001f;
	}

	public void UpdateCollidersSizes()
	{
		if (smallItemCollider.radius < smallColliderRadius)
		{
			if (smallItemCollider.radius.EqualsTo(smallColliderRadius, 0.005f))
			{
				smallItemCollider.radius = smallColliderRadius;
			}
			else
			{
				smallItemCollider.radius += 0.005f;
			}
		}
	}

	private Vector3 CorrectSpawnPos()
	{
		Vector3 vector = RandPos();
		bool flag = false;
		flag = IsOverlappingSomething(data.Position + vector, Vector3.zero, 0.05f);
		if (flag)
		{
			int num = 15;
			while (flag && num > 0)
			{
				num--;
				vector = RandPos();
				flag = IsOverlappingSomething(data.Position + vector, Vector3.zero, 0.05f);
			}
		}
		if (!flag)
		{
			data.Position += vector;
		}
		return vector;
	}

	private static bool IsOverlappingSomething(Vector3 pos, Vector3 dir, float radius)
	{
		return Physics.OverlapSphere(pos + dir / 2f, radius, 16843008).Length != 0;
	}

	private Vector3 RandPos()
	{
		return new Vector3(UnityEngine.Random.Range(0f - dropRandomizerRadius, dropRandomizerRadius), 0f, UnityEngine.Random.Range(0f - dropRandomizerRadius, dropRandomizerRadius)) * positionRandomization;
	}

	private void OnCollisionStay(Collision collisionSource)
	{
		if (data.Size == ItemSize.Big && !isPhysicDisabled && !isDespawning && !isKicked && collisionSource.gameObject.layer == 10)
		{
			DoKick(collisionSource.transform);
		}
	}

	public BurstableBounds GetChunkableData()
	{
		return new BurstableBounds(rb.position + bounds.center, bounds.size);
	}

	public void UpdateChunkVisibility(bool isVisible)
	{
		rb.isKinematic = !isVisible || isPhysicDisabled;
		rb.useGravity = isVisible && !isPhysicDisabled;
		if (data != null)
		{
			data.CanNotBeAutoDestroyed.UpdateFlag(CanNotBeAutoDestroyedReason.WhenVisibleOnScreen, isVisible);
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		this.DrawChunkGizmos();
	}
}
