using System;

[Serializable]
public class ConveyorMovableItemData
{
	public Direction direction;

	public string itemId;

	public bool isCommon;

	public ConveyorMovableItemData(string itemId, int rotationIndex, bool isCommon)
	{
		this.itemId = itemId;
		switch (rotationIndex)
		{
		case 0:
			direction = Direction.Down;
			break;
		case 1:
			direction = Direction.Left;
			break;
		case 2:
			direction = Direction.Up;
			break;
		case 3:
			direction = Direction.Right;
			break;
		}
		this.isCommon = isCommon;
	}

	public ConveyorMovableItemData(string itemId, Direction direction, bool isCommon)
	{
		this.itemId = itemId;
		this.direction = direction;
		this.isCommon = isCommon;
	}
}
