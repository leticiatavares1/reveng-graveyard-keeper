using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ZombieSystemData
{
	public const string DEFAULT_SKIN_ID = "zmb_01_00_worker";

	public List<ZombieWgoData> zombieDrops = new List<ZombieWgoData>();

	public List<SGuid> zombieOnSceneWgoIds = new List<SGuid>();

	private Dictionary<Guid, ZombieWgoData> cache;

	public Dictionary<Guid, ZombieWgoData> Cache
	{
		get
		{
			if (cache == null)
			{
				cache = new Dictionary<Guid, ZombieWgoData>();
				for (int i = 0; i < zombieDrops.Count; i++)
				{
					cache.Add(zombieDrops[i].UniqueId.Guid, zombieDrops[i]);
				}
				for (int num = zombieOnSceneWgoIds.Count - 1; num >= 0; num--)
				{
					SGuid sGuid = zombieOnSceneWgoIds[num];
					if (!(MainGame.WorldData.GetWgoData(sGuid) is ZombieWgoData value))
					{
						zombieOnSceneWgoIds.RemoveAt(num);
					}
					else
					{
						cache.Add(sGuid.Guid, value);
					}
				}
			}
			return cache;
		}
	}

	public void PrepareForGame()
	{
		foreach (ZombieWgoData zombieDrop in zombieDrops)
		{
			zombieDrop.PrepareForGame();
		}
	}

	public void ResumeCrafterWorkAfterLoad()
	{
		for (int i = 0; i < zombieOnSceneWgoIds.Count; i++)
		{
			GetZombie(zombieOnSceneWgoIds[i])?.TryResumeCrafterWorkAfterLoad();
		}
	}

	public ZombieWgoData CreateZombieDrop(string id, Vector3 position, string gameSceneId, Item zombieItem, string collarId = "collar_bronze", string name = null, bool rollSkin = true)
	{
		ZombieWgoData zombieWgoData = new ZombieWgoData(id, position, gameSceneId);
		zombieItem.UniqueId.SetGuid(zombieWgoData.UniqueId);
		Cache.Add(zombieWgoData.UniqueId.Guid, zombieWgoData);
		zombieDrops.Add(zombieWgoData);
		zombieWgoData.SetZombieItem(zombieItem);
		foreach (Item item3 in zombieItem.Inventory)
		{
			if (!string.IsNullOrEmpty(item3.Definition.bodyLinkedPerk))
			{
				zombieWgoData.AddPerk(item3.Definition.bodyLinkedPerk);
			}
		}
		zombieItem.AddItemToInventory(new Item(collarId));
		zombieWgoData.equippedCollar.SetGuid(zombieItem.GetItemByType(ItemType.Collar).UniqueId);
		if (zombieItem.TryGetProperty<BodyZombieStartItemsSerializedItemProperty>(out var property))
		{
			if (!string.IsNullOrEmpty(property.handsId))
			{
				Item item = new Item(property.handsId);
				zombieItem.AddItemToInventory(item);
				zombieWgoData.equippedHand.SetGuid(item.UniqueId);
			}
			if (!string.IsNullOrEmpty(property.armorId))
			{
				Item item2 = new Item(property.armorId);
				zombieItem.AddItemToInventory(item2);
				zombieWgoData.equippedArmor.SetGuid(item2.UniqueId);
			}
			zombieItem.RemoveProperty<BodyZombieStartItemsSerializedItemProperty>();
		}
		else
		{
			Debug.Log("Not found body start items property for zombie item: " + zombieItem.id);
		}
		if (rollSkin)
		{
			ZombieSkinHelper.RollAndApplyZombieSkinToWgoData(zombieWgoData, "zombie_worker");
		}
		return zombieWgoData;
	}

	public void PutZombieFromGameSceneToStore(ZombieWgoData zombieWgoData)
	{
		MainGame.WorldData.RemoveWgoDataFromGameScene(zombieWgoData.UniqueId);
		zombieOnSceneWgoIds.Remove(zombieWgoData.UniqueId);
		zombieDrops.Add(zombieWgoData);
	}

	public void PutZombieFromGameSceneToStoreForPlayer(PlayerData playerData, ZombieWgoData zombieWgoData)
	{
		playerData.AddOverheadItem(zombieWgoData.ZombieItem);
		PutZombieFromGameSceneToStore(zombieWgoData);
	}

	public ZombieWgoData PutZombieFromStoreToGameScene(SGuid uniqueId, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		ZombieWgoData zombie = GetZombie(uniqueId);
		zombieOnSceneWgoIds.Add(zombie.UniqueId);
		zombieDrops.Remove(zombie);
		zombie.WorldId = gameSceneId;
		zombie.Position = position;
		zombie.direction.Value = direction.ConvertToVector2XZ();
		zombie.SetDefaultAnimState();
		zombie.PrepareForGameBase();
		MainGame.WorldData.AddWgoData(zombie);
		if (!string.IsNullOrEmpty(zombie.Definition?.attachedScript))
		{
			WgoDataScriptsManager.CreateScript(zombie, zombie.Definition.attachedScript);
		}
		return zombie;
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneFromPlayer(PlayerData playerData, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		return PutZombieFromStoreToGameSceneFromPlayer(playerData, ResolveOverheadZombieItem(playerData), gameSceneId, position, direction);
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneFromPlayer(PlayerData playerData, Item zombieItem, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		zombieItem = ResolveOverheadZombieItem(playerData, zombieItem);
		ZombieWgoData result = PutZombieFromStoreToGameScene(zombieItem.UniqueId, gameSceneId, position, direction);
		playerData.RemoveOverheadItem(zombieItem);
		return result;
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneAsAssistant(PlayerData playerData, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		return PutZombieFromStoreToGameSceneAsAssistant(playerData, ResolveOverheadZombieItem(playerData), gameSceneId, position, direction);
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneAsAssistant(PlayerData playerData, Item zombieItem, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		zombieItem = ResolveOverheadZombieItem(playerData, zombieItem);
		ZombieWgoData zombie = GetZombie(zombieItem.UniqueId);
		zombieDrops.Remove(zombie);
		Cache.Remove(zombie.UniqueId.Guid);
		ZombieWgoData zombieWgoData = zombie.CreateAssistantFromThis(position, gameSceneId, direction);
		MainGame.WorldData.AddWgoData(zombieWgoData);
		Cache.Add(zombieWgoData.UniqueId.Guid, zombieWgoData);
		zombieOnSceneWgoIds.Add(zombieWgoData.UniqueId);
		playerData.RemoveOverheadItem(zombieItem);
		return zombieWgoData;
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneAsCommon(PlayerData playerData, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		return PutZombieFromStoreToGameSceneAsCommon(playerData, ResolveOverheadZombieItem(playerData), gameSceneId, position, direction);
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneAsCommon(PlayerData playerData, Item zombieItem, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		zombieItem = ResolveOverheadZombieItem(playerData, zombieItem);
		ZombieWgoData zombie = GetZombie(zombieItem.UniqueId);
		if (zombie.id == "zombie")
		{
			return PutZombieFromStoreToGameSceneFromPlayer(playerData, zombieItem, gameSceneId, position, direction);
		}
		zombieDrops.Remove(zombie);
		Cache.Remove(zombie.UniqueId.Guid);
		ZombieWgoData zombieWgoData = zombie.CreateCommonZombieFromThis(position, gameSceneId, direction);
		MainGame.WorldData.AddWgoData(zombieWgoData);
		Cache.Add(zombieWgoData.UniqueId.Guid, zombieWgoData);
		zombieOnSceneWgoIds.Add(zombieWgoData.UniqueId);
		playerData.RemoveOverheadItem(zombieItem);
		return zombieWgoData;
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneAsGardener(PlayerData playerData, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		return PutZombieFromStoreToGameSceneAsGardener(playerData, ResolveOverheadZombieItem(playerData), gameSceneId, position, direction);
	}

	public ZombieWgoData PutZombieFromStoreToGameSceneAsGardener(PlayerData playerData, Item zombieItem, string gameSceneId, Vector3 position, Direction direction = Direction.Down)
	{
		zombieItem = ResolveOverheadZombieItem(playerData, zombieItem);
		ZombieWgoData zombie = GetZombie(zombieItem.UniqueId);
		if (zombie.id == "zombie")
		{
			return PutZombieFromStoreToGameSceneFromPlayer(playerData, zombieItem, gameSceneId, position, direction);
		}
		zombieDrops.Remove(zombie);
		Cache.Remove(zombie.UniqueId.Guid);
		ZombieWgoData zombieWgoData = zombie.CreateCommonZombieFromThis(position, gameSceneId, direction);
		MainGame.WorldData.AddWgoData(zombieWgoData);
		Cache.Add(zombieWgoData.UniqueId.Guid, zombieWgoData);
		zombieOnSceneWgoIds.Add(zombieWgoData.UniqueId);
		playerData.RemoveOverheadItem(zombieItem);
		return zombieWgoData;
	}

	private static Item ResolveOverheadZombieItem(PlayerData playerData, Item zombieItem = null)
	{
		if (zombieItem != null && !zombieItem.IsEmpty)
		{
			return zombieItem;
		}
		if (playerData != null && playerData.TryGetOverheadItem((Item item) => item.Definition.itemGroupIds.Contains("zombie"), out var item2))
		{
			return item2;
		}
		return playerData?.overheadItem;
	}

	public ZombieWgoData GetZombie(SGuid uniqueId)
	{
		Cache.TryGetValue(uniqueId.Guid, out var value);
		return value;
	}

	public void RemoveZombie(SGuid uniqueId)
	{
		for (int i = 0; i < zombieDrops.Count; i++)
		{
			if (zombieDrops[i].UniqueId == uniqueId)
			{
				zombieDrops.RemoveAt(i);
				break;
			}
		}
		zombieOnSceneWgoIds.Remove(uniqueId);
		Cache.Remove(uniqueId.Guid);
		if (MainGame.WorldData.GetWgoData(uniqueId) != null)
		{
			MainGame.WorldData.RemoveWgoDataFromGameScene(uniqueId);
		}
	}
}
