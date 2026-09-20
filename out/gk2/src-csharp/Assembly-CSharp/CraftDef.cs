using System;
using System.Collections.Generic;
using System.Text;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CraftDef : CraftDefBase
{
	[SerializeField]
	private string description;

	[AutoParse("skip_queue")]
	public bool skipQueue;

	[AutoParse("disable_add_to_queue")]
	public bool isAddToQueueDisabled;

	[AutoParse("bronze_level")]
	public int bronzeLevel;

	[AutoParse("silver_level")]
	public int silverLevel;

	[AutoParse("gold_level")]
	public int goldLevel;

	[AutoParse("is_needs_unlock")]
	public bool isNeedsUnlock;

	[AutoParse("is_one_time_craft")]
	public bool isOneTimeCraft;

	[AutoParse("is_forced_difficult_craft")]
	public bool isForcedDifficultCraft;

	[AutoParse("is_obj_destroy_craft")]
	public bool isObjDestroyCraft;

	[AutoParse("disable_multicraft")]
	public bool isMulticraftDisabled;

	[AutoParse("do_not_show_in_tooltips")]
	public bool doNotShowInTooltips;

	[AutoParse("is_conveyor_craft")]
	public bool isConveyorCraft;

	[AutoParse("tab_id")]
	public string tabId;

	[AutoParse("custom_action")]
	public ItemType customItemTypeAction;

	[AutoParse("transfer_destination_start")]
	public TransferDestination transferDestinationStart;

	[AutoParse("destination_item_start")]
	public string destinationItemStart;

	[AutoParse("transfer_destination_end")]
	public TransferDestination transferDestinationEnd;

	[AutoParse("destination_item_end")]
	public string destinationItemEnd;

	[AutoParse("transfer_needs_on_start")]
	public bool transferNeedsToDestinationOnStart;

	[AutoParse("transfer_needs_on_finish")]
	public bool transferNeedsToDestinationOnFinish;

	[AutoParse("use_dur_of_needs")]
	public float needItemsDurabilityUse;

	[AutoParse("use_dur_of_needs_idx")]
	public int needItemsDurabilityUseIndex = -1;

	public NeedItemData durabilityUseItem;

	public bool hasDurabilityUseItem;

	[AutoParse("drop_from_wgo_start")]
	public List<NeedItemData> dropFromWgoItemsStart = new List<NeedItemData>();

	[AutoParse("drop_from_wgo_end")]
	public List<NeedItemData> dropFromWgoItemsEnd = new List<NeedItemData>();

	[AutoParse("autopsy_type_craft")]
	public AutopsyTypeCraft autopsyTypeCraft;

	public string autopsyItemId;

	[AutoParse("remove_from_wgo")]
	public List<NeedItemData> removeItemsFromWgo = new List<NeedItemData>();

	[AutoParse("tech_r")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression techRed = new LazyExpression();

	[AutoParse("tech_g")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression techGreen = new LazyExpression();

	[AutoParse("tech_b")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression techBlue = new LazyExpression();

	[AutoParse("replace_to_wgo")]
	public string replaceWgoId;

	[AutoParse("transfer_data")]
	public bool transferDataOnReplace;

	[AutoParse("expression_on_replace")]
	public List<LazyExpression> executeOnReplace = new List<LazyExpression>();

	[AutoParse("fx_on_replace")]
	public string worldFxOnReplace;

	[AutoParse("end_script")]
	public string globalScriptOnCraftEnd;

	[AutoParse("custom_craft_result_icon")]
	[LazyExpressionPureValueType(PureValueType.String)]
	public LazyExpression customCraftResultIcon = new LazyExpression();

	public string iconId;

	[AutoParse("on_craft_add_queue_expressions")]
	public List<LazyExpression> onCraftAddQueueExpressions = new List<LazyExpression>();

	[AutoParse("on_craft_start_expressions")]
	public List<LazyExpression> onCraftStartExpressions = new List<LazyExpression>();

	[AutoParse("on_craft_end_expressions")]
	public List<LazyExpression> onCraftEndExpressions = new List<LazyExpression>();

	public List<GameResPerProgress> gameresPerSuccessfulProgress;

	[AutoParse("zombie_speed_item_modificator")]
	public GameRes zombieSpeedItemModificators = new GameRes();

	private ItemDef cachedResultingItemDef;

	private List<ItemDef> cachedPossibleResultingItemDefs;

	private Vector3Int cachedBoostRunesAsVector3Int;

	public bool IsMultipleCraftsDisabled
	{
		get
		{
			if (!isMulticraftDisabled)
			{
				return isOneTimeCraft;
			}
			return true;
		}
	}

	public override AutopsyTypeCraft AutopsyType => autopsyTypeCraft;

	public string Description
	{
		get
		{
			string text = LLBase.L(description);
			if (id.EndsWith("_boost"))
			{
				text += "\n";
				for (int i = 0; i < addWgoParamsOnStart.List.Count; i++)
				{
					GameResAtom gameResAtom = addWgoParamsOnStart.List[i];
					switch (gameResAtom.type)
					{
					case "rune_r":
					case "rune_b":
					case "rune_g":
						text += gameResAtom.ToFormattedString(showOnlyType: true);
						break;
					}
				}
			}
			return text;
		}
	}

	public CraftDef Copy()
	{
		CraftDef craftDef = new CraftDef();
		craftDef.id = id;
		craftDef.description = description;
		craftDef.craftsIn = craftsIn;
		craftDef.isHidden = isHidden;
		craftDef.needItems = needItems;
		craftDef.needItemsFromWgo = needItemsFromWgo;
		craftDef.isStarCraft = isStarCraft;
		craftDef.isFuelCraft = isFuelCraft;
		craftDef.haveStarCraftOutput = haveStarCraftOutput;
		craftDef.duration = duration;
		craftDef.energyPerTick = energyPerTick;
		craftDef.insanityPerTick = insanityPerTick;
		craftDef.insanityLock = insanityLock;
		craftDef.talentLock = talentLock;
		craftDef.linkedPerks = linkedPerks;
		craftDef.insanityLock = insanityLock;
		craftDef.outputItems = outputItems;
		craftDef.setWgoParamsOnStart = setWgoParamsOnStart;
		craftDef.setWgoParamsOnFinish = setWgoParamsOnFinish;
		craftDef.addWgoParamsOnStart = addWgoParamsOnStart;
		craftDef.addWgoParamsOnFinish = addWgoParamsOnFinish;
		craftDef.addItemsToWgoOnStart = addItemsToWgoOnStart;
		craftDef.addItemsToWgoOnFinish = addItemsToWgoOnFinish;
		craftDef.isAddToQueueDisabled = isAddToQueueDisabled;
		craftDef.skipQueue = skipQueue;
		craftDef.bronzeLevel = bronzeLevel;
		craftDef.silverLevel = silverLevel;
		craftDef.goldLevel = goldLevel;
		craftDef.isNeedsUnlock = isNeedsUnlock;
		craftDef.isOneTimeCraft = isOneTimeCraft;
		craftDef.isForcedDifficultCraft = isForcedDifficultCraft;
		craftDef.isObjDestroyCraft = isObjDestroyCraft;
		craftDef.isMulticraftDisabled = isMulticraftDisabled;
		craftDef.customItemTypeAction = customItemTypeAction;
		craftDef.transferDestinationStart = transferDestinationStart;
		craftDef.destinationItemStart = destinationItemStart;
		craftDef.transferDestinationEnd = transferDestinationEnd;
		craftDef.destinationItemEnd = destinationItemEnd;
		craftDef.transferNeedsToDestinationOnStart = transferNeedsToDestinationOnStart;
		craftDef.transferNeedsToDestinationOnFinish = transferNeedsToDestinationOnFinish;
		craftDef.needItemsDurabilityUse = needItemsDurabilityUse;
		craftDef.needItemsDurabilityUseIndex = needItemsDurabilityUseIndex;
		craftDef.durabilityUseItem = durabilityUseItem;
		craftDef.hasDurabilityUseItem = hasDurabilityUseItem;
		craftDef.dropFromWgoItemsStart = dropFromWgoItemsStart;
		craftDef.dropFromWgoItemsEnd = dropFromWgoItemsEnd;
		craftDef.removeItemsFromWgo = removeItemsFromWgo;
		craftDef.autopsyTypeCraft = autopsyTypeCraft;
		craftDef.autopsyItemId = autopsyItemId;
		craftDef.techRed = techRed;
		craftDef.techGreen = techGreen;
		craftDef.techBlue = techBlue;
		craftDef.replaceWgoId = replaceWgoId;
		craftDef.transferDataOnReplace = transferDataOnReplace;
		craftDef.executeOnReplace = executeOnReplace;
		craftDef.worldFxOnReplace = worldFxOnReplace;
		craftDef.globalScriptOnCraftEnd = globalScriptOnCraftEnd;
		craftDef.customCraftResultIcon = customCraftResultIcon;
		craftDef.iconId = iconId;
		craftDef.onCraftAddQueueExpressions = onCraftAddQueueExpressions;
		craftDef.onCraftStartExpressions = onCraftStartExpressions;
		craftDef.onCraftEndExpressions = onCraftEndExpressions;
		craftDef.gameresPerSuccessfulProgress = gameresPerSuccessfulProgress;
		return craftDef;
	}

	public string GetCraftResultIcon(WgoData wgoData = null)
	{
		if (AlchemyMixDef.IsUnknownMixResult(id))
		{
			return "i_slot-question";
		}
		if (customCraftResultIcon != null && customCraftResultIcon.HasExpression)
		{
			string text = customCraftResultIcon.Evaluate(wgoData);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		ItemDef itemDef = TryGetResultingItemDef(tryGetFromPreview: false);
		if (itemDef != null)
		{
			return itemDef.iconId;
		}
		return GetOutputPreviewFromItemsOrLinkedGrowing(wgoData)?.IconId ?? string.Empty;
	}

	private OutputPreview GetOutputPreviewFromItems(WgoData wgoData = null)
	{
		OutputPreview outputPreview = (isFuelCraft ? addItemsToWgoOnFinish.GetOutputPreview(id, wgoData) : base.GetOutputPreview(wgoData));
		if (outputPreview == null)
		{
			outputPreview = addItemsToWgoOnFinish.GetOutputPreview(id, wgoData);
		}
		return outputPreview;
	}

	private OutputPreview GetOutputPreviewFromItemsOrLinkedGrowing(WgoData wgoData = null)
	{
		OutputPreview outputPreviewFromItems = GetOutputPreviewFromItems(wgoData);
		if (outputPreviewFromItems != null)
		{
			return outputPreviewFromItems;
		}
		return TryGetLinkedGardenGrowingOutputPreview(wgoData);
	}

	private OutputPreview TryGetLinkedGardenGrowingOutputPreview(WgoData wgoData)
	{
		if (string.IsNullOrEmpty(id) || !id.Contains("_planting"))
		{
			return null;
		}
		string text = id.Replace("_planting", "_growing");
		if (text == id || GameBalance.Me == null)
		{
			return null;
		}
		if (!GameBalance.Me.gardenGrowingCrafts.TryGetValue(text, out var value) || value == null)
		{
			return null;
		}
		OutputPreview outputPreviewFromItems = value.GetOutputPreviewFromItems(wgoData);
		if (outputPreviewFromItems == null)
		{
			return null;
		}
		return new OutputPreview(id, outputPreviewFromItems.itemId, outputPreviewFromItems.isStarOutput, outputPreviewFromItems.count, outputPreviewFromItems.quality, outputPreviewFromItems.customIconId);
	}

	public override OutputPreview GetOutputPreview(WgoData wgoData = null)
	{
		OutputPreview outputPreview = GetOutputPreviewFromItemsOrLinkedGrowing(wgoData);
		if (outputPreview == null)
		{
			outputPreview = new OutputPreview(id, "", isStarCraft, 1, (!isStarCraft) ? (-1) : 0, id);
		}
		if (customCraftResultIcon != null && customCraftResultIcon.HasExpression)
		{
			string text = customCraftResultIcon.Evaluate(wgoData);
			if (!string.IsNullOrEmpty(text))
			{
				outputPreview.customIconId = text;
			}
		}
		ItemDef itemDef = TryGetResultingItemDef(tryGetFromPreview: false);
		if (!isStarCraft && itemDef != null && itemDef.qualityType == ItemDef.QualityType.Star)
		{
			outputPreview.quality = itemDef.quality;
		}
		if (AlchemyMixDef.IsUnknownMixResult(id))
		{
			outputPreview.customIconId = "i_slot-question";
			outputPreview.itemId = string.Empty;
			outputPreview.quality = -1;
		}
		return outputPreview;
	}

	public ItemDef TryGetResultingItemDef(bool tryGetFromPreview = true)
	{
		if (!IsCachedNullOrEmpty())
		{
			return cachedResultingItemDef;
		}
		if (outputItems.chanceOutputItems.Count == 0 && outputItems.groupChanceOutputItems.Count == 0)
		{
			if (dropFromWgoItemsEnd.Count > 0)
			{
				NeedItemData needItemData = dropFromWgoItemsEnd[0];
				switch (needItemData.groupType)
				{
				case ItemGroup.None:
					cachedResultingItemDef = GameBalance.Me.GetDataOrNull<ItemDef>(needItemData.id);
					break;
				case ItemGroup.Common:
					cachedResultingItemDef = GameBalance.Me.groupItemsCache[needItemData.id][0];
					break;
				case ItemGroup.Star:
					cachedResultingItemDef = GameBalance.Me.starGroupItemsCache[needItemData.id][0];
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			if (IsCachedNullOrEmpty() && removeItemsFromWgo.Count > 0)
			{
				NeedItemData needItemData2 = removeItemsFromWgo[0];
				switch (needItemData2.groupType)
				{
				case ItemGroup.None:
					cachedResultingItemDef = GameBalance.Me.GetDataOrNull<ItemDef>(needItemData2.id);
					break;
				case ItemGroup.Common:
					cachedResultingItemDef = GameBalance.Me.groupItemsCache[needItemData2.id][0];
					break;
				case ItemGroup.Star:
					cachedResultingItemDef = GameBalance.Me.starGroupItemsCache[needItemData2.id][0];
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		else if (outputItems.chanceOutputItems.Count > 0)
		{
			if (outputItems.chanceOutputItems[0].isStarGroup)
			{
				cachedResultingItemDef = GameBalance.Me.starGroupItemsCache[outputItems.chanceOutputItems[0].id][0];
			}
			else
			{
				cachedResultingItemDef = GameBalance.Me.GetDataOrNull<ItemDef>(outputItems.chanceOutputItems[0].id);
			}
		}
		else if (outputItems.groupChanceOutputItems.Count > 0)
		{
			if (outputItems.groupChanceOutputItems[0].chanceItems[0].isStarGroup)
			{
				cachedResultingItemDef = GameBalance.Me.starGroupItemsCache[outputItems.groupChanceOutputItems[0].chanceItems[0].id][0];
			}
			else
			{
				cachedResultingItemDef = GameBalance.Me.GetDataOrNull<ItemDef>(outputItems.groupChanceOutputItems[0].chanceItems[0].id);
			}
		}
		if (tryGetFromPreview && IsCachedNullOrEmpty())
		{
			cachedResultingItemDef = GameBalance.Me.GetData<ItemDef>(GetOutputPreview().itemId);
		}
		return cachedResultingItemDef;
		bool IsCachedNullOrEmpty()
		{
			if (cachedResultingItemDef != null)
			{
				return string.IsNullOrEmpty(cachedResultingItemDef.id);
			}
			return true;
		}
	}

	public List<ItemDef> GetPossibleResultingItemDefs(bool tryGetFromPreview = true)
	{
		if (cachedPossibleResultingItemDefs != null && cachedPossibleResultingItemDefs.Count > 0)
		{
			return cachedPossibleResultingItemDefs;
		}
		List<ItemDef> list = new List<ItemDef>();
		CollectPossibleResultingItemDefsFromOutput(outputItems, list);
		if (list.Count == 0)
		{
			CollectPossibleResultingItemDefsFromNeeds(dropFromWgoItemsEnd, list);
		}
		if (list.Count == 0)
		{
			CollectPossibleResultingItemDefsFromNeeds(removeItemsFromWgo, list);
		}
		if (list.Count == 0)
		{
			ItemDef itemDef = TryGetResultingItemDef(tryGetFromPreview);
			if (itemDef != null && !string.IsNullOrEmpty(itemDef.id))
			{
				list.Add(itemDef);
			}
		}
		if (list.Count > 0)
		{
			cachedPossibleResultingItemDefs = list;
		}
		return list;
	}

	private static void CollectPossibleResultingItemDefsFromOutput(OutputItems items, List<ItemDef> result)
	{
		if (items == null)
		{
			return;
		}
		for (int i = 0; i < items.chanceOutputItems.Count; i++)
		{
			AddPossibleResultingItemDef(items.chanceOutputItems[i], result);
		}
		for (int j = 0; j < items.groupChanceOutputItems.Count; j++)
		{
			List<ChanceOutputItem> chanceItems = items.groupChanceOutputItems[j].chanceItems;
			for (int k = 0; k < chanceItems.Count; k++)
			{
				AddPossibleResultingItemDef(chanceItems[k], result);
			}
		}
	}

	private static void AddPossibleResultingItemDef(ChanceOutputItem chanceOutputItem, List<ItemDef> result)
	{
		if (chanceOutputItem.isStarGroup)
		{
			if (GameBalance.Me.starGroupItemsCache.TryGetValue(chanceOutputItem.id, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					result.Add(value[i]);
				}
			}
		}
		else
		{
			ItemDef dataOrNull = GameBalance.Me.GetDataOrNull<ItemDef>(chanceOutputItem.id);
			if (dataOrNull != null)
			{
				result.Add(dataOrNull);
			}
		}
	}

	private static void CollectPossibleResultingItemDefsFromNeeds(List<NeedItemData> needItems, List<ItemDef> result)
	{
		if (needItems == null || needItems.Count == 0)
		{
			return;
		}
		NeedItemData needItemData = needItems[0];
		switch (needItemData.groupType)
		{
		case ItemGroup.None:
		{
			ItemDef dataOrNull = GameBalance.Me.GetDataOrNull<ItemDef>(needItemData.id);
			if (dataOrNull != null)
			{
				result.Add(dataOrNull);
			}
			break;
		}
		case ItemGroup.Common:
		{
			if (GameBalance.Me.groupItemsCache.TryGetValue(needItemData.id, out var value2))
			{
				for (int j = 0; j < value2.Count; j++)
				{
					result.Add(value2[j]);
				}
			}
			break;
		}
		case ItemGroup.Star:
		{
			if (GameBalance.Me.starGroupItemsCache.TryGetValue(needItemData.id, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					result.Add(value[i]);
				}
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public string GetBoostRunesAsString()
	{
		Vector3Int boostRunesAsVector3Int = GetBoostRunesAsVector3Int();
		StringBuilder stringBuilder = new StringBuilder();
		int x = boostRunesAsVector3Int.x;
		int y = boostRunesAsVector3Int.y;
		int z = boostRunesAsVector3Int.z;
		if (x > 0)
		{
			stringBuilder.Append(string.Format("{0}{1}", "rune_r".FontIcon(), x));
		}
		if (y > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format("{0}{1}", "rune_g".FontIcon(), y));
		}
		if (z > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format("{0}{1}", "rune_b".FontIcon(), z));
		}
		return stringBuilder.ToString();
	}

	public Vector3Int GetBoostRunesAsVector3Int()
	{
		if (id.EndsWith("_boost"))
		{
			if (cachedBoostRunesAsVector3Int != default(Vector3Int))
			{
				return cachedBoostRunesAsVector3Int;
			}
			cachedBoostRunesAsVector3Int = new Vector3Int(addWgoParamsOnStart.GetInt("rune_r"), addWgoParamsOnStart.GetInt("rune_g"), addWgoParamsOnStart.GetInt("rune_b"));
		}
		return cachedBoostRunesAsVector3Int;
	}

	public int GetQualityForGardenProgressTick(int tick)
	{
		int result = -1;
		foreach (GameResPerProgress item in gameresPerSuccessfulProgress)
		{
			if (item.sucessfulProgressTick == tick)
			{
				if (item.gameRes.GetInt("crop") > 0)
				{
					result = 0;
					break;
				}
				if (item.gameRes.GetInt("crop_b") > 0)
				{
					result = 1;
					break;
				}
				if (item.gameRes.GetInt("crop_s") > 0)
				{
					result = 2;
					break;
				}
				if (item.gameRes.GetInt("crop_g") > 0)
				{
					result = 3;
					break;
				}
			}
		}
		return result;
	}
}
