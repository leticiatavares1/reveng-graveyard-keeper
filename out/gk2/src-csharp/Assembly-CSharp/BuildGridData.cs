using System.Collections.Generic;
using UnityEngine;

public class BuildGridData : MonoBehaviour
{
	private const int HALF_RANGE_SCAN_GRIDS_COUNT = 100;

	private BuildCellData[,] gridData;

	private BuildCellSelectionData[,] selectionGridData;

	private BuildCellBuffUsageData[,] buffUsageGridData;

	private BuildCellData.BuildMode buildMode;

	private BuildingDef currentBuildingDef;

	private bool drawExtensions;

	private HashSet<ModuleSlotArea> busySlotAreas = new HashSet<ModuleSlotArea>();

	private HashSet<ModuleSlotArea> freeSlotAreas = new HashSet<ModuleSlotArea>();

	public BuildCellData[,] GridData => gridData;

	public BuildCellSelectionData[,] SelectionGridData => selectionGridData;

	public BuildCellBuffUsageData[,] BuffUsageGridData => buffUsageGridData;

	public BuildCellData.BuildMode BuildMode => buildMode;

	public BuildingDef CurrentBuildingDef => currentBuildingDef;

	public bool DrawExtensions => drawExtensions;

	public void FormGridData(Vector3 buildPos, string worldZoneId, Rect worldZoneRect, BuildingDef buildingDef = null, bool useExtensions = true)
	{
		currentBuildingDef = buildingDef;
		int num = 200;
		BuildCellData[,] array = new BuildCellData[num, num];
		selectionGridData = new BuildCellSelectionData[num, num];
		buffUsageGridData = new BuildCellBuffUsageData[num, num];
		Vector3 vector = new Vector3(BuildConsts.CELL_SIZE.x, 0f, BuildConsts.CELL_SIZE.y);
		drawExtensions = useExtensions;
		for (int i = -100; i < 100; i++)
		{
			for (int j = -100; j < 100; j++)
			{
				Vector3 coords = buildPos + Vector3.Scale(new Vector3(i, 0f, j), vector) + vector / 2f;
				if (worldZoneRect.Contains(new Vector2(coords.x, coords.z)))
				{
					array[i + 100, j + 100] = BuildCellData.GetData(coords, worldZoneId, buildingDef);
				}
			}
		}
		gridData = array;
		if (buildingDef != null)
		{
			BuildCellData.BuildMode buildMode = ((buildingDef.buildingMode == BuildingDef.BuildingMode.Remove) ? BuildCellData.BuildMode.Remove : BuildCellData.BuildMode.Place);
			this.buildMode = buildMode;
		}
		else
		{
			this.buildMode = BuildCellData.BuildMode.Remove;
		}
		UpdateModuleSlotAreas();
	}

	public void FormSelectionGridData(Rect selectionRect)
	{
		if (gridData == null)
		{
			selectionGridData = null;
			return;
		}
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		selectionGridData = new BuildCellSelectionData[length, length2];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				Vector3 coords = gridData[i, j].Coords;
				int state = (selectionRect.Contains(new Vector2(coords.x, coords.z)) ? 1 : 0);
				selectionGridData[i, j] = new BuildCellSelectionData(coords, state);
			}
		}
	}

	public void FormSelectionGridData(List<Rect> selectionRects)
	{
		if (gridData == null)
		{
			selectionGridData = null;
			return;
		}
		if (selectionRects == null)
		{
			ClearSelectionGridData();
			return;
		}
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		selectionGridData = new BuildCellSelectionData[length, length2];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				Vector3 coords = gridData[i, j].Coords;
				Vector2 point = new Vector2(coords.x, coords.z);
				int state = 0;
				foreach (Rect selectionRect in selectionRects)
				{
					if (selectionRect.Contains(point))
					{
						state = 1;
						break;
					}
				}
				selectionGridData[i, j] = new BuildCellSelectionData(coords, state);
			}
		}
	}

	public void FormBuffUsageGridData(List<Rect> buffUsageRects)
	{
		if (gridData == null)
		{
			buffUsageGridData = null;
			return;
		}
		if (buffUsageRects == null)
		{
			ClearBuffUsageGridData();
			return;
		}
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		buffUsageGridData = new BuildCellBuffUsageData[length, length2];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				Vector3 coords = gridData[i, j].Coords;
				Vector2 point = new Vector2(coords.x, coords.z);
				int state = 0;
				foreach (Rect buffUsageRect in buffUsageRects)
				{
					if (buffUsageRect.Contains(point))
					{
						state = 2;
						break;
					}
				}
				buffUsageGridData[i, j] = new BuildCellBuffUsageData(coords, state);
			}
		}
	}

	public void FormBuffUsageGridData(Rect buffUsageRect)
	{
		if (gridData == null)
		{
			buffUsageGridData = null;
			return;
		}
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		buffUsageGridData = new BuildCellBuffUsageData[length, length2];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				Vector3 coords = gridData[i, j].Coords;
				int state = (buffUsageRect.Contains(new Vector2(coords.x, coords.z)) ? 2 : 0);
				buffUsageGridData[i, j] = new BuildCellBuffUsageData(coords, state);
			}
		}
	}

	public void UpdateData()
	{
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				gridData[i, j].UpdateData();
			}
		}
		UpdateModuleSlotAreas();
	}

	public void EraseData()
	{
		gridData = new BuildCellData[0, 0];
		selectionGridData = new BuildCellSelectionData[0, 0];
		buffUsageGridData = new BuildCellBuffUsageData[0, 0];
		busySlotAreas.Clear();
		freeSlotAreas.Clear();
	}

	private void UpdateModuleSlotAreas()
	{
		busySlotAreas.Clear();
		freeSlotAreas.Clear();
		if (gridData == null)
		{
			return;
		}
		string text = currentBuildingDef?.customBuildAreaId;
		HashSet<ModuleSlotArea> hashSet = new HashSet<ModuleSlotArea>();
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				BuildCellData buildCellData = gridData[i, j];
				ModuleSlotArea slotArea = buildCellData.SlotArea;
				if (!(slotArea == null))
				{
					hashSet.Add(slotArea);
					if (((uint)buildCellData.State & 4u) != 0)
					{
						busySlotAreas.Add(slotArea);
					}
				}
			}
		}
		foreach (ModuleSlotArea item in hashSet)
		{
			bool flag = !busySlotAreas.Contains(item);
			if (item.BuildArea != null && !string.IsNullOrEmpty(text))
			{
				flag &= item.BuildArea.Id == text;
			}
			if (flag)
			{
				freeSlotAreas.Add(item);
			}
			item.ApplyVisibility(flag);
		}
	}

	private void ClearSelectionGridData()
	{
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		selectionGridData = new BuildCellSelectionData[length, length2];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				Vector3 coords = gridData[i, j].Coords;
				selectionGridData[i, j] = new BuildCellSelectionData(coords, 0);
			}
		}
	}

	private void ClearBuffUsageGridData()
	{
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		buffUsageGridData = new BuildCellBuffUsageData[length, length2];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				Vector3 coords = gridData[i, j].Coords;
				buffUsageGridData[i, j] = new BuildCellBuffUsageData(coords, 0);
			}
		}
	}
}
