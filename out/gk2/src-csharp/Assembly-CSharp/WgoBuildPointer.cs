using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class WgoBuildPointer : BuildPointerObject
{
	private enum BuildPointerRole
	{
		None,
		Extension,
		ExtensionParent
	}

	protected Wgo target;

	private List<Collider> buildColliders = new List<Collider>();

	protected GameScene gameScene;

	private BuildingDef buildingDef;

	private Func<bool> canTakeResources;

	protected Action takeResourcesAction;

	private int overlapMask;

	private bool hasCustomBuildArea;

	private Vector3 offset;

	private BoxCollider triggerCollider;

	private readonly HashSet<Wgo> wgosShownAsInactiveWhenExtensionIsPlacing = new HashSet<Wgo>();

	private readonly List<BuffCell> buffCells = new List<BuffCell>();

	private readonly List<Rect> buffUsageRects = new List<Rect>();

	private readonly List<Rect> selectionRects = new List<Rect>();

	private readonly Collider[] buffUsageOverlapColliders = new Collider[10];

	private readonly Collider[] buffAreaOverlapColliders = new Collider[10];

	private Color unDestroyableSelectionTintColor = Color.gray;

	private string unDestroyableSelectionTintColorHex = "#444e50";

	private readonly List<ModuleSlotArea> moduleSlotAreas = new List<ModuleSlotArea>();

	private readonly Collider[] fullCoverSoftAreaOverlap = new Collider[32];

	private readonly Collider[] fullCoverSoftOccupantsOverlap = new Collider[64];

	private bool fullCoverSoftQueryCached;

	private bool cachedHasCoveredFullCoverSoftArea;

	private BuildArea cachedFullCoverSoftArea;

	private bool cachedFullCoverSoftSlotFree;

	private bool fullCoverSoftQueryReusableForAvailability;

	private readonly HashSet<Wgo> overlappingHostsBuffer = new HashSet<Wgo>();

	private readonly HashSet<Wgo> lastIconOverlapHosts = new HashSet<Wgo>();

	private readonly List<Wgo> cachedEligibleIconHosts = new List<Wgo>();

	private readonly Dictionary<Wgo, WorkbenchAdditionWorldIconPresenter.State> iconStatesBuffer = new Dictionary<Wgo, WorkbenchAdditionWorldIconPresenter.State>();

	private readonly HashSet<WGODef> extensionParentDefsBuffer = new HashSet<WGODef>();

	private string extensionParentDefsBufferForId;

	private HashSet<string> cachedAllowedExtensionIds;

	private string cachedAllowedExtensionIdsForParentId;

	private bool hasPublishedIconState;

	private BuildPointerRole cachedIconRole;

	private string cachedIconPlacingWgoId;

	private BuildPointerRole cachedEligibleHostsRole;

	private string cachedEligibleHostsPlacingWgoId;

	private int cachedEligibleHostsZoneWgosVersion = int.MinValue;

	public Vector3 PreviewOffset => offset;

	public bool DrawBuffAreas { get; set; } = true;


	public Wgo Target => target;

	public void SetTarget(Wgo target, GameScene gameScene, BuildingDef buildingDef, List<NeedItemData> itemNeeds = null, MultiInventory multiInventory = null)
	{
		this.target = target;
		this.target.transform.SetParent(base.transform);
		this.gameScene = gameScene;
		this.buildingDef = buildingDef;
		hasCustomBuildArea = !string.IsNullOrEmpty(buildingDef.customBuildAreaId);
		canTakeResources = delegate
		{
			if (multiInventory == null || itemNeeds == null || multiInventory.HasItemsById(itemNeeds))
			{
				BuildingDef obj = buildingDef;
				if (obj == null || !obj.HasLimits)
				{
					return true;
				}
				return buildingDef.limitMax > buildingDef.currentLimitExpression.EvaluateInt();
			}
			return false;
		};
		takeResourcesAction = delegate
		{
			if (multiInventory != null && itemNeeds != null)
			{
				multiInventory.RemoveItems(itemNeeds);
			}
		};
	}

	public override bool HasRotation()
	{
		return target.CanBeRotated();
	}

	public override void Rotate()
	{
		target.MainWgoPart.Rotate();
	}

	public override bool TryDoBuildAction()
	{
		if (shownAsActive)
		{
			takeResourcesAction?.Invoke();
			WgoData wgoData = new WgoData(buildData.WgoId, target.Data.Position, gameScene.Id);
			if (target.CanBeRotated() && target.MainWgoPart.WgoPartData.rotationIndex != -1)
			{
				wgoData.MainWgoPartData.variationId = target.MainWgoPart.WgoPartData.variationId;
				wgoData.MainWgoPartData.rotationIndex = target.MainWgoPart.WgoPartData.rotationIndex;
			}
			Wgo wgo = gameScene.AddWgoData(wgoData);
			if (buildData.Definition != null)
			{
				foreach (LazyExpression item in buildData.Definition.expressionAfterBuilding)
				{
					item.EvaluateBool(wgoData);
				}
			}
			TrySetCustomRotation(wgo);
			if (buildingDef.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverSoft && BuildController.Instance.CurrentFullCoverSoftHintArea != null)
			{
				Wgo componentInParent = BuildController.Instance.CurrentFullCoverSoftHintArea.GetComponentInParent<Wgo>();
				if (componentInParent != null)
				{
					componentInParent.Data.AddWorkbenchExtension(wgo.Data.UniqueId);
					wgo.Data.AddWorkbenchParent(componentInParent.Data.UniqueId);
				}
			}
			UpdateUnbuffableObjectsTint();
			UpdateSelectionStuff();
			return true;
		}
		return false;
	}

	public override void SetupSelectionCells(BuildSelectionCell[] prefabCells, Transform parent)
	{
		overlapMask = 537526528;
		if (buildData.Definition.id != "graveyard_module_p")
		{
			overlapMask |= 65536;
		}
		target.transform.localPosition = Vector3.zero;
		Physics.SyncTransforms();
		Collider[] componentsInChildren = target.GetComponentsInChildren<Collider>();
		Bounds bounds = default(Bounds);
		bool flag = false;
		Collider[] array = new Collider[20];
		List<Collider> list = new List<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			int layer = collider.gameObject.layer;
			if ((layer != 19 && layer != 28) || PlacementBlockingArea.TryGet(collider, out var _))
			{
				continue;
			}
			if (collider.TryGetComponent<BuildArea>(out var component))
			{
				if (component.foprceShowAsBuffAreaForPointerPlacement)
				{
					list.Add(component.Collider);
					continue;
				}
				if (component.ignoreForPointerPlacement)
				{
					continue;
				}
			}
			if (collider.gameObject.layer != 28 || buildingDef.chooseCustomBuildAreaType != BuildingDef.BuildAreaChoosingType.FullCoverSoft)
			{
				Bounds bounds2 = collider.bounds;
				if (!flag)
				{
					flag = true;
					bounds = bounds2;
				}
				else
				{
					bounds.Encapsulate(bounds2);
				}
				buildColliders.Add(collider);
			}
		}
		Bounds bounds3 = bounds;
		foreach (Collider item3 in list)
		{
			if (!buildColliders.Contains(item3))
			{
				buildColliders.Add(item3);
				bounds3.Encapsulate(item3.bounds);
			}
		}
		Vector3 min = bounds.min;
		offset = VisualConsts.GetRoundedPosXZ(min, BuildConsts.BUILD_GRID_SIZE) - min;
		offset.y = 0f;
		target.transform.position += offset;
		if (!offset.magnitude.EqualsTo(0f))
		{
			Physics.SyncTransforms();
		}
		Bounds bounds4 = new Bounds(bounds.center + offset, bounds.size);
		roundedBounds = VisualConsts.GetGreaterRoundedBoundsXZ(bounds4, BuildConsts.BUILD_GRID_SIZE);
		Bounds greaterRoundedBoundsXZ = VisualConsts.GetGreaterRoundedBoundsXZ(new Bounds(bounds3.center + offset, bounds3.size), BuildConsts.BUILD_GRID_SIZE);
		Vector2Int vector2Int = 2 * new Vector2Int(16, 15);
		int num = Mathf.CeilToInt(greaterRoundedBoundsXZ.size.x / 0.01f / (float)vector2Int.x);
		int num2 = Mathf.CeilToInt(greaterRoundedBoundsXZ.size.z / 0.01f / (float)vector2Int.y);
		cellsGrid = new BuildSelectionCell[num, num2];
		cellsGridMask = new byte[num, num2];
		Vector3 vector = new Vector3(BuildConsts.CELL_SIZE.x, 0f, BuildConsts.CELL_SIZE.y);
		Vector3 vector2 = new Vector3(greaterRoundedBoundsXZ.min.x, 0.01f, greaterRoundedBoundsXZ.min.z) + vector / 2f;
		BuildSelectionCell source = prefabCells[0];
		BuildSelectionCell source2 = prefabCells[1];
		for (int j = 0; j < num; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				Vector3 vector3 = vector2 + Vector3.Scale(vector, new Vector3(j, 0f, k));
				int num3 = Physics.OverlapBoxNonAlloc(vector3 + parent.transform.position, BuildConsts.CASTING_BOX_HALF_EXTENTS, array, Quaternion.identity, 268959744);
				Debug.DrawRay(vector3, Vector3.up, Color.blue, 10f);
				for (int l = 0; l < num3; l++)
				{
					Collider collider2 = array[l];
					if (!buildColliders.Contains(collider2))
					{
						continue;
					}
					BuildSelectionCell buildSelectionCell = null;
					if (collider2.gameObject.layer == 19)
					{
						buildSelectionCell = ((!list.Contains(collider2)) ? source.Copy(parent) : source2.Copy(parent));
					}
					else if (collider2.gameObject.layer == 28)
					{
						buildSelectionCell = source2.Copy(parent);
					}
					if (!buildSelectionCell)
					{
						continue;
					}
					buildSelectionCell.transform.localPosition = vector3;
					cells.Add(buildSelectionCell);
					if (buildSelectionCell is BuffCell item)
					{
						buffCells.Add(item);
					}
					byte b = (byte)((!(buildSelectionCell is BuffCell)) ? 1u : 2u);
					if (cellsGridMask[j, k] == 0 || cellsGridMask[j, k] > b)
					{
						if (cellsGrid[j, k] != null)
						{
							cellsGrid[j, k].gameObject.SetActive(value: false);
						}
						cellsGrid[j, k] = buildSelectionCell;
						cellsGridMask[j, k] = b;
					}
					else
					{
						buildSelectionCell.gameObject.SetActive(value: false);
					}
				}
			}
		}
		ModuleSlotArea[] componentsInChildren2 = target.GetComponentsInChildren<ModuleSlotArea>();
		foreach (ModuleSlotArea item2 in componentsInChildren2)
		{
			moduleSlotAreas.Add(item2);
		}
		ColorUtility.TryParseHtmlString(unDestroyableSelectionTintColorHex, out unDestroyableSelectionTintColor);
		UpdateUnbuffableObjectsTint();
		DoBuffDrawingLogic();
	}

	public override void ApplySelectionCellsVisuals()
	{
		if (cellsGrid == null || cellsGridMask == null)
		{
			return;
		}
		int length = cellsGridMask.GetLength(0);
		int length2 = cellsGridMask.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				if (cellsGridMask[i, j] != 0)
				{
					BuildSelectionCell buildSelectionCell = cellsGrid[i, j];
					if (!(buildSelectionCell == null))
					{
						byte b = cellsGridMask[i, j];
						bool flag = i > 0 && cellsGridMask[i - 1, j] > 0 && cellsGridMask[i - 1, j] <= b;
						bool flag2 = i + 1 < length && cellsGridMask[i + 1, j] > 0 && cellsGridMask[i + 1, j] <= b;
						bool flag3 = j + 1 < length2 && cellsGridMask[i, j + 1] > 0 && cellsGridMask[i, j + 1] <= b;
						bool flag4 = j > 0 && cellsGridMask[i, j - 1] > 0 && cellsGridMask[i, j - 1] <= b;
						bool flag5 = i > 0 && j + 1 < length2 && cellsGridMask[i - 1, j + 1] > 0 && cellsGridMask[i - 1, j + 1] <= b;
						bool flag6 = i + 1 < length && j + 1 < length2 && cellsGridMask[i + 1, j + 1] > 0 && cellsGridMask[i + 1, j + 1] <= b;
						bool flag7 = i > 0 && j > 0 && cellsGridMask[i - 1, j - 1] > 0 && cellsGridMask[i - 1, j - 1] <= b;
						bool flag8 = i + 1 < length && j > 0 && cellsGridMask[i + 1, j - 1] > 0 && cellsGridMask[i + 1, j - 1] <= b;
						BuildSelectionCellVariation variation = ((flag && flag3 && !flag5) ? BuildSelectionCellVariation.CornerInsideTopLeft : ((flag2 && flag3 && !flag6) ? BuildSelectionCellVariation.CornerInsideTopRight : ((flag && flag4 && !flag7) ? BuildSelectionCellVariation.CornerInsideBottomLeft : ((flag2 && flag4 && !flag8) ? BuildSelectionCellVariation.CornerInsideBottomRight : ((!flag && !flag3) ? BuildSelectionCellVariation.CornerTopLeft : ((!flag2 && !flag3) ? BuildSelectionCellVariation.CornerTopRight : ((!flag && !flag4) ? BuildSelectionCellVariation.CornerBottomLeft : ((!flag2 && !flag4) ? BuildSelectionCellVariation.CornerBottomRight : ((!flag) ? BuildSelectionCellVariation.EdgeLeft : ((!flag2) ? BuildSelectionCellVariation.EdgeRight : ((!flag3) ? BuildSelectionCellVariation.EdgeTop : ((!flag4) ? BuildSelectionCellVariation.EdgeBottom : BuildSelectionCellVariation.Central))))))))))));
						buildSelectionCell.SetVariation(variation);
					}
				}
			}
		}
	}

	public override void UpdateSelectionCellsState()
	{
		bool flag = !fullCoverSoftQueryReusableForAvailability;
		if (flag)
		{
			InvalidateFullCoverSoftQueryCache();
		}
		fullCoverSoftQueryReusableForAvailability = false;
		Collider[] array = new Collider[20];
		shownAsActive = canTakeResources?.Invoke() ?? true;
		bool flag2 = buildingDef.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverSoft;
		List<string> value = null;
		if (flag2 && hasCustomBuildArea && GameBalance.Me != null)
		{
			GameBalance.Me.customBuildAreaIdToWgoIds.TryGetValue(buildingDef.customBuildAreaId, out value);
		}
		for (int i = 0; i < cells.Count; i++)
		{
			if (!shownAsActive)
			{
				break;
			}
			BuildSelectionCell buildSelectionCell = cells[i];
			if (buildSelectionCell is BuffCell)
			{
				continue;
			}
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			int num = buildSelectionCell.OverlapBoxNonAlloc(array, overlapMask);
			bool flag7 = true;
			for (int j = 0; j < num; j++)
			{
				Collider collider = array[j];
				if (collider == null)
				{
					continue;
				}
				if (collider.gameObject.layer == 29)
				{
					flag7 = false;
					break;
				}
				if (collider.TryGetComponent<WorldZone>(out var component))
				{
					if (!flag3)
					{
						flag3 = component.Data.Definition.id == worldZoneId;
					}
				}
				else
				{
					if (buildColliders.Contains(collider))
					{
						continue;
					}
					BuildArea component2;
					if (PlacementBlockingArea.TryGet(collider, out var _))
					{
						if (PlacementBlockingArea.IsBlockingFor(collider, buildingDef, target))
						{
							flag7 = false;
							break;
						}
					}
					else if (collider.TryGetComponent<BuildArea>(out component2))
					{
						if (component2.foprceShowAsBuffAreaForPointerPlacement)
						{
							continue;
						}
						flag4 = true;
						bool flag8 = false;
						switch (buildingDef.chooseCustomBuildAreaType)
						{
						case BuildingDef.BuildAreaChoosingType.Soft:
						case BuildingDef.BuildAreaChoosingType.FullCoverWithCount:
							if (hasCustomBuildArea && !flag5 && component2.Id == buildingDef.customBuildAreaId)
							{
								flag5 = true;
							}
							continue;
						case BuildingDef.BuildAreaChoosingType.Strict:
							flag6 = component2.Id == buildingDef.customBuildAreaId;
							if (flag6)
							{
								continue;
							}
							flag8 = true;
							break;
						}
						if (flag8)
						{
							break;
						}
					}
					else
					{
						if (flag2)
						{
							continue;
						}
						int layer = collider.gameObject.layer;
						if (layer != 8 && layer != 19 && layer != 16)
						{
							continue;
						}
						Wgo componentInParent = collider.GetComponentInParent<Wgo>();
						switch (buildingDef.chooseCustomBuildAreaType)
						{
						case BuildingDef.BuildAreaChoosingType.Soft:
						case BuildingDef.BuildAreaChoosingType.Strict:
						case BuildingDef.BuildAreaChoosingType.FullCoverWithCount:
							if (componentInParent == target || (componentInParent != null && componentInParent.Data != null && componentInParent.Data.isTempObject))
							{
								continue;
							}
							if (buildData.Definition != null)
							{
								Wgo componentInParent2 = collider.GetComponentInParent<Wgo>();
								if (componentInParent2 != null && buildData.Definition.ShouldIgnoreWgoGroupAsObstacle(componentInParent2.Data.Definition.wgoGroup))
								{
									continue;
								}
							}
							break;
						case BuildingDef.BuildAreaChoosingType.FullCoverSoft:
							if (flag2 && value != null && value.Count > 0 && (componentInParent == null || componentInParent.Data == null || componentInParent == target || componentInParent.Data.isTempObject || !value.Contains(componentInParent.Data.id)))
							{
								continue;
							}
							break;
						}
						flag7 = false;
						break;
					}
				}
			}
			if (!flag3)
			{
				flag7 = false;
			}
			switch (buildingDef.chooseCustomBuildAreaType)
			{
			case BuildingDef.BuildAreaChoosingType.Soft:
			case BuildingDef.BuildAreaChoosingType.FullCoverWithCount:
				if (hasCustomBuildArea && !flag5)
				{
					flag7 = false;
				}
				if (!hasCustomBuildArea && !flag4)
				{
					flag7 = false;
				}
				break;
			case BuildingDef.BuildAreaChoosingType.Strict:
				if (!flag6)
				{
					flag7 = false;
				}
				break;
			case BuildingDef.BuildAreaChoosingType.FullCoverSoft:
				if (!flag4)
				{
					flag7 = false;
				}
				break;
			}
			shownAsActive &= flag7;
			if (!shownAsActive)
			{
				break;
			}
		}
		if (shownAsActive && buildingDef.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverSoft && hasCustomBuildArea)
		{
			shownAsActive &= IsFullCoverSoftPlacementAllowed();
		}
		if (buildingDef.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverWithCount)
		{
			int num2 = 0;
			int fullCoveringCount = buildingDef.fullCoveringCount;
			foreach (PreSetModuleBuildView preSetModuleBuildView in base.PreSetModuleBuildViews)
			{
				if (preSetModuleBuildView.IsFullyInsideIn(base.WorldRoundedBounds))
				{
					num2++;
				}
			}
			shownAsActive &= fullCoveringCount == num2;
		}
		for (int k = 0; k < cells.Count; k++)
		{
			BuildSelectionCell buildSelectionCell2 = cells[k];
			if (!(buildSelectionCell2 is BuffCell))
			{
				buildSelectionCell2.IsAvailableForBuild = shownAsActive;
			}
		}
		foreach (ModuleSlotArea moduleSlotArea in moduleSlotAreas)
		{
			moduleSlotArea.ApplyVisibility(shownAsActive);
		}
		if (flag)
		{
			UpdateSelectionStuff();
		}
	}

	private static bool FullyContainsXZ(Bounds container, Bounds inner, float eps = 0.001f)
	{
		if (inner.min.x >= container.min.x - eps && inner.max.x <= container.max.x + eps && inner.min.z >= container.min.z - eps)
		{
			return inner.max.z <= container.max.z + eps;
		}
		return false;
	}

	private void InvalidateFullCoverSoftQueryCache()
	{
		fullCoverSoftQueryCached = false;
		cachedHasCoveredFullCoverSoftArea = false;
		cachedFullCoverSoftArea = null;
		cachedFullCoverSoftSlotFree = false;
	}

	private void EnsureFullCoverSoftQueryCache()
	{
		if (!fullCoverSoftQueryCached)
		{
			fullCoverSoftQueryCached = true;
			cachedHasCoveredFullCoverSoftArea = TryGetCoveredFullCoverSoftBuildArea(out cachedFullCoverSoftArea);
			cachedFullCoverSoftSlotFree = cachedHasCoveredFullCoverSoftArea && IsFullCoverSoftSlotFree(cachedFullCoverSoftArea);
		}
	}

	private bool TryGetCoveredFullCoverSoftBuildArea(out BuildArea coveredArea)
	{
		coveredArea = null;
		if (buildingDef == null || buildingDef.chooseCustomBuildAreaType != BuildingDef.BuildAreaChoosingType.FullCoverSoft || string.IsNullOrEmpty(buildingDef.customBuildAreaId))
		{
			return false;
		}
		Bounds worldRoundedBounds = base.WorldRoundedBounds;
		int num = Physics.OverlapBoxNonAlloc(worldRoundedBounds.center, worldRoundedBounds.extents, fullCoverSoftAreaOverlap, Quaternion.identity, 524288);
		for (int i = 0; i < num; i++)
		{
			Collider collider = fullCoverSoftAreaOverlap[i];
			if (!(collider == null) && collider.TryGetComponent<BuildArea>(out var component) && !(component.Id != buildingDef.customBuildAreaId) && FullyContainsXZ(worldRoundedBounds, collider.bounds))
			{
				coveredArea = component;
				return true;
			}
		}
		return false;
	}

	private void TryAddFullCoverSoftSlotHost(HashSet<WGODef> eligibleDefs, HashSet<Wgo> overlappingHosts, List<Rect> outSelectionRects)
	{
		if (overlappingHosts == null)
		{
			return;
		}
		EnsureFullCoverSoftQueryCache();
		if (!cachedHasCoveredFullCoverSoftArea || !cachedFullCoverSoftSlotFree)
		{
			return;
		}
		Wgo componentInParent = cachedFullCoverSoftArea.GetComponentInParent<Wgo>();
		if (IsEligibleWorldIconHost(componentInParent) && eligibleDefs != null && componentInParent.Data != null && eligibleDefs.Contains(componentInParent.Data.Definition))
		{
			overlappingHosts.Add(componentInParent);
			Rect buildAreaRect = GetBuildAreaRect(componentInParent);
			if (buildAreaRect != Rect.zero && !outSelectionRects.Contains(buildAreaRect))
			{
				outSelectionRects.Add(buildAreaRect);
			}
		}
	}

	private bool IsFullCoverSoftPlacementAllowed()
	{
		EnsureFullCoverSoftQueryCache();
		if (cachedHasCoveredFullCoverSoftArea)
		{
			return cachedFullCoverSoftSlotFree;
		}
		return false;
	}

	private bool IsFullCoverSoftSlotFree(BuildArea coveredArea)
	{
		if (coveredArea == null)
		{
			return false;
		}
		Collider collider = ((coveredArea.Collider != null) ? coveredArea.Collider : coveredArea.GetComponent<Collider>());
		if (collider == null)
		{
			return true;
		}
		Wgo componentInParent = coveredArea.GetComponentInParent<Wgo>();
		Bounds bounds = collider.bounds;
		Vector3 halfExtents = Vector3.Max(Vector3.zero, bounds.extents - VisualConsts.XYZ_STEP);
		int num = Physics.OverlapBoxNonAlloc(bounds.center, halfExtents, fullCoverSoftOccupantsOverlap, Quaternion.identity, 590080);
		for (int i = 0; i < num; i++)
		{
			Collider collider2 = fullCoverSoftOccupantsOverlap[i];
			if (collider2 == null || collider2.TryGetComponent<BuildArea>(out var _) || collider2.TryGetComponent<ModuleSlotArea>(out var _))
			{
				continue;
			}
			if (PlacementBlockingArea.TryGet(collider2, out var _))
			{
				if (PlacementBlockingArea.IsBlockingFor(collider2, buildData.Definition, target))
				{
					return false;
				}
				continue;
			}
			Wgo componentInParent2 = collider2.GetComponentInParent<Wgo>();
			if (componentInParent2 != null)
			{
				if (!(componentInParent2 == target) && !(componentInParent2 == componentInParent) && componentInParent2.Data != null && !componentInParent2.Data.isTempObject && (buildData.Definition == null || !buildData.Definition.ShouldIgnoreWgoGroupAsObstacle(componentInParent2.Data.Definition.wgoGroup)))
				{
					return false;
				}
				continue;
			}
			int layer = collider2.gameObject.layer;
			if (layer == 8 || layer == 16)
			{
				return false;
			}
		}
		return true;
	}

	public override void UpdatePosition(Vector3 position)
	{
		target.Data.Position = position + offset;
		UpdateSelectionStuff();
		fullCoverSoftQueryReusableForAvailability = true;
	}

	public override void ShowHints()
	{
		BuildingHUDData data = new BuildingHUDData(target.DockPoints, HasRotation());
		LazyUI.Get<BuildingHUD>().Draw(data);
	}

	public override void ClearSelectionCells()
	{
		buildColliders.Clear();
		buffCells.Clear();
		buffUsageRects.Clear();
		selectionRects.Clear();
		InvalidateFullCoverSoftQueryCache();
		fullCoverSoftQueryReusableForAvailability = false;
		base.ClearSelectionCells();
	}

	private void UpdateSelectionStuff()
	{
		BuildLayout buildLayout = LazySingleton<BuildManager>.Instance?.BuildController?.BuildLayout;
		if (buildLayout == null)
		{
			return;
		}
		buffUsageRects.Clear();
		selectionRects.Clear();
		overlappingHostsBuffer.Clear();
		InvalidateFullCoverSoftQueryCache();
		if (GameBalance.Me == null)
		{
			buildLayout.UpdateSelection(selectionRects, buffUsageRects);
			WorkbenchAdditionWorldIconPresenter.Clear();
			ResetAdditionWorldIconPublishState();
			return;
		}
		var (text, wGODef) = GetPlacingWgo();
		var (buildPointerRole, hashSet) = GetPointerRoleAndEligibleDefs(text, wGODef);
		switch (buildPointerRole)
		{
		case BuildPointerRole.ExtensionParent:
			FillSelectionRectsForParentOverlappingExtensions(wGODef, overlappingHostsBuffer, selectionRects);
			buildLayout.UpdateSelection(selectionRects, buffUsageRects);
			UpdateAdditionWorldIcons(buildPointerRole, text, overlappingHostsBuffer);
			return;
		case BuildPointerRole.Extension:
			TryAddFullCoverSoftSlotHost(hashSet, overlappingHostsBuffer, selectionRects);
			break;
		}
		if (buffCells.Count == 0)
		{
			buildLayout.UpdateSelection(selectionRects, buffUsageRects);
			UpdateAdditionWorldIcons(buildPointerRole, text, overlappingHostsBuffer);
			return;
		}
		if (hashSet == null || hashSet.Count == 0)
		{
			buildLayout.UpdateSelection(selectionRects, buffUsageRects);
			UpdateAdditionWorldIcons(buildPointerRole, text, overlappingHostsBuffer);
			return;
		}
		FillRectsFromBuffCells(hashSet, overlappingHostsBuffer, buffUsageRects);
		AddBuildAreaRects(selectionRects, overlappingHostsBuffer);
		if (buildPointerRole == BuildPointerRole.Extension)
		{
			AddAlreadyConnectedExtensionParents(text, hashSet, overlappingHostsBuffer, selectionRects);
		}
		buildLayout.UpdateSelection(selectionRects, buffUsageRects);
		UpdateAdditionWorldIcons(buildPointerRole, text, overlappingHostsBuffer);
	}

	private (string placingWgoId, WGODef placingDef) GetPlacingWgo()
	{
		string wgoId = buildingDef?.wgoId ?? target?.Data?.id;
		return ResolveExtensionLogicWgo(wgoId);
	}

	private (string placingWgoId, WGODef placingDef) ResolveExtensionLogicWgo(string wgoId)
	{
		if (string.IsNullOrEmpty(wgoId) || GameBalance.Me == null)
		{
			return (placingWgoId: wgoId, placingDef: null);
		}
		if (buildingDef != null && !string.IsNullOrEmpty(buildingDef.customWgoPlacePreview))
		{
			string customWgoPlacePreview = buildingDef.customWgoPlacePreview;
			WGODef workbenchExtensionLogicDef = GameBalance.Me.GetWorkbenchExtensionLogicDef(customWgoPlacePreview);
			if (workbenchExtensionLogicDef != null && (GameBalance.Me.workbenchesWhichUseExtensions.Contains(workbenchExtensionLogicDef) || GameBalance.Me.IsWorkbenchExtensionId(customWgoPlacePreview)))
			{
				return (placingWgoId: customWgoPlacePreview, placingDef: workbenchExtensionLogicDef);
			}
		}
		WGODef workbenchExtensionLogicDef2 = GameBalance.Me.GetWorkbenchExtensionLogicDef(wgoId);
		if (workbenchExtensionLogicDef2 == null)
		{
			return (placingWgoId: wgoId, placingDef: null);
		}
		if (GameBalance.Me.workbenchesWhichUseExtensions.Contains(workbenchExtensionLogicDef2) || GameBalance.Me.IsWorkbenchExtensionId(workbenchExtensionLogicDef2.id))
		{
			return (placingWgoId: workbenchExtensionLogicDef2.id, placingDef: workbenchExtensionLogicDef2);
		}
		return (placingWgoId: wgoId, placingDef: workbenchExtensionLogicDef2);
	}

	private (BuildPointerRole role, HashSet<WGODef> eligibleDefs) GetPointerRoleAndEligibleDefs(string placingWgoId, WGODef placingDef)
	{
		HashSet<WGODef> hashSet = GameBalance.Me.workbenchesWhichUseExtensions;
		bool flag = !string.IsNullOrEmpty(placingWgoId) && GameBalance.Me.IsWorkbenchExtensionId(placingWgoId);
		if (placingDef != null && hashSet != null && hashSet.Contains(placingDef))
		{
			return (role: BuildPointerRole.ExtensionParent, eligibleDefs: hashSet);
		}
		if (flag)
		{
			if (GameBalance.Me.TryGetParentWorkbenchDefsForExtension(placingWgoId, out var parentWorkbenchDefs))
			{
				hashSet = GetExtensionParentDefs(placingWgoId, parentWorkbenchDefs);
			}
			return (role: BuildPointerRole.Extension, eligibleDefs: hashSet);
		}
		return (role: BuildPointerRole.None, eligibleDefs: hashSet);
	}

	private HashSet<WGODef> GetExtensionParentDefs(string placingWgoId, List<WGODef> parents)
	{
		if (extensionParentDefsBufferForId == placingWgoId)
		{
			return extensionParentDefsBuffer;
		}
		extensionParentDefsBufferForId = placingWgoId;
		extensionParentDefsBuffer.Clear();
		if (parents != null)
		{
			for (int i = 0; i < parents.Count; i++)
			{
				if (parents[i] != null)
				{
					extensionParentDefsBuffer.Add(parents[i]);
				}
			}
		}
		return extensionParentDefsBuffer;
	}

	private HashSet<string> GetCachedAllowedExtensionIds(string parentWorkbenchId)
	{
		if (cachedAllowedExtensionIdsForParentId == parentWorkbenchId && cachedAllowedExtensionIds != null)
		{
			return cachedAllowedExtensionIds;
		}
		cachedAllowedExtensionIdsForParentId = parentWorkbenchId;
		cachedAllowedExtensionIds = GameBalance.Me.GetAllowedExtensionIdsForParentWorkbench(parentWorkbenchId);
		return cachedAllowedExtensionIds;
	}

	private void FillSelectionRectsForParentOverlappingExtensions(WGODef parentDef, HashSet<Wgo> overlappingExtensions, List<Rect> outSelectionRects)
	{
		if (parentDef == null)
		{
			return;
		}
		HashSet<string> hashSet = GetCachedAllowedExtensionIds(parentDef.id);
		for (int i = 0; i < buildColliders.Count; i++)
		{
			Collider collider = buildColliders[i];
			if (collider == null || collider.gameObject.layer != 19)
			{
				continue;
			}
			Vector3 center;
			Vector3 halfExtents;
			Quaternion rotation;
			if (collider is BoxCollider boxCollider)
			{
				center = boxCollider.transform.TransformPoint(boxCollider.center);
				halfExtents = Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale);
				halfExtents.x = Mathf.Max(0f, halfExtents.x - 0.01f);
				halfExtents.z = Mathf.Max(0f, halfExtents.z - 0.0125f);
				rotation = boxCollider.transform.rotation;
			}
			else
			{
				Bounds bounds = collider.bounds;
				center = bounds.center;
				halfExtents = bounds.extents;
				halfExtents.x = Mathf.Max(0f, halfExtents.x - 0.01f);
				halfExtents.z = Mathf.Max(0f, halfExtents.z - 0.0125f);
				rotation = collider.transform.rotation;
			}
			int num = Physics.OverlapBoxNonAlloc(center, halfExtents, buffAreaOverlapColliders, rotation, 268435456, QueryTriggerInteraction.Collide);
			for (int j = 0; j < num; j++)
			{
				Collider collider2 = buffAreaOverlapColliders[j];
				if (collider2 == null)
				{
					continue;
				}
				Wgo componentInParent = collider2.GetComponentInParent<Wgo>();
				if (!(componentInParent == null) && hashSet.Contains(componentInParent.Data.id))
				{
					overlappingExtensions.Add(componentInParent);
					Rect buildAreaRect = GetBuildAreaRect(componentInParent);
					if (buildAreaRect != Rect.zero && !outSelectionRects.Contains(buildAreaRect))
					{
						outSelectionRects.Add(buildAreaRect);
					}
				}
			}
		}
	}

	private void FillRectsFromBuffCells(HashSet<WGODef> eligibleDefs, HashSet<Wgo> overlappingWorkbenches, List<Rect> outBuffUsageRects)
	{
		Vector2 cELL_SIZE = BuildConsts.CELL_SIZE;
		for (int i = 0; i < buffCells.Count; i++)
		{
			BuffCell buffCell = buffCells[i];
			int num = buffCell.OverlapBoxNonAlloc(buffUsageOverlapColliders, 524288);
			bool flag = false;
			for (int j = 0; j < num; j++)
			{
				Collider collider = buffUsageOverlapColliders[j];
				if (!(collider == null) && !buildColliders.Contains(collider))
				{
					Wgo componentInParent = collider.GetComponentInParent<Wgo>();
					if (!(componentInParent == null) && !(componentInParent == target) && eligibleDefs.Contains(componentInParent.Data.Definition))
					{
						overlappingWorkbenches.Add(componentInParent);
						flag = true;
					}
				}
			}
			if (flag)
			{
				Vector3 position = buffCell.transform.position;
				outBuffUsageRects.Add(new Rect(position.x - cELL_SIZE.x * 0.5f, position.z - cELL_SIZE.y * 0.5f, cELL_SIZE.x, cELL_SIZE.y));
			}
		}
	}

	private void AddBuildAreaRects(List<Rect> outSelectionRects, HashSet<Wgo> workbenches)
	{
		foreach (Wgo workbench in workbenches)
		{
			Rect buildAreaRect = GetBuildAreaRect(workbench);
			if (buildAreaRect != Rect.zero)
			{
				outSelectionRects.Add(buildAreaRect);
			}
		}
	}

	private void AddAlreadyConnectedExtensionParents(string placingExtensionId, HashSet<WGODef> eligibleDefs, HashSet<Wgo> overlappingWorkbenches, List<Rect> outSelectionRects)
	{
		if (string.IsNullOrEmpty(placingExtensionId))
		{
			return;
		}
		foreach (Wgo wgo in LazySingleton<BuildManager>.Instance.WorldZone.Wgos)
		{
			if (wgo.Data.id != placingExtensionId)
			{
				continue;
			}
			Collider[] componentsInChildren = wgo.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.gameObject.layer != 28)
				{
					continue;
				}
				Collider[] array = new Collider[10];
				int num = Physics.OverlapBoxNonAlloc(collider.bounds.center, GetBuffAreaOverlapHalfExtents(collider), array, collider.transform.rotation, 524288);
				for (int j = 0; j < num; j++)
				{
					Collider collider2 = array[j];
					if (collider2 == null)
					{
						continue;
					}
					Wgo componentInParent = collider2.GetComponentInParent<Wgo>();
					if (!(componentInParent == null) && eligibleDefs.Contains(componentInParent.Data.Definition) && !overlappingWorkbenches.Contains(componentInParent))
					{
						Rect buildAreaRect = GetBuildAreaRect(componentInParent);
						if (buildAreaRect != Rect.zero && !outSelectionRects.Contains(buildAreaRect))
						{
							outSelectionRects.Add(buildAreaRect);
						}
					}
				}
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 29)
		{
			UpdateSelectionCellsState();
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 29)
		{
			UpdateSelectionCellsState();
		}
	}

	public override Vector3 GetCellsCenterLocal()
	{
		if (target != null)
		{
			CustomVisualCenterBuildPointer componentInChildren = target.GetComponentInChildren<CustomVisualCenterBuildPointer>(includeInactive: true);
			if (componentInChildren != null)
			{
				Vector3 result = base.transform.InverseTransformPoint(componentInChildren.transform.position);
				result.y = 0f;
				return result;
			}
		}
		return base.GetCellsCenterLocal();
	}

	public override void UpdateCollider()
	{
		BoxCollider boxCollider = null;
		if (triggerCollider == null)
		{
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
			triggerCollider = boxCollider;
		}
		triggerCollider.center = GetObjectCenterLocal();
		triggerCollider.size = new Vector3(roundedBounds.size.x, 1f, roundedBounds.size.z);
		triggerCollider.isTrigger = true;
	}

	public override void OnPointerDisable()
	{
		base.OnPointerDisable();
		ClearSelectionTint();
		WorkbenchAdditionWorldIconPresenter.Clear();
		ResetAdditionWorldIconPublishState();
		InvalidateFullCoverSoftQueryCache();
		fullCoverSoftQueryReusableForAvailability = false;
		moduleSlotAreas.Clear();
	}

	private void ClearSelectionTint()
	{
		foreach (Wgo item in wgosShownAsInactiveWhenExtensionIsPlacing)
		{
			item.SetSelectionTint(Color.white, 0f);
		}
		wgosShownAsInactiveWhenExtensionIsPlacing.Clear();
	}

	private void UpdateUnbuffableObjectsTint()
	{
		ClearSelectionTint();
		string item = GetPlacingWgo().placingWgoId;
		if (GameBalance.Me.TryGetParentWorkbenchDefsForExtension(item, out var parentWorkbenchDefs))
		{
			foreach (Wgo wgo in LazySingleton<BuildManager>.Instance.WorldZone.Wgos)
			{
				if (!parentWorkbenchDefs.Contains(wgo.Data.Definition))
				{
					wgosShownAsInactiveWhenExtensionIsPlacing.Add(wgo);
				}
			}
		}
		foreach (Wgo item2 in wgosShownAsInactiveWhenExtensionIsPlacing)
		{
			item2.SetSelectionTint(unDestroyableSelectionTintColor, 0.4f);
		}
	}

	private void UpdateAdditionWorldIcons(BuildPointerRole role, string placingWgoId, HashSet<Wgo> overlappingHosts)
	{
		if (role == BuildPointerRole.None)
		{
			WorkbenchAdditionWorldIconPresenter.Clear();
			ResetAdditionWorldIconPublishState();
			return;
		}
		WorldZone worldZone = LazySingleton<BuildManager>.Instance?.WorldZone;
		if (worldZone == null)
		{
			WorkbenchAdditionWorldIconPresenter.Clear();
			ResetAdditionWorldIconPublishState();
			return;
		}
		if (overlappingHosts == null)
		{
			overlappingHosts = overlappingHostsBuffer;
		}
		bool flag = RefreshEligibleIconHosts(role, placingWgoId, worldZone);
		if (hasPublishedIconState && !flag && role == cachedIconRole && placingWgoId == cachedIconPlacingWgoId && overlappingHosts.SetEquals(lastIconOverlapHosts))
		{
			return;
		}
		cachedIconRole = role;
		cachedIconPlacingWgoId = placingWgoId;
		lastIconOverlapHosts.Clear();
		foreach (Wgo overlappingHost in overlappingHosts)
		{
			lastIconOverlapHosts.Add(overlappingHost);
		}
		hasPublishedIconState = true;
		iconStatesBuffer.Clear();
		switch (role)
		{
		case BuildPointerRole.Extension:
		{
			string additionWorldIconId = GetAdditionWorldIconId(placingWgoId);
			for (int j = 0; j < cachedEligibleIconHosts.Count; j++)
			{
				Wgo wgo2 = cachedEligibleIconHosts[j];
				if (IsEligibleWorldIconHost(wgo2))
				{
					iconStatesBuffer[wgo2] = new WorkbenchAdditionWorldIconPresenter.State(additionWorldIconId, overlappingHosts.Contains(wgo2));
				}
			}
			break;
		}
		case BuildPointerRole.ExtensionParent:
		{
			for (int i = 0; i < cachedEligibleIconHosts.Count; i++)
			{
				Wgo wgo = cachedEligibleIconHosts[i];
				if (IsEligibleWorldIconHost(wgo))
				{
					iconStatesBuffer[wgo] = new WorkbenchAdditionWorldIconPresenter.State(GetAdditionWorldIconId(wgo.Data.id), overlappingHosts.Contains(wgo));
				}
			}
			break;
		}
		}
		WorkbenchAdditionWorldIconPresenter.Replace(iconStatesBuffer);
	}

	private bool RefreshEligibleIconHosts(BuildPointerRole role, string placingWgoId, WorldZone worldZone)
	{
		int wgosVersion = worldZone.WgosVersion;
		if (role == cachedEligibleHostsRole && placingWgoId == cachedEligibleHostsPlacingWgoId && wgosVersion == cachedEligibleHostsZoneWgosVersion)
		{
			return false;
		}
		cachedEligibleHostsRole = role;
		cachedEligibleHostsPlacingWgoId = placingWgoId;
		cachedEligibleHostsZoneWgosVersion = wgosVersion;
		cachedEligibleIconHosts.Clear();
		switch (role)
		{
		case BuildPointerRole.Extension:
		{
			if (!GameBalance.Me.TryGetParentWorkbenchDefsForExtension(placingWgoId, out var parentWorkbenchDefs) || parentWorkbenchDefs == null || parentWorkbenchDefs.Count == 0)
			{
				return true;
			}
			HashSet<WGODef> extensionParentDefs = GetExtensionParentDefs(placingWgoId, parentWorkbenchDefs);
			foreach (Wgo wgo in worldZone.Wgos)
			{
				if (IsEligibleWorldIconHost(wgo) && extensionParentDefs.Contains(wgo.Data.Definition))
				{
					cachedEligibleIconHosts.Add(wgo);
				}
			}
			break;
		}
		case BuildPointerRole.ExtensionParent:
		{
			HashSet<string> hashSet = GetCachedAllowedExtensionIds(placingWgoId);
			foreach (Wgo wgo2 in worldZone.Wgos)
			{
				if (IsEligibleWorldIconHost(wgo2) && hashSet.Contains(wgo2.Data.id))
				{
					cachedEligibleIconHosts.Add(wgo2);
				}
			}
			break;
		}
		}
		return true;
	}

	private void ResetAdditionWorldIconPublishState()
	{
		hasPublishedIconState = false;
		cachedIconRole = BuildPointerRole.None;
		cachedIconPlacingWgoId = null;
		lastIconOverlapHosts.Clear();
		cachedEligibleIconHosts.Clear();
		cachedEligibleHostsRole = BuildPointerRole.None;
		cachedEligibleHostsPlacingWgoId = null;
		cachedEligibleHostsZoneWgosVersion = int.MinValue;
		iconStatesBuffer.Clear();
		extensionParentDefsBuffer.Clear();
		extensionParentDefsBufferForId = null;
		cachedAllowedExtensionIds = null;
		cachedAllowedExtensionIdsForParentId = null;
	}

	private bool IsEligibleWorldIconHost(Wgo wgo)
	{
		if (wgo != null && wgo != target && wgo.Data != null && !wgo.Data.isTempObject)
		{
			return !wgo.IsDespawning;
		}
		return false;
	}

	private static string GetAdditionWorldIconId(string additionWgoId)
	{
		if (!string.IsNullOrEmpty(additionWgoId) && GameBalance.Me.buildableWgos.TryGetValue(additionWgoId, out var value))
		{
			return value.BuildResultIcon;
		}
		if (!string.IsNullOrEmpty(additionWgoId))
		{
			return "i_b_" + additionWgoId;
		}
		return "i_b_blueprint_placeholder";
	}

	private Rect GetBuildAreaRect(Wgo wgo)
	{
		bool flag = false;
		Bounds bounds = default(Bounds);
		Collider[] componentsInChildren = wgo.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.gameObject.layer == 19 && !PlacementBlockingArea.TryGet(collider, out var _))
			{
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
		if (!flag)
		{
			return Rect.zero;
		}
		return new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
	}

	private static Vector3 GetBuffAreaOverlapHalfExtents(Collider buffAreaCollider)
	{
		Vector3 extents = buffAreaCollider.bounds.extents;
		extents.x = Mathf.Max(0f, extents.x - 0.01f);
		extents.z = Mathf.Max(0f, extents.z - 0.0125f);
		return extents;
	}

	private void DoBuffDrawingLogic()
	{
		BuildGrid3D buildGrid3D = (LazySingleton<BuildManager>.Instance?.BuildController?.BuildLayout)?.BuildGrid3D;
		if (!(buildGrid3D == null) && !(GameBalance.Me == null) && !(target == null))
		{
			var (text, wGODef) = GetPlacingWgo();
			if (wGODef == null)
			{
				buildGrid3D.SetAllowedExtensionWgoIds(null);
			}
			else if (GameBalance.Me.IsWorkbenchExtensionId(text))
			{
				buildGrid3D.SetAllowedExtensionWgoIds(new HashSet<string> { text });
			}
			else if (GameBalance.Me.workbenchesWhichUseExtensions.Contains(wGODef))
			{
				HashSet<string> allowedExtensionIdsForParentWorkbench = GameBalance.Me.GetAllowedExtensionIdsForParentWorkbench(text);
				buildGrid3D.SetAllowedExtensionWgoIds(allowedExtensionIdsForParentWorkbench);
			}
			else
			{
				buildGrid3D.SetAllowedExtensionWgoIds(new HashSet<string>());
			}
		}
	}

	private void TrySetCustomRotation(Wgo builtWgo)
	{
		Vector3 halfExtents = base.WorldRoundedBounds.extents / 4f;
		halfExtents.y = 0.5f;
		Collider[] array = new Collider[10];
		if (Physics.OverlapBoxNonAlloc(base.WorldRoundedBounds.center, halfExtents, array, Quaternion.identity, 524288) <= 0)
		{
			return;
		}
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!(collider == null) && collider.TryGetComponent<BuildArea>(out var component) && component.HasRotationRequirement)
			{
				builtWgo.Data.MainWgoPartData.TryApplyState(builtWgo.Data.UniqueId, builtWgo.MainWgoPart.WgoPartData.variationId, component.RotationRequirement);
			}
		}
	}
}
