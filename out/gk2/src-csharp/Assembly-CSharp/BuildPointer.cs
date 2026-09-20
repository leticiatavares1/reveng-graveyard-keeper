using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class BuildPointer : MonoBehaviour, IBubbleDrawable
{
	[SerializeField]
	private BuildSelectionCell selectionCellPrefab;

	[SerializeField]
	private BuildSelectionCell buffCellPrefab;

	[SerializeField]
	private BuildSelectionCell removeCellPrefab;

	private Vector3 pos;

	private BuildPointerObject pointerObject;

	private Vector3 shiftToVisualCenter;

	[SerializeField]
	private Transform visualCenter;

	[SerializeField]
	private float modulesLimitsWidgetOffsetZ = 0.35f;

	private readonly SGuid bubbleUniqueId = new SGuid();

	private readonly List<LazyWidgetDataBase> bubbleWidgets = new List<LazyWidgetDataBase>();

	public IBuildPointerObject PointerObject => pointerObject;

	public Vector3 ShiftToVisualCenter => shiftToVisualCenter;

	public Transform VisualCenter => visualCenter;

	public SGuid BubbleDrawableUniqueId => bubbleUniqueId;

	public List<LazyWidgetDataBase> BubbleDrawableWidgets => bubbleWidgets;

	public Vector3 BubbleDrawablePosition
	{
		get
		{
			Vector3 result = ((visualCenter != null) ? visualCenter.position : base.transform.position);
			result.z += modulesLimitsWidgetOffsetZ;
			return result;
		}
	}

	public void Enable(BuildData buildData, string worldZoneId, Vector3 startPos, List<NeedItemData> itemNeeds = null, MultiInventory multiInventory = null)
	{
		base.transform.localPosition = Vector3.zero;
		pointerObject = CreatePointerObject(buildData, itemNeeds, multiInventory);
		pointerObject.Init(buildData, worldZoneId);
		pointerObject.SetupSelectionCells(new BuildSelectionCell[3] { selectionCellPrefab, buffCellPrefab, removeCellPrefab }, pointerObject.transform);
		pointerObject.UpdateCollider();
		pointerObject.ApplySelectionCellsVisuals();
		CalculateShiftToVisualCenter();
		UpdatePos(startPos);
		UpdateModulesLimitsWidget();
	}

	public void Disable()
	{
		HideModulesLimitsWidget();
		pointerObject.OnPointerDisable();
		pointerObject.ClearSelectionCells();
		UnityEngine.Object.Destroy(pointerObject.gameObject);
	}

	public void UpdateModulesLimitsWidget()
	{
		bubbleWidgets.Clear();
		BuildingDef buildingDef = pointerObject?.BuildData?.Definition;
		if (buildingDef != null && buildingDef.HasLimits)
		{
			bubbleWidgets.Add(new UIModulesLimitsWidgetData(buildingDef));
		}
		if (!(UIObjectBubbleManager.Instance == null))
		{
			if (bubbleWidgets.Count > 0)
			{
				UIObjectBubbleManager.Instance.RequestDisplay(this);
			}
			else
			{
				UIObjectBubbleManager.Instance.Hide(this);
			}
		}
	}

	private void HideModulesLimitsWidget()
	{
		bubbleWidgets.Clear();
		if (UIObjectBubbleManager.Instance != null)
		{
			UIObjectBubbleManager.Instance.Hide(this);
		}
	}

	public void UpdatePos(Vector3 position)
	{
		pos = position;
		base.transform.position = pos;
		pointerObject.UpdatePosition(position);
	}

	public void UpdateAvailability()
	{
		pointerObject.UpdateSelectionCellsState();
	}

	public bool TryBuildActionInput()
	{
		bool num = pointerObject.TryDoBuildAction();
		if (num && pointerObject.BuildData.Definition != null && pointerObject.BuildData.Definition.buildingMode != BuildingDef.BuildingMode.Remove)
		{
			LazyAudio.PlayAndForget("build_place");
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.BuildBuilding, pointerObject.BuildData.WgoId);
		}
		return num;
	}

	public void Rotate()
	{
		base.transform.localPosition = Vector3.zero;
		pointerObject.transform.localPosition = Vector3.zero;
		pointerObject.ClearSelectionCells();
		pointerObject.Rotate();
		pointerObject.SetupSelectionCells(new BuildSelectionCell[2] { selectionCellPrefab, buffCellPrefab }, pointerObject.transform);
		pointerObject.UpdateCollider();
		pointerObject.ApplySelectionCellsVisuals();
		pointerObject.UpdateSelectionCellsState();
		CalculateShiftToVisualCenter();
		pointerObject.ShowHints();
	}

	public void SetVisibleSelectionCells(bool isVisible)
	{
		pointerObject.SetVisibleSelectionCells(isVisible);
	}

	private void Awake()
	{
		selectionCellPrefab.gameObject.SetActive(value: false);
		buffCellPrefab.gameObject.SetActive(value: false);
		removeCellPrefab.gameObject.SetActive(value: false);
	}

	private void CalculateShiftToVisualCenter()
	{
		shiftToVisualCenter = pointerObject.GetCellsCenterLocal();
		shiftToVisualCenter.y = 0f;
		visualCenter.localPosition = shiftToVisualCenter;
	}

	private BuildPointerObject CreatePointerObject(BuildData buildData, List<NeedItemData> itemNeeds = null, MultiInventory multiInventory = null)
	{
		GameObject gameObject = new GameObject("BuildPointerObject");
		gameObject.transform.SetParent(base.gameObject.transform);
		gameObject.transform.localPosition = Vector3.zero;
		GameScene currentGameScene = MainGame.PlayerController.CurrentGameScene;
		switch (buildData.BuildingMode)
		{
		case BuildingDef.BuildingMode.Place:
		{
			WgoBuildPointer wgoBuildPointer2 = gameObject.AddComponent<WgoBuildPointer>();
			return PrepareAndSpawnWgoBuildPointer(wgoBuildPointer2, buildData, currentGameScene, itemNeeds, multiInventory);
		}
		case BuildingDef.BuildingMode.ConveyorPlace:
		{
			WgoBuildPointer wgoBuildPointer = gameObject.AddComponent<ConveyorBuildPointer>();
			return PrepareAndSpawnWgoBuildPointer(wgoBuildPointer, buildData, currentGameScene, itemNeeds, multiInventory);
		}
		case BuildingDef.BuildingMode.FightingPlace:
		{
			WgoBuildPointer wgoBuildPointer3 = gameObject.AddComponent<FightingBuildPointer>();
			return PrepareAndSpawnWgoBuildPointer(wgoBuildPointer3, buildData, currentGameScene, itemNeeds, multiInventory);
		}
		case BuildingDef.BuildingMode.FightBuilding:
		{
			WgoBuildPointer wgoBuildPointer5 = gameObject.AddComponent<MilitaryBaseBuildPointer>();
			return PrepareAndSpawnWgoBuildPointer(wgoBuildPointer5, buildData, currentGameScene, itemNeeds, multiInventory);
		}
		case BuildingDef.BuildingMode.Remove:
			return gameObject.AddComponent<RemovePointer>();
		case BuildingDef.BuildingMode.Upgrade:
		{
			UpgradeBuildPointer wgoBuildPointer4 = gameObject.AddComponent<UpgradeBuildPointer>();
			return PrepareAndSpawnWgoBuildPointer(wgoBuildPointer4, buildData, currentGameScene, itemNeeds, multiInventory);
		}
		default:
			throw new NotImplementedException(string.Format("{0} for data type [{1}] wasn't implemented", "BuildingMode", buildData.Definition?.buildingMode));
		}
	}

	private WgoBuildPointer PrepareAndSpawnWgoBuildPointer(WgoBuildPointer wgoBuildPointer, BuildData buildData, GameScene gameScene, List<NeedItemData> itemNeeds = null, MultiInventory multiInventory = null)
	{
		Wgo wgo = Wgo.Spawn(new WgoData(string.IsNullOrEmpty(buildData.Definition.customWgoPlacePreview) ? buildData.WgoId : buildData.Definition.customWgoPlacePreview, base.transform.position, gameScene.Id)
		{
			isTempObject = true
		}, gameScene.transform, registerInChunkManagerIfStatic: true, ignoreChunkRegistration: true, applyDefaultWgoPartState: true);
		wgo.UpdateChunkVisibility(isVisible: true);
		wgo.MainWgoPart?.TryApplyCustomRotationSequenceStart();
		wgoBuildPointer.SetTarget(wgo, gameScene, buildData.Definition, itemNeeds, multiInventory);
		wgoBuildPointer.ShowHints();
		Collider[] componentsInChildren = wgo.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			int layer = collider.gameObject.layer;
			if (layer == 8 || layer == 11 || layer == 6)
			{
				collider.gameObject.SetActive(value: false);
			}
		}
		NavMeshCutBoxCustom[] componentsInChildren2 = wgo.GetComponentsInChildren<NavMeshCutBoxCustom>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].gameObject.SetActive(value: false);
		}
		return wgoBuildPointer;
	}
}
