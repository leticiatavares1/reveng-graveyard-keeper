using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LinqTools;
using UnityEngine;

[Serializable]
public class ItemDefinition : BalanceBaseObject
{
	public enum QualityType
	{
		Default,
		Stars
	}

	public enum ItemType
	{
		PseudoitemFirst = -1,
		None = 0,
		Axe = 1,
		Pickaxe = 2,
		Shovel = 3,
		Sword = 4,
		Hammer = 5,
		FishingRod = 6,
		Torch = 9,
		Hand = 10,
		Item = 11,
		HeadArmor = 12,
		BodyArmor = 13,
		Preach = 20,
		Bait = 31,
		Crate = 50,
		Rat = 60,
		RatBuff = 61,
		GraveStone = 101,
		GraveFence = 102,
		GraveCover = 103,
		Body = 200,
		BodyHead = 201,
		BodyBody = 202,
		BodyArmR = 203,
		BodyArmL = 204,
		BodyLegR = 205,
		BodyLegL = 206,
		BodyHeadPart = 210,
		BodyBodyPart = 220,
		BodyArmPart = 230,
		BodyLegPart = 250,
		BodyUniversalPart = 270,
		SoulBodyPart = 271,
		Soul = 280,
		ZombieWorker = 300,
		Bag = 400,
		GraveStoneReq = 10101,
		GraveFenceReq = 10102,
		GraveCoverReq = 10103,
		PseudoitemLast = 100000
	}

	public enum EquipmentType
	{
		None = 0,
		Axe = 1,
		Pickaxe = 2,
		Shovel = 3,
		Sword = 4,
		Hammer = 5,
		FishingRod = 6,
		HeadArmor = 12,
		BodyArmor = 13
	}

	public enum BagType
	{
		None,
		Universal,
		Alchemy,
		Farming,
		Fishing,
		Tools,
		Potions,
		Builder,
		Food
	}

	public class ItemDetails
	{
		public ItemDetailsAlchemy alchemy;

		public List<ObjectDefinition> crafts_in = new List<ObjectDefinition>();
	}

	public class ItemDetailsAlchemy
	{
		public enum DetailsType
		{
			Decompose,
			Slots
		}

		public DetailsType details_type;

		public List<List<bool>> slots = new List<List<bool>>();

		public List<int> decomposes = new List<int>();
	}

	public enum AlchemyType
	{
		None = 0,
		Powder = 1,
		Fluid = 2,
		Essence = 3,
		Universal = 9
	}

	[Serializable]
	public class ItemReplaceData
	{
		public string player_flag;

		public string replace_id;
	}

	private const int MAX_ALCHEMY_TIER = 3;

	public const int MAX_PRODUCT_TIER = 3;

	private static string[] _params_on_use_types = new string[3] { "hp", "energy", "sanity" };

	public string icon;

	public string custom_ovr_icon;

	public bool not_used;

	public ItemType type;

	public List<int> can_be_placed_on = new List<int>();

	public int stack_count;

	public GameRes parameters = new GameRes();

	public int item_size = 1;

	public int base_count;

	public int product_tier;

	public string run_script_after_drop;

	public bool destroy_after_drop;

	public float base_price;

	public float custom_sell_price_koeff;

	public bool is_static_cost;

	public float quality;

	public float quality_multiplyer;

	public QualityType quality_type;

	public float durability_decrease;

	public bool durability_decrease_on_use;

	public float durability_decrease_on_use_speed;

	public float durability_modificator;

	public bool has_durability;

	public bool is_update_children_durability;

	public float durability_modificator_for_children = 1f;

	public float efficiency;

	public float armor;

	public string on_use_script = "";

	public string on_use_snd = "";

	public List<SmartExpression> on_use_expressions = new List<SmartExpression>();

	public List<SmartExpression> on_trade_expressions = new List<SmartExpression>();

	public bool dont_break_on_zero_dur;

	public string dur_0_change;

	public List<Item> drop_on_use;

	public int q_plus;

	public int q_minus;

	public string q_hint;

	public bool player_cant_throw_out;

	public SmartExpression cooldown;

	public bool stay_on_use = true;

	public List<string> product_types = new List<string>();

	public int product_weight;

	public bool can_be_used;

	public bool close_inv_on_use;

	public bool autouse;

	public GameRes params_on_use = new GameRes();

	public SmartExpression tool_energy_k;

