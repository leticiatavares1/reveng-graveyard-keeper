public class ConveyorPalletBuildConnector : BuildConnector
{
	public override bool TryConnect(Wgo other)
	{
		switch (other.Data.Definition.conveyorType)
		{
		case ConveyorElementType.Cell:
		case ConveyorElementType.UndergroundCell:
			base.Parent.Connect(other.Data as ConveyorWgoData, ConveyorConnectionType.Out);
			break;
		case ConveyorElementType.Splitter:
			base.Parent.Connect(other.Data as ConveyorWgoData, ConveyorConnectionType.Out);
			break;
		}
		return false;
	}
}
