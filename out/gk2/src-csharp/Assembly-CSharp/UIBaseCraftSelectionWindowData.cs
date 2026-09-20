using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIBaseCraftSelectionWindowData : LazyWidgetDataBase
{
	public CraftComponent.DelCraftAddedToQueue onCraftAddedToQueue;

	public CraftComponent.DelCraftRemovedFromQueue onCraftRemovedFromQueue;

	private bool subscribedToDataChanges;

	public Action OnCraftToQueueAdded { get; private set; }

	public Action OnCraftStarted { get; private set; }

	public Action OnPressPlusQueue { get; private set; }

	public Action OnPressMinusQueue { get; private set; }

	public CraftComponent CraftComponent { get; private set; }

	public CraftDef CraftDefinition { get; private set; }

	public int CraftsCount { get; protected set; }

	public List<UICraftItemCellData> CraftItemCellsData { get; private set; }

	public List<UICraftRequirementWidgetData> CraftRequirementWidgetData { get; private set; }

	public CraftParamsData ParamsData { get; private set; }

	public WgoData WgoData { get; private set; }

	public IWorker DisplayableWorker { get; private set; }

	public bool CanStartCraft { get; private set; }

	public List<NeedItemData> CurrentNeedItems { get; private set; }

	public List<CraftElementBase> CraftQueue { get; private set; }

	protected virtual int MinCraftsCount => 1;

	protected virtual int MaxCraftsCount => 999;

	public UIBaseCraftSelectionWindowData(WgoData wgoData, CraftDef craftDef, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onAddToQueue, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onStartCraft)
	{
		UIBaseCraftSelectionWindowData uIBaseCraftSelectionWindowData = this;
		WgoData = wgoData;
		DisplayableWorker = wgoData.Worker ?? MainGame.PlayerController;
		CraftComponent = wgoData.CraftComponent;
		CraftsCount = 1;
		CraftDefinition = craftDef;
		CraftQueue = wgoData.CraftComponent.CraftElementsQueue;
		OnCraftToQueueAdded = delegate
		{
			uIBaseCraftSelectionWindowData.OnPressAction(onAddToQueue);
		};
		OnCraftStarted = delegate
		{
			uIBaseCraftSelectionWindowData.OnPressAction(onStartCraft);
		};
		OnPressPlusQueue = OnPlus;
		OnPressMinusQueue = OnMinus;
		UpdateRequirements();
		FillCraftItemCellsData();
		UpdateCraftParamsDataAndCanStartStatus();
	}

	public void SubscribeToDataChanges()
	{
		if (!subscribedToDataChanges)
		{
			subscribedToDataChanges = true;
			CraftComponent.OnCraftAddedToQueue += onCraftAddedToQueue;
			CraftComponent.OnCraftRemovedFromQueue += onCraftRemovedFromQueue;
		}
	}

	public void UnsubscribeFromDataChanges()
	{
		if (subscribedToDataChanges)
		{
			subscribedToDataChanges = false;
			CraftComponent.OnCraftAddedToQueue -= onCraftAddedToQueue;
			CraftComponent.OnCraftRemovedFromQueue -= onCraftRemovedFromQueue;
		}
	}

	public void SetCraftCount(int value)
	{
		CraftsCount = value;
	}

	public void AddCraftsCount(int delta)
	{
		if (delta != 0)
		{
			int num = Math.Clamp(CraftsCount + delta, MinCraftsCount, MaxCraftsCount);
			if (num != CraftsCount)
			{
				CraftsCount = num;
				UpdateCanStartStatus();
				UpdateRequirements();
			}
		}
	}

	private void FillCraftItemCellsData(bool drawRunes = false)
	{
		if (CraftItemCellsData != null)
		{
			CraftItemCellsData.Clear();
		}
		else
		{
			CraftItemCellsData = new List<UICraftItemCellData>();
		}
		MultiInventory craftableMultiInventory = CraftComponent.CraftableObject.GetCraftableMultiInventory();
		foreach (NeedItemData needItem in CraftDefinition.needItems)
		{
			if (!CraftDefinition.id.StartsWith("mix") || needItem.IsGroup || needItem.ItemDef == null || !needItem.ItemDef.isFuel)
			{
				CraftItemCellsData.Add(new UICraftItemCellData(needItem, craftableMultiInventory, UpdateCraftParamsDataAndCanStartStatus, 0f, drawRunes, WgoData));
			}
		}
		CraftDef craftDefinition = CraftDefinition;
		if (craftDefinition != null && craftDefinition.hasDurabilityUseItem)
		{
			CraftItemCellsData.Add(new UICraftItemCellData(craftDefinition.durabilityUseItem, craftableMultiInventory, UpdateCraftParamsDataAndCanStartStatus, craftDefinition.needItemsDurabilityUse, drawRunes: false, WgoData));
		}
	}

	private void OnPressAction(Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onPress)
	{
		onPress?.Invoke(CraftDefinition, CurrentNeedItems, ParamsData, CraftsCount);
	}

	private void OnPlus()
	{
		AddCraftsCount(1);
	}

	protected virtual void OnMinus()
	{
		AddCraftsCount(-1);
	}

	protected void UpdateRequirements()
	{
		if (CraftRequirementWidgetData == null)
		{
			CraftRequirementWidgetData = new List<UICraftRequirementWidgetData>();
		}
		else
		{
			CraftRequirementWidgetData.Clear();
		}
		List<PerkData> linkedActivePerks = GetLinkedActivePerks();
		Item toolForWorkOnCraft = DisplayableWorker.GetToolForWorkOnCraft(WgoData, CraftDefinition);
		if (CraftDefinition.energyPerTick.HasExpression)
		{
			float num = CraftDefinition.energyPerTick.EvaluateFloat();
			foreach (PerkData item5 in linkedActivePerks)
			{
				if (!item5.Definition.energyAdd.EqualsTo(0f))
				{
					num += item5.Definition.energyAdd;
				}
			}
			if (!toolForWorkOnCraft.IsEmpty && !toolForWorkOnCraft.Definition.GetGameResOnUse("energy").EqualsTo(0f))
			{
				num -= toolForWorkOnCraft.Definition.GetGameResOnUse("energy");
			}
			string iconId = "energy_1";
			if (num >= ConstDef.Get("energy_craft_border_2").FloatValue)
			{
				iconId = "energy_3";
			}
			else if (num >= ConstDef.Get("energy_craft_border_1").FloatValue)
			{
				iconId = "energy_2";
			}
			UICraftRequirementWidgetData item = new UICraftRequirementWidgetData("energy", iconId, 0f, linkedActivePerks, CraftDefinition, toolForWorkOnCraft, isRequirement: false);
			CraftRequirementWidgetData.Add(item);
		}
		if (CraftDefinition.insanityPerTick.HasExpression)
		{
			float num = CraftDefinition.insanityPerTick.EvaluateFloat();
			foreach (PerkData item6 in linkedActivePerks)
			{
				if (!item6.Definition.insanityAdd.EqualsTo(0f))
				{
					num += item6.Definition.insanityAdd;
				}
			}
			if (!toolForWorkOnCraft.IsEmpty && !toolForWorkOnCraft.Definition.GetGameResOnUse("insanity").EqualsTo(0f))
			{
				num -= toolForWorkOnCraft.Definition.GetGameResOnUse("insanity");
			}
			string iconId2 = "insanity_1";
			if (num >= ConstDef.Get("insanity_craft_border_2").FloatValue)
			{
				iconId2 = "insanity_3";
			}
			else if (num >= ConstDef.Get("insanity_craft_border_1").FloatValue)
			{
				iconId2 = "insanity_2";
			}
			UICraftRequirementWidgetData item2 = new UICraftRequirementWidgetData("insanity", iconId2, 0f, linkedActivePerks, CraftDefinition, toolForWorkOnCraft, isRequirement: false);
			CraftRequirementWidgetData.Add(item2);
		}
		if (CraftDefinition.insanityLock.HasExpression)
		{
			float num = CraftDefinition.insanityLock.EvaluateFloat();
			UICraftRequirementWidgetData item3 = new UICraftRequirementWidgetData("insanity_lock", "icon_sanity_lock", num, linkedActivePerks, CraftDefinition, toolForWorkOnCraft, isRequirement: false);
			CraftRequirementWidgetData.Add(item3);
		}
		if (!CraftDefinition.id.StartsWith("mix"))
		{
			return;
		}
		MultiInventory craftableMultiInventory = WgoData.GetCraftableMultiInventory(excludeWorkerInventory: true);
		foreach (NeedItemData needItem in CraftDefinition.needItems)
		{
			if (!needItem.IsGroup && needItem.ItemDef != null && needItem.ItemDef.isFuel)
			{
				UICraftRequirementWidgetData item4 = new UICraftRequirementWidgetData(needItem.ItemDef.id, needItem.ItemDef.id, needItem.GetCount(WgoData), new List<PerkData>(), CraftDefinition, toolForWorkOnCraft, isRequirement: true, craftableMultiInventory.GetTotalCount(needItem.ItemDef.id) >= needItem.GetCount(WgoData));
				CraftRequirementWidgetData.Add(item4);
			}
		}
	}

	private List<PerkData> GetLinkedActivePerks()
	{
		List<PerkData> list = new List<PerkData>();
		PerkSystemData perkSystemData = MainGame.Instance.GameSave.perkSystemData;
		foreach (string linkedPerk in CraftDefinition.linkedPerks)
		{
			int num = perkSystemData.activePerks.FindIndex((PerkData x) => x.id == linkedPerk);
			if (num != -1)
			{
				list.Add(perkSystemData.activePerks[num]);
			}
		}
		return list;
	}

	private void UpdateCraftParamsDataAndCanStartStatus()
	{
		CurrentNeedItems = GetCurrentNeedItems();
		if (ParamsData == null)
		{
			ParamsData = new CraftParamsData(CraftDefinition.id, WgoData);
		}
		ParamsData.RecalculateParams(CurrentNeedItems, DisplayableWorker);
		UpdateCanStartStatus();
	}

	protected void UpdateCanStartStatus()
	{
		if (CraftDefinition.isFuelCraft)
		{
			MultiInventory craftableMultiInventory = WgoData.GetCraftableMultiInventory();
			{
				foreach (UICraftItemCellData craftItemCellsDatum in CraftItemCellsData)
				{
					if (craftableMultiInventory.GetTotalCount(craftItemCellsDatum.currentItem.Id) < CraftsCount * craftItemCellsDatum.currentItem.GetCount(WgoData))
					{
						CanStartCraft = false;
						break;
					}
					CanStartCraft = CraftDefinition.CanActuallyStartInstantCraft(CurrentNeedItems, WgoData);
				}
				return;
			}
		}
		CraftElement craftElement = new CraftElement(CraftDefinition.id, 1, CurrentNeedItems, ParamsData);
		CanStartCraft = WgoData.CraftComponent.GetStartCraftStatus(craftElement) == CraftStatus.OK;
	}

	private List<NeedItemData> GetCurrentNeedItems()
	{
		List<NeedItemData> list = new List<NeedItemData>();
		foreach (UICraftItemCellData craftItemCellsDatum in CraftItemCellsData)
		{
			list.Add(craftItemCellsDatum.currentItem);
		}
		return list;
	}
}
