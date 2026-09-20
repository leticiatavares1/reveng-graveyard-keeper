using UnityEngine;

public class ConveyorCellStationBuildConnector : BuildConnector
{
	[SerializeField]
	private Direction direction;

	public override bool TryConnect(Wgo other)
	{
		return false;
	}
}
