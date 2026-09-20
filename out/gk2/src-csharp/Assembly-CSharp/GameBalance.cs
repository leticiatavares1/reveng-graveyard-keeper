using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class GameBalance : GameBalanceBase
{
	private const string GAME_BALANCE_ASSET_PATH = "Assets/Resources/GameBalance.asset";

	[BalanceTab("Items", 4)]
	public List<ItemDef> itemDefs = new List<ItemDef>();

	[BalanceTab("WGOs", -1)]
	public List<WGODef> wgoDefs = new List<WGODef>();

	[BalanceTab("WSOs", -1)]
	public List<WSODef> wsoDefs = new List<WSODef>();

	[BalanceTab("WGOGroups", -1)]
	public List<WgoGroupDef> wgoGroupDefs = new List<WgoGroupDef>();

	[BalanceTab("ToolTypes", -1)]
	public List<ToolTypeDef> toolTypes = new List<ToolTypeDef>();

	[BalanceTab("Fighters", -1)]
	public List<FighterDef> fighterDefs = new List<FighterDef>();

	[BalanceTab("Crafts", -1)]
	public List<CraftDef> craftDefs = new List<CraftDef>();

	[BalanceTab("Talents", -1)]
	public List<TalentDef> talentDefs = new List<TalentDef>();

	[BalanceTab("TalentExpLevels", -1)]
	public List<TalentExpLevelDef> talentExpLevelDefs = new List<TalentExpLevelDef>();

	[BalanceTab("Inspirations", -1)]
	public List<InspirationDef> inspirationDefs = new List<InspirationDef>();

	[BalanceTab("Perks", -1)]
	public List<PerkDef> perkDefs = new List<PerkDef>();

	[BalanceTab("WorldZones", -1)]
	public List<WorldZoneDef> worldZoneDefs = new List<WorldZoneDef>();

	[BalanceTab("Buildings", -1)]
	public List<BuildingDef> buildingDefs = new List<BuildingDef>();

	[BalanceTab("Sermons", -1)]
	public List<SermonDef> sermonDefs = new List<SermonDef>();

	[BalanceTab("Survey", -1)]
	public List<SurveyDef> surveyDefs = new List<SurveyDef>();

	[BalanceTab("SermonConfigs", -1)]
	public List<SermonConfigDef> sermonConfigDefs = new List<SermonConfigDef>();

	[BalanceTab("Logics", -1)]
	public List<GameLogicDef> gameLogicsDefs = new List<GameLogicDef>();

	[BalanceTab("Bodies", -1)]
	public List<BodyDef> bodyDefs = new List<BodyDef>();

	[BalanceTab("Techs", -1)]
	public List<TechDef> techDefs = new List<TechDef>();

	[BalanceTab("GameResSystem", -1)]
	public List<GameResSystemDef> gameResSystemDefs = new List<GameResSystemDef>();

	[BalanceTab("Vendors", -1)]
	public List<VendorDef> vendorDefs = new List<VendorDef>();

	[BalanceTab("VendorOrders", -1)]
	public List<VendorOrderDef> vendorOrderDefs = new List<VendorOrderDef>();

	[BalanceTab("TalentsLevelUps", -1)]
	public List<TalentLevelUpDef> talentLevelUpDefs = new List<TalentLevelUpDef>();

	[BalanceTab("Quests", -1)]
	public List<QuestDef> questDefs = new List<QuestDef>();

	[BalanceTab("Alchemy_formulas", -1)]
	public List<AlchemyFormulaDef> alchemyFormulaDefs = new List<AlchemyFormulaDef>();

	[BalanceTab("Fishing", -1)]
	public List<FishingDef> fishingDefs = new List<FishingDef>();

	[BalanceTab("Consts", -1)]
	public List<ConstDef> constDefs = new List<ConstDef>();

	[BalanceTab("TownBuildings", -1)]
	public List<TownBuildingDef> townBuildingDefs = new List<TownBuildingDef>();

	[BalanceTab("Fights", -1)]
	public List<FightDef> fightDefinitions = new List<FightDef>();

	[BalanceTab("PorterStations", -1)]
	public List<PorterStationDef> porterStationDefs = new List<PorterStationDef>();

	[BalanceTab("Mercenaries", -1)]
	public List<MercenariesDef> mercenariesDefs = new List<MercenariesDef>();

	[BalanceTab("Achievements", -1)]
	public List<AchievementDefinition> achievementDefs = new List<AchievementDefinition>();

	public List<AlchemyMixSourceDef> alchemyMixSourceDefs = new List<AlchemyMixSourceDef>();

	public TalentExpLevelBalanceData yellow = new TalentExpLevelBalanceData();

	public TalentExpLevelBalanceData green = new TalentExpLevelBalanceData();

	public TalentExpLevelBalanceData red = new TalentExpLevelBalanceData();

	public TalentExpLevelBalanceData orange = new TalentExpLevelBalanceData();

	public TalentExpLevelBalanceData blue = new TalentExpLevelBalanceData();

	[NonSerialized]
	public Dictionary<string, List<ItemDef>> starGroupItemsCache = new Dictionary<string, List<ItemDef>>();

	[NonSerialized]
	public Dictionary<string, List<ItemDef>> groupItemsCache = new Dictionary<string, List<ItemDef>>();

	[NonSerialized]
	public Dictionary<string, TalentExpLevelBalanceData> talentExpLevelsCache = new Dictionary<string, TalentExpLevelBalanceData>();

	[NonSerialized]
	public List<InspirationLevelData> inspirationLevels = new List<InspirationLevelData>();

	[NonSerialized]
	public Dictionary<string, InspirationLevelData> inspirationLevelsCache = new Dictionary<string, InspirationLevelData>();

	[NonSerialized]
	public Dictionary<string, List<CraftDefBase>> craftsInCache = new Dictionary<string, List<CraftDefBase>>();

	[NonSerialized]
	public Dictionary<string, List<string>> craftInItemsCache = new Dictionary<string, List<string>>();

	[NonSerialized]
	public Dictionary<string, List<string>> craftInItemsCacheShownInTooltips = new Dictionary<string, List<string>>();

	[NonSerialized]
	public Dictionary<string, List<BuildingDef>> buildDefsInBuilder = new Dictionary<string, List<BuildingDef>>();

	[NonSerialized]
	public Dictionary<string, BuildingDef> buildableWgos = new Dictionary<string, BuildingDef>();

	[NonSerialized]
	public Dictionary<string, BuildingDef> removableWgos = new Dictionary<string, BuildingDef>();

	[NonSerialized]
	public Dictionary<string, List<QuestDef>> questsByQuestIdCache = new Dictionary<string, List<QuestDef>>();

	[NonSerialized]
	public Dictionary<string, List<string>> questBrothersCache = new Dictionary<string, List<string>>();

	[NonSerialized]
	public Dictionary<string, QuestDef> questDefByReqPhrase = new Dictionary<string, QuestDef>();

	[NonSerialized]
	public Dictionary<ItemDef, List<CraftDef>> gardenCraftsPerItemCache = new Dictionary<ItemDef, List<CraftDef>>();

	[NonSerialized]
	public Dictionary<string, WGODef> conveyorWgosCache = new Dictionary<string, WGODef>();

	[NonSerialized]
	public Dictionary<string, Dictionary<string, List<CraftDef>>> workbenchCraftsByParentAndExtensionCache = new Dictionary<string, Dictionary<string, List<CraftDef>>>();

	[NonSerialized]
	public Dictionary<string, List<WGODef>> workbenchParentsByExtensionCache = new Dictionary<string, List<WGODef>>();

	[NonSerialized]
	public Dictionary<string, HashSet<string>> workbenchExtensionsByParentCache = new Dictionary<string, HashSet<string>>();

	[NonSerialized]
	public HashSet<string> workbenchExtensionIdsCache = new HashSet<string>();

	[NonSerialized]
	public HashSet<WGODef> workbenchesWhichUseExtensions = new HashSet<WGODef>();

	[NonSerialized]
	public HashSet<string> fighterWgoIdsCache = new HashSet<string>();

	[NonSerialized]
	public Dictionary<string, List<string>> customBuildAreaIdToWgoIds = new Dictionary<string, List<string>>();

	[NonSerialized]
	public Dictionary<AutopsyTypeCraft, Dictionary<string, CraftDef>> autopsyCraftsByTypeAndItemCache = new Dictionary<AutopsyTypeCraft, Dictionary<string, CraftDef>>();

	[NonSerialized]
	public Dictionary<string, CraftDef> gardenGrowingCrafts = new Dictionary<string, CraftDef>();

	[NonSerialized]
	public Dictionary<string, CraftParamsData.GardenType> gardenGrowingCraftTypes = new Dictionary<string, CraftParamsData.GardenType>();

	[NonSerialized]
	public Dictionary<string, AlchemyMixDef> alchemyMixDefsCache = new Dictionary<string, AlchemyMixDef>();

	[NonSerialized]
	public Dictionary<string, AlchemyMixSourceDef> alchemyMixSourcesByIdCache = new Dictionary<string, AlchemyMixSourceDef>();

	[NonSerialized]
	public Dictionary<string, CraftDef> runtimeCraftDefsCacheAlchemy = new Dictionary<string, CraftDef>();

	[NonSerialized]
	public Dictionary<string, QuestDef> questDefByFinishPhrase = new Dictionary<string, QuestDef>();

	[NonSerialized]
	public Dictionary<string, List<string>> wgoIdsByGroup = new Dictionary<string, List<string>>();

	private static GameBalance instance;

	public static GameBalance Me
	{
		get
		{
			if ((bool)instance)
			{
				return instance;
			}
			LoadGameBalance();
			return instance;
		}
	}

	public static void LoadGameBalance()
	{
		instance = Resources.Load<GameBalance>("GameBalance");
		if (instance == null)
		{
			Debug.LogError("Game data load failed");
			return;
		}
		GameBalanceBase.Instance = instance;
		instance.InitCache();
	}

	public override void InitCache()
	{
		base.InitCache();
		InitAlchemyCache();
		InitTalentExpLevels();
		InitInspirationLevels();
		FillItemsSortingOrder();
		CreateCraftCache();
		CreateAutopsyCraftsCache();
		CreateCraftGroupsCache();
		CreateCraftInItemsCache();
		CreateBuildCache();
		CreateNewVendorProductsCache();
		CreateQuestsCache();
		SetTalentIdsForInstruments();
		CreateGardenCraftsPerItemCache();
		CreateConveyorCache();
		CreateWorkbenchExtensionsCache();
		CreateFightersCache();
		CreateCustomBuildAreaIdToWgoIdsCache();
		CreateGardenGrowingCraftsCache();
		CreateQuestDefByFinishPhraseCache();
		CreateWgoIdsByGroupCache();
	}

	private void CreateCraftCache()
	{
		craftsInCache.Clear();
		foreach (CraftDef craftDef in craftDefs)
		{
			craftDef.SetIsFuelRelated();
			foreach (string item in craftDef.craftsIn)
			{
				if (!craftsInCache.ContainsKey(item))
				{
					craftsInCache.Add(item, new List<CraftDefBase>());
				}
				craftsInCache[item].Add(craftDef);
			}
		}
	}

	private void CreateAutopsyCraftsCache()
	{
		autopsyCraftsByTypeAndItemCache.Clear();
		foreach (CraftDef craftDef in craftDefs)
		{
			if (craftDef.autopsyTypeCraft != 0)
			{
				if (!autopsyCraftsByTypeAndItemCache.TryGetValue(craftDef.autopsyTypeCraft, out var value))
				{
					value = new Dictionary<string, CraftDef>();
					autopsyCraftsByTypeAndItemCache.Add(craftDef.autopsyTypeCraft, value);
				}
				if (!value.TryAdd(craftDef.autopsyItemId, craftDef))
				{
					Debug.LogError($"Craft [{craftDef.id}] duplicates autopsy craft [{value[craftDef.autopsyItemId].id}] of type [{craftDef.autopsyTypeCraft}] for item [{craftDef.autopsyItemId}].");
				}
			}
		}
	}

	private void CreateCraftInItemsCache()
	{
		List<string> allPossibleItemsAsOutput;
		foreach (CraftDef craftDef in craftDefs)
		{
			if (craftDef.isHidden || craftDef.id.StartsWith("rem_") || craftDef.id.StartsWith("set_") || craftDef.id.StartsWith("town_building_craft:"))
			{
				continue;
			}
			allPossibleItemsAsOutput = new List<string>();
			ScanOutputItems(craftDef.outputItems);
			ScanOutputItems(craftDef.addItemsToWgoOnStart);
			ScanOutputItems(craftDef.addItemsToWgoOnFinish);
			ScanNeedItems(craftDef.dropFromWgoItemsEnd);
			ScanNeedItems(craftDef.dropFromWgoItemsStart);
			for (int i = 0; i < allPossibleItemsAsOutput.Count; i++)
			{
				if (!craftInItemsCache.ContainsKey(allPossibleItemsAsOutput[i]))
				{
					craftInItemsCache.Add(allPossibleItemsAsOutput[i], new List<string>());
					craftInItemsCacheShownInTooltips.Add(allPossibleItemsAsOutput[i], new List<string>());
				}
				for (int j = 0; j < craftDef.craftsIn.Count; j++)
				{
					if (!craftInItemsCache[allPossibleItemsAsOutput[i]].Contains(craftDef.craftsIn[j]))
					{
						craftInItemsCache[allPossibleItemsAsOutput[i]].Add(craftDef.craftsIn[j]);
						if (!craftDef.doNotShowInTooltips)
						{
							craftInItemsCacheShownInTooltips[allPossibleItemsAsOutput[i]].Add(craftDef.craftsIn[j]);
						}
					}
				}
			}
			void ScanNeedItems(List<NeedItemData> needItems)
			{
				for (int k = 0; k < needItems.Count; k++)
				{
					NeedItemData needItemData = needItems[k];
					switch (needItemData.groupType)
					{
					case ItemGroup.None:
						allPossibleItemsAsOutput.Add(needItemData.id);
						break;
					case ItemGroup.Common:
					{
						for (int m = 0; m < groupItemsCache[needItemData.id].Count; m++)
						{
							allPossibleItemsAsOutput.Add(groupItemsCache[needItemData.id][m].id);
						}
						break;
					}
					case ItemGroup.Star:
					{
						for (int l = 0; l < starGroupItemsCache[needItemData.id].Count; l++)
						{
							allPossibleItemsAsOutput.Add(starGroupItemsCache[needItemData.id][l].id);
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
			}
			void ScanOutputItems(OutputItems outputItems)
			{
				for (int num = 0; num < outputItems.chanceOutputItems.Count; num++)
				{
					AddChanceItemDef(outputItems.chanceOutputItems[num]);
				}
				for (int num2 = 0; num2 < outputItems.groupChanceOutputItems.Count; num2++)
				{
					for (int num3 = 0; num3 < outputItems.groupChanceOutputItems[num2].chanceItems.Count; num3++)
					{
						AddChanceItemDef(outputItems.groupChanceOutputItems[num2].chanceItems[num3]);
					}
				}
			}
		}
		void AddChanceItemDef(ChanceOutputItem chanceOutputItem)
		{
			if (chanceOutputItem.isStarGroup)
			{
				for (int n = 0; n < starGroupItemsCache[chanceOutputItem.id].Count; n++)
				{
					allPossibleItemsAsOutput.Add(starGroupItemsCache[chanceOutputItem.id][n].id);
				}
			}
			else
			{
				allPossibleItemsAsOutput.Add(chanceOutputItem.id);
			}
		}
	}

	private void AddCraftDefToCache(CraftDef craftDef)
	{
		craftDef.SetIsFuelRelated();
		foreach (string item in craftDef.craftsIn)
		{
			if (!craftsInCache.ContainsKey(item))
			{
				craftsInCache.Add(item, new List<CraftDefBase>());
			}
			craftsInCache[item].Add(craftDef);
		}
		if (craftDef.isHidden || craftDef.id.StartsWith("rem_") || craftDef.id.StartsWith("set_"))
		{
			return;
		}
		List<string> allPossibleItemsAsOutput = new List<string>();
		ScanOutputItems(craftDef.outputItems);
		ScanOutputItems(craftDef.addItemsToWgoOnStart);
		ScanOutputItems(craftDef.addItemsToWgoOnFinish);
		ScanNeedItems(craftDef.dropFromWgoItemsEnd);
		ScanNeedItems(craftDef.dropFromWgoItemsStart);
		for (int i = 0; i < allPossibleItemsAsOutput.Count; i++)
		{
			if (!craftInItemsCache.ContainsKey(allPossibleItemsAsOutput[i]))
			{
				craftInItemsCache.Add(allPossibleItemsAsOutput[i], new List<string>());
				craftInItemsCacheShownInTooltips.Add(allPossibleItemsAsOutput[i], new List<string>());
			}
			for (int j = 0; j < craftDef.craftsIn.Count; j++)
			{
				if (!craftInItemsCache[allPossibleItemsAsOutput[i]].Contains(craftDef.craftsIn[j]))
				{
					craftInItemsCache[allPossibleItemsAsOutput[i]].Add(craftDef.craftsIn[j]);
					if (!craftDef.doNotShowInTooltips)
					{
						craftInItemsCacheShownInTooltips[allPossibleItemsAsOutput[i]].Add(craftDef.craftsIn[j]);
					}
				}
			}
		}
		void AddChanceItemDef(ChanceOutputItem chanceOutputItem)
		{
			if (chanceOutputItem.isStarGroup)
			{
				for (int n = 0; n < starGroupItemsCache[chanceOutputItem.id].Count; n++)
				{
					allPossibleItemsAsOutput.Add(starGroupItemsCache[chanceOutputItem.id][n].id);
				}
			}
			else
			{
				allPossibleItemsAsOutput.Add(chanceOutputItem.id);
			}
		}
		void ScanNeedItems(List<NeedItemData> needItems)
		{
			for (int k = 0; k < needItems.Count; k++)
			{
				NeedItemData needItemData = needItems[k];
				switch (needItemData.groupType)
				{
				case ItemGroup.None:
					allPossibleItemsAsOutput.Add(needItemData.id);
					break;
				case ItemGroup.Common:
				{
					for (int m = 0; m < groupItemsCache[needItemData.id].Count; m++)
					{
						allPossibleItemsAsOutput.Add(groupItemsCache[needItemData.id][m].id);
					}
					break;
				}
				case ItemGroup.Star:
				{
					for (int l = 0; l < starGroupItemsCache[needItemData.id].Count; l++)
					{
						allPossibleItemsAsOutput.Add(starGroupItemsCache[needItemData.id][l].id);
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		void ScanOutputItems(OutputItems outputItems)
		{
			for (int num = 0; num < outputItems.chanceOutputItems.Count; num++)
			{
				AddChanceItemDef(outputItems.chanceOutputItems[num]);
			}
			for (int num2 = 0; num2 < outputItems.groupChanceOutputItems.Count; num2++)
			{
				for (int num3 = 0; num3 < outputItems.groupChanceOutputItems[num2].chanceItems.Count; num3++)
				{
					AddChanceItemDef(outputItems.groupChanceOutputItems[num2].chanceItems[num3]);
				}
			}
		}
	}

	public static T GetCraftDef<T>(string craftId) where T : CraftDefBase
	{
		if (typeof(T) == typeof(SermonDef))
		{
			return GetSermonDef(craftId) as T;
		}
		if (typeof(T) == typeof(SurveyDef))
		{
			return GetSurveyDef(craftId) as T;
		}
		if (typeof(T) == typeof(AlchemyMixDef))
		{
			return GetAlchemyMixDef(craftId) as T;
		}
		if (typeof(T) == typeof(CraftDef))
		{
			return GetCraftDef(craftId) as T;
		}
		return GetCraftDefBase(craftId) as T;
	}

	public static CraftDefBase GetCraftDefBase(string craftId)
	{
		CraftDefBase dataOrNull = Me.GetDataOrNull<CraftDef>(craftId);
		if (dataOrNull != null)
		{
			return dataOrNull;
		}
		dataOrNull = Me.GetDataOrNull<SermonDef>(craftId);
		if (dataOrNull != null)
		{
			return dataOrNull;
		}
		dataOrNull = Me.GetDataOrNull<SurveyDef>(craftId);
		if (dataOrNull != null)
		{
			return dataOrNull;
		}
		dataOrNull = GetAlchemyMixDef(craftId);
		if (dataOrNull != null)
		{
			return dataOrNull;
		}
		return GetAlchemyMixCraftDef(craftId);
	}

	public static CraftDef GetCraftDef(string craftId)
	{
		if (craftId.StartsWith("mix"))
		{
			return GetAlchemyMixCraftDef(craftId);
		}
		return Me.GetData<CraftDef>(craftId);
	}

	public static CraftDef GetAutopsyCraftDef(AutopsyTypeCraft autopsyType, string itemId = "")
	{
		if (!Me.autopsyCraftsByTypeAndItemCache.TryGetValue(autopsyType, out var value))
		{
			return null;
		}
		if (autopsyType == AutopsyTypeCraft.PocketExtract)
		{
			using (Dictionary<string, CraftDef>.ValueCollection.Enumerator enumerator = value.Values.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			return null;
		}
		value.TryGetValue(itemId, out var value2);
		return value2;
	}

	public static SurveyDef GetSurveyDef(string craftId)
	{
		return Me.GetData<SurveyDef>(craftId);
	}

	public static SurveyDef GetSurveyDefOrNull(string craftId)
	{
		return Me.GetDataOrNull<SurveyDef>(craftId);
	}

	public static SurveyDef GetSurveyDefForItemOrNull(string itemId)
	{
		SurveyDef surveyDefOrNull = GetSurveyDefOrNull("surv:" + itemId);
		if (surveyDefOrNull != null)
		{
			return surveyDefOrNull;
		}
		ItemDef dataOrNull = Me.GetDataOrNull<ItemDef>(itemId);
		if (dataOrNull == null)
		{
			return null;
		}
		foreach (SurveyDef surveyDef in Me.surveyDefs)
		{
			if (surveyDef.IsSurveyForItem(dataOrNull))
			{
				return surveyDef;
			}
		}
		return null;
	}

	public static SermonDef GetSermonDef(string craftId)
	{
		return Me.GetData<SermonDef>(craftId);
	}

	public static AlchemyMixDef GetAlchemyMixDef(string mixId)
	{
		if (Me.alchemyMixDefsCache.TryGetValue(mixId, out var value) && value != null)
		{
			Debug.Log("#alch# GetAlchemyMixDef:[" + mixId + "] has cached");
			return value;
		}
		if (!Me.alchemyMixSourcesByIdCache.TryGetValue(mixId, out var value2))
		{
			Debug.Log("#alch# GetAlchemyMixDef:[" + mixId + "] return null");
			return null;
		}
		AlchemyMixDef alchemyMixDef = Me.BuildAlchemyMixDef(value2);
		Debug.Log($"#alch# GetAlchemyMixDef:[{mixId}] build new is null:[{alchemyMixDef == null}]");
		Me.alchemyMixDefsCache[mixId] = alchemyMixDef;
		return alchemyMixDef;
	}

	private void FillItemsSortingOrder()
	{
		for (int i = 0; i < itemDefs.Count; i++)
		{
			itemDefs[i].sortOrder = i;
		}
	}

	private void CreateCraftGroupsCache()
	{
		starGroupItemsCache.Clear();
		groupItemsCache.Clear();
		foreach (ItemDef itemDef in itemDefs)
		{
			for (int i = 0; i < itemDef.itemGroupIds.Count; i++)
			{
				if (!groupItemsCache.ContainsKey(itemDef.itemGroupIds[i]))
				{
					groupItemsCache.Add(itemDef.itemGroupIds[i], new List<ItemDef>());
				}
				groupItemsCache[itemDef.itemGroupIds[i]].Add(itemDef);
			}
			if (itemDef.qualityType == ItemDef.QualityType.Star)
			{
				string key = itemDef.id.Split(":")[0];
				if (!starGroupItemsCache.ContainsKey(key))
				{
					starGroupItemsCache.Add(key, new List<ItemDef>());
				}
				starGroupItemsCache[key].Add(itemDef);
			}
		}
	}

	private void CreateBuildCache()
	{
		buildDefsInBuilder.Clear();
		buildableWgos.Clear();
		removableWgos.Clear();
		foreach (BuildingDef buildingDef in buildingDefs)
		{
			foreach (string item in buildingDef.buildsIn)
			{
				if (!buildDefsInBuilder.ContainsKey(item))
				{
					buildDefsInBuilder.Add(item, new List<BuildingDef>());
				}
				buildDefsInBuilder[item].Add(buildingDef);
			}
			switch (buildingDef.buildingMode)
			{
			case BuildingDef.BuildingMode.Place:
			case BuildingDef.BuildingMode.ConveyorPlace:
			case BuildingDef.BuildingMode.FightingPlace:
			case BuildingDef.BuildingMode.FightBuilding:
				buildableWgos.Add(buildingDef.wgoId, buildingDef);
				break;
			case BuildingDef.BuildingMode.Remove:
				removableWgos.Add(buildingDef.wgoId, buildingDef);
				break;
			}
		}
	}

	private void CreateNewVendorProductsCache()
	{
		foreach (VendorDef vendorDef in vendorDefs)
		{
			if (vendorDef.startTier == 3)
			{
				continue;
			}
			foreach (VendorTierData tierData in vendorDef.tierDataList)
			{
				tierData.newProducts.Clear();
				foreach (VendorProductData vendorProduct in tierData.vendorProducts)
				{
					bool flag = false;
					foreach (VendorTierData tierData2 in vendorDef.tierDataList)
					{
						if (tierData2 == tierData)
						{
							break;
						}
						foreach (VendorProductData vendorProduct2 in tierData2.vendorProducts)
						{
							if (vendorProduct2.itemId == vendorProduct.itemId)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (!flag)
					{
						tierData.newProducts.Add(vendorProduct.itemId);
					}
				}
			}
		}
	}

	private void CreateQuestsCache()
	{
		questDefByReqPhrase.Clear();
		questsByQuestIdCache.Clear();
		questBrothersCache.Clear();
		foreach (QuestDef questDef in questDefs)
		{
			if (!string.IsNullOrEmpty(questDef.finishCheck.phrase))
			{
				questDefByReqPhrase.TryAdd(questDef.finishCheck.phrase, questDef);
			}
			if (questsByQuestIdCache.TryGetValue(questDef.id, out var value))
			{
				value.Add(questDef);
				continue;
			}
			questsByQuestIdCache.Add(questDef.id, new List<QuestDef> { questDef });
		}
		CreateQuestBrothersCache();
	}

	private void CreateQuestBrothersCache()
	{
		Dictionary<Vector2Int, List<string>> dictionary = new Dictionary<Vector2Int, List<string>>();
		foreach (QuestDef questDef in questDefs)
		{
			if (!dictionary.TryGetValue(questDef.TreePos, out var value))
			{
				value = new List<string>();
				dictionary.Add(questDef.TreePos, value);
			}
			value.Add(questDef.id);
		}
		foreach (QuestDef questDef2 in questDefs)
		{
			HashSet<string> hashSet = new HashSet<string>(questDef2.brotherIds);
			foreach (string item in dictionary[questDef2.TreePos])
			{
				if (item != questDef2.id)
				{
					hashSet.Add(item);
				}
			}
			questBrothersCache.Add(questDef2.id, new List<string>(hashSet));
		}
	}

	private void CreateGardenCraftsPerItemCache()
	{
		foreach (ItemDef itemDef in itemDefs)
		{
			if (!itemDef.isSeed && !itemDef.isFertilizer)
			{
				continue;
			}
			if (!gardenCraftsPerItemCache.ContainsKey(itemDef))
			{
				gardenCraftsPerItemCache.Add(itemDef, new List<CraftDef>());
			}
			foreach (CraftDef craftDef in craftDefs)
			{
				foreach (NeedItemData needItem in craftDef.needItems)
				{
					switch (needItem.groupType)
					{
					case ItemGroup.None:
						if (needItem.id == itemDef.id)
						{
							gardenCraftsPerItemCache[itemDef].Add(craftDef as CraftDef);
						}
						break;
					case ItemGroup.Common:
						if (groupItemsCache[needItem.id].Contains(itemDef))
						{
							gardenCraftsPerItemCache[itemDef].Add(craftDef as CraftDef);
						}
						break;
					case ItemGroup.Star:
						if (starGroupItemsCache[needItem.id].Contains(itemDef))
						{
							gardenCraftsPerItemCache[itemDef].Add(craftDef as CraftDef);
						}
						break;
					}
				}
			}
		}
	}

	private void InitTalentExpLevels()
	{
		talentExpLevelsCache.Clear();
		yellow = new TalentExpLevelBalanceData("talent_yellow");
		green = new TalentExpLevelBalanceData("talent_green");
		red = new TalentExpLevelBalanceData("talent_red");
		orange = new TalentExpLevelBalanceData("talent_orange");
		blue = new TalentExpLevelBalanceData("talent_blue");
		talentExpLevelsCache.Add("talent_yellow", yellow);
		talentExpLevelsCache.Add("talent_green", green);
		talentExpLevelsCache.Add("talent_red", red);
		talentExpLevelsCache.Add("talent_orange", orange);
		talentExpLevelsCache.Add("talent_blue", blue);
		foreach (TalentExpLevelDef talentExpLevelDef in talentExpLevelDefs)
		{
			yellow.expLevels.Add(talentExpLevelDef.yellow);
			green.expLevels.Add(talentExpLevelDef.green);
			red.expLevels.Add(talentExpLevelDef.red);
			orange.expLevels.Add(talentExpLevelDef.orange);
			blue.expLevels.Add(talentExpLevelDef.blue);
		}
	}

	private void InitAlchemyCache()
	{
		runtimeCraftDefsCacheAlchemy = new Dictionary<string, CraftDef>();
		alchemyMixDefsCache = new Dictionary<string, AlchemyMixDef>();
		alchemyMixSourcesByIdCache = new Dictionary<string, AlchemyMixSourceDef>();
		foreach (AlchemyMixSourceDef alchemyMixSourceDef in alchemyMixSourceDefs)
		{
			alchemyMixSourcesByIdCache[alchemyMixSourceDef.mixId] = alchemyMixSourceDef;
		}
	}

	private void InitInspirationLevels()
	{
		inspirationLevels = new List<InspirationLevelData>();
		inspirationLevelsCache = new Dictionary<string, InspirationLevelData>();
		foreach (InspirationDef inspirationDef in inspirationDefs)
		{
			string idWithoutLvl = inspirationDef.idWithoutLvl;
			if (inspirationLevelsCache.TryGetValue(idWithoutLvl, out var value))
			{
				value.levels.Add(inspirationDef);
				value.inspirationLocks.AddRange(inspirationDef.inpsirationLocks);
				value.techLocks.AddRange(inspirationDef.techLocks);
				value.questLocks.AddRange(inspirationDef.questLocks);
				continue;
			}
			InspirationLevelData inspirationLevelData = new InspirationLevelData();
			inspirationLevelData.id = idWithoutLvl;
			inspirationLevelData.talentId = inspirationDef.talentId;
			inspirationLevelData.levels.Add(inspirationDef);
			inspirationLevelData.inspirationLocks.AddRange(inspirationDef.inpsirationLocks);
			inspirationLevelData.techLocks.AddRange(inspirationDef.techLocks);
			inspirationLevelData.questLocks.AddRange(inspirationDef.questLocks);
			inspirationLevels.Add(inspirationLevelData);
			inspirationLevelsCache.Add(idWithoutLvl, inspirationLevelData);
		}
	}

	private void SetTalentIdsForInstruments()
	{
		foreach (ItemDef itemDef in itemDefs)
		{
			GameBalance me = Me;
			int type = (int)itemDef.type;
			ToolTypeDef dataOrNull = me.GetDataOrNull<ToolTypeDef>(type.ToString());
			if (dataOrNull != null)
			{
				itemDef.talentIds = dataOrNull.talentIds;
			}
		}
	}

	public void CreateTownBuildingCrafts()
	{
		foreach (TownBuildingDef townBuildingDef in townBuildingDefs)
		{
			CraftDef craftDef = new CraftDef();
			craftDef.id = "town_building_craft:" + townBuildingDef.id;
			craftDef.craftsIn = townBuildingDef.craftsIn;
			craftDef.needItems = townBuildingDef.needItems;
			craftDef.duration = new LazyExpression();
			craftDef.isAuto = true;
			craftDef.autoFinishAutoCraft = true;
			craftDef.isNeedsUnlock = townBuildingDef.isNeedsUnlock;
			craftDef.onCraftEndExpressions = townBuildingDef.onCraftEndExpressions;
			craftDef.outputItems = townBuildingDef.dropItemsOnBuildingFinished;
			if (!townBuildingDef.dontPlaceBuildingOnWgo)
			{
				craftDef.onCraftEndExpressions.Insert(0, new LazyExpression("CreateTownBuilding()"));
			}
			else
			{
				craftDef.onCraftEndExpressions.Insert(0, new LazyExpression("DestroySignboard()"));
			}
			craftDefs.Add(craftDef);
		}
	}

	private AlchemyMixDef BuildAlchemyMixDef(AlchemyMixSourceDef source)
	{
		AlchemyFormulaDef data = GetData<AlchemyFormulaDef>(source.formulaId);
		Vector3Int runesAsVector3Int = data.GetRunesAsVector3Int();
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(source.ingredient1))
		{
			list.Add(source.ingredient1);
		}
		if (!string.IsNullOrEmpty(source.ingredient2))
		{
			list.Add(source.ingredient2);
		}
		if (!string.IsNullOrEmpty(source.ingredient3))
		{
			list.Add(source.ingredient3);
		}
		AlchemyMixDef alchemyMixDef = new AlchemyMixDef();
		alchemyMixDef.id = source.mixId;
		alchemyMixDef.ingredients = list.ToArray();
		alchemyMixDef.craftsIn = data.craftsIn;
		alchemyMixDef.duration = new LazyExpression(3.ToString());
		alchemyMixDef.isMulticraftDisabled = true;
		alchemyMixDef.talentLock = 0;
		alchemyMixDef.techRed = new LazyExpression(runesAsVector3Int.x.ToString());
		alchemyMixDef.techGreen = new LazyExpression(runesAsVector3Int.y.ToString());
		alchemyMixDef.techBlue = new LazyExpression(runesAsVector3Int.z.ToString());
		alchemyMixDef.outputItems = new OutputItems();
		ChanceOutputItem item = new ChanceOutputItem
		{
			id = data.id,
			count = new LazyExpression(1.ToString())
		};
		alchemyMixDef.outputItems.chanceOutputItems.Add(item);
		alchemyMixDef.onCraftEndExpressions = data.onCraftEndExpressions;
		return alchemyMixDef;
	}

	public static CraftDef GetAlchemyMixCraftDef(string craftId)
	{
		if (Me.runtimeCraftDefsCacheAlchemy.TryGetValue(craftId, out var value))
		{
			return value;
		}
		AlchemyMixDef alchemyMixDef = GetAlchemyMixDef(craftId);
		if (alchemyMixDef == null)
		{
			return null;
		}
		Me.CreateRelatedStuffForSingleMix(alchemyMixDef);
		if (Me.runtimeCraftDefsCacheAlchemy.TryGetValue(craftId, out var value2))
		{
			return value2;
		}
		string text = ((alchemyMixDef.outputItems != null && alchemyMixDef.outputItems.chanceOutputItems.Count > 0) ? alchemyMixDef.outputItems.chanceOutputItems[0].id : "<empty>");
		Debug.LogError("#alch# Failed to create alchemy workbench craft. craftId:[" + craftId + "] outputId:[" + text + "]");
		return null;
	}

	public void CreateAlchemyMixingDefs()
	{
		List<ItemDef> list = new List<ItemDef>();
		List<CraftDef> list2 = new List<CraftDef>();
		alchemyMixSourceDefs.Clear();
		alchemyMixSourcesByIdCache.Clear();
		for (int j = 0; j < itemDefs.Count; j++)
		{
			ItemDef itemDef = itemDefs[j];
			if (itemDef.canBeUsedInAlchemy && (itemDef.runesRed.EvaluateInt() > 0 || itemDef.runesGreen.EvaluateInt() > 0 || itemDef.runesBlue.EvaluateInt() > 0))
			{
				list.Add(itemDef);
			}
		}
		for (int k = 0; k < craftDefs.Count; k++)
		{
			CraftDef craftDef = craftDefs[k];
			if (craftDef.id.EndsWith("_boost"))
			{
				list2.Add(craftDef);
			}
		}
		foreach (AlchemyFormulaDef alchemyFormulaDef in alchemyFormulaDefs)
		{
			Vector3Int runesAsVector3Int = alchemyFormulaDef.GetRunesAsVector3Int();
			for (int l = 0; l < list.Count; l++)
			{
				ItemDef itemDef2 = list[l];
				Vector3Int runesAsVector3Int2 = itemDef2.GetRunesAsVector3Int();
				if (runesAsVector3Int == runesAsVector3Int2)
				{
					TryAddSource(alchemyFormulaDef, new List<ItemDef> { itemDef2 }, null);
				}
				foreach (CraftDef item in list2)
				{
					if (item.GetBoostRunesAsVector3Int() + runesAsVector3Int2 == runesAsVector3Int)
					{
						TryAddSource(alchemyFormulaDef, new List<ItemDef> { itemDef2 }, item);
					}
				}
				for (int m = 0; m < list.Count; m++)
				{
					ItemDef itemDef3 = list[m];
					Vector3Int runesAsVector3Int3 = itemDef3.GetRunesAsVector3Int();
					if (runesAsVector3Int2 + runesAsVector3Int3 == runesAsVector3Int)
					{
						TryAddSource(alchemyFormulaDef, new List<ItemDef> { itemDef2, itemDef3 }, null);
					}
					foreach (CraftDef item2 in list2)
					{
						if (item2.GetBoostRunesAsVector3Int() + runesAsVector3Int2 + runesAsVector3Int3 == runesAsVector3Int)
						{
							TryAddSource(alchemyFormulaDef, new List<ItemDef> { itemDef2, itemDef3 }, item2);
						}
					}
					for (int n = 0; n < list.Count; n++)
					{
						ItemDef itemDef4 = list[n];
						Vector3Int runesAsVector3Int4 = itemDef4.GetRunesAsVector3Int();
						if (runesAsVector3Int2 + runesAsVector3Int3 + runesAsVector3Int4 == runesAsVector3Int)
						{
							TryAddSource(alchemyFormulaDef, new List<ItemDef> { itemDef2, itemDef3, itemDef4 }, null);
						}
						foreach (CraftDef item3 in list2)
						{
							if (item3.GetBoostRunesAsVector3Int() + runesAsVector3Int2 + runesAsVector3Int3 + runesAsVector3Int4 == runesAsVector3Int)
							{
								TryAddSource(alchemyFormulaDef, new List<ItemDef> { itemDef2, itemDef3, itemDef4 }, item3);
							}
						}
					}
				}
			}
		}
		AddMixCraftsAliases();
		void TryAddSource(AlchemyFormulaDef formulaDef, List<ItemDef> ingredients, CraftDef boostCraft)
		{
			string text = AlchemyMixDef.MixId(ingredients.Select((ItemDef i) => i.id).ToArray(), boostCraft);
			if (!alchemyMixSourcesByIdCache.ContainsKey(text))
			{
				AlchemyMixSourceDef alchemyMixSourceDef = new AlchemyMixSourceDef
				{
					mixId = text,
					formulaId = formulaDef.id,
					ingredient1 = ((ingredients.Count > 0) ? ingredients[0].id : ""),
					ingredient2 = ((ingredients.Count > 1) ? ingredients[1].id : ""),
					ingredient3 = ((ingredients.Count > 2) ? ingredients[2].id : "")
				};
				alchemyMixSourceDefs.Add(alchemyMixSourceDef);
				alchemyMixSourcesByIdCache[text] = alchemyMixSourceDef;
			}
		}
	}

	private void CreateRelatedStuffForSingleMix(AlchemyMixDef cur)
	{
		CraftDef dataOrNull = GetDataOrNull<CraftDef>("default_alchemy_craft");
		string id = cur.outputItems.chanceOutputItems[0].id;
		CraftDef dataOrNull2 = GetDataOrNull<CraftDef>(cur.id);
		if (dataOrNull2 != null)
		{
			runtimeCraftDefsCacheAlchemy[cur.id] = dataOrNull2;
			return;
		}
		if (dataOrNull == null)
		{
			Debug.LogError("#alch# Can't create alchemy workbench craft [" + cur.id + "], default_alchemy_craft is missing.");
			return;
		}
		dataOrNull2 = dataOrNull.Copy();
		dataOrNull2.id = cur.id;
		dataOrNull2.outputItems = new OutputItems();
		dataOrNull2.talentLock = cur.talentLock;
		dataOrNull2.techBlue = cur.techBlue;
		dataOrNull2.techRed = cur.techRed;
		dataOrNull2.techGreen = cur.techGreen;
		dataOrNull2.tabId = id;
		dataOrNull2.needItems = new List<NeedItemData>();
		string[] ingredients = cur.ingredients;
		foreach (string ingredient in ingredients)
		{
			NeedItemData needItemData = dataOrNull2.needItems.Find((NeedItemData i) => i.id == ingredient);
			if (needItemData != null)
			{
				needItemData.Reinitialize(needItemData.id, needItemData.GetCount() + 1);
			}
			else
			{
				dataOrNull2.needItems.Add(new NeedItemData(ingredient, 1));
			}
		}
		dataOrNull2.needItems.Add(new NeedItemData("alchemy_flask", 1));
		dataOrNull2.outputItems.chanceOutputItems.Add(cur.outputItems.chanceOutputItems[0]);
		if (cur.id.EndsWith("_boost"))
		{
			CraftDef boostCraft = cur.BoostCraft;
			dataOrNull2.craftsIn = new List<string>();
			dataOrNull2.craftsIn.AddRange(boostCraft.craftsIn);
			dataOrNull2.needItems.AddRange(boostCraft.needItems);
			if (boostCraft.talentLock > dataOrNull2.talentLock)
			{
				dataOrNull2.talentLock = boostCraft.talentLock;
			}
		}
		runtimeCraftDefsCacheAlchemy[dataOrNull2.id] = dataOrNull2;
		AddCraftDefToCache(dataOrNull2);
	}

	public void AddMixCraftsAliases()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (AlchemyMixSourceDef alchemyMixSourceDef in alchemyMixSourceDefs)
		{
			list.Add(alchemyMixSourceDef.mixId);
			list2.Add(alchemyMixSourceDef.formulaId);
		}
		LLBase.AddAliases(list, list2);
	}

	private void FindCoordsForFreeTalentLevelUps()
	{
		for (int i = 0; i < talentDefs.Count; i++)
		{
			string id = talentDefs[i].id;
			Dictionary<Vector2, TalentLevelUpDef> dictionary = new Dictionary<Vector2, TalentLevelUpDef>();
			Queue<TalentLevelUpDef> queue = new Queue<TalentLevelUpDef>();
			foreach (TalentLevelUpDef talentLevelUpDef in talentLevelUpDefs)
			{
				if (!(talentLevelUpDef.talentId != id))
				{
					if (!talentLevelUpDef.isFreeCoordinates)
					{
						dictionary[talentLevelUpDef.TreePos] = talentLevelUpDef;
					}
					else
					{
						queue.Enqueue(talentLevelUpDef);
					}
				}
			}
			if (queue.Count == 0)
			{
				break;
			}
			for (float num = 0f; num < 5f; num += 0.5f)
			{
				for (float num2 = 0f; num2 < 6f; num2 += 0.5f)
				{
					Vector2 key = new Vector2(num, num2);
					if (!dictionary.ContainsKey(key))
					{
						dictionary[key] = queue.Dequeue();
						if (queue.Count == 0)
						{
							return;
						}
					}
				}
			}
		}
	}

	private void CreateConveyorCache()
	{
		conveyorWgosCache.Clear();
		foreach (WGODef wgoDef in wgoDefs)
		{
			if (wgoDef.conveyorType != 0)
			{
				conveyorWgosCache.Add(wgoDef.id, wgoDef);
			}
		}
	}

	private void CreateWorkbenchExtensionsCache()
	{
		workbenchCraftsByParentAndExtensionCache.Clear();
		workbenchParentsByExtensionCache.Clear();
		workbenchExtensionsByParentCache.Clear();
		workbenchExtensionIdsCache.Clear();
		workbenchesWhichUseExtensions.Clear();
		foreach (WGODef wgoDef in wgoDefs)
		{
			if (wgoDef.attachedWorkbenchExtensionIds.Count > 0)
			{
				workbenchesWhichUseExtensions.Add(wgoDef);
			}
			foreach (string attachedWorkbenchExtensionId in wgoDef.attachedWorkbenchExtensionIds)
			{
				workbenchExtensionIdsCache.Add(attachedWorkbenchExtensionId);
				if (workbenchParentsByExtensionCache.TryGetValue(attachedWorkbenchExtensionId, out var value))
				{
					if (!value.Contains(wgoDef))
					{
						value.Add(wgoDef);
					}
				}
				else
				{
					workbenchParentsByExtensionCache[attachedWorkbenchExtensionId] = new List<WGODef> { wgoDef };
				}
				if (!workbenchExtensionsByParentCache.TryGetValue(wgoDef.id, out var value2))
				{
					value2 = new HashSet<string>();
					workbenchExtensionsByParentCache[wgoDef.id] = value2;
				}
				value2.Add(attachedWorkbenchExtensionId);
			}
		}
		foreach (CraftDef craftDef in craftDefs)
		{
			if (string.IsNullOrEmpty(craftDef.extensionNeedId))
			{
				continue;
			}
			foreach (string item in craftDef.craftsIn)
			{
				if (!workbenchCraftsByParentAndExtensionCache.TryGetValue(item, out var value3))
				{
					value3 = new Dictionary<string, List<CraftDef>>();
					workbenchCraftsByParentAndExtensionCache[item] = value3;
				}
				if (!value3.TryGetValue(craftDef.extensionNeedId, out var value4))
				{
					value4 = new List<CraftDef>();
					value3[craftDef.extensionNeedId] = value4;
				}
				if (!value4.Contains(craftDef))
				{
					value4.Add(craftDef);
				}
			}
		}
	}

	public bool IsWorkbenchExtensionId(string extensionId)
	{
		if (string.IsNullOrEmpty(extensionId))
		{
			return false;
		}
		return workbenchExtensionIdsCache.Contains(extensionId);
	}

	public WGODef GetWorkbenchExtensionLogicDef(string wgoId)
	{
		if (string.IsNullOrEmpty(wgoId))
		{
			return null;
		}
		WGODef data = GetData<WGODef>(wgoId);
		if (data == null)
		{
			return null;
		}
		if (workbenchesWhichUseExtensions.Contains(data) || IsWorkbenchExtensionId(wgoId))
		{
			return data;
		}
		if (wgoId.EndsWith("_place", StringComparison.Ordinal))
		{
			int length = "_place".Length;
			string text = wgoId.Substring(0, wgoId.Length - length);
			WGODef data2 = GetData<WGODef>(text);
			if (data2 != null && (workbenchesWhichUseExtensions.Contains(data2) || IsWorkbenchExtensionId(text)))
			{
				return data2;
			}
		}
		return data;
	}

	public bool TryGetParentWorkbenchDefsForExtension(string extensionId, out List<WGODef> parentWorkbenchDefs)
	{
		if (string.IsNullOrEmpty(extensionId))
		{
			parentWorkbenchDefs = null;
			return false;
		}
		return workbenchParentsByExtensionCache.TryGetValue(extensionId, out parentWorkbenchDefs);
	}

	public bool IsExtensionAllowedForParentWorkbench(string parentWorkbenchId, string extensionId)
	{
		if (string.IsNullOrEmpty(parentWorkbenchId) || string.IsNullOrEmpty(extensionId))
		{
			return false;
		}
		if (workbenchExtensionsByParentCache.TryGetValue(parentWorkbenchId, out var value))
		{
			return value.Contains(extensionId);
		}
		return false;
	}

	public HashSet<string> GetAllowedExtensionIdsForParentWorkbench(string parentWorkbenchId)
	{
		if (string.IsNullOrEmpty(parentWorkbenchId))
		{
			return new HashSet<string>();
		}
		if (workbenchExtensionsByParentCache.TryGetValue(parentWorkbenchId, out var value))
		{
			return new HashSet<string>(value);
		}
		return new HashSet<string>();
	}

	public List<CraftDef> GetWorkbenchExtensionCrafts(string parentWorkbenchId, string extensionId)
	{
		if (string.IsNullOrEmpty(parentWorkbenchId) || string.IsNullOrEmpty(extensionId))
		{
			return new List<CraftDef>();
		}
		if (workbenchCraftsByParentAndExtensionCache.TryGetValue(parentWorkbenchId, out var value) && value.TryGetValue(extensionId, out var value2))
		{
			return new List<CraftDef>(value2);
		}
		return new List<CraftDef>();
	}

	public bool HasWgoIdByGroup(string wgoGroup, string wgoId)
	{
		if (string.IsNullOrEmpty(wgoGroup))
		{
			return false;
		}
		if (!wgoIdsByGroup.TryGetValue(wgoGroup, out var value))
		{
			return false;
		}
		return value.Contains(wgoId);
	}

	private void CreateFightersCache()
	{
		fighterWgoIdsCache.Clear();
		foreach (FighterDef fighterDef in fighterDefs)
		{
			fighterWgoIdsCache.Add(fighterDef.id);
		}
	}

	private void CreateCustomBuildAreaIdToWgoIdsCache()
	{
		customBuildAreaIdToWgoIds.Clear();
		foreach (BuildingDef buildingDef in buildingDefs)
		{
			if (!string.IsNullOrEmpty(buildingDef.customBuildAreaId))
			{
				if (!customBuildAreaIdToWgoIds.ContainsKey(buildingDef.customBuildAreaId))
				{
					customBuildAreaIdToWgoIds.Add(buildingDef.customBuildAreaId, new List<string>());
				}
				customBuildAreaIdToWgoIds[buildingDef.customBuildAreaId].Add(buildingDef.wgoId);
			}
		}
	}

	private void CreateGardenGrowingCraftsCache()
	{
		gardenGrowingCrafts.Clear();
		foreach (CraftDef craftDef in craftDefs)
		{
			if (craftDef.id.StartsWith("garden_") && craftDef.id.Contains("_growing"))
			{
				gardenGrowingCrafts.TryAdd(craftDef.id, craftDef);
				if (craftDef.id.Contains("grape") || craftDef.id.Contains("hop"))
				{
					gardenGrowingCraftTypes.TryAdd(craftDef.id, CraftParamsData.GardenType.Vineyard);
				}
				else
				{
					gardenGrowingCraftTypes.TryAdd(craftDef.id, CraftParamsData.GardenType.None);
				}
			}
		}
	}

	private void CreateQuestDefByFinishPhraseCache()
	{
		if (FinishPhrasesByWgoParser.PhrasesByWgoData == null)
		{
			return;
		}
		foreach (QuestDef questDef in questDefs)
		{
			if (!string.IsNullOrEmpty(questDef.finishCheck.phrase) && FinishPhrasesByWgoParser.PhrasesByWgoData.finishPhrasesByWgo.Find((PhrasesByWgo x) => x.phrases.Contains(questDef.finishCheck.phrase)) != null)
			{
				questDefByFinishPhrase.TryAdd(questDef.finishCheck.phrase, questDef);
			}
		}
	}

	private void CreateWgoIdsByGroupCache()
	{
		foreach (WGODef wgoDef in wgoDefs)
		{
			if (!string.IsNullOrEmpty(wgoDef.wgoGroup))
			{
				if (!wgoIdsByGroup.TryGetValue(wgoDef.wgoGroup, out var value))
				{
					wgoIdsByGroup.Add(wgoDef.wgoGroup, new List<string> { wgoDef.id });
				}
				else
				{
					value.Add(wgoDef.id);
				}
			}
		}
	}
}
