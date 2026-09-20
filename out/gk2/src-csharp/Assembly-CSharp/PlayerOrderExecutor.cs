using UnityEngine;

public class PlayerOrderExecutor : IOrderExecutor
{
	private MultiInventory multiInventory = new MultiInventory(MainGame.PlayerData);

	public bool CanAddItem(Item item)
	{
		return true;
	}

	public bool HasItem(Item item)
	{
		if (item.Definition.itemSize == ItemSize.Big)
		{
			if (!MainGame.PlayerData.TryGetOverheadItem((Item overhead) => overhead.id == item.id && overhead.Count >= item.Count, out var _))
			{
				return multiInventory.HasItemQuantity(item.id, item.Count);
			}
			return true;
		}
		return multiInventory.HasItemQuantity(item.id, item.Count);
	}

	public void AddItem(Item item)
	{
		PlayerData playerData = MainGame.PlayerData;
		MainGame.Instance.dropSystem.DropItem(new Item(item.id, item.Count), MainGame.PlayerData.currentGameSceneId, playerData.position.Value + new Vector3(playerData.Direction.x, 0f, playerData.Direction.y));
	}

	public void RemoveItem(Item item)
	{
		if (item.Definition.itemSize == ItemSize.Big)
		{
			if (MainGame.PlayerData.TryGetOverheadItem((Item overhead) => overhead.id == item.id && overhead.Count >= item.Count, out var item2))
			{
				MainGame.PlayerData.RemoveOverheadItem(item2);
			}
			else
			{
				multiInventory.RemoveItem(new Item(item.id, item.Count));
			}
		}
		else
		{
			multiInventory.RemoveItem(new Item(item.id, item.Count));
		}
	}
}
