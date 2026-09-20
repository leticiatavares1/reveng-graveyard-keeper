using System;

[Serializable]
public class ConveyorChestSlotData
{
	public SGuid conveyorWgoDataUniqueId;

	public string slotItemId;

	public Direction slotPosDirection;

	public int slotIndex = -1;

	private ConveyorWgoData conveyorWgoData;

	public ConveyorWgoData ConveyorWgoData
	{
		get
		{
			if (conveyorWgoData != null)
			{
				return conveyorWgoData;
			}
			if (conveyorWgoDataUniqueId == null)
			{
				return null;
			}
			conveyorWgoData = MainGame.Instance.GameSave.worldData.GetWgoData(conveyorWgoDataUniqueId) as ConveyorWgoData;
			return conveyorWgoData;
		}
	}

	public ConveyorChestSlotData(Direction slotPosDirection)
	{
		this.slotPosDirection = slotPosDirection;
	}

	public ConveyorChestSlotData(Direction slotPosDirection, int slotIndex)
		: this(slotPosDirection)
	{
		this.slotIndex = slotIndex;
	}

	public void Clear()
	{
		conveyorWgoDataUniqueId = null;
		slotItemId = string.Empty;
		conveyorWgoData = null;
	}

	public void SetItemSlotId(string itemSlotId)
	{
		slotItemId = itemSlotId;
	}
}
