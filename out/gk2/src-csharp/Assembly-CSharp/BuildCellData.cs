using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public struct BuildCellData
{
	public enum CellState
	{
		Empty = 1,
		Available = 2,
		Busy = 4,
		Buff = 8,
		RemovableWgo = 0x10,
		UnremovableWgo = 0x20,
		BuffUsage = 0x40
	}

	public enum BuildMode
	{
		Place,
		Remove
	}

	private int state;

	private Vector3 coords;

	[CanBeNull]
	private BuildingDef buildingDef;

	private BuildingDef.BuildAreaChoosingType chooseBuildAreaType;

	private BuildArea buildAreaWithCovering;

	private bool hasCustomBuildAreaId;

	private static Collider[] buildAreaColliders = new Collider[30];

	private HashSet<SGuid> extensionList;

	private bool containsExtensionParent;

	private ModuleSlotArea slotArea;

	public int State => state;

	public Vector3 Coords => coords;

	public BuildArea BuildAreaWithCovering => buildAreaWithCovering;

	public HashSet<SGuid> ExtensionList => extensionList;

	public bool ContainsExtensionParent => containsExtensionParent;

	public ModuleSlotArea SlotArea => slotArea;

	public void AddState(CellState stateFlag)
	{
		state |= (int)stateFlag;
	}

	public static BuildCellData GetData(Vector3 coords, string zoneId, BuildingDef buildingDef = null)
	{
		BuildCellData buildCellData = default(BuildCellData);
		buildCellData.coords = coords;
		buildCellData.buildingDef = buildingDef;
		buildCellData.hasCustomBuildAreaId = !string.IsNullOrEmpty(buildingDef?.customBuildAreaId);
		buildCellData.chooseBuildAreaType = buildingDef?.chooseCustomBuildAreaType ?? BuildingDef.BuildAreaChoosingType.None;
		BuildCellData result = buildCellData;
		result.UpdateData();
		return result;
	}

	public void UpdateData()
	{
		containsExtensionParent = false;
		slotArea = null;
		if (extensionList == null)
		{
			extensionList = new HashSet<SGuid>();
		}
		extensionList.Clear();
		int num = Physics.OverlapBoxNonAlloc(coords, BuildConsts.CASTING_BOX_HALF_EXTENTS, buildAreaColliders, Quaternion.identity, 269091072);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		bool flag10 = false;
		for (int i = 0; i < num; i++)
		{
			Collider collider = buildAreaColliders[i];
			if (collider == null)
			{
				continue;
			}
			if (collider.TryGetComponent<WorldZone>(out var _))
			{
				flag = true;
			}
			else
			{
				if (collider.gameObject.layer == 19 && collider.gameObject.GetComponentInParent<BuildPointerObject>() != null)
				{
					continue;
				}
				if (PlacementBlockingArea.TryGet(collider, out var _))
				{
					if (buildingDef != null && PlacementBlockingArea.IsBlockingFor(collider, buildingDef))
					{
						flag10 = true;
						flag5 = true;
					}
					continue;
				}
				if (!flag7 && collider.gameObject.layer == 28)
				{
					flag7 = true;
				}
				if (collider.TryGetComponent<BuildArea>(out var component2))
				{
					flag2 = true;
					if (slotArea == null)
					{
						slotArea = component2.GetComponent<ModuleSlotArea>();
					}
					Wgo componentInParent = collider.GetComponentInParent<Wgo>();
					if (componentInParent != null && componentInParent.GetComponentInParent<BuildPointerObject>() == null && componentInParent.IsBuildRemovable())
					{
						flag6 = true;
						if (buildingDef == null)
						{
							flag9 = true;
							flag5 = true;
						}
					}
					switch (chooseBuildAreaType)
					{
					case BuildingDef.BuildAreaChoosingType.Soft:
						if (hasCustomBuildAreaId && component2.Id == buildingDef?.customBuildAreaId)
						{
							flag3 = true;
						}
						break;
					case BuildingDef.BuildAreaChoosingType.Strict:
						if (!(component2.Id == buildingDef?.customBuildAreaId))
						{
							flag4 = false;
						}
						continue;
					case BuildingDef.BuildAreaChoosingType.FullCoverWithCount:
						if (component2.Id == buildingDef?.customBuildAreaId)
						{
							flag3 = true;
							if (component2.fullCoveringMode)
							{
								buildAreaWithCovering = component2;
							}
							continue;
						}
						flag4 = false;
						break;
					case BuildingDef.BuildAreaChoosingType.FullCoverSoft:
						if (!flag3 && component2.Id == buildingDef?.customBuildAreaId)
						{
							flag3 = true;
							flag8 = true;
						}
						break;
					}
					Wgo componentInParent2 = collider.GetComponentInParent<Wgo>();
					if (componentInParent2 != null && componentInParent2.GetComponentInParent<BuildPointerObject>() == null && GameBalance.Me.workbenchesWhichUseExtensions.Contains(componentInParent2.Data.Definition))
					{
						containsExtensionParent = true;
					}
					continue;
				}
				Wgo componentInParent3 = collider.GetComponentInParent<Wgo>();
				if (componentInParent3 != null)
				{
					if (componentInParent3.GetComponentInParent<BuildPointerObject>() != null || collider.TryGetComponent<ModuleSlotArea>(out var _) || (buildingDef != null && buildingDef.ShouldIgnoreWgoGroupAsObstacle(componentInParent3.Data.Definition.wgoGroup)))
					{
						continue;
					}
					if (collider.gameObject.layer == 28)
					{
						extensionList.Add(componentInParent3.Data.UniqueId);
						continue;
					}
					if (chooseBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverSoft && GameBalance.Me.customBuildAreaIdToWgoIds.TryGetValue(buildingDef?.customBuildAreaId, out var value) && value.Contains(componentInParent3.Data.id))
					{
						flag8 = false;
					}
					flag5 = true;
					if (!flag9)
					{
						flag6 = componentInParent3.IsBuildRemovable();
					}
				}
				else
				{
					int layer = collider.gameObject.layer;
					if (layer == 8 || layer == 19)
					{
						flag5 = true;
					}
				}
			}
		}
		state = 0;
		state |= 1;
		if (!flag)
		{
			return;
		}
		if (flag7)
		{
			state |= 8;
		}
		if (containsExtensionParent && extensionList.Count > 0)
		{
			state |= 64;
		}
		switch (chooseBuildAreaType)
		{
		case BuildingDef.BuildAreaChoosingType.Soft:
		case BuildingDef.BuildAreaChoosingType.FullCoverWithCount:
			if ((hasCustomBuildAreaId && !flag3) || (!hasCustomBuildAreaId && !flag2))
			{
				return;
			}
			break;
		case BuildingDef.BuildAreaChoosingType.Strict:
			if (!flag2)
			{
				return;
			}
			if (!flag4)
			{
				state |= 6;
				return;
			}
			break;
		case BuildingDef.BuildAreaChoosingType.FullCoverSoft:
			if (!flag3)
			{
				return;
			}
			break;
		}
		if (!flag2 && chooseBuildAreaType == BuildingDef.BuildAreaChoosingType.None)
		{
			return;
		}
		if (flag10)
		{
			state |= 6;
		}
		else if (flag5 && !flag8)
		{
			state |= 6;
			if (flag6)
			{
				state |= 16;
			}
			else if (!flag9)
			{
				state |= 32;
			}
		}
		else
		{
			state |= 2;
		}
	}
}