	public static readonly ItemDefinition none = new ItemDefinition
	{
		id = ""
	};

	public float rat_speed;

	public float rat_obedience;

	public float rat_speed_add;

	public float rat_obedience_add;

	public float rat_speed_multiply;

	public float rat_obedience_multiply;

	public bool can_insert_into_barmen;

	public float[] tavern_event_coeffs = new float[4];

	public BagType bag_type;

	public int bag_size_x;

	public int bag_size_y;

	public List<BagType> can_be_inserted_in_bag = new List<BagType>();

	private ItemDetails _details;

	private CraftDefinition _linked_craft;

	public SmartExpression show_q_hint;

	public ItemReplaceData item_replace;

	public AlchemyType alch_type;

	public bool is_small => item_size == 1;

	public bool is_big => item_size > 1;

	public bool is_crate => type == ItemType.Crate;

	public bool is_placeholder => id.Contains("placeholder");

	public bool is_bag => type == ItemType.Bag;

	public EquipmentType equipment_type => type switch
	{
		ItemType.Axe => EquipmentType.Axe, 
		ItemType.Pickaxe => EquipmentType.Pickaxe, 
		ItemType.Shovel => EquipmentType.Shovel, 
		ItemType.Sword => EquipmentType.Sword, 
		ItemType.Hammer => EquipmentType.Hammer, 
		ItemType.FishingRod => EquipmentType.FishingRod, 
		ItemType.HeadArmor => EquipmentType.HeadArmor, 
		ItemType.BodyArmor => EquipmentType.BodyArmor, 
		_ => EquipmentType.None, 
	};

	public bool is_placable => can_be_placed_on.Count > 0;

	public bool is_tool
	{
		get
		{
			if (type < ItemType.Hand)
			{
				return type != ItemType.None;
			}
			return false;
		}
	}

	public CraftDefinition linked_craft
	{
		get
		{
			if (_linked_craft == null || string.IsNullOrEmpty(_linked_craft.id))
			{
				_linked_craft = GameBalance.me.GetData<CraftDefinition>("pray:" + id);
				if (_linked_craft == null)
				{
					Debug.LogError("Craft for sermon not found: " + id);
				}
			}
			return _linked_craft;
		}
	}

	public string GetQualityString(Item real_item = null)
	{
		string text = $"{quality:0.#}";
		if (real_item != null && !real_item.GetItemQuality().EqualsTo(quality))
		{
			text = GJL.L("n_of", $"{real_item.GetItemQuality():0.#}", text);
		}
		return text;
	}

