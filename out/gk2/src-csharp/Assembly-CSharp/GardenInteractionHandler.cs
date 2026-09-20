using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class GardenInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		Item interactingItem = MainGame.PlayerData.interactingItem;
		CraftComponent craftComponent = assignedWgo.Data.CraftComponent;
		WgoData data = assignedWgo.Data;
		if (!craftComponent.IsStarted && interactingItem != null && (interactingItem.IsSeed || interactingItem.IsFertilizer))
		{
			CraftDefBase craftDefBase = TryFindGardenCraft(interactingItem, assignedWgo.Data);
			if (craftDefBase == null)
			{
				return false;
			}
			bool flag = false;
			if (data.Worker == null)
			{
				data.TrySetWorker(interactor);
				flag = true;
			}
			if (interactor.GetMasteryLevelForTalentBranch("talent_green") <= 0)
			{
				Bubble.Talk(new PhraseData(isPlayer: true, null, "gardening_no_mastery", null, null, SpeechBubbleType.Think));
				return false;
			}
			craftComponent.Clear();
			if (interactingItem.IsSeed && TryApplySeed(interactingItem, craftDefBase, data))
			{
				ClearWorkerAndUpdateVisuals(flag, data);
				return true;
			}
			if (interactingItem.IsFertilizer && HasFreeFertilizerPerkSlot(data) && TryApplyFertilizer(interactingItem, craftDefBase, data))
			{
				ClearWorkerAndUpdateVisuals(flag, data);
				TryAssignPerkSlotForNewestAddedPerk();
				return true;
			}
			if (flag)
			{
				data.ClearWorker();
			}
			Debug.Log("Can not start planting craft");
			return false;
		}
		if (assignedWgo.Data.CraftComponent.CurrentCraftElement == null || GameBalance.Me.gardenGrowingCrafts.TryGetValue(assignedWgo.Data.CraftComponent.CurrentCraftElement.Def.id, out var _))
		{
			UIGardenBedWindow window = LazyUI.GetWindow<UIGardenBedWindow>();
			UIGardenBedWindowData data2 = new UIGardenBedWindowData(data);
			window.Open(data2);
		}
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		Item interactingItem = MainGame.PlayerData.interactingItem;
		CraftComponent craftComponent = assignedWgo.Data.CraftComponent;
		WgoData data = assignedWgo.Data;
		if (!craftComponent.IsStarted && interactingItem != null && (interactingItem.IsSeed || interactingItem.IsFertilizer))
		{
			if (TryFindGardenCraft(interactingItem, data) != null)
			{
				return true;
			}
			return false;
		}
		if (assignedWgo.Data.CraftComponent.CurrentCraftElement == null || GameBalance.Me.gardenGrowingCrafts.TryGetValue(assignedWgo.Data.CraftComponent.CurrentCraftElement.Def.id, out var _))
		{
			return true;
		}
		if ((craftComponent.IsAutoCraftable && craftComponent.IsStarted) || (interactingItem == null && craftComponent.CurrentCraftElement == null))
		{
			return true;
		}
		if (!craftComponent.IsAutoCraftable && craftComponent.IsStarted)
		{
			return true;
		}
		return false;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		CraftComponent craftComponent = assignedWgo.Data.CraftComponent;
		InteractionInfos interactionInfos2 = new InteractionInfos();
		Item interactingItem = MainGame.PlayerData.interactingItem;
		string hint;
		if (!craftComponent.IsAutoCraftable && craftComponent.IsStarted)
		{
			interactionInfos2.Add(GetInteractionInfoByUsingTool());
		}
		else if ((craftComponent.IsAutoCraftable && craftComponent.IsStarted) || (interactingItem == null && craftComponent.CurrentCraftElement == null))
		{
			string text = LocalizeHintWithActionIcon("action_inspect", GameKey.Interaction);
			interactionInfos2.Add(new InteractionInfo(text));
		}
		else if (craftComponent.CurrentCraftElement == null)
		{
			if (interactingItem != null && interactingItem.IsSeed)
			{
				string text2 = LocalizeHintWithActionIcon("hint_plant", GameKey.Interaction);
				interactionInfos2.Add(new InteractionInfo(text2));
			}
			else if (interactingItem != null && interactingItem.IsFertilizer && HasFreeFertilizerPerkSlot(assignedWgo.Data))
			{
				string text3 = LocalizeHintWithActionIcon("hint_fertilize", GameKey.Interaction);
				interactionInfos2.Add(new InteractionInfo(text3));
			}
		}
		else if (TryGetCustomInteractionStr(out hint))
		{
			interactionInfos2.Add(new InteractionInfo(hint));
		}
		return interactionInfos2;
	}

	public static bool TryApplyFertilizer(Item fertilizer, CraftDefBase cropCraft, WgoData wgoData)
	{
		List<NeedItemData> list = FormNeedItems(cropCraft, fertilizer);
		CraftParamsData craftParamsData = new CraftParamsData(cropCraft.id, wgoData, CraftParamsData.CraftParamsType.Common, fertilizer.Definition.talentValue);
		IWorker worker;
		if (wgoData.Worker != null)
		{
			worker = wgoData.Worker;
		}
		else
		{
			IWorker playerController = MainGame.PlayerController;
			worker = playerController;
		}
		craftParamsData.RecalculateParams(list, worker);
		CraftElement craftElement = new CraftElement(cropCraft.id, 1, list, craftParamsData);
		if (wgoData.CraftComponent.GetStartCraftStatus(craftElement) == CraftStatus.OK)
		{
			wgoData.CraftComponent.ProcessInstantCraft(wgoData, craftElement);
			return true;
		}
		return false;
	}

	public static bool TryApplySeed(Item seed, CraftDefBase cropCraft, WgoData wgoData)
	{
		List<NeedItemData> list = FormNeedItems(cropCraft, seed);
		CraftParamsData craftParamsData = new CraftParamsData(cropCraft.id, wgoData, CraftParamsData.CraftParamsType.GardenPlanting, seed.Definition.talentValue);
		IWorker worker;
		if (wgoData.Worker != null)
		{
			worker = wgoData.Worker;
		}
		else
		{
			IWorker playerController = MainGame.PlayerController;
			worker = playerController;
		}
		craftParamsData.RecalculateParams(list, worker);
		CraftElement craftElement = new CraftElement(cropCraft.id, 1, list, craftParamsData);
		if (wgoData.CraftComponent.GetStartCraftStatus(craftElement) == CraftStatus.OK)
		{
			wgoData.CraftComponent.TryStartCraft(craftElement);
			wgoData.SetGameRes("seed_mastery_lock", seed.Definition.talentValue);
			return true;
		}
		return false;
	}

	public static CraftDefBase TryFindGardenCraft(Item gardenItem, WgoData wgoData, bool logWarning = true)
	{
		if (!GameBalance.Me.gardenCraftsPerItemCache.TryGetValue(gardenItem.Definition, out var value))
		{
			if (logWarning)
			{
				Debug.LogWarning("Can not find garden crafts for item [" + gardenItem.id + "]");
			}
			return null;
		}
		foreach (CraftDef item in value)
		{
			if (item.craftsIn.Contains("garden_empty") && wgoData.id == "garden_empty")
			{
				return item;
			}
			if (item.craftsIn.Contains("vineyard_empty") && wgoData.id == "vineyard_empty")
			{
				return item;
			}
		}
		if (logWarning)
		{
			Debug.LogWarning("Can not find garden craft for item [" + gardenItem.id + "]");
		}
		return null;
	}

	public static List<NeedItemData> FormNeedItems(CraftDefBase craftDef, Item seed)
	{
		return new List<NeedItemData>
		{
			new NeedItemData(seed.id, craftDef.needItems[0].count)
		};
	}

	public static bool HasFreeFertilizerPerkSlot(WgoData wgoData)
	{
		int num = 0;
		foreach (PerkData activePerk in wgoData.ActivePerks)
		{
			if (activePerk.Definition.IsFertilizerPerk)
			{
				num++;
			}
		}
		return num < MainGame.PlayerData.GetResInt("g_garden_fertilizer_slots");
	}

	public static bool IsSeedableSeed(string wgoId, Item item)
	{
		if (wgoId.StartsWith("garden_"))
		{
			if (item.Definition.isSeed)
			{
				return !item.Definition.itemGroupIds.Contains("vineyard_seed");
			}
			return false;
		}
		if (wgoId.StartsWith("vineyard_"))
		{
			if (item.Definition.isSeed)
			{
				return item.Definition.itemGroupIds.Contains("vineyard_seed");
			}
			return false;
		}
		return false;
	}

	private void TryAssignPerkSlotForNewestAddedPerk()
	{
		List<int> list = new List<int> { 1, 2, 3 };
		PerkData perkData = null;
		foreach (PerkData activePerk in assignedWgo.Data.ActivePerks)
		{
			if (activePerk.Definition.IsFertilizerPerk)
			{
				int gameResInt = assignedWgo.Data.GetGameResInt("perk_fertilize_" + activePerk.Definition.id);
				if (gameResInt > 0)
				{
					list.Remove(gameResInt);
				}
				else
				{
					perkData = activePerk;
				}
			}
		}
		if (perkData != null && list.Count > 0)
		{
			assignedWgo.Data.SetGameRes("perk_fertilize_" + perkData.Definition.id, list[0]);
			return;
		}
		if (perkData == null)
		{
			Debug.LogError("Gardening: Can not assign perk slot for newest added perk: No new perk");
		}
		if (list.Count > 0)
		{
			Debug.LogError("Gardening: Can not assign perk slot for newest added perk: No free slots");
		}
	}

	private void ClearWorkerAndUpdateVisuals(bool wasPlayerSetAsWorker, WgoData wgoData)
	{
		if (wasPlayerSetAsWorker)
		{
			wgoData.ClearWorker();
		}
		MainGame.PlayerData.UpdateInteractingItem();
		PlayPlantingFeedback();
	}

	public static void PlayPlantingFeedback()
	{
		MainGame.PlayerController.View.PlayerAnimation.SetState(AnimationState.Planting);
		WorldFX.Spawn(MainGame.PlayerData.position.Value, "planting");
		LazyAudio.Play("planting");
	}

	private bool HasInteraction(CustomInteraction customInteraction)
	{
		if (customInteraction.IsInteractable(assignedWgo.Data))
		{
			return true;
		}
		return false;
	}

	public static bool TryPlacePlantOrder(WgoData wgoData)
	{
		using (IEnumerator<SGuid> enumerator = wgoData.AttachedWorkbenchExtensions.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				SGuid current = enumerator.Current;
				WgoData wgoData2 = MainGame.Instance.GameSave.WorldData.GetWgoData(current);
				if (!wgoData2.id.StartsWith("garden_tablet_"))
				{
					return false;
				}
				string text = wgoData2.id.Split("garden_tablet_")[1] + "_seed";
				string text2 = text;
				if (GameBalance.Me.starGroupItemsCache.TryGetValue(text, out var value))
				{
					text2 = value[0].id;
				}
				CraftDefBase craftDefBase = TryFindGardenCraft(new Item(text2), wgoData);
				if (craftDefBase == null)
				{
					return false;
				}
				if (wgoData.WorldZoneData.FindOrdersByTarget(wgoData.UniqueId, typeof(PlantOrder)).Count > 0)
				{
					return false;
				}
				Debug.Log("Placed PLANT garden order for seed [" + text + "]");
				wgoData.WorldZoneData.PlaceNewOrder(new PlantOrder(wgoData.UniqueId, new Item(text, craftDefBase.needItems[0].GetCount(wgoData)), text2 != text));
				return true;
			}
		}
		return false;
	}

	public static bool TryPlaceGatherOrder(WgoData wgoData)
	{
		using (IEnumerator<SGuid> enumerator = wgoData.AttachedWorkbenchExtensions.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				SGuid current = enumerator.Current;
				if (!MainGame.Instance.GameSave.WorldData.GetWgoData(current).id.StartsWith("garden_tablet_"))
				{
					return false;
				}
				if (wgoData.WorldZoneData.FindOrdersByTarget(wgoData.UniqueId, typeof(GatherOrder)).Count > 0)
				{
					return false;
				}
				Debug.Log("Placed GATHER garden order");
				wgoData.WorldZoneData.PlaceNewOrder(new GatherOrder(wgoData.UniqueId, Item.Empty));
				return true;
			}
		}
		return false;
	}
}
