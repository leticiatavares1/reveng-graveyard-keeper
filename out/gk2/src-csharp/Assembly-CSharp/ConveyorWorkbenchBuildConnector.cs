using UnityEngine;

public class ConveyorWorkbenchBuildConnector : BuildConnector
{
	[SerializeField]
	private Direction direction;

	public override bool TryConnect(Wgo other)
	{
		switch (other.Data.Definition.conveyorType)
		{
		case ConveyorElementType.Cell:
		case ConveyorElementType.UndergroundCell:
			base.Parent.Connect(other.Data as ConveyorWgoData, base.ConnectionType, direction);
			break;
		case ConveyorElementType.Splitter:
			base.Parent.Connect(other.Data as ConveyorWgoData, base.ConnectionType, direction);
			break;
		}
		return false;
	}
}
