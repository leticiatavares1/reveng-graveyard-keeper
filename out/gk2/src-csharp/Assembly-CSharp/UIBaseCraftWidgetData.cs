using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIBaseCraftWidgetData : LazyWidgetDataBase
{
	private bool subscribedToDataChages;

	public Action OnPress { get; private set; }

	public Action OnOver { get; private set; }

	public Action OnOut { get; private set; }

	public Action OnPressPlusQueue { get; private set; }

	public Action OnPressMinusQueue { get; private set; }

	public CraftComponent CraftComponent { get; private set; }

	public CraftDef CraftDefinition { get; private set; }

	public int CraftsCount { get; private set; }

	public List<UICraftItemCellData> CraftItemCellsData { get; private set; }

	public List<UICraftRequirementWidgetData> CraftRequirementWidgetData { get; private set; }

	public UICraftRequirementWidgetData FuelRequirementWidgetData { get; private set; }

	public CraftParamsData ParamsData { get; private set; }

	public WgoData WgoData { get; set; }

	public IWorker DisplayableWorker { get; set; }

	public UIBaseCraftWidgetData(WgoData wgoData, CraftDef craftDef, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onPress, Action onOver, Action onOut)
	{
		UIBaseCraftWidgetData uIBaseCraftWidgetData = this;
		WgoData = wgoData;
		DisplayableWorker = wgoData.Worker ?? MainGame.PlayerController;
		CraftComponent = wgoData.CraftComponent;
		CraftsCount = 1;
		CraftDefinition = craftDef;
		OnPress = delegate
		{
			uIBaseCraftWidgetData.OnPressAction(onPress);
		};
		OnOver = onOver;
		OnOut = onOut;
		OnPressPlusQueue = OnPlus;
		OnPressMinusQueue = OnMinus;
		UpdateRequirements();
		FillCraftItemCellsData();
		UpdateCraftParamsData();
	}

	public UIBaseCraftWidgetData(WgoData wgoData, AlchemyMixDef mixDef, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onPress, Action onOver, Action onOut)
	{
		UIBaseCraftWidgetData uIBaseCraftWidgetData = this;
		WgoData = wgoData;
		DisplayableWorker = wgoData.Worker ?? MainGame.PlayerController;
		CraftComponent = wgoData.CraftComponent;
		CraftsCount = 1;
		CraftDefinition = mixDef;
		OnPress = delegate
		{
			uIBaseCraftWidgetData.OnPressAction(onPress);
		};
		OnOver = onOver;
		OnOut = onOut;
		OnPressPlusQueue = OnPlus;
		OnPressMinusQueue = OnMinus;
		UpdateRequirements();
		FillCraftItemCellsData(drawRunes: true);
		UpdateCraftParamsData();
	}

	public void UpdateCraftParamsData()
	{
		if (ParamsData == null)
		{
			ParamsData = new CraftParamsData(CraftDefinition.id, WgoData);
		}
		ParamsData.RecalculateParams(GetCurrentNeedItems(), DisplayableWorker);
	}

	public List<NeedItemData> GetCurrentNeedItems()
	{
		List<NeedItemData> list = new List<NeedItemData>();
		foreach (UICraftItemCellData craftItemCellsDatum in CraftItemCellsData)
		{
			list.Add(craftItemCellsDatum.currentItem);
		}
		return list;
	}

	public void SubscribeToDataChanges()
	{
		if (!subscribedToDataChages)
		{
			subscribedToDataChages = true;
		}
	}

	public void UnsubscribeFromDataChanges()
	{
		if (subscribedToDataChages)
		{
			subscribedToDataChages = false;
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
			CraftItemCellsData.Add(new UICraftItemCellData(needItem, craftableMultiInventory, UpdateCraftParamsData, 0f, drawRunes, WgoData));
		}
		CraftDef craftDefinition = CraftDefinition;
		if (craftDefinition != null && craftDefinition.hasDurabilityUseItem)
		{
			CraftItemCellsData.Add(new UICraftItemCellData(craftDefinition.durabilityUseItem, craftableMultiInventory, UpdateCraftParamsData, craftDefinition.needItemsDurabilityUse, drawRunes: false, WgoData));
		}
	}

	private void OnPressAction(Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onPress)
	{
		onPress?.Invoke(CraftDefinition, GetCurrentNeedItems(), ParamsData, CraftsCount);
	}

	private void OnPlus()
	{
		AddCraftsCount(1);
	}

	private void OnMinus()
	{
		AddCraftsCount(-1);
	}

	public void AddCraftsCount(int delta)
	{
		if (delta != 0)
		{
			int num = Math.Clamp(CraftsCount + delta, 1, 999);
			if (num != CraftsCount)
			{
				CraftsCount = num;
				UpdateRequirements();
			}
		}
	}

	private void UpdateRequirements()
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
			foreach (PerkData item4 in linkedActivePerks)
			{
				if (!item4.Definition.energyAdd.EqualsTo(0f))
				{
					num -= item4.Definition.energyAdd;
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
			UICraftRequirementWidgetData item = new UICraftRequirementWidgetData("energy", iconId, num, linkedActivePerks, CraftDefinition, toolForWorkOnCraft, isRequirement: true, PlayerEnergyGameResSystem.GetSystem().IsEnoughValue(num));
			CraftRequirementWidgetData.Add(item);
		}
		if (CraftDefinition.insanityPerTick.HasExpression)
		{
			float num = CraftDefinition.insanityPerTick.EvaluateFloat();
			foreach (PerkData item5 in linkedActivePerks)
			{
				if (!item5.Definition.insanityAdd.EqualsTo(0f))
				{
					num -= item5.Definition.insanityAdd;
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
			UICraftRequirementWidgetData item2 = new UICraftRequirementWidgetData("insanity", iconId2, num, linkedActivePerks, CraftDefinition, toolForWorkOnCraft, isRequirement: true, PlayerInsanityGameResSystem.GetSystem().CanChangeInsanity(num));
			CraftRequirementWidgetData.Add(item2);
		}
		if (CraftDefinition.insanityLock.HasExpression)
		{
			float num = CraftDefinition.insanityLock.EvaluateFloat();
			UICraftRequirementWidgetData item3 = new UICraftRequirementWidgetData("insanity_lock", "icon_sanity_lock", num, linkedActivePerks, CraftDefinition, toolForWorkOnCraft, isRequirement: true, PlayerInsanityGameResSystem.GetSystem().IsEnoughValue(num));
			CraftRequirementWidgetData.Add(item3);
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
}
