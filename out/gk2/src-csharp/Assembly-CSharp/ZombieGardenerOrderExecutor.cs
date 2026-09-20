public class ZombieGardenerOrderExecutor : IOrderExecutor
{
	public ZombieWgoData zombieWgoData;

	public ZombieGardenerOrderExecutor(ZombieWgoData zombieWgoData)
	{
		this.zombieWgoData = zombieWgoData;
	}

	public bool CanAddItem(Item item)
	{
		return new Inventory(zombieWgoData.ZombieItem).CanAddItemToInventory(item);
	}

	public bool HasItem(Item item)
	{
		Inventory inventory = new Inventory(zombieWgoData.ZombieItem);
		MultiInventory multiInventory = new MultiInventory(zombieWgoData.WorldZoneData);
		if (!inventory.Data.HasItemQuantityInInventory(item.id, item.Count))
		{
			return multiInventory.HasItemQuantity(item.id, item.Count);
		}
		return true;
	}

	public void AddItem(Item item)
	{
		new Inventory(zombieWgoData.ZombieItem).AddItemToInventory(item);
	}

	public void RemoveItem(Item item)
	{
		new Inventory(zombieWgoData.ZombieItem).RemoveItemById(item.id, item.Count);
	}
}
