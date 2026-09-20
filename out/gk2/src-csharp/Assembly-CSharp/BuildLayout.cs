using System.Collections.Generic;
using UnityEngine;

public class BuildLayout : MonoBehaviour
{
	private const float Y_OFFSET = 0.001f;

	[SerializeField]
	private BuildGridData buildGridData;

	[SerializeField]
	private BuildGrid3D buildGrid3D;

	private string worldZoneId;

	public BuildGrid3D BuildGrid3D => buildGrid3D;

	public void EnableBuildingMode(Vector3 position, string worldZoneId, Rect worldZoneXZRect, BuildingDef buildingDef = null, bool useExtensions = false, List<BuildElevationArea> elevationAreas = null)
	{
		base.transform.position = position + Vector3.up * 0.001f;
		this.worldZoneId = worldZoneId;
		buildGridData.FormGridData(position, worldZoneId, worldZoneXZRect, buildingDef, useExtensions);
		buildGrid3D.SetElevationAreas(elevationAreas);
		buildGrid3D.Draw(buildGridData.GridData, buildGridData.SelectionGridData, buildGridData.BuffUsageGridData, buildGridData.BuildMode, buildGridData.DrawExtensions);
		base.gameObject.SetActive(value: true);
	}

	public void UpdateBuildingMode()
	{
		buildGridData.UpdateData();
		buildGrid3D.Draw(buildGridData.GridData, buildGridData.SelectionGridData, buildGridData.BuffUsageGridData, buildGridData.BuildMode, buildGridData.DrawExtensions);
	}

	public void UpdateSelection(List<Rect> selectionRects)
	{
		if (buildGridData.GridData != null)
		{
			buildGridData.FormSelectionGridData(selectionRects);
			buildGrid3D.UpdateSelection(buildGridData.SelectionGridData, buildGridData.BuffUsageGridData);
		}
	}

	public void UpdateSelection(List<Rect> selectionRects, List<Rect> buffUsageRects)
	{
		if (buildGridData.GridData != null)
		{
			buildGridData.FormSelectionGridData(selectionRects);
			buildGridData.FormBuffUsageGridData(buffUsageRects);
			buildGrid3D.UpdateSelection(buildGridData.SelectionGridData, buildGridData.BuffUsageGridData);
		}
	}

	public void DisableBuildingMode()
	{
		base.gameObject.SetActive(value: false);
		buildGrid3D.Clear();
		buildGridData.EraseData();
	}
}