	public string GetItemDescription(Item real_item = null)
	{
		string text = id + "_d";
		string text2 = GJL.L(text);
		if (text2 == text && id.Contains(":"))
		{
			text2 = GJL.L(id.Substring(0, id.LastIndexOf(':')) + "_d");
		}
		text2 = LocalizedLabel.ColorizeTags(text2, LocalizedLabel.TextColor.SpeechBubble);
		string text3 = "";
		GameRes gameRes = new GameRes();
		if (on_use_expressions != null && on_use_expressions.Count > 0)
		{
			foreach (SmartExpression on_use_expression in on_use_expressions)
			{
				if (on_use_expression == null || on_use_expression.HasNoExpresion())
				{
					continue;
				}
				foreach (GameResAtom item in GameRes.ParseSmartExpression(on_use_expression).ToAtomList())
				{
					gameRes.Add(item);
				}
			}
		}
		string text4 = params_on_use.ToFormattedString(colorize_values: true, gameRes);
		if (!can_be_used && is_tool && !string.IsNullOrEmpty(text4))
		{
			text3 = text3.ConcatWithSeparator(GJL.L("tool_energy_spend", text4));
		}
		if (type == ItemType.Preach)
		{
			if (linked_craft == null)
			{
				Debug.LogError("Sermon " + id + " has no preaching craft!");
			}
			else
			{
				text3 = text3.ConcatWithSeparator(GJL.L("preach_params", "(cross)" + linked_craft.needs_quality));
			}
		}
		if (!string.IsNullOrEmpty(q_hint) && (!show_q_hint.has_expression || show_q_hint.EvaluateBoolean()))
		{
			text3 = text3.ConcatWithSeparator(GJL.L(q_hint, GetQualityString(real_item)));
		}
		if (is_tool)
		{
			if (type == ItemType.Sword)
			{
				int @int = parameters.GetInt("damage");
				if (@int > 0)
				{
					text3 = text3.ConcatWithSeparator(@int + " " + GJL.L("damage"));
				}
			}
			else
			{
				int toolEfficiencyPercent = GameBalance.me.GetToolEfficiencyPercent(this);
				if (toolEfficiencyPercent > 0)
				{
					text3 = text3.ConcatWithSeparator(GJL.L("tool_eff_hint", toolEfficiencyPercent + "%"));
				}
			}
		}
		if (can_be_used)
		{
			string text5 = "";
			foreach (SmartExpression on_use_expression2 in on_use_expressions)
			{
				Regex regex = new Regex("AddBuff\\(\"(.+?)\"\\)");
				string rawExpressionString = on_use_expression2.GetRawExpressionString();
				if (string.IsNullOrEmpty(rawExpressionString) || !rawExpressionString.Contains("AddBuff("))
				{
					continue;
				}
				Match match = regex.Match(rawExpressionString);
				if (!match.Success)
				{
					continue;
				}
				string text6 = match.Groups[1].Captures[0].ToString();
				BuffDefinition data = GameBalance.me.GetData<BuffDefinition>(text6);
				if (data != null)
				{
					text5 = "[c][C16000]" + data.GetLocalizedName() + "[-][/c]";
					string descriptionIfExists = data.GetDescriptionIfExists();
					if (!string.IsNullOrEmpty(descriptionIfExists))
					{
						text5 = text5 + " (" + descriptionIfExists + ")";
					}
				}
			}
			if (!string.IsNullOrEmpty(text5) || !string.IsNullOrEmpty(text4))
			{
				string text7 = text4;
				if (string.IsNullOrEmpty(text4))
				{
					text7 = text5;
				}
				else if (!string.IsNullOrEmpty(text5))
				{
					text7 = text7 + ", " + text5;
				}
				text3 = text3.ConcatWithSeparator(GJL.L("item_effect_on_use")) + " " + text7;
			}
		}
		if (real_item?.definition != null)
		{
			if (real_item.definition.type == ItemType.Rat)
			{
				foreach (Item allRatBuff in real_item.GetAllRatBuffs())
				{
					if (allRatBuff.definition.has_durability)
					{
						int num = (int)(allRatBuff.durability / allRatBuff.definition.durability_decrease);
						int num2 = num % 60;
						text3 = text3.ConcatWithSeparator(GJL.L("rat_status") + ": " + allRatBuff.definition.GetItemName() + "(" + num / 60 + ":" + ((num2 < 10) ? "0" : "") + num2 + ")");
						break;
					}
				}
				text3 = text3.ConcatWithSeparator(real_item.GetRatDescription(include_base: false));
			}
			else if (real_item.definition.type == ItemType.RatBuff)
			{
				string text8 = string.Empty;
				if (!real_item.definition.rat_speed_add.EqualsTo(0f, 0.01f))
				{
					text8 = text8 + ((real_item.definition.rat_speed_add > 0f) ? "+" : string.Empty) + real_item.definition.rat_speed_add + "(speed) ";
				}
				if (!real_item.definition.rat_obedience_add.EqualsTo(0f, 0.01f))
				{
					text8 = text8 + ((real_item.definition.rat_obedience_add > 0f) ? "+" : string.Empty) + real_item.definition.rat_obedience_add + "(obedience) ";
				}
				if (!real_item.definition.rat_speed_multiply.EqualsTo(1f, 0.01f))
				{
					text8 = text8 + ((real_item.definition.rat_speed_multiply > 0f) ? "+" : "") + (real_item.definition.rat_speed_multiply - 1f) * 100f + "%(speed) ";
				}
				if (!real_item.definition.rat_obedience_multiply.EqualsTo(1f, 0.01f))
				{
					text8 = text8 + ((real_item.definition.rat_obedience_multiply > 0f) ? "+" : "") + (real_item.definition.rat_obedience_multiply - 1f) * 100f + "%(obedience) ";
				}
				if (!string.IsNullOrEmpty(text8))
				{
					text3 = text3.ConcatWithSeparator(text8);
				}
			}
		}
		if (text == text2)
		{
			return text3;
		}
		if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3))
		{
			text3 += "\n";
		}
		if (!string.IsNullOrEmpty(text3))
		{
			return text3 + text2;
		}
		return text2;
	}

	public string GetItemName(bool localized = true)
	{
		string text = id;
		if (quality_type == QualityType.Stars && text.Contains(":") && quality > 0.1f)
		{
			text = text.Substring(0, text.LastIndexOf(':'));
		}
		string text2 = GJL.L(text);
		if (text2 != text)
		{
			if (!localized)
			{
				return text;
			}
			return text2;
		}
		if (!localized)
		{
			return id;
		}
		return GJL.L(id);
	}

	private void AddWidgetSeparator(List<BubbleWidgetData> res, bool full_detail)
	{
		if (full_detail)
		{
			res.Add(new BubbleWidgetSeparatorData());
		}
	}

	public List<BubbleWidgetData> GetTooltipData(Item item = null, bool full_detail = true)
	{
		List<BubbleWidgetData> list = new List<BubbleWidgetData>
		{
			new BubbleWidgetTextData(GetItemName(), UITextStyles.TextStyle.HintTitle),
			new BubbleWidgetTextData(GetItemDescription(item), UITextStyles.TextStyle.TinyDescription)
		};
		if (item == null)
		{
			return list;
		}
		if (item.durability < 1f)
		{
			list.Add(new BubbleWidgetTextData(item.GetDurabilityHint(), UITextStyles.TextStyle.TinyDescription));
		}
		if (item.definition != null && item.definition.type == ItemType.Preach && item.definition.linked_craft != null)
		{
			string text = "";
			CraftDefinition craftDefinition = item.definition.linked_craft;
			if (craftDefinition != null)
			{
				if (craftDefinition.k_money > 0f)
				{
					text = text.ConcatWithSeparator(GJL.L("sermon_money_k", $"+{craftDefinition.k_money * 100f:0}%"));
				}
				if (craftDefinition.k_faith > 0f)
				{
					text = text.ConcatWithSeparator(GJL.L("sermon_faith_k", $"+{craftDefinition.k_faith * 100f:0}%"));
				}
				if (craftDefinition.output.Count > 0)
				{
					string text2 = "";
					foreach (Item item2 in craftDefinition.output)
					{
						string itemName = item2.GetItemName();
						text2 = text2.ConcatWithSeparator(itemName, ", ");
					}
					text = text.ConcatWithSeparator(text2);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(new BubbleWidgetTextData(GJL.L("preach_params_2"), UITextStyles.TextStyle.HintTitle));
				list.Add(new BubbleWidgetTextData(text, UITextStyles.TextStyle.TinyDescription));
			}
		}
		string itemBodyModificators = item.GetItemBodyModificators();
		if (!string.IsNullOrEmpty(itemBodyModificators))
		{
			if (item.definition.type == ItemType.BodyUniversalPart)
			{
				AddWidgetSeparator(list, full_detail);
				list.Add(new BubbleWidgetTextData(itemBodyModificators, UITextStyles.TextStyle.TinyDescription));
			}
			else
			{
				AddWidgetSeparator(list, full_detail);
				list.Add(new BubbleWidgetTextData(GJL.L("embalming_effect") + "\n" + itemBodyModificators, UITextStyles.TextStyle.TinyDescription));
			}
		}
		ItemDetails itemDetails = GetItemDetails();
		CraftDefinition surveyCraft = GetSurveyCraft();
		if (full_detail && surveyCraft != null)
		{
			if (surveyCraft.sub_type == CraftDefinition.CraftSubType.SurveySciencePoints)
			{
				int num = ((surveyCraft.output_to_wgo.Count != 0) ? surveyCraft.output_to_wgo[0].value : 0);
				if (num > 0)
				{
					AddWidgetSeparator(list, full_detail);
					list.Add(new BubbleWidgetTextData(GJL.L("hint_science_decompose", num), UITextStyles.TextStyle.TinyDescription));
				}
			}
			if (MainGame.me.save.completed_one_time_crafts.Contains(surveyCraft.id))
			{
				AddWidgetSeparator(list, full_detail);
				list.Add(new BubbleWidgetTextData(GJL.L("survey") + GJL.L(":") + " " + GJL.L("survey_complete"), UITextStyles.TextStyle.TinyDescription));
				if (itemDetails?.alchemy != null)
				{
					list.Add(new BubbleWidgetAlchemyItemData(id));
				}
			}
			else
			{
				string text3 = "";
				foreach (Item item3 in MainGame.game_started ? ResModificator.ProcessItemsListBeforeDrop(surveyCraft.output, null, MainGame.me.player) : surveyCraft.output)
				{
					if (TechDefinition.TECH_POINTS.Contains(item3.id) && (!MainGame.game_started || item3.value > 0))
					{
						text3 = text3 + "(" + item3.id + ")";
					}
				}
				if (!string.IsNullOrEmpty(text3))
				{
					AddWidgetSeparator(list, full_detail);
					text3 = " (" + text3 + ")";
					list.Add(new BubbleWidgetTextData(GJL.L("survey") + ": [c][B73B1F]" + GJL.L("survey_not_complete") + "[-][/c]" + text3, UITextStyles.TextStyle.TinyDescription));
				}
			}
		}
		if (itemDetails != null)
		{
			string text4 = ", ";
			string text5 = GJL.L(",");
			if (!string.IsNullOrEmpty(text5))
			{
				text4 = text5;
			}
			string text6 = string.Empty;
			for (int i = 0; i < itemDetails.crafts_in.Count; i++)
			{
				if (i > 0)
				{
					text6 += text4;
				}
				text6 += GJL.L(itemDetails.crafts_in[i].id);
			}
			if (!string.IsNullOrEmpty(text6))
			{
				AddWidgetSeparator(list, full_detail);
				list.Add(new BubbleWidgetTextData(GJL.L("crafted_at") + " " + text6, UITextStyles.TextStyle.TinyDescription));
			}
		}
		return list;
	}

	public List<BubbleWidgetData> GetTooltipDataCraftAt(Item item = null)
	{
		List<BubbleWidgetData> list = new List<BubbleWidgetData>();
		ItemDetails itemDetails = GetItemDetails();
		if (itemDetails != null)
		{
			string text = ", ";
			string text2 = GJL.L(",");
			if (!string.IsNullOrEmpty(text2))
			{
				text = text2;
			}
			string text3 = string.Empty;
			for (int i = 0; i < itemDetails.crafts_in.Count; i++)
			{
				if (i > 0)
				{
					text3 += text;
				}
				text3 += GJL.L(itemDetails.crafts_in[i].id);
			}
			if (!string.IsNullOrEmpty(text3))
			{
				list.Add(new BubbleWidgetTextData(GJL.L("crafted_at") + " " + text3, UITextStyles.TextStyle.TinyDescription));
			}
		}
		return list;
	}

	public bool IsWeapon()
	{
		return type == ItemType.Sword;
	}

	public bool IsEquipment()
	{
		switch (type)
		{
		case ItemType.Axe:
		case ItemType.Pickaxe:
		case ItemType.Shovel:
		case ItemType.Hammer:
		case ItemType.FishingRod:
		case ItemType.HeadArmor:
		case ItemType.BodyArmor:
			return true;
		default:
			return false;
		}
	}

	public static int GetProductTier(string string_to_parse)
	{
		if (!int.TryParse(string_to_parse.Replace(" ", "").ToLower().Replace("common", "3")
			.Replace("unusual", "6")
			.Replace("rare", "9")
			.Replace("epic", "12")
			.Replace("legendary", "15"), out var result))
		{
			Debug.LogError("Can't parse a product tier: [" + string_to_parse + "]");
			return 0;
		}
		return result;
	}

	public float GetPrice(int cur_count, int modified_base_count = 0)
	{
		if (cur_count <= 0)
		{
			cur_count = 1;
		}
		if (modified_base_count == 0)
		{
			modified_base_count = base_count;
		}
		if (is_static_cost)
		{
			return base_price;
		}
		return base_price * Mathf.Sqrt((float)modified_base_count / (float)cur_count);
	}

	public static int CompareProductTypes(string left_product_type, string right_product_type)
	{
		ProductTypeDefinition dataOrNull = GameBalance.me.GetDataOrNull<ProductTypeDefinition>(left_product_type);
		ProductTypeDefinition dataOrNull2 = GameBalance.me.GetDataOrNull<ProductTypeDefinition>(right_product_type);
		if (dataOrNull == null && dataOrNull2 == null)
		{
			return 0;
		}
		if (dataOrNull == null)
		{
			return 1;
		}
		if (dataOrNull2 == null)
		{
			return -1;
		}
		if (dataOrNull.sort_weight < dataOrNull2.sort_weight)
		{
			return 1;
		}
		if (dataOrNull.sort_weight > dataOrNull2.sort_weight)
		{
			return -1;
		}
		return 0;
	}

	public void ResetLanguageCache()
	{
		_details = null;
	}

	public ItemDetails GetItemDetails()
	{
		if (_details == null)
		{
			_details = new ItemDetails
			{
				crafts_in = GameBalance.me.GetItemCraftsIn(id)
			};
			if (alch_type != 0)
			{
				return _details;
			}
			_details.alchemy = new ItemDetailsAlchemy
			{
				details_type = ItemDetailsAlchemy.DetailsType.Decompose
			};
			if (_details.alchemy.details_type != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			foreach (CraftDefinition craft_datum in GameBalance.me.craft_data)
			{
				if (craft_datum.craft_type == CraftDefinition.CraftType.AlchemyDecompose && (craft_datum.needs[0].id == id || (id.Contains(':') && id.Contains(craft_datum.needs[0].id))) && craft_datum.output[0].definition.alch_type != 0)
				{
					int item = (int)craft_datum.output[0].definition.alch_type;
					if (!_details.alchemy.decomposes.Contains(item))
					{
						_details.alchemy.decomposes.Add(item);
					}
				}
			}
		}
		return _details;
	}

	public Sprite TryGetQualitySprite()
	{
		string qualityIconName = GetQualityIconName();
		if (!string.IsNullOrEmpty(qualityIconName))
		{
			return EasySpritesCollection.GetSprite(qualityIconName);
		}
		return null;
	}

	public string GetQualityIconName()
	{
		if (quality_type == QualityType.Default)
		{
			return null;
		}
		string result = "";
		if (quality_type == QualityType.Stars && Mathf.FloorToInt(quality) > 0)
		{
			result = "item_star_" + quality;
		}
		return result;
	}

	public void TryDrawQualityOrDisableGameObject(UI2DSprite ui_sprite)
	{
		ui_sprite.sprite2D = TryGetQualitySprite();
		ui_sprite.SetActive(ui_sprite.sprite2D != null);
	}

	public string GetNameWithoutQualitySuffix()
	{
		return StaticGetNameWithoutQualitySuffix(id);
	}

	public static string StaticGetNameWithoutQualitySuffix(string id)
	{
		int num = id.LastIndexOf(':');
		if (num == -1)
		{
			return id;
		}
		return id.Substring(0, num);
	}

	public CraftDefinition GetSurveyCraft()
	{
		CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>("surv:" + GetNameWithoutQualitySuffix());
		if (dataOrNull != null)
		{
			return dataOrNull;
		}
		return GameBalance.me.GetDataOrNull<CraftDefinition>("surv:" + id);
	}

	public string GetIcon()
	{
		if (!string.IsNullOrEmpty(icon))
		{
			return icon;
		}
		return "i_" + id;
	}

	public string GetOverheadIcon()
	{
		if (!string.IsNullOrEmpty(custom_ovr_icon))
		{
			return custom_ovr_icon;
		}
		return GetIcon();
	}

	public static string GetGooFromAlchemyIngridient(string ingridient)
	{
		if (string.IsNullOrEmpty(ingridient))
		{
			return string.Empty;
		}
		if (ingridient.Contains(":"))
		{
			string[] array = ingridient.Split(':');
			ingridient = array[array.Length - 1];
		}
		if (ingridient.StartsWith("alchemy_"))
		{
			if (ingridient.Length < 10)
			{
				Debug.LogError("Wrong alchemy ingridient: " + ingridient);
				return string.Empty;
			}
			int num = 0;
			switch (ingridient[8])
			{
			case '1':
				num = 1;
				break;
			case '2':
				num = 2;
				break;
			case '3':
				num = 3;
				break;
			default:
				Debug.LogError("Wrong alchemy ingridient number: " + ingridient + "[8]=" + ingridient[8]);
				return string.Empty;
			}
			ingridient = ingridient.Replace("alchemy_" + num + "_", "");
		}
		else if (ingridient.StartsWith("powder_"))
		{
			ingridient = ingridient.Replace("powder_", "");
		}
		else if (ingridient.StartsWith("drop_"))
		{
			ingridient = ingridient.Replace("drop_", "");
		}
		ingridient = "goo_" + ingridient;
		return ingridient;
	}
}
