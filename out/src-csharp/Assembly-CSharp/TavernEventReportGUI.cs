using System;
using UnityEngine;

public class TavernEventReportGUI : DialogGUI
{
	public PrayReportItemGUI row_prefab;

	public UITableOrGrid rows_table;

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	protected override bool OnPressedSelect()
	{
		OnClosePressed();
		return true;
	}

	public void OpenPlayersTavernEventResult(WorldGameObject barmen_wgo, TavernEventDefinition tavern_event)
	{
		if (barmen_wgo == null)
		{
			Debug.LogError("OpenPlayersTavernEventResult error: barmen_wgo is null!");
			return;
		}
		if (tavern_event == null)
		{
			Debug.LogError("OpenPlayersTavernEventResult error: tavern_event is null!");
			return;
		}
		OpenOK(tavern_event.id + "_header");
		row_prefab.gameObject.SetActive(value: false);
		rows_table.DestroyChildren(new PrayReportItemGUI[1] { row_prefab });
		int num = Mathf.RoundToInt(tavern_event.max_food_sold.EvaluateFloat(barmen_wgo, MainGame.me.player));
		TavernEventDefinition.TavernEventType type = tavern_event.type;
		float num2 = 0f;
		float num3 = 0f;
		while (num > 0 && barmen_wgo.data.inventory.Count != 0)
		{
			Item item = null;
			foreach (Item item3 in barmen_wgo.data.inventory)
			{
				if (item3?.definition != null && item3.definition.can_insert_into_barmen)
				{
					item = item3;
					break;
				}
			}
			if (item == null)
			{
				break;
			}
			float num4 = item.definition.tavern_event_coeffs[(int)type];
			if (barmen_wgo.data.inventory.Count > 1)
			{
				for (int i = 1; i < barmen_wgo.data.inventory.Count; i++)
				{
					Item item2 = barmen_wgo.data.inventory[i];
					if (item2?.definition != null && item2.definition.can_insert_into_barmen && item2.definition.tavern_event_coeffs[(int)type] > num4)
					{
						item = item2;
						num4 = item2.definition.tavern_event_coeffs[(int)type];
					}
				}
			}
			string text = item.definition.GetItemName();
			if (item.definition.quality_type == ItemDefinition.QualityType.Stars)
			{
				text += " ";
				switch (Mathf.RoundToInt(item.definition.quality))
				{
				case 1:
					text += "(s1)";
					break;
				case 2:
					text += "(s2)";
					break;
				case 3:
					text += "(s3)";
					break;
				}
			}
			string text2 = Trading.FormatMoney(item.definition.base_price * num4);
			string text3 = ((num4 > 0.85f) ? "(:-))" : ((num4 < 0.65f) ? "(:-()" : "(:-|)"));
			int num5;
			if (num > item.value)
			{
				num5 = item.value;
				num -= item.value;
				barmen_wgo.data.RemoveItem(item);
			}
			else
			{
				num5 = num;
				num = 0;
				barmen_wgo.data.RemoveItem(item, num5);
			}
			num2 += item.definition.base_price * num4 * (float)num5;
			num3 += num4 * (float)num5;
			DrawResultRow(text, num5 + " x " + text2 + " " + text3);
		}
		string empty = string.Empty;
		float num6 = 1000f;
		switch (type)
		{
		case TavernEventDefinition.TavernEventType.Aclofest:
			num6 = 60f;
			empty = "dlc_stories_good_alcoparty";
			break;
		case TavernEventDefinition.TavernEventType.Standup:
			num6 = 80f;
			empty = "dlc_stories_good_stand_up";
			break;
		case TavernEventDefinition.TavernEventType.Sharmel_song:
			num6 = 120f;
			empty = "dlc_stories_good_sharmel_song";
			break;
		case TavernEventDefinition.TavernEventType.RatRace:
			num6 = 100f;
			empty = "dlc_stories_good_rat_race";
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Debug.Log($"Achievement check: event_type = {type}, by_money = {false}, total_money = {num2}, sum_of_coeffs = {num3}");
		if (num3 > num6)
		{
			MainGame.me.save.quests.CheckKeyQuests(empty);
		}
		num2 += (float)Mathf.RoundToInt(MainGame.me.player.GetParam("zombie_greeter_placed")) * 5f;
		num2 += tavern_event.unconditional_income;
		DrawResultRow(GJL.L("event_income"), Trading.FormatMoney(tavern_event.unconditional_income, print_zero: true));
		bool flag = tavern_event.id == "rat_race" && MainGame.me.player.GetParamInt("rat_race_double_money") == 1;
		MainGame.me.player.SetParam("rat_race_double_money", 0f);
		MainGame.me.player.data.money += num2 * (float)((!flag) ? 1 : 2);
		string text4 = Trading.FormatMoney(num2, print_zero: true);
		DrawResultRow(GJL.L("total_money"), string.IsNullOrEmpty(text4 + (flag ? " x2" : "")) ? "-" : text4);
		rows_table.Reposition();
	}

	private PrayReportItemGUI DrawResultRow(string txt, string value)
	{
		PrayReportItemGUI prayReportItemGUI = row_prefab.Copy();
		prayReportItemGUI.txt.text = txt + ":";
		prayReportItemGUI.value.text = value;
		return prayReportItemGUI;
	}
}
