public class ZombieCaretakerOrderExecutor : IOrderExecutor
{
	private ZombieWgoData zombieWgoData;

	public ZombieCaretakerOrderExecutor(ZombieWgoData zombieWgoData)
	{
		this.zombieWgoData = zombieWgoData;
	}

	public bool CanAddItem(Item item)
	{
		return zombieWgoData.CaretakerPortableItem.IsEmpty;
	}

	public bool HasItem(Item item)
	{
		if (zombieWgoData.CaretakerPortableItem != null)
		{
			return !zombieWgoData.CaretakerPortableItem.IsEmpty;
		}
		return false;
	}

	public void AddItem(Item item)
	{
		zombieWgoData.CaretakerPortableItem = new Item(item.id, item.Count);
	}

	public void RemoveItem(Item item)
	{
		zombieWgoData.CaretakerPortableItem.Count -= item.Count;
		if (zombieWgoData.CaretakerPortableItem.Count < 0)
		{
			zombieWgoData.CaretakerPortableItem = Item.Empty;
		}
		else
		{
			zombieWgoData.CaretakerPortableItem = zombieWgoData.CaretakerPortableItem;
		}
	}
}
