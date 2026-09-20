using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftDefinition : BalanceBaseObject
{
	public enum CraftType
	{
		None,
		ResourcesBasedCraft,
		Survey,
		MixedCraft,
		Fixing,
		AlchemyDecompose,
		PrayCraft,
		RatBuff,
		RefugeeCampCraft
	}

	public enum CraftSubType
	{
		None,
		Alchemy,
		SurveySciencePoints
	}

	public enum EnqueueType
	{
		Default,
		CanEnqueue,
		NeverEnqueue
	}

	[Serializable]
	public class MultiqualityCraftResult
	{
		public float value_items;

		public float value_perks;

		public float value_difficulty;

		public float qp_1;

		public float qp_2;

		public float qp_3;

		public float[] quality_probabilities => new float[3] { qp_1, qp_2, qp_3 };

		public float value_items_and_perks_sum => value_items + value_perks;

		public float value_result => value_items_and_perks_sum - value_difficulty;

		public void SetProbabilities(float p1, float p2, float p3)
		{
			qp_1 = p1;
			qp_2 = p2;
			qp_3 = p3;
		}
	}

	public const int DONT_REALLY_CRAFT_OUTPUT = 1;

	public List<string> craft_in = new List<string>();

	public List<Item> needs = new List<Item>();

	public List<Item> needs_from_wgo = new List<Item>();

	public List<Item> output = new List<Item>();

	public List<SmartExpression> out_items_expressions = new List<SmartExpression>();

	public GameRes output_res_wgo = new GameRes();

	public GameRes output_set_res_wgo = new GameRes();

	public GameRes set_when_cancelled = new GameRes();

	public List<Item> output_to_wgo = new List<Item>();

	public List<Item> output_to_wgo_on_start = new List<Item>();

	public ToolActions tool_actions = new ToolActions();

	public SmartExpression condition = new SmartExpression();

	public string end_script = "";

	public string end_event = "";

	public int flag;

	public SmartExpression craft_time;

	public SmartExpression energy;

	public SmartExpression gratitude_points_craft_cost;

	public SmartExpression sanity;

	public bool hidden;

	public bool needs_unlock;

	public string icon = "";

	public CraftType craft_type;

	public bool is_auto;

	public bool not_hide_gui;

	public bool can_craft_always;

	public string game_res_to_mirror_name = "";

	public float game_res_to_mirror_max;

	public string change_wgo = "";

	public bool use_variations;

	public int variation_index;

	public string craft_after_finish = "";

	public bool one_time_craft;

	public bool force_multi_craft;

	public bool disable_multi_craft;

	public CraftSubType sub_type;

	public bool transfer_needs_to_wgo;

	public bool set_out_wgo_params_on_start;

	public GameRes itempars_add = new GameRes();

	public GameRes itempars_set = new GameRes();

	public List<Item> item_output = new List<Item>();

	public List<Item> item_needs = new List<Item>();

	public bool item_needs_leave;

	public float dur_needs_item;

	public int dur_needs_item_index;

	public float difficulty;

	public List<string> linked_perks = new List<string>();

	public List<string> linked_buffs = new List<string>();

	public string custom_name = "";

	public string tab_id = "";

	public string buff = "";

	public float needs_quality;

	public float k_money;

	public float k_faith;

	public string linked_sub_id = "";

	public bool dont_close_window_on_craft;

	public float dur_parameter;

	public bool dont_show_in_hint;

	public string ach_key = "";

	public bool craft_time_is_zero;

	public bool puff_when_replaced;

	public bool is_item_crating_craft;

	public int store_last_craft_slot;

	public bool hide_quality_icon;

	public EnqueueType enqueue_type;

	[NonSerialized]
	private string _cached_name;

	public static string[] alchemy_goos = new string[21]
	{
		"goo_alcohol", "goo_ash", "goo_blood", "goo_brown", "goo_d_blue", "goo_d_green", "goo_d_violet", "goo_diamond", "goo_gold", "goo_graphite",
		"goo_green", "goo_oil", "goo_red", "goo_salt", "goo_silver", "goo_spice", "goo_violet", "goo_water", "goo_white", "goo_yellow",
		"goo_yellow_electro"
	};

	public bool takes_item_durability => !dur_needs_item.EqualsTo(0f);

	public string GetNameNonLocalized()
	{
		if (!string.IsNullOrEmpty(custom_name))
		{
			return custom_name;
		}
		if (string.IsNullOrEmpty(_cached_name))
		{
			if (id.Contains(":") && !id.StartsWith("mix:mf_alchemy"))
			{
				_cached_name = id.Split(':')[2];
			}
			else
			{
				Item firstRealOutput = GetFirstRealOutput();
				_cached_name = id;
				if (firstRealOutput != null)
				{
					ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>(firstRealOutput.id);
					if (dataOrNull != null)
					{
						_cached_name = dataOrNull.GetItemName(localized: false);
					}
				}
			}
		}
		return _cached_name;
	}

	public Item GetFirstRealOutput()
	{
		foreach (Item item in output)
		{
			if (!TechDefinition.TECH_POINTS.Contains(item.id))
			{
				return item;
			}
		}
		return null;
	}

	public string GetDescription()
	{
		if (output.Count > 0 && (is_item_crating_craft || GetNameNonLocalized() == output[0].id))
		{
			if (output[0].is_multiquality)
			{
				return output[0].GetMultiqualityItemDescription();
			}
			return output[0].definition.GetItemDescription();
		}
		string text = GetNameNonLocalized() + "_d";
		string text2 = GJL.L(text);
		string text3 = ((text == text2) ? "" : text2);
		if (this is ObjectCraftDefinition { build_type: ObjectCraftDefinition.BuildType.Put } objectCraftDefinition && objectCraftDefinition.id != "_remove_")
		{
			ObjectDefinition data = GameBalance.me.GetData<ObjectDefinition>(objectCraftDefinition.out_obj);
			if (data != null && (data.quality_type == ObjectDefinition.QualityType.Shown || (data.quality_type == ObjectDefinition.QualityType.Grave && !data.quality.EvaluateFloat().EqualsTo(0f))))
			{
				float num = data.quality.EvaluateFloat();
				if (text3.Length > 0)
				{
					text3 += ", ";
				}
				if (data.id.EndsWith("_place"))
				{
					string text4 = data.id.Replace("_place", "");
					ObjectDefinition data2 = GameBalance.me.GetData<ObjectDefinition>(text4);
					if (data2 != null && (data2.quality_type == ObjectDefinition.QualityType.Shown || data2.quality_type == ObjectDefinition.QualityType.Grave))
					{
						num = data2.quality.EvaluateFloat();
					}
				}
				text3 = text3 + "(*)" + num;
			}
		}
		return text3;
	}

	public bool IsMultiqualityOutput()
	{
		foreach (Item item in output)
		{
			if (item.is_multiquality)
			{
				return true;
			}
		}
		return false;
	}

	public List<string> GetNeededPerks()
	{
		return linked_perks;
	}

	public float GetPerkValue(string perk_id)
	{
		if (!MainGame.me.save.unlocked_perks.Contains(perk_id))
		{
			return 0f;
		}
		return GameBalance.me.GetData<PerkDefinition>(perk_id).stars;
	}

	public float GetBuffValue(string buff_id)
	{
		if (BuffsLogics.FindBuffByID(buff_id) == null)
		{
			return 0f;
		}
		return GameBalance.me.GetData<BuffDefinition>(buff_id).craft_q;
	}

	public MultiqualityCraftResult GetMultiqualityResult(List<string> multiquality_ids, List<string> unlocked_perks = null)
	{
		if (!IsMultiqualityOutput())
		{
			return null;
		}
		if (unlocked_perks == null)
		{
			unlocked_perks = MainGame.me.save.unlocked_perks;
		}
		MultiqualityCraftResult multiqualityCraftResult = new MultiqualityCraftResult
		{
			value_items = 0f,
			value_perks = 0f
		};
		foreach (string neededPerk in GetNeededPerks())
		{
			multiqualityCraftResult.value_perks += GetPerkValue(neededPerk);
		}
		foreach (string linked_buff in linked_buffs)
		{
			multiqualityCraftResult.value_perks += GetBuffValue(linked_buff);
		}
		multiqualityCraftResult.value_difficulty = difficulty;
		int num = 0;
		for (int i = 0; i < needs.Count; i++)
		{
			if ((i < multiquality_ids.Count && needs[i].is_multiquality) || !(needs[i].definition.quality < 0.5f))
			{
				num++;
				string text = (string.IsNullOrEmpty(multiquality_ids[i]) ? needs[i].id : multiquality_ids[i]);
				ItemDefinition data = GameBalance.me.GetData<ItemDefinition>(text);
				if (data != null)
				{
					multiqualityCraftResult.value_items += data.quality;
				}
			}
		}
		if (num > 0)
		{
			multiqualityCraftResult.value_items /= num;
		}
		multiqualityCraftResult.SetProbabilities(Mathf.Clamp(multiqualityCraftResult.value_result, 0f, 1f), Mathf.Clamp(multiqualityCraftResult.value_result, 1f, 2f) - 1f, Mathf.Clamp(multiqualityCraftResult.value_result, 2f, 3f) - 2f);
		return multiqualityCraftResult;
	}

	public string GetSpendTxt(WorldGameObject wgo, int multiplier = 1)
	{
		string text = "";
		int num = ((!GlobalCraftControlGUI.is_global_control_active) ? ((energy != null && energy.has_expression) ? Mathf.RoundToInt(energy.EvaluateFloat(wgo)) : 0) : ((gratitude_points_craft_cost != null && gratitude_points_craft_cost.has_expression) ? Mathf.RoundToInt(gratitude_points_craft_cost.EvaluateFloat(wgo)) : 0));
		if (num != 0)
		{
			float num2 = 1f;
			if (wgo?.obj_def?.tool_actions != null)
			{
				for (int i = 0; i < wgo.obj_def.tool_actions.action_tools.Count; i++)
				{
					ItemDefinition.ItemType itemType = wgo.obj_def.tool_actions.action_tools[i];
					if (itemType == ItemDefinition.ItemType.Hand)
					{
						continue;
					}
					Item equippedTool = MainGame.me.player.GetEquippedTool(itemType);
					if (equippedTool?.definition?.tool_energy_k != null && equippedTool.definition.tool_energy_k.has_expression)
					{
						float num3 = equippedTool.definition.tool_energy_k.EvaluateFloat(wgo, MainGame.me.player);
						if (num3 < num2)
						{
							num2 = num3;
						}
					}
				}
			}
			if (!num2.EqualsTo(1f, 0.01f))
			{
				num = Mathf.RoundToInt((float)num * num2);
			}
			text = ((!GlobalCraftControlGUI.is_global_control_active) ? (text + "[c](en)[/c]" + num) : ((!(MainGame.me.player.gratitude_points < (gratitude_points_craft_cost?.EvaluateFloat(MainGame.me.player) ?? 0f))) ? (text + "[c](gratitude_points)[/c]" + num) : (text + "(gratitude_points)[c][ff1111]" + num + "[/c]")));
		}
		if (is_auto)
		{
			int num4 = ((craft_time != null && craft_time.has_expression) ? Mathf.RoundToInt(craft_time.EvaluateFloat(wgo)) : 0);
			if (num4 != 0)
			{
				TimeSpan timeSpan = TimeSpan.FromSeconds(num4);
				text = text.ConcatWithSeparator($"[c](time)[/c]{timeSpan.Minutes:0}:{timeSpan.Seconds:00}");
			}
		}
		foreach (Item item in needs_from_wgo)
		{
			if (item.id == "fire")
			{
				string text2 = $"[c](fire2)[/c]{item.value * multiplier:0}";
				if (!wgo.data.IsEnoughItems(item, "", 0, multiplier))
				{
					text2 = "[ff1111]" + text2 + "[/c]";
				}
				text = text.ConcatWithSeparator(text2);
			}
		}
		if (wgo?.obj_def?.tool_actions != null)
		{
			for (int j = 0; j < wgo.obj_def.tool_actions.action_tools.Count; j++)
			{
				ItemDefinition.ItemType itemType2 = wgo.obj_def.tool_actions.action_tools[j];
				if (itemType2 != ItemDefinition.ItemType.Hand)
				{
					string text3 = itemType2.ToString().ToLower();
					float num5 = wgo.obj_def.tool_actions.action_k[j];
					int num6 = Mathf.FloorToInt(100f * num5);
					Item equippedTool2 = MainGame.me.player.GetEquippedTool(itemType2);
					if (equippedTool2 == null)
					{
						text = text + "\n[c][ff1111](" + text3 + "_s)[-][/c]";
						continue;
					}
					num6 = Mathf.FloorToInt((float)num6 * equippedTool2.definition.efficiency);
					text += $"\n[c]({text3}_s)[/c]\n{num6}%";
				}
			}
		}
		return text;
	}

	public string GetCraftIcon()
	{
		if (!string.IsNullOrEmpty(icon))
		{
			return icon;
		}
		Item firstRealOutput = GetFirstRealOutput();
		if (firstRealOutput != null)
		{
			return firstRealOutput.GetIcon();
		}
		return "";
	}

	public bool IsBodyPartExtractionCraft()
	{
		return id.StartsWith("ex:");
	}

	public bool IsBodyPartInsertionCraft()
	{
		return id.StartsWith("insert:");
	}

	public bool IsLocked()
	{
		if (!needs_unlock || MainGame.me.save.unlocked_crafts.Contains(id))
		{
			return MainGame.me.save.locked_crafts.Contains(id);
		}
		return true;
	}

	public bool CanCraftMultiple()
	{
		if (disable_multi_craft)
		{
			return false;
		}
		if (force_multi_craft)
		{
			return true;
		}
		if (takes_item_durability)
		{
			return false;
		}
		if (IsMultiqualityOutput())
		{
			return false;
		}
		if (one_time_craft)
		{
			return false;
		}
		if (id == "_remove_")
		{
			return false;
		}
		if (takes_item_durability)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(end_script) || !string.IsNullOrEmpty(change_wgo) || !string.IsNullOrEmpty(craft_after_finish) || !string.IsNullOrEmpty(end_event))
		{
			if (enqueue_type == EnqueueType.CanEnqueue)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public bool CanEnqueue()
	{
		switch (enqueue_type)
		{
		case EnqueueType.CanEnqueue:
			return true;
		case EnqueueType.NeverEnqueue:
			return false;
		default:
			if (!string.IsNullOrEmpty(change_wgo))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(end_script))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(end_event))
			{
				return false;
			}
			if (IsMultiqualityOutput())
			{
				return false;
			}
			return true;
		}
	}
}
