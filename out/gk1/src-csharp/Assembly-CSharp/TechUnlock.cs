using System.Collections.Generic;
using UnityEngine;

public class TechUnlock
{
	public class TechUnlockData
	{
		public Sprite sprite;

		public Sprite quality_icon;

		public string name;

		public string description;
	}

	public enum TechUnlockType
	{
		Craft,
		Work,
		Phrase,
		Perk
	}

	public TechUnlockType type;

	public string id;

	public bool visible = true;

	private TechUnlockData _data;

	public TechUnlock()
	{
	}

	public TechUnlock(string id, TechUnlockType type)
	{
		if (string.IsNullOrEmpty(id))
		{
			Debug.LogError("Can't create a TechUnlock with an empty id");
			return;
		}
		if (id[0] == '@')
		{
			id = id.Substring(1);
			visible = false;
		}
		this.id = id;
		this.type = type;
		_data = null;
	}

	public void ResetLanguageCache()
	{
		_data = null;
	}

	public TechUnlockData GetData()
	{
		if (_data == null)
		{
			_data = new TechUnlockData();
			string text = "";
			string text2 = "";
			string text3 = "unknown";
			switch (type)
			{
			case TechUnlockType.Craft:
			{
				CraftDefinition craftDefinition = GameBalance.me.GetDataOrNull<CraftDefinition>(id);
				text2 = "recipe";
				if (craftDefinition == null && id.Contains(":"))
				{
					craftDefinition = GameBalance.me.GetData<ObjectCraftDefinition>(id);
					text2 = "blueprint";
				}
				if (craftDefinition == null)
				{
					Debug.LogError("Craft " + id + " not found!");
					break;
				}
				text3 = craftDefinition.GetNameNonLocalized();
				_ = craftDefinition.craft_type;
				_ = 6;
				text = craftDefinition.GetCraftIcon();
				Debug.Log("Craft id = " + craftDefinition.id + ", icon = " + text);
				if (craftDefinition.IsBodyPartExtractionCraft())
				{
					text2 = "extract";
				}
				break;
			}
			case TechUnlockType.Work:
			{
				ObjectGroupDefinition data2 = GameBalance.me.GetData<ObjectGroupDefinition>(id);
				if (data2 != null)
				{
					text = data2.tech_icon;
					text3 = id;
					text2 = "gathering";
				}
				break;
			}
			case TechUnlockType.Perk:
			{
				PerkDefinition data = GameBalance.me.GetData<PerkDefinition>(id);
				if (data != null)
				{
					text = data.GetIcon();
					text3 = id;
					text2 = (data.show ? "perk" : "gathering");
				}
				break;
			}
			}
			_data.sprite = EasySpritesCollection.GetSprite(text);
			_data.name = (string.IsNullOrEmpty(text2) ? "" : (GJL.L(text2) + GJL.L(":") + " ")) + GJL.L(text3);
			string text4 = text3 + "_d";
			string text5 = GJL.L(text4);
			_data.description = ((text4 == text5) ? "" : text5);
		}
		return _data;
	}

