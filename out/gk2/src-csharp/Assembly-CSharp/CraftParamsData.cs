using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class CraftParamsData
{
	public enum CraftParamsType
	{
		Common,
		GardenGrowing,
		GardenPlanting
	}

	public enum GardenType
	{
		None,
		Vineyard
	}

	[SerializeField]
	private string craftId;

	[SerializeField]
	private SGuid wgoUniqueId;

	[SerializeField]
	private int itemsCraftStartTicksBonusValue;

	public float totalPerks;

	public float totalTalentsAurasAndPerks;

	public float totalItemsQuality = 1f;

	public float totalTalentsAurasPerksAndItemQuality;

	public float total;

	public GameRes customRes = new GameRes();

	public int customMasteryLock = -1;

	public CraftParamsType craftParamsType;

	public GardenType gardenType;

	public ItemType selectedOrganTypeForChange;

	[NonSerialized]
	private CraftDefBase craftDef;

	[NonSerialized]
	private TalentDef assignedTalentDef;

	[NonSerialized]
	private ItemType requiredToolType;

	[CanBeNull]
	private WgoData WgoData
	{
		get
		{
			if (!(wgoUniqueId == null))
			{
				return MainGame.Instance.GameSave.worldData.GetWgoData(wgoUniqueId);
			}
			return null;
		}
	}

	public CraftDefBase CraftDef => GameBalance.GetCraftDefBase(craftId);

	public TalentDef TalentDef
	{
		get
		{
			if (WgoData == null)
			{
				return null;
			}
			if (assignedTalentDef == null)
			{
				return assignedTalentDef = GameBalance.Me.GetDataOrNull<TalentDef>(WgoData.Definition.talent);
			}
			return assignedTalentDef;
		}
	}

	public int MasteryLock
	{
		get
		{
			if (customMasteryLock > -1)
			{
				return customMasteryLock;
			}
			if (CraftDef == null)
			{
				return 1;
			}
			return CraftDef.talentLock;
		}
	}

	public int MasteryValue
	{
		get
		{
			bool flag = WgoData != null && WgoData.Worker != null && !(WgoData.Worker is UnityEngine.Object);
			int num = 0;
			if (WgoData == null || flag || craftParamsType != CraftParamsType.GardenGrowing)
			{
				num = ((TalentDef == null) ? 1 : (flag ? WgoData.Worker.GetMasteryLevelForTalentBranch(TalentDef.id, CraftDef) : MainGame.PlayerController.GetMasteryLevelForTalentBranch(TalentDef.id, CraftDef)));
			}
			else
			{
				int num2 = ((gardenType != GardenType.Vineyard) ? MainGame.PlayerData.GetResInt("g_garden_farming_base") : MainGame.PlayerData.GetResInt("g_vineyard_farming_base"));
				num = num2;
			}
			return num + GetWgoPerksCraftMasteryBonusValue();
		}
	}

	public ItemType RequiredToolType
	{
		get
		{
			if (requiredToolType == ItemType.None)
			{
				requiredToolType = GetRequiredToolType();
			}
			return requiredToolType;
		}
	}

	public bool HasRequiredTool
	{
		get
		{
			if (WgoData == null)
			{
				return true;
			}
			if (WgoData.Worker == null)
			{
				return MainGame.PlayerController.HasToolForWork(WgoData, CraftDef);
			}
			return WgoData.Worker.HasToolForWork(WgoData, CraftDef);
		}
	}

	public int PerksCraftStartTicksBonusValue
	{
		get
		{
			int num = ((WgoData != null) ? ((WgoData.Worker != null) ? WgoData.Worker.GetPerksCraftStartTicksBonusValue(CraftDef) : MainGame.PlayerController.GetPerksCraftStartTicksBonusValue(CraftDef)) : 0);
			return num + GetWgoPerksCraftStartTicksBonusValue();
		}
	}

	public int PerksCraftAddTotalProgressTicksValue
	{
		get
		{
			int num = ((WgoData != null) ? ((WgoData.Worker != null) ? WgoData.Worker.GetPerksCraftAddTotalProgressTicksValue(CraftDef) : MainGame.PlayerController.GetPerksCraftAddTotalProgressTicksValue(CraftDef)) : 0);
			return num + GetWgoPerksCraftAddTotalProgressTicks();
		}
	}

	public int PerksCraftMasteryBonusValue
	{
		get
		{
			if (WgoData != null)
			{
				return GetWgoPerksCraftMasteryBonusValue();
			}
			return 0;
		}
	}

	public int CraftStartTicks => ((craftParamsType != CraftParamsType.GardenPlanting) ? (itemsCraftStartTicksBonusValue + PerksCraftStartTicksBonusValue) : 0) + ((WgoData != null && craftParamsType == CraftParamsType.GardenGrowing) ? WgoData.GetGameResInt("succeded_cells") : 0);

	public int FailedStartTicks
	{
		get
		{
			if (WgoData == null || craftParamsType != CraftParamsType.GardenGrowing)
			{
				return 0;
			}
			return WgoData.GetGameResInt("failed_cells");
		}
	}

	public CraftParamsData(string craftId, WgoData wgoData, CraftParamsType craftParamsType = CraftParamsType.Common, int customMasteryLock = -1)
	{
		this.craftId = craftId;
		wgoUniqueId = wgoData.UniqueId;
		this.customMasteryLock = customMasteryLock;
		if (craftParamsType == CraftParamsType.Common && GameBalance.Me.gardenGrowingCrafts.TryGetValue(craftId, out var _))
		{
			this.craftParamsType = CraftParamsType.GardenGrowing;
			gardenType = GameBalance.Me.gardenGrowingCraftTypes[craftId];
		}
		else
		{
			this.craftParamsType = craftParamsType;
		}
	}

	public CraftParamsData(string craftId, GameRes customRes)
	{
		this.craftId = craftId;
		this.customRes = customRes;
	}

	public CraftParamsData(CraftParamsData other)
	{
		craftId = other.craftId;
		wgoUniqueId = other.wgoUniqueId;
		itemsCraftStartTicksBonusValue = other.itemsCraftStartTicksBonusValue;
		totalPerks = other.totalPerks;
		totalTalentsAurasAndPerks = other.totalTalentsAurasAndPerks;
		totalItemsQuality = other.totalItemsQuality;
		totalTalentsAurasPerksAndItemQuality = other.totalTalentsAurasPerksAndItemQuality;
		total = other.total;
		customRes = other.customRes;
		customMasteryLock = other.customMasteryLock;
		craftParamsType = other.craftParamsType;
		gardenType = other.gardenType;
		craftDef = other.craftDef;
		assignedTalentDef = other.assignedTalentDef;
		requiredToolType = other.requiredToolType;
	}

	public void RecalculateParams(List<NeedItemData> needItems, IWorker worker)
	{
		if (craftParamsType != CraftParamsType.GardenGrowing)
		{
			RecalculateItemsDependentParams(needItems);
		}
		RecalculateItemsNotDependentParams(worker);
	}

	public void RecalculateParams(List<Item> needItems, IWorker worker)
	{
		if (craftParamsType != CraftParamsType.GardenGrowing)
		{
			RecalculateItemsDependentParams(needItems);
		}
		RecalculateItemsNotDependentParams(worker);
	}

	public bool Equals(CraftParamsData other)
	{
		if (craftId == other.craftId && totalItemsQuality.Equals(other.totalItemsQuality) && totalPerks.Equals(other.totalPerks) && totalTalentsAurasAndPerks.Equals(other.totalTalentsAurasAndPerks) && totalTalentsAurasPerksAndItemQuality.Equals(other.totalTalentsAurasPerksAndItemQuality) && total.Equals(other.total) && itemsCraftStartTicksBonusValue.Equals(other.itemsCraftStartTicksBonusValue))
		{
			return PerksCraftStartTicksBonusValue.Equals(other.PerksCraftStartTicksBonusValue);
		}
		return false;
	}

	private void RecalculateItemsDependentParams(List<NeedItemData> needItems)
	{
		totalItemsQuality = NeedItemData.GetQualitySum(needItems);
		itemsCraftStartTicksBonusValue = NeedItemData.GetCraftStartTicksBonusValue(needItems);
	}

	private void RecalculateItemsDependentParams(List<Item> needItems)
	{
		totalItemsQuality = Item.GetQualityAverage(needItems);
		itemsCraftStartTicksBonusValue = Item.GetCraftStartTicksBonusValue(needItems);
	}

	private void RecalculateItemsNotDependentParams(IWorker worker)
	{
		totalTalentsAurasAndPerks = 0f;
		totalPerks = 0f;
		totalTalentsAurasAndPerks = totalPerks;
		totalTalentsAurasPerksAndItemQuality = totalTalentsAurasAndPerks + totalItemsQuality;
		total = totalTalentsAurasPerksAndItemQuality;
	}

	private ItemType GetRequiredToolType()
	{
		ItemType itemType = ItemType.None;
		if (CraftDef is CraftDef { customItemTypeAction: not ItemType.None } craftDef && !CraftDef.isAuto)
		{
			itemType = craftDef.customItemTypeAction;
		}
		if (itemType == ItemType.None && WgoData != null)
		{
			itemType = WgoData.Definition.toolAction.actionableTool;
		}
		return itemType;
	}

	private int GetWgoPerksCraftAddTotalProgressTicks()
	{
		int num = 0;
		if (WgoData != null)
		{
			foreach (string linkedPerk in CraftDef.linkedPerks)
			{
				if (WgoData.HasPerk(linkedPerk))
				{
					num += GameBalance.Me.GetData<PerkDef>(linkedPerk).craftTotalProgressTicksBonus;
				}
			}
		}
		return num;
	}

	private int GetWgoPerksCraftStartTicksBonusValue()
	{
		int num = 0;
		if (WgoData != null)
		{
			foreach (string linkedPerk in CraftDef.linkedPerks)
			{
				if (WgoData.HasPerk(linkedPerk))
				{
					num += GameBalance.Me.GetData<PerkDef>(linkedPerk).craftStartTicks;
				}
			}
		}
		return num;
	}

	private int GetWgoPerksCraftMasteryBonusValue()
	{
		int num = 0;
		if (WgoData != null)
		{
			foreach (string linkedPerk in CraftDef.linkedPerks)
			{
				if (WgoData.HasPerk(linkedPerk))
				{
					num += GameBalance.Me.GetData<PerkDef>(linkedPerk).craftMasteryBonus;
				}
			}
		}
		return num;
	}
}
