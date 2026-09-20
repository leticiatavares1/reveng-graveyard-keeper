using UnityEngine;

public struct BuildCellSelectionData
{
	public int state;

	public Vector3 coords;

	public BuildCellSelectionData(Vector3 coords, int state)
	{
		this.coords = coords;
		this.state = state;
	}
}