	public void GetTooltip(Tooltip tooltip)
	{
		if (tooltip == null)
		{
			return;
		}
		TechUnlockData data = GetData();
		tooltip.AddData(new BubbleWidgetTextData(data.name, UITextStyles.TextStyle.HintTitle, NGUIText.Alignment.Left));
		tooltip.AddData(new BubbleWidgetBlankSeparatorData());
		if (type == TechUnlockType.Craft)
		{
			CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>(id);
			if (dataOrNull == null && id.Contains(":"))
			{
				GameBalance.me.GetData<ObjectCraftDefinition>(id);
			}
			if (dataOrNull == null)
			{
				return;
			}
			Item item = null;
			List<ItemDefinition> list = null;
			foreach (Item item3 in dataOrNull.output)
			{
				if (item3.id == "r" || item3.id == "g" || item3.id == "b" || !item3.is_multiquality)
				{
					continue;
				}
				bool flag = false;
				list = new List<ItemDefinition>();
				foreach (string multiquality_item in item3.multiquality_items)
				{
					ItemDefinition data2 = GameBalance.me.GetData<ItemDefinition>(multiquality_item);
					if (data2 == null)
					{
						flag = true;
						break;
					}
					if (data2.type != ItemDefinition.ItemType.Preach)
					{
						flag = true;
						break;
					}
					list.Add(data2);
				}
				if (!flag)
				{
					item = item3;
					break;
				}
			}
			bool flag2 = item != null && list != null && list.Count > 0;
			if (flag2)
			{
				string text = "";
				List<float> list2 = new List<float>();
				List<float> list3 = new List<float>();
				List<float> list4 = new List<float>();
				List<float> list5 = new List<float>();
				List<float> list6 = new List<float>();
				for (int i = 0; i < list.Count; i++)
				{
					CraftDefinition linked_craft = list[i].linked_craft;
					if (linked_craft == null)
					{
						continue;
					}
					list2.Add(linked_craft.needs_quality);
					list3.Add(linked_craft.k_money * 100f);
					list4.Add(linked_craft.k_faith * 100f);
					if (linked_craft.output.Count <= 0)
					{
						continue;
					}
					foreach (Item item4 in linked_craft.output)
					{
						switch (item4.id)
						{
						case "money":
							list5.Add((float)item4.value / 100f);
							break;
						case "faith":
							list6.Add(item4.value);
							break;
						}
					}
				}
				string text2 = string.Empty;
				if (list2.Count > 0)
				{
					list2.Sort();
					if (list2[0] < list2[list2.Count - 1])
					{
						text2 = text2.ConcatWithSeparator(GJL.L("preach_params", "(cross)" + list2[0] + "-" + list2[list2.Count - 1]));
					}
					else if (list2[list2.Count - 1] > 0f)
					{
						text2 = text2.ConcatWithSeparator(GJL.L("preach_params", "(cross)" + list2[list2.Count - 1]));
					}
				}
				text2 += GJL.L(item.id + "_d");
				text2 = LocalizedLabel.ColorizeTags(text2, LocalizedLabel.TextColor.SpeechBubble);
				if (list3.Count > 0)
				{
					list3.Sort();
					if (list3[0] < list3[list3.Count - 1])
					{
						text = text.ConcatWithSeparator(GJL.L("sermon_money_k", $"+{list3[0]:0}-{list3[list3.Count - 1]:0}%"));
					}
					else if (list3[list3.Count - 1] > 0f)
					{
						text = text.ConcatWithSeparator(GJL.L("sermon_money_k", $"+{list3[list3.Count - 1]:0}%"));
					}
				}
				if (list4.Count > 0)
				{
					list4.Sort();
					if (list4[0] < list4[list4.Count - 1])
					{
						text = text.ConcatWithSeparator(GJL.L("sermon_faith_k", $"+{list4[0]:0}-{list4[list4.Count - 1]:0}%"));
					}
					else if (list4[list4.Count - 1] > 0f)
					{
						text = text.ConcatWithSeparator(GJL.L("sermon_faith_k", $"+{list4[list4.Count - 1]:0}%"));
					}
				}
				string text3 = string.Empty;
				if (list6.Count > 0)
				{
					list6.Sort();
					if (list6[0] < list6[list6.Count - 1])
					{
						text3 = text3.ConcatWithSeparator(GJL.L("faith") + $" (x{list6[0]:0}-{list6[list6.Count - 1]})", ",");
					}
					else if (list6[list6.Count - 1] > 0f)
					{
						text3 = text3.ConcatWithSeparator(GJL.L("faith") + $" (x{list6[list6.Count - 1]})", ",");
					}
				}
				if (list5.Count > 0)
				{
					list5.Sort();
					if (list5[0] < list5[list5.Count - 1])
					{
						text3 = text3.ConcatWithSeparator(Trading.FormatMoney(list5[0]) + "-" + Trading.FormatMoney(list5[list5.Count - 1]), ",");
					}
					else if (list5[list5.Count - 1] > 0f)
					{
						text3 = text3.ConcatWithSeparator(Trading.FormatMoney(list5[list5.Count - 1]), ",");
					}
				}
				if (!string.IsNullOrEmpty(text3))
				{
					text = text.ConcatWithSeparator(text3);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					tooltip.AddData(new BubbleWidgetTextData(text2, UITextStyles.TextStyle.TinyDescription, NGUIText.Alignment.Left));
				}
				if (!string.IsNullOrEmpty(text))
				{
					tooltip.AddData(new BubbleWidgetTextData(GJL.L("preach_params_2"), UITextStyles.TextStyle.HintTitle, NGUIText.Alignment.Left));
					tooltip.AddData(new BubbleWidgetTextData(text, UITextStyles.TextStyle.TinyDescription, NGUIText.Alignment.Left));
				}
			}
			if (dataOrNull.output.Count <= 0)
			{
				return;
			}
			Item item2 = null;
			ItemDefinition itemDefinition = null;
			for (int j = 0; j < dataOrNull.output.Count; j++)
			{
				if (!(dataOrNull.output[j].id == "r") && !(dataOrNull.output[j].id == "g") && !(dataOrNull.output[j].id == "b") && !(dataOrNull.output[j].id == "v"))
				{
					if (dataOrNull.output[j].definition != null)
					{
						item2 = dataOrNull.output[j];
						itemDefinition = item2.definition;
					}
					else
					{
						item2 = dataOrNull.output[j];
						itemDefinition = GameBalance.me.GetDataOrNull<ItemDefinition>(item2.id + ":1");
					}
					break;
				}
			}
			if (item2 != null && itemDefinition != null && !flag2)
			{
				List<BubbleWidgetData> tooltipData = itemDefinition.GetTooltipData(item2, full_detail: false);
				tooltipData.RemoveAt(0);
				foreach (BubbleWidgetData item5 in tooltipData)
				{
					item5.TrySetAlign(NGUIText.Alignment.Left);
					tooltip.AddData(item5);
				}
			}
			if (!flag2)
			{
				return;
			}
			{
				foreach (BubbleWidgetData item6 in itemDefinition.GetTooltipDataCraftAt(item2))
				{
					item6.TrySetAlign(NGUIText.Alignment.Left);
					tooltip.AddData(item6);
				}
				return;
			}
		}
		tooltip.AddData(new BubbleWidgetTextData(data.description, UITextStyles.TextStyle.TinyDescription, NGUIText.Alignment.Left));
	}
}
