using System;

[Serializable]
public class ConveyorConnectorsSetup : IAutoParsable
{
	public Direction direction;

	public int slotsCount;

	public bool IsEmpty
	{
		get
		{
			if (slotsCount > 0)
			{
				return direction == Direction.None;
			}
			return true;
		}
	}

	public ConveyorConnectorsSetup()
	{
	}

	public ConveyorConnectorsSetup(Direction direction, int slotsCount)
	{
		this.direction = direction;
		this.slotsCount = slotsCount;
	}
}
