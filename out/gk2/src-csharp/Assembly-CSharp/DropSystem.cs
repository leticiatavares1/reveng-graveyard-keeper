using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class DropSystem : ICustomUpdatable
{
	public WorldData WorldData => MainGame.Instance.GameSave.worldData;

	public bool DropItem(Item droppableItem, string worldId, Vector3 pos, List<Item> droppedItemsList = null)
	{
		if (string.IsNullOrEmpty(worldId))
		{
			Debug.LogError("DropItem: worldId couldn't be empty");
			return false;
		}
		Debug.Log($"DropItem: {droppableItem.id} {droppableItem.Count}");
		if (droppableItem.Definition.isFuel)
		{
			return false;
		}
		if (droppableItem.Definition.isTechPoint)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (droppableItem.id.EndsWith("tech_red"))
			{
				num += droppableItem.Count;
			}
			if (droppableItem.id.EndsWith("tech_green"))
			{
				num2 += droppableItem.Count;
			}
			if (droppableItem.id.EndsWith("tech_blue"))
			{
				num3 += droppableItem.Count;
			}
			if (num + num2 + num3 > 0)
			{
				DropTechPoints(pos, num, num2, num3);
				return true;
			}
			return false;
		}
		if (droppableItem.Count > 1)
		{
			if (droppableItem.Count > 5 && droppableItem.Definition.itemSize == ItemSize.Small)
			{
				int num4 = droppableItem.Count;
				while (num4 > 0)
				{
					int num5 = num4;
					if (droppableItem.Definition.stackCount > 0 && num5 > droppableItem.Definition.stackCount)
					{
						num5 = droppableItem.Definition.stackCount;
					}
					num4 -= num5;
					Item item = new Item(droppableItem.id, num5);
					droppedItemsList?.Add(item);
					DropItemInternal(item, worldId, pos);
				}
				return true;
			}
			for (int i = 0; i < droppableItem.Count; i++)
			{
				Item item2 = new Item(droppableItem.id);
				droppedItemsList?.Add(item2);
				DropItemInternal(item2, worldId, pos);
			}
			return true;
		}
		droppedItemsList?.Add(droppableItem);
		DropItemInternal(droppableItem, worldId, pos);
		return true;
	}

	public bool DropGameResAtom(GameResAtom gameResAtom, string worldId, Vector3 pos)
	{
		return DropItem(gameResAtom.ItemFromAtom(), worldId, pos);
	}

	public void DropItemAsDropView(Item droppableItem, string worldId, Vector3 pos)
	{
		if (droppableItem != null && !string.IsNullOrEmpty(droppableItem.id) && !string.IsNullOrEmpty(worldId))
		{
			DropItemInternal(droppableItem, worldId, pos);
		}
	}

	public void CollectAllGameResDropsToPlayer(float duration)
	{
		Transform playerDropCollectorTransform = GetPlayerDropCollectorTransform();
		if (!(playerDropCollectorTransform == null))
		{
			for (int i = 0; i < WorldData.gameSceneDataList.Count; i++)
			{
				GameSceneData gameSceneData = WorldData.gameSceneDataList[i];
				CollectGameResDropsFromList(gameSceneData.droppedItems, playerDropCollectorTransform, duration);
				CollectGameResDropsFromList(gameSceneData.queuedDrops, playerDropCollectorTransform, duration);
			}
			TechPointsSpawner.FlushPendingAsWorldDrops();
			TechPointDrop.CollectAllToPlayer(playerDropCollectorTransform, duration);
			CollectViewlessTechPointDrops();
		}
	}

	public void RemoveDrop(DropData drop, string worldId)
	{
		if (string.IsNullOrEmpty(worldId))
		{
			Debug.LogError("DropItem: worldId couldn't be empty");
		}
		else
		{
			WorldData.GetGameSceneDataById(worldId).RemoveDrop(drop);
		}
	}

	public void CustomUpdate(float deltaTime)
	{
		for (int num = WorldData.gameSceneDataList.Count - 1; num >= 0; num--)
		{
			GameSceneData gameSceneData = WorldData.gameSceneDataList[num];
			for (int num2 = gameSceneData.droppedItems.Count - 1; num2 >= 0; num2--)
			{
				DropData dropData = gameSceneData.droppedItems[num2];
				if (!dropData.CanNotBeAutoDestroyed.ResultFlag && !dropData.IsRemoving && dropData.AutoDestroyTimer > 0f)
				{
					dropData.AutoDestroyTimer -= deltaTime;
					if (dropData.AutoDestroyTimer <= 0f)
					{
						MainGame.PlayerData.SubRes("cur_bodies_count", 1f);
						gameSceneData.RemoveDrop(dropData);
						if (MainGame.PlayerData.CurrentWorldZoneData != null && MainGame.PlayerData.CurrentWorldZoneData.Definition.id == "morgue")
						{
							GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
						}
					}
				}
			}
		}
	}

	public void DropTechPoints(Vector3 pos, int r, int g, int b)
	{
		TechPointsSpawner.CreateSpawner(pos, r, g, b, 0);
	}

	private void CollectGameResDropsFromList(List<DropData> drops, Transform target, float duration)
	{
		if (drops == null || drops.Count == 0)
		{
			return;
		}
		for (int num = drops.Count - 1; num >= 0; num--)
		{
			DropData dropData = drops[num];
			if (dropData != null && dropData.IsResDrop && !dropData.IsRemoving)
			{
				DropView dropView = FindDropView(dropData);
				if (dropView != null && !dropView.IsDespawning)
				{
					dropView.MoveToCollectorTimed(target, duration);
				}
				else
				{
					MainGame.PlayerData.CollectResDrop(dropData);
				}
			}
		}
	}

	private void CollectViewlessTechPointDrops()
	{
		PlayerController playerController = MainGame.PlayerController;
		Vector3 pos = ((playerController != null) ? playerController.transform.position : Vector3.zero);
		for (int i = 0; i < WorldData.gameSceneDataList.Count; i++)
		{
			GameSceneData gameSceneData = WorldData.gameSceneDataList[i];
			List<TechPointDropData> techPointDrops = gameSceneData.techPointDrops;
			if (techPointDrops == null || techPointDrops.Count == 0)
			{
				continue;
			}
			for (int num = techPointDrops.Count - 1; num >= 0; num--)
			{
				TechPointDropData techPointDropData = techPointDrops[num];
				if (techPointDropData != null && !TechPointDrop.IsTracked(techPointDropData))
				{
					string techPointName = TechDef.FlyingReses[(int)techPointDropData.type];
					FlyingTechPoint.Drop(pos, techPointName);
					LazyAudio.Play("tech_point_collect");
					gameSceneData.RemoveTechPointDrop(techPointDropData);
				}
			}
		}
	}

	private static DropView FindDropView(DropData drop)
	{
		if (drop?.Item == null || LazySingleton<GameSceneManager>.Instance == null)
		{
			return null;
		}
		List<GameScene> loadedGameScenes = LazySingleton<GameSceneManager>.Instance.LoadedGameScenes;
		for (int i = 0; i < loadedGameScenes.Count; i++)
		{
			GameScene gameScene = loadedGameScenes[i];
			if (gameScene != null && gameScene.TryGetDropView(drop.Item, out var dropView))
			{
				return dropView;
			}
		}
		return null;
	}

	private static Transform GetPlayerDropCollectorTransform()
	{
		PlayerController playerController = MainGame.PlayerController;
		if (playerController == null)
		{
			return null;
		}
		DropSearcher componentInChildren = playerController.GetComponentInChildren<DropSearcher>(includeInactive: true);
		if (componentInChildren != null && componentInChildren.CollectorObjectTransform != null)
		{
			return componentInChildren.CollectorObjectTransform;
		}
		return playerController.transform;
	}

	private void DropItemInternal(Item droppableItem, string worldId, Vector3 pos)
	{
		if (WorldData.LoadedScenes.Contains(worldId))
		{
			DropItemIntoWorld(droppableItem, worldId, pos);
		}
		else
		{
			WorldData.GetGameSceneDataById(worldId).AddDropToQueue(droppableItem, pos);
		}
	}

	private void DropItemIntoWorld(Item droppableItem, string worldId, Vector3 pos)
	{
		if (string.IsNullOrEmpty(droppableItem.id))
		{
			Debug.LogError("[DropSystem]: tried to drop an empty item.");
		}
		else
		{
			WorldData.GetGameSceneDataById(worldId).AddDrop(droppableItem, pos);
		}
	}
}
