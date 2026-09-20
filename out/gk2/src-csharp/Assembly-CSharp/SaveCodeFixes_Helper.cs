using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public static class SaveCodeFixes_Helper
{
	private enum FloraKind
	{
		Spawner,
		Stump,
		Tree,
		Bush
	}

	private const float ConfigPositionEpsilonSqr = 0.0001f;

	private const string StumpsWgoGroup = "stumps";

	private const string BushesWgoGroup = "bushes";

	private const string CollectableBushesWgoGroup = "collectable_bushes";

	public static List<WgoData> GetSignboardsWithSpawnedTent(GameSave gameSave)
	{
		List<WgoData> list = new List<WgoData>();
		List<GameSceneData> list2 = gameSave?.worldData?.gameSceneDataList;
		if (list2 == null)
		{
			return list;
		}
		List<WgoData> list3 = new List<WgoData>();
		List<WgoData> list4 = new List<WgoData>();
		for (int i = 0; i < list2.Count; i++)
		{
			List<WgoData> list5 = list2[i]?.wgoDataList;
			if (list5 == null)
			{
				continue;
			}
			for (int j = 0; j < list5.Count; j++)
			{
				WgoData wgoData = list5[j];
				if (wgoData != null)
				{
					if (IsSignboard(wgoData))
					{
						list3.Add(wgoData);
					}
					else if (IsTent(wgoData))
					{
						list4.Add(wgoData);
					}
				}
			}
		}
		if (list4.Count == 0)
		{
			return list;
		}
		for (int k = 0; k < list3.Count; k++)
		{
			if (HasTentAtConfigurationCoordinates(list3[k], list4))
			{
				list.Add(list3[k]);
			}
		}
		return list;
	}

	public static List<string> GetSignboardsPostfixes(List<WgoData> signboards)
	{
		List<string> list = new List<string>();
		string text = "t_b_signboard_";
		for (int i = 0; i < signboards.Count; i++)
		{
			WgoData wgoData = signboards[i];
			if (wgoData != null && HasCustomTagPrefix(wgoData, text))
			{
				list.Add(wgoData.CustomTag.Replace(text, ""));
			}
		}
		return list;
	}

	private static bool IsSignboard(WgoData wgo)
	{
		if (HasCustomTagPrefix(wgo, "t_b_signboard_"))
		{
			return true;
		}
		if (GameBalance.Me != null && wgo.Definition != null)
		{
			return wgo.Definition.interactionType == WGODef.InteractionType.TownBuildingPlace;
		}
		return false;
	}

	private static bool IsTent(WgoData wgo)
	{
		return HasCustomTagPrefix(wgo, "t_b_tent_");
	}

	private static bool HasCustomTagPrefix(WgoData wgo, string prefix)
	{
		if (!string.IsNullOrEmpty(wgo.CustomTag))
		{
			return wgo.CustomTag.StartsWith(prefix);
		}
		return false;
	}

	private static bool HasTentAtConfigurationCoordinates(WgoData signboard, List<WgoData> tents)
	{
		List<TownBuildingTierSceneConfiguration> list = signboard.TownBuildingWgoComponent?.SceneConfiguration?.tierDataList;
		if (list == null)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (HasTentAtConfigObject(tents, signboard.WorldId, list[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasTentAtConfigObject(List<WgoData> tents, string worldId, TownBuildingTierSceneConfiguration tier)
	{
		if (tier == null)
		{
			return false;
		}
		if (!HasTentAtConfigPosition(tents, worldId, tier.tent) && !HasTentAtConfigPosition(tents, worldId, tier.sign) && !HasTentAtConfigPosition(tents, worldId, tier.yard) && !HasTentAtConfigPosition(tents, worldId, tier.decor1) && !HasTentAtConfigPosition(tents, worldId, tier.decor2))
		{
			return HasTentAtConfigPosition(tents, worldId, tier.decor3);
		}
		return true;
	}

	private static bool HasTentAtConfigPosition(List<WgoData> tents, string worldId, TownBuildingObjectConfiguration config)
	{
		if (config == null || string.IsNullOrEmpty(config.wgoId))
		{
			return false;
		}
		for (int i = 0; i < tents.Count; i++)
		{
			WgoData wgoData = tents[i];
			if (IsSameWorld(worldId, wgoData.WorldId) && (wgoData.Position - config.position).sqrMagnitude <= 0.0001f)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsSameWorld(string worldIdA, string worldIdB)
	{
		if (string.IsNullOrEmpty(worldIdA) || string.IsNullOrEmpty(worldIdB))
		{
			return true;
		}
		return worldIdA == worldIdB;
	}

	public static void UnlockOrHideTechByQuest(SaveFixContext ctx, string techId, string unlockQuestId)
	{
		TechDef dataOrNull = GameBalance.Me.GetDataOrNull<TechDef>(techId);
		if (dataOrNull == null)
		{
			ctx.LogError("Missing TechDef [" + techId + "]");
		}
		else if (ctx.GameSave.questSystemData.IsQuestInStatus(unlockQuestId, QuestStatus.Completed))
		{
			if (!ctx.GameSave.knowledgeSystem.IsTechUnlocked(dataOrNull.id))
			{
				dataOrNull.Unlock(free: true);
			}
		}
		else
		{
			ctx.GameSave.knowledgeSystem.HideTech(dataOrNull.id);
		}
	}

	public static void CopyUnlockedTutorialsToViewed(KnowledgeSystem knowledge)
	{
		if (knowledge?.unlockedTutorials != null)
		{
			for (int i = 0; i < knowledge.unlockedTutorials.Count; i++)
			{
				knowledge.AddViewedTutorial(knowledge.unlockedTutorials[i]);
			}
		}
	}

	public static void ResetNpcLifeSimulator(GameSave save)
	{
		save.npcLifeSimulatorData.ClearData();
		foreach (NPCGroupPointOfInterestConfiguration allGroup in LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.AllGroups)
		{
			save.npcLifeSimulatorData.AddGroup(new NPCGroupPointOfInterestData(allGroup));
		}
		foreach (NPCPointOfInterestConfiguration allPont in LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.AllPonts)
		{
			save.npcLifeSimulatorData.AddPoint(new NPCPointOfInterestData(allPont));
		}
		save.npcLifeSimulatorData.PrepareForGame();
	}

	public static bool WasQuestStarted(QuestSystemData quests, string questId)
	{
		if (!quests.IsQuestInStatus(questId, QuestStatus.InProgress))
		{
			return quests.IsQuestInStatus(questId, QuestStatus.Completed);
		}
		return true;
	}

	public static void GiveItemIfQuestCompleted(SaveFixContext ctx, QuestSystemData quests, string questId, string itemId, int count)
	{
		if (quests.IsQuestInStatus(questId, QuestStatus.Completed))
		{
			GiveItem(ctx, itemId, count);
		}
	}

	public static void GiveItem(SaveFixContext ctx, string itemId, int count)
	{
		if (count > 0)
		{
			PlayerData playerData = ctx.GameSave.playerData;
			Inventory inventory = playerData.Inventory;
			int num = inventory.Data.CanAddItemCountToInventory(new Item(itemId, count));
			int num2 = count - num;
			if (num > 0)
			{
				AddItemsToInventory(inventory, itemId, num);
				ctx.Log($"Gave [{itemId} x{num}]");
			}
			if (num2 > 0)
			{
				MainGame.Instance.dropSystem.DropItem(new Item(itemId, num2), playerData.currentGameSceneId, playerData.position.Value + new Vector3(playerData.Direction.x, 0f, playerData.Direction.y));
				ctx.Log($"Dropped leftover [{itemId} x{num2}] (inventory full)");
			}
		}
	}

	private static void AddItemsToInventory(Inventory inventory, string itemId, int count)
	{
		ItemDef dataOrNull = GameBalance.Me.GetDataOrNull<ItemDef>(itemId);
		if (dataOrNull != null && dataOrNull.stackCount == 1)
		{
			for (int i = 0; i < count; i++)
			{
				inventory.AddItemToInventory(new Item(itemId));
			}
		}
		else
		{
			inventory.AddItemToInventory(new Item(itemId, count));
		}
	}

	public static void HideInspiration(KnowledgeSystem knowledge, string inspirationId)
	{
		if (knowledge.hiddenInspirations == null)
		{
			knowledge.hiddenInspirations = new List<string>();
		}
		if (!knowledge.hiddenInspirations.Contains(inspirationId))
		{
			knowledge.hiddenInspirations.Add(inspirationId);
		}
	}

	public static void UnlockOrHideInspirationByQuest(SaveFixContext ctx, KnowledgeSystem knowledge, QuestSystemData quests, string inspirationId, string unlockQuestId)
	{
		if (quests.IsQuestInStatus(unlockQuestId, QuestStatus.Completed))
		{
			if (knowledge.IsInspirationHidden(inspirationId))
			{
				knowledge.RevealInspiration(inspirationId);
				ctx.Log("Revealed inspiration [" + inspirationId + "] from completed quest [" + unlockQuestId + "]");
			}
		}
		else
		{
			HideInspiration(knowledge, inspirationId);
		}
	}

	public static void AddInspirationIfQuestCompleted(GameSave save, QuestSystemData quests, string questId, string inspirationId, int value)
	{
		if (quests.IsQuestInStatus(questId, QuestStatus.Completed))
		{
			save.talentSystemData.AddToInspiration(inspirationId, value);
		}
	}

	public static void UnlockTechIfQuestCompleted(SaveFixContext ctx, QuestSystemData quests, KnowledgeSystem knowledge, string questId, string techId)
	{
		if (quests.IsQuestInStatus(questId, QuestStatus.Completed) && !knowledge.IsTechUnlocked(techId))
		{
			TechDef dataOrNull = GameBalance.Me.GetDataOrNull<TechDef>(techId);
			if (dataOrNull == null)
			{
				ctx.LogError("Missing TechDef [" + techId + "]");
				return;
			}
			dataOrNull.Unlock(free: true);
			ctx.Log("Unlocked tech [" + techId + "] from completed quest [" + questId + "]");
		}
	}

	public static void RevealTechsFromCompletedQuests(SaveFixContext ctx)
	{
		KnowledgeSystem knowledgeSystem = ctx?.GameSave?.knowledgeSystem;
		if (knowledgeSystem != null)
		{
			int num = knowledgeSystem.hiddenTechs?.Count ?? 0;
			knowledgeSystem.RevealTechsFromCompletedQuests();
			int num2 = num - (knowledgeSystem.hiddenTechs?.Count ?? 0);
			if (num2 > 0)
			{
				ctx.Log($"Revealed {num2} tech(s) from completed quests");
			}
		}
	}

	public static void RevealHiddenTechsIfParentRevealed(SaveFixContext ctx)
	{
		KnowledgeSystem knowledgeSystem = ctx?.GameSave?.knowledgeSystem;
		if (knowledgeSystem != null)
		{
			int num = knowledgeSystem.hiddenTechs?.Count ?? 0;
			knowledgeSystem.CatchUpRevealedTechsFromTree();
			int num2 = num - (knowledgeSystem.hiddenTechs?.Count ?? 0);
			if (num2 > 0)
			{
				ctx.Log($"Revealed {num2} tech(s) whose parent is already visible in the tech tree");
			}
		}
	}

	public static void RevokeUnlockedCraft(SaveFixContext ctx, KnowledgeSystem knowledge, string craftId)
	{
		if (knowledge?.unlockedCrafts != null && knowledge.unlockedCrafts.Contains(craftId))
		{
			knowledge.RemoveUnlockedCraft(craftId);
			ctx.Log("Revoked unlocked craft [" + craftId + "]");
		}
	}

	public static void ResyncQuestVisualisation(SaveFixContext ctx, QuestSystemData quests, string questId)
	{
		QuestDef dataOrNull = GameBalance.Me.GetDataOrNull<QuestDef>(questId);
		if (dataOrNull == null)
		{
			ctx.LogError("Missing QuestDef [" + questId + "]");
			return;
		}
		bool flag = quests.IsQuestInStatus(questId, QuestStatus.InProgress) || quests.IsQuestInStatus(questId, QuestStatus.Completed);
		bool flag2 = !(dataOrNull.hasPosInBalance && flag) && dataOrNull.isHidden;
		quests.ChangeQuestHiddenState(questId, flag2);
		quests.ChangeQuestUnknownState(questId, dataOrNull.isUnknown);
		ctx.Log($"Resynced visualisation of quest [{questId}]: hidden={flag2}, unknown={dataOrNull.isUnknown}");
	}

	public static void CompleteVisualQuestIfQuestCompleted(SaveFixContext ctx, QuestSystemData quests, string questId, string visualQuestId)
	{
		if (!quests.IsQuestInStatus(questId, QuestStatus.Completed) || quests.IsQuestInStatus(visualQuestId, QuestStatus.Completed))
		{
			return;
		}
		QuestDef dataOrNull = GameBalance.Me.GetDataOrNull<QuestDef>(visualQuestId);
		if (dataOrNull == null)
		{
			ctx.LogError("Missing QuestDef [" + visualQuestId + "]");
			return;
		}
		quests.CompleteQuest(visualQuestId);
		if (dataOrNull.hasPosInBalance)
		{
			quests.ChangeQuestHiddenState(visualQuestId, state: false);
		}
		ctx.Log("Completed visual quest [" + visualQuestId + "] because [" + questId + "] is completed");
	}

	public static void HideTech(SaveFixContext ctx, KnowledgeSystem knowledge, string techId)
	{
		if (GameBalance.Me.GetDataOrNull<TechDef>(techId) == null)
		{
			ctx.LogError("Missing TechDef [" + techId + "]");
			return;
		}
		knowledge.HideTech(techId);
		ctx.Log("Hid tech [" + techId + "]");
	}

	public static void RevealHiddenTechsFromWorldObjects(SaveFixContext ctx)
	{
		GameSave gameSave = ctx?.GameSave;
		KnowledgeSystem knowledgeSystem = gameSave?.knowledgeSystem;
		if (knowledgeSystem != null)
		{
			RevealTechIfAnyWgoPresent(ctx, knowledgeSystem, gameSave, "autopsy_resurection", "power_switch_table");
			RevealTechIfAnyWgoPresent(ctx, knowledgeSystem, gameSave, "kitchen_table_2", "kitchen_table", "kitchen_table_t2");
			RevealTechIfAnyWgoPresent(ctx, knowledgeSystem, gameSave, "kitchen_oven_2", "kitchen_oven", "kitchen_oven_t2");
			bool flag = gameSave.playerData != null && gameSave.playerData.GetResInt("g_garden_farming_base") >= 4;
			if (knowledgeSystem.IsTechHidden("garden_improve_2") && (flag || HasAnyWgoWithId(gameSave, "garden_t1", "garden_t2")))
			{
				knowledgeSystem.RevealTech("garden_improve_2");
				ctx.Log("Revealed tech [garden_improve_2]: garden farming base was upgraded");
			}
		}
	}

	private static void RevealTechIfAnyWgoPresent(SaveFixContext ctx, KnowledgeSystem knowledge, GameSave save, string techId, params string[] wgoIds)
	{
		if (knowledge.IsTechHidden(techId) && HasAnyWgoWithId(save, wgoIds))
		{
			knowledge.RevealTech(techId);
			ctx.Log("Revealed tech [" + techId + "]: world object [" + wgoIds[0] + "] is present");
		}
	}

	private static bool HasAnyWgoWithId(GameSave save, params string[] wgoIds)
	{
		List<GameSceneData> list = save?.worldData?.gameSceneDataList;
		if (list == null || wgoIds == null || wgoIds.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			List<WgoData> list2 = list[i]?.wgoDataList;
			if (list2 == null)
			{
				continue;
			}
			for (int j = 0; j < list2.Count; j++)
			{
				WgoData wgoData = list2[j];
				if (wgoData == null || string.IsNullOrEmpty(wgoData.id))
				{
					continue;
				}
				for (int k = 0; k < wgoIds.Length; k++)
				{
					if (wgoData.id == wgoIds[k])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static void UnlockBuildingIfTechUnlocked(KnowledgeSystem knowledge, string techId, string buildingId)
	{
		if (knowledge != null && knowledge.IsTechUnlocked(techId))
		{
			knowledge.UnlockBuilding(buildingId);
		}
	}

	public static void CatchUpVendorTiers(SaveFixContext ctx, VendorSystem vendorSystem, QuestSystemData quests, string vendorId, params string[] completedQuestIds)
	{
		Vendor vendor = vendorSystem.GetVendor(vendorId);
		if (vendor?.Definition == null)
		{
			ctx.LogWarning("Vendor [" + vendorId + "] not found, skip tier catch-up");
			return;
		}
		int num = 0;
		for (int i = 0; i < completedQuestIds.Length; i++)
		{
			if (quests.IsQuestInStatus(completedQuestIds[i], QuestStatus.Completed))
			{
				num++;
			}
		}
		int num2 = vendor.Definition.startTier + num;
		int num3 = 0;
		while (vendor.CurTier < num2)
		{
			int curTier = vendor.CurTier;
			vendor.ForceLevelUp();
			if (vendor.CurTier <= curTier)
			{
				break;
			}
			num3++;
		}
		if (num3 > 0)
		{
			ctx.Log($"Leveled vendor [{vendorId}] +{num3} to tier {vendor.CurTier}");
		}
	}

	public static void ForceHideTalentLevelUp(KnowledgeSystem knowledge, string talentLevelUpId)
	{
		knowledge.revealedTalentLevelUps?.Remove(talentLevelUpId);
		if (knowledge.hiddenTalentLevelUps == null)
		{
			knowledge.hiddenTalentLevelUps = new List<string>();
		}
		if (!knowledge.hiddenTalentLevelUps.Contains(talentLevelUpId))
		{
			knowledge.hiddenTalentLevelUps.Add(talentLevelUpId);
		}
	}

	public static void MigrateDelayedSpawnWgoUniqueIds(SaveFixContext ctx)
	{
		List<SpawnDelayedObject> list = (ctx?.GameSave)?.wgoDelayedSpawnSystemData?.spawnDelayedObjects;
		if (list == null)
		{
			return;
		}
		int num = 0;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			SpawnDelayedObject spawnDelayedObject = list[num2];
			if (spawnDelayedObject == null)
			{
				list.RemoveAt(num2);
			}
			else
			{
				if (SGuid.IsNullOrEmpty(spawnDelayedObject.wgoUniqueId) && spawnDelayedObject.wgoData != null)
				{
					spawnDelayedObject.wgoUniqueId = SGuid.Empty;
					spawnDelayedObject.wgoUniqueId.SetGuid(spawnDelayedObject.wgoData.UniqueId);
					num++;
				}
				if (SGuid.IsNullOrEmpty(spawnDelayedObject.wgoUniqueId) || !ctx.HasWgo(spawnDelayedObject.wgoUniqueId))
				{
					ctx.Log($"Dropped delayed spawn, WGO [{spawnDelayedObject.wgoUniqueId}] not found in save");
					list.RemoveAt(num2);
				}
				else
				{
					spawnDelayedObject.wgoData = null;
				}
			}
		}
		if (num > 0)
		{
			ctx.Log($"Migrated {num} delayed spawn SGuid(s)");
		}
	}

	public static void RemoveDuplicateFloraAtSameCoordinates(SaveFixContext ctx)
	{
		if (ctx?.GameSave == null)
		{
			return;
		}
		if (GameBalance.Me == null)
		{
			ctx.LogError("RemoveDuplicateFloraAtSameCoordinates: GameBalance is not loaded");
			return;
		}
		List<GameSceneData> list = ctx.GameSave.worldData?.gameSceneDataList;
		if (list == null)
		{
			return;
		}
		int num = 0;
		List<WgoData> list2 = new List<WgoData>();
		for (int i = 0; i < list.Count; i++)
		{
			List<WgoData> list3 = list[i]?.wgoDataList;
			if (list3 == null)
			{
				continue;
			}
			list2.Clear();
			for (int j = 0; j < list3.Count; j++)
			{
				WgoData wgoData = list3[j];
				if (TryGetFloraKind(wgoData, out var _))
				{
					list2.Add(wgoData);
				}
			}
			if (list2.Count >= 2)
			{
				num += RemoveDuplicateFloraClusters(ctx, list2);
			}
		}
		if (num > 0)
		{
			ctx.Log($"Removed {num} stacked flora WGO(s) at duplicate coordinates");
		}
	}

	private static int RemoveDuplicateFloraClusters(SaveFixContext ctx, List<WgoData> flora)
	{
		int num = 0;
		bool[] array = new bool[flora.Count];
		List<WgoData> list = new List<WgoData>();
		for (int i = 0; i < flora.Count; i++)
		{
			if (array[i])
			{
				continue;
			}
			list.Clear();
			list.Add(flora[i]);
			array[i] = true;
			for (int j = 0; j < list.Count; j++)
			{
				for (int k = i + 1; k < flora.Count; k++)
				{
					if (!array[k] && !(DistanceXzSq(list[j].Position, flora[k].Position) > 0.0001f))
					{
						array[k] = true;
						list.Add(flora[k]);
					}
				}
			}
			if (list.Count < 2)
			{
				continue;
			}
			WgoData wgoData = PickFloraKeeper(list);
			if (wgoData == null)
			{
				continue;
			}
			for (int l = 0; l < list.Count; l++)
			{
				WgoData wgoData2 = list[l];
				if (wgoData2 != wgoData && RemoveFloraWgo(ctx, wgoData2))
				{
					num++;
				}
			}
		}
		return num;
	}

	private static WgoData PickFloraKeeper(List<WgoData> cluster)
	{
		WgoData wgoData = null;
		WgoData wgoData2 = null;
		WgoData wgoData3 = null;
		WgoData wgoData4 = null;
		WgoData wgoData5 = null;
		for (int i = 0; i < cluster.Count; i++)
		{
			WgoData wgoData6 = cluster[i];
			if (!TryGetFloraKind(wgoData6, out var kind))
			{
				continue;
			}
			switch (kind)
			{
			case FloraKind.Spawner:
				if (wgoData == null)
				{
					wgoData = wgoData6;
				}
				break;
			case FloraKind.Stump:
				if (wgoData2 == null)
				{
					wgoData2 = wgoData6;
				}
				break;
			case FloraKind.Tree:
				if (wgoData3 == null)
				{
					wgoData3 = wgoData6;
				}
				if (wgoData4 == null && !IsOnetimeId(wgoData6.id))
				{
					wgoData4 = wgoData6;
				}
				break;
			case FloraKind.Bush:
				if (wgoData5 == null)
				{
					wgoData5 = wgoData6;
				}
				break;
			}
		}
		if (wgoData != null)
		{
			return wgoData;
		}
		if (wgoData2 != null)
		{
			return wgoData2;
		}
		if (wgoData4 != null)
		{
			return wgoData4;
		}
		if (wgoData3 != null)
		{
			return wgoData3;
		}
		return wgoData5;
	}

	private static bool RemoveFloraWgo(SaveFixContext ctx, WgoData wgo)
	{
		if (wgo == null || SGuid.IsNullOrEmpty(wgo.UniqueId))
		{
			return false;
		}
		SGuid uniqueId = wgo.UniqueId;
		string id = wgo.id;
		if (!ctx.RemoveWgoData(uniqueId))
		{
			ctx.LogWarning($"RemoveDuplicateFloraAtSameCoordinates: failed to remove [{id}] [{uniqueId}]");
			return false;
		}
		RemoveDelayedSpawnReferences(ctx, uniqueId);
		RemoveDelayedEventReferences(ctx, uniqueId);
		RemoveZombieOnSceneReferences(ctx, uniqueId);
		ctx.WarnAboutDanglingReferences(uniqueId, id);
		return true;
	}

	private static bool TryGetFloraKind(WgoData wgo, out FloraKind kind)
	{
		kind = FloraKind.Spawner;
		if (wgo == null || string.IsNullOrEmpty(wgo.id))
		{
			return false;
		}
		if (IsInWgoGroup(wgo, "spawner"))
		{
			kind = FloraKind.Spawner;
			return true;
		}
		if (IsInWgoGroup(wgo, "stumps"))
		{
			kind = FloraKind.Stump;
			return true;
		}
		if (IsInWgoGroup(wgo, "trees"))
		{
			kind = FloraKind.Tree;
			return true;
		}
		if (IsInWgoGroup(wgo, "bushes") || IsInWgoGroup(wgo, "collectable_bushes"))
		{
			kind = FloraKind.Bush;
			return true;
		}
		return false;
	}

	private static bool IsInWgoGroup(WgoData wgo, string wgoGroup)
	{
		if (GameBalance.Me.HasWgoIdByGroup(wgoGroup, wgo.id))
		{
			return true;
		}
		if (wgo.Definition != null)
		{
			return wgo.Definition.wgoGroup == wgoGroup;
		}
		return false;
	}

	private static bool IsOnetimeId(string wgoId)
	{
		if (!string.IsNullOrEmpty(wgoId))
		{
			return wgoId.EndsWith("_onetime", StringComparison.Ordinal);
		}
		return false;
	}

	private static float DistanceXzSq(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return num * num + num2 * num2;
	}

	private static void RemoveDelayedSpawnReferences(SaveFixContext ctx, SGuid uniqueId)
	{
		List<SpawnDelayedObject> list = ctx.GameSave.wgoDelayedSpawnSystemData?.spawnDelayedObjects;
		if (list == null)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			SpawnDelayedObject spawnDelayedObject = list[num];
			if (spawnDelayedObject == null)
			{
				list.RemoveAt(num);
			}
			else
			{
				SGuid sGuid = spawnDelayedObject.wgoUniqueId;
				if (SGuid.IsNullOrEmpty(sGuid) && spawnDelayedObject.wgoData != null)
				{
					sGuid = spawnDelayedObject.wgoData.UniqueId;
				}
				if (sGuid == uniqueId)
				{
					list.RemoveAt(num);
				}
			}
		}
	}

	private static void RemoveDelayedEventReferences(SaveFixContext ctx, SGuid uniqueId)
	{
		List<SGuid> list = ctx.GameSave.wgoDelayedEventSystemData?.wgoUniqueIds;
		if (list == null)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == uniqueId)
			{
				list.RemoveAt(num);
			}
		}
	}

	private static void RemoveZombieOnSceneReferences(SaveFixContext ctx, SGuid uniqueId)
	{
		List<SGuid> list = ctx.GameSave.zombieSystemData?.zombieOnSceneWgoIds;
		if (list == null)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == uniqueId)
			{
				list.RemoveAt(num);
			}
		}
	}

	public static void ApplyActivePerkSetResOnAdd(SaveFixContext ctx, GameSave save, string[] perkIds)
	{
		PerkSystemData perkSystemData = save.perkSystemData;
		if (perkSystemData?.activePerks == null || perkIds == null)
		{
			return;
		}
		foreach (string text in perkIds)
		{
			if (perkSystemData.HasPerk(text))
			{
				PerkDef dataOrNull = GameBalance.Me.GetDataOrNull<PerkDef>(text);
				if (dataOrNull == null)
				{
					ctx.LogError("Missing PerkDef [" + text + "]");
				}
				else if (!(dataOrNull.setGameResOnAdd == null) && !dataOrNull.setGameResOnAdd.IsEmpty())
				{
					save.playerData.SetRes(dataOrNull.setGameResOnAdd);
					ctx.Log("Applied set_res_on_add for active perk [" + text + "]");
				}
			}
		}
	}

	public static void DespawnSewrenaFightingLevelsForCompletedQuests(SaveFixContext ctx)
	{
		(string, string)[] array = new(string, string)[7]
		{
			("59_town_sewrena_AR1", "fight_AR1_1"),
			("59_town_sewrena_AR2", "fight_AR2_1"),
			("59_town_sewrena_AR4", "fight_AR4_1"),
			("59_town_sewrena_AR5", "fight_AR5_1"),
			("59_town_sewrena_AR6", "fight_AR6_1"),
			("59_town_sewrena_AR7", "fight_AR7_1"),
			("59_town_sewrena_AR10", "fight_AR10_1")
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (questId, fightingLevelId) = array[i];
			DespawnFightingLevelIfQuestCompleted(ctx, questId, fightingLevelId);
		}
	}

	public static void DespawnFightingLevelIfQuestCompleted(SaveFixContext ctx, string questId, string fightingLevelId)
	{
		if (ctx?.GameSave?.questSystemData != null && !string.IsNullOrEmpty(questId) && !string.IsNullOrEmpty(fightingLevelId) && ctx.GameSave.questSystemData.IsQuestInStatus(questId, QuestStatus.Completed) && TryRemoveFightingLevelFromSave(ctx, fightingLevelId))
		{
			ctx.Log("Despawned fighting level [" + fightingLevelId + "] because quest [" + questId + "] is completed");
		}
	}

	private static bool TryRemoveFightingLevelFromSave(SaveFixContext ctx, string fightingLevelId)
	{
		GameSceneData gameSceneData = FindSceneWithFightingLevel(ctx, fightingLevelId);
		if (gameSceneData == null)
		{
			return false;
		}
		gameSceneData.RemoveFightingLevelData(fightingLevelId);
		TryUnloadFightingLevelContent(ctx, gameSceneData, fightingLevelId);
		return true;
	}

	private static void TryUnloadFightingLevelContent(SaveFixContext ctx, GameSceneData ownerScene, string fightingLevelId)
	{
		WorldData worldData = ctx.GameSave?.worldData;
		if (worldData != null && ownerScene != null)
		{
			GameSceneConfig gameSceneConfig = MainGame.Instance?.gameSceneConfigs?.Find((GameSceneConfig c) => c != null && c.name == ownerScene.id);
			if (!(gameSceneConfig == null) && gameSceneConfig.IsSceneContentDataLoaded(fightingLevelId))
			{
				worldData.UnloadContentData(gameSceneConfig, ownerScene, fightingLevelId);
			}
		}
	}

	private static GameSceneData FindSceneWithFightingLevel(SaveFixContext ctx, string fightingLevelId)
	{
		List<GameSceneData> list = ctx.GameSave.worldData?.gameSceneDataList;
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			GameSceneData gameSceneData = list[i];
			if (gameSceneData?.fightingLevels != null && gameSceneData.fightingLevels.Exists((FightingLevelData level) => level != null && level.id == fightingLevelId))
			{
				return gameSceneData;
			}
		}
		return null;
	}
}
