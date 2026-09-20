using UnityEngine;

public struct BuildCellBuffUsageData
{
	public int state;

	public Vector3 coords;

	public BuildCellBuffUsageData(Vector3 coords, int state)
	{
		this.coords = coords;
		this.state = state;
	}
}
