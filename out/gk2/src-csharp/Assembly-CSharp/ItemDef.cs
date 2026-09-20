using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemDef : BalanceBaseObject
{
	public enum QualityType
	{
		None,
		Star
	}

	public const string EMPTY_ITEM_ID = "empty";

	public const string FAITH_ITEM_ID = "faith";

	[AutoParse("group")]
	public List<string> itemGroupIds = new List<string>();

	[AutoParse("stack_count")]
	public int stackCount;

	[SerializeField]
	[AutoParse("custom_icon")]
	private string customIcon;

	[AutoParse("type")]
	public ItemType type;

	public ItemSize itemSize;

	[AutoParse("inventory_size")]
	public int inventorySize;

	[AutoParse("quality")]
	public int quality;

	[AutoParse("quality_type")]
	public QualityType qualityType;

	[AutoParse("quality_icon")]
	public string qualityIcon;

	[AutoParse("talent_bonus")]
	public int talentBonus;

	[AutoParse("talent_type")]
	public string talentType;

	[AutoParse("talent_value")]
	public int talentValue;

	[AutoParse("can_be_used")]
	[LazyExpressionPureValueType(PureValueType.Bool)]
	public LazyExpression canBeUsed = new LazyExpression();

	[AutoParse("stay_on_use")]
	public bool stayOnUse;

	[AutoParse("add_res_on_use")]
	public List<ExpressionGameRes> gameResOnUse = new List<ExpressionGameRes>();

	[AutoParse("exp_on_use")]
	public List<LazyExpression> onUseExpressions;

	[AutoParse("on_use_sound")]
	public string onUseSound;

	[AutoParse("cannot_be_destr")]
	[LazyExpressionPureValueType(PureValueType.Bool)]
	public LazyExpression canNotBeDestroyed = new LazyExpression();

	[AutoParse("has_durability")]
	public bool hasDurability;

	[AutoParse("dur_decrease_on_use")]
	public float durDecreaseOnUse;

	[AutoParse("can_be_used_in_alchemy")]
	public bool canBeUsedInAlchemy;

	[AutoParse("runes_r")]
	public LazyExpression runesRed = new LazyExpression();

	[AutoParse("runes_g")]
	public LazyExpression runesGreen = new LazyExpression();

	[AutoParse("runes_b")]
	public LazyExpression runesBlue = new LazyExpression();

	[AutoParse("on_drop_collected")]
	public List<LazyExpression> onDropCollected = new List<LazyExpression>();

	[AutoParse("is_linked_to_wgo")]
	public bool isLinkedToWgo;

	[AutoParse("red_skulls")]
	public int redSkulls;

	[AutoParse("white_skulls")]
	public int whiteSkulls;

	[AutoParse("body_linked_perk")]
	public string bodyLinkedPerk;

	public int redSkullsMinCollar;

	public int redSkullsMaxCollar;

	public int whiteSkullsMinCollar;

	public int whiteSkullsMaxCollar;

	[AutoParse("is_product")]
	public bool isProduct;

	[AutoParse("base_price")]
	public int basePrice;

	[AutoParse("is_static_cost")]
	public bool isStaticCost;

	[AutoParse("bag_item_groups")]
	public List<string> bagItemGroups;

	[AutoParse("bag_size_x")]
	public int bagSizeX;

	[AutoParse("bag_size_y")]
	public int bagSizeY;

	[AutoParse("stamina_cost")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression staminaCost = new LazyExpression();

	[AutoParse("damage")]
	public LazyExpression damage;

	[AutoParse("atk_range")]
	public float atkRange;

	[AutoParse("atk_pause")]
	public float atkPause;

	[AutoParse("knockback_force")]
	public float knockbackForce;

	[AutoParse("target_filter_dock_point_tag")]
	public DockPointTag dockPointTag;

	[AutoParse("expr_on_sell")]
	public List<LazyExpression> expressionsOnSell = new List<LazyExpression>();

	[AutoParse("expr_on_buy")]
	public List<LazyExpression> expressionsOnBuy = new List<LazyExpression>();

	public bool isBag;

	public bool isSeed;

	public bool isFertilizer;

	public bool isBattlePotion;

	public bool isTechPoint;

	public bool isTool;

	public bool isWeapon;

	public bool isFuel;

	public bool isMainOrgan;

	public bool isOrganMistake;

	public string iconId;

	public int bagSize;

	public int sortOrder;

	public List<string> talentIds = new List<string>();

	private List<PerkDef> perksOnUseCache;

	private bool perksCached;

	private Vector3Int cachedBoostRunesAsVector3Int;

	public bool CanBeUsed
	{
		get
		{
			if (!canBeUsed.HasExpression)
			{
				return false;
			}
			return canBeUsed.EvaluateBool();
		}
	}

	public bool CanBePinnedToHotBar
	{
		get
		{
			if (!CanBeUsed && !isSeed && !isFertilizer)
			{
				return isBattlePotion;
			}
			return true;
		}
	}

	public bool CanNotBeDestroyed => canNotBeDestroyed.EvaluateBool();

	public bool IsAnimationDrivenTool => true;

	public ItemDef Copy()
	{
		return new ItemDef
		{
			itemGroupIds = itemGroupIds,
			stackCount = stackCount,
			customIcon = customIcon,
			type = type,
			itemSize = itemSize,
			inventorySize = inventorySize,
			quality = quality,
			qualityType = qualityType,
			talentBonus = talentBonus,
			talentType = talentType,
			talentValue = talentValue,
			canBeUsed = canBeUsed,
			gameResOnUse = gameResOnUse,
			onUseExpressions = onUseExpressions,
			canNotBeDestroyed = canNotBeDestroyed,
			hasDurability = hasDurability,
			durDecreaseOnUse = durDecreaseOnUse,
			canBeUsedInAlchemy = canBeUsedInAlchemy,
			runesRed = runesRed,
			runesGreen = runesGreen,
			runesBlue = runesBlue,
			onDropCollected = onDropCollected,
			isLinkedToWgo = isLinkedToWgo,
			redSkulls = redSkulls,
			whiteSkulls = whiteSkulls,
			isProduct = isProduct,
			basePrice = basePrice,
			isStaticCost = isStaticCost,
			bagItemGroups = bagItemGroups,
			bagSizeX = bagSizeX,
			bagSizeY = bagSizeY,
			sortOrder = sortOrder,
			talentIds = talentIds,
			isBag = isBag,
			isSeed = isSeed,
			isFertilizer = isFertilizer,
			isTechPoint = isTechPoint,
			isTool = isTool,
			isWeapon = isWeapon,
			isFuel = isFuel,
			isMainOrgan = isMainOrgan,
			isOrganMistake = isOrganMistake,
			iconId = iconId
		};
	}

	public GameRes GetGameResOnUse()
	{
		GameRes gameRes = new GameRes();
		for (int i = 0; i < gameResOnUse.Count; i++)
		{
			ExpressionGameRes expressionGameRes = gameResOnUse[i];
			float value = expressionGameRes.expression.EvaluateFloat();
			gameRes.Add(expressionGameRes.name, value);
		}
		return gameRes;
	}

	public float GetGameResOnUse(string resId)
	{
		float num = 0f;
		for (int i = 0; i < gameResOnUse.Count; i++)
		{
			ExpressionGameRes expressionGameRes = gameResOnUse[i];
			if (expressionGameRes.name == resId)
			{
				num += expressionGameRes.expression.EvaluateFloat();
			}
		}
		return num;
	}

	public bool HasGameResOnUse(string resId)
	{
		for (int i = 0; i < gameResOnUse.Count; i++)
		{
			if (gameResOnUse[i].name == resId)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyGameResOnUse()
	{
		if (gameResOnUse.Count > 0)
		{
			return !GetGameResOnUse().IsEmpty();
		}
		return false;
	}

	public bool CanBeInsertedInBag(ItemDef bag)
	{
		if (isBag)
		{
			return false;
		}
		foreach (string bagItemGroup in bag.bagItemGroups)
		{
			if (itemGroupIds.Contains(bagItemGroup))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanItemBeEquipped()
	{
		ItemType itemType = type;
		if ((uint)(itemType - 1) <= 4u || (uint)(itemType - 11) <= 1u || (uint)(itemType - 22) <= 5u)
		{
			return true;
		}
		return false;
	}

	public bool IsFightingEquipment()
	{
		ItemType itemType = type;
		return itemType == ItemType.BodyArmor || itemType == ItemType.Sword || itemType == ItemType.Bow;
	}

	public Vector3Int GetRunesAsVector3Int()
	{
		if (cachedBoostRunesAsVector3Int == default(Vector3Int))
		{
			cachedBoostRunesAsVector3Int = new Vector3Int(runesRed.EvaluateInt(), runesGreen.EvaluateInt(), runesBlue.EvaluateInt());
		}
		return cachedBoostRunesAsVector3Int;
	}

	public string GetRunesAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = runesRed.EvaluateInt();
		int num2 = runesGreen.EvaluateInt();
		int num3 = runesBlue.EvaluateInt();
		if (num > 0)
		{
			stringBuilder.Append(string.Format("{0}{1}", "rune_r".FontIcon(), num));
		}
		if (num2 > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format("{0}{1}", "rune_g".FontIcon(), num2));
		}
		if (num3 > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format("{0}{1}", "rune_b".FontIcon(), num3));
		}
		return stringBuilder.ToString();
	}

	public string GetSkullsAsString(TextStyle minusStyle)
	{
		return GetSkullsRangeAsString(redSkulls, redSkulls, whiteSkulls, whiteSkulls, minusStyle, null);
	}

	public static string GetSkullsRangeAsString(int redMin, int redMax, int whiteMin, int whiteMax, TextStyle minusStyle, TextStyle valueStyle)
	{
		string result = string.Empty;
		AppendSkullRange(ref result, redMin, redMax, "rskull", minusStyle, valueStyle);
		AppendSkullRange(ref result, whiteMin, whiteMax, "skull", minusStyle, valueStyle);
		return result;
	}

	private static void AppendSkullRange(ref string result, int min, int max, string iconId, TextStyle minusStyle, TextStyle valueStyle)
	{
		if (min == 0 && max == 0)
		{
			return;
		}
		if (!string.IsNullOrEmpty(result))
		{
			result += " ";
		}
		if (min == max)
		{
			if (min < 0)
			{
				result += minusStyle.ApplyStyleToString("-");
			}
			for (int i = 0; i < Math.Abs(min); i++)
			{
				result += iconId.FontIcon();
			}
		}
		else if (max <= 0)
		{
			result += minusStyle.ApplyStyleToString("-");
			int min2 = Math.Min(Math.Abs(min), Math.Abs(max));
			int max2 = Math.Max(Math.Abs(min), Math.Abs(max));
			result = result + iconId.FontIcon() + FormatSkullRangeValue(min2, max2, valueStyle);
		}
		else
		{
			_ = 0;
			result = result + iconId.FontIcon() + FormatSkullRangeValue(min, max, valueStyle);
		}
	}

	private static string FormatSkullRangeValue(int min, int max, TextStyle valueStyle)
	{
		string text = $"{min}-{max}";
		if (!(valueStyle != null))
		{
			return text;
		}
		return valueStyle.ApplyStyleToString(text);
	}

	public List<PerkDef> GetPerksOnUse()
	{
		if (!perksCached)
		{
			perksCached = true;
			perksOnUseCache = new List<PerkDef>();
			for (int i = 0; i < onUseExpressions.Count; i++)
			{
				string rawExpressionString = onUseExpressions[i].GetRawExpressionString();
				if (rawExpressionString.StartsWith("AddPerk"))
				{
					PerkDef data = GameBalance.Me.GetData<PerkDef>(Regex.Match(rawExpressionString, "\\\"([^\\\"]+)\\\"").Groups[1].Value);
					if (data != null)
					{
						perksOnUseCache.Add(data);
					}
				}
			}
		}
		return perksOnUseCache;
	}

	public string GetItemCraftPrefix()
	{
		if (itemGroupIds.Contains("bodypart"))
		{
			return LLBase.L("ui_extract");
		}
		if (type == ItemType.Embalm)
		{
			return LLBase.L("ui_insert");
		}
		return LLBase.L("ui_create");
	}

	public string GetHeader()
	{
		if (qualityType == QualityType.Star)
		{
			if (LLBase.HasL(id))
			{
				return LLBase.L(id);
			}
			return LLBase.L(id.Split(':')[0]);
		}
		return LLBase.L(id);
	}

	public string GetDescription()
	{
		return LLBase.L(GetDescriptionLocale());
	}

	public string GetDescriptionLocale()
	{
		if (qualityType == QualityType.Star)
		{
			string text = id + "_d";
			if (LLBase.HasL(text))
			{
				return text;
			}
			return id.Split(':')[0] + "_d";
		}
		return id + "_d";
	}

	public ItemDef GetMistakeForThisItem()
	{
		string value = null;
		switch (type)
		{
		case ItemType.Bones:
			value = "surgeon_mistake_bones";
			break;
		case ItemType.Brain:
			value = "surgeon_mistake_brain";
			break;
		case ItemType.Heart:
			value = "surgeon_mistake_heart";
			break;
		case ItemType.Guts:
			value = "surgeon_mistake_guts";
			break;
		case ItemType.Skin:
			value = "surgeon_mistake_skin";
			break;
		case ItemType.Skull:
			value = "surgeon_mistake_skull";
			break;
		}
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}
		return GameBalance.Me.GetData<ItemDef>(value);
	}

	public bool SkullsInBorders(int white, int red)
	{
		if (type != ItemType.Collar)
		{
			Debug.LogError("Do not check skull borders on not collar items!!!");
			return false;
		}
		if (redSkullsMaxCollar > redSkullsMinCollar && (red < redSkullsMinCollar || red > redSkullsMaxCollar))
		{
			return false;
		}
		if (whiteSkullsMaxCollar > whiteSkullsMinCollar && (white < whiteSkullsMinCollar || white > whiteSkullsMaxCollar))
		{
			return false;
		}
		return true;
	}
}
