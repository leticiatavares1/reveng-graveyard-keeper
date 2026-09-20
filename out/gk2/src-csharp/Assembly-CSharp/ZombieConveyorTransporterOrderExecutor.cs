public class ZombieConveyorTransporterOrderExecutor : IOrderExecutor
{
	private ZombieWgoData zombieWgoData;

	public ZombieConveyorTransporterOrderExecutor(ZombieWgoData zombieWgoData)
	{
		this.zombieWgoData = zombieWgoData;
	}

	public bool CanAddItem(Item item)
	{
		return zombieWgoData.ConveyorTransporterPortableItem.IsEmpty;
	}

	public bool HasItem(Item item)
	{
		if (zombieWgoData.ConveyorTransporterPortableItem != null)
		{
			return !zombieWgoData.ConveyorTransporterPortableItem.IsEmpty;
		}
		return false;
	}

	public void AddItem(Item item)
	{
		zombieWgoData.ConveyorTransporterPortableItem = new Item(item.id, item.Count);
	}

	public void RemoveItem(Item item)
	{
		zombieWgoData.ConveyorTransporterPortableItem.Count -= item.Count;
		if (zombieWgoData.ConveyorTransporterPortableItem.Count < 0)
		{
			zombieWgoData.ConveyorTransporterPortableItem = Item.Empty;
		}
		else
		{
			zombieWgoData.ConveyorTransporterPortableItem = zombieWgoData.ConveyorTransporterPortableItem;
		}
	}
}
