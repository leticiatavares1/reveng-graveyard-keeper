using UnityEngine;

public class PrayReportGUI : DialogGUI
{
	public PrayReportItemGUI row_prefab;

	public UITableOrGrid rows_table;

	public override void Init()
	{
		base.Init();
	}

	public void Open(PrayLogics.PrayResult pray_result)
	{
		OpenOK("sermon_report_header");
		row_prefab.gameObject.SetActive(value: false);
		rows_table.DestroyChildren(new PrayReportItemGUI[1] { row_prefab });
		DrawResultRow(GJL.L("serm_res_people"), pray_result.people.ToString());
		DrawResultRow(GJL.L("serm_res_faith"), pray_result.faith.ToString());
		string text = Trading.FormatMoney(pray_result.money);
		DrawResultRow(GJL.L("serm_res_money"), string.IsNullOrEmpty(text) ? "-" : text);
		DrawResultRow(GJL.L("serm_res_success"), GJL.L("serm_res_" + (pray_result.success ? "1" : "0")));
		DrawResultRow(GJL.L("sermon_success_chance", "").Replace(": ", ""), pray_result.success_percent + "%");
		if (pray_result.success)
		{
			if (pray_result.faith_bonus > 0)
			{
				DrawResultRow(GJL.L("serm_res_faith_bonus"), pray_result.faith_bonus.ToString());
			}
			if (!pray_result.money_bonus.EqualsTo(0f))
			{
				DrawResultRow(GJL.L("serm_res_money_bonus"), Trading.FormatMoney(pray_result.money_bonus));
			}
		}
		rows_table.Reposition();
	}

	private PrayReportItemGUI DrawResultRow(string txt, string value)
	{
		PrayReportItemGUI prayReportItemGUI = row_prefab.Copy();
		prayReportItemGUI.txt.text = txt + ":";
		prayReportItemGUI.value.text = value;
		return prayReportItemGUI;
	}

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

	public void OpenMerchantTraidingResult(WorldGameObject cashbox_wgo)
	{
		if (cashbox_wgo == null)
		{
			Debug.LogError("Can not OpenMerchantTraidingResult: cashbox_wgo is NULL!");
			return;
		}
		OpenOK("traiding_report_header");
		row_prefab.gameObject.SetActive(value: false);
		rows_table.DestroyChildren(new PrayReportItemGUI[1] { row_prefab });
		DrawResultRow(GJL.L("total_crates_sold"), cashbox_wgo.data.GetParamInt("total_crates_sold").ToString());
		string text = string.Empty;
		ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>("box_vegetables_silver");
		if (dataOrNull != null)
		{
			text = " x " + Trading.FormatMoney(dataOrNull.base_price);
		}
		DrawResultRow(GJL.L("silver_crates_sold"), cashbox_wgo.data.GetParamInt("silver_crates_sold") + text);
		string text2 = string.Empty;
		ItemDefinition dataOrNull2 = GameBalance.me.GetDataOrNull<ItemDefinition>("box_vegetables_gold");
		if (dataOrNull2 != null)
		{
			text2 = " x " + Trading.FormatMoney(dataOrNull2.base_price);
		}
		DrawResultRow(GJL.L("gold_crates_sold"), cashbox_wgo.data.GetParamInt("gold_crates_sold") + text2);
		string text3 = string.Empty;
		ItemDefinition dataOrNull3 = GameBalance.me.GetDataOrNull<ItemDefinition>("box_goods");
		if (dataOrNull3 != null)
		{
			text3 = " x " + Trading.FormatMoney(dataOrNull3.base_price);
		}
		DrawResultRow(GJL.L("goods_crates_sold"), cashbox_wgo.data.GetParamInt("goods_crates_sold") + text3);
		string text4 = Trading.FormatMoney(cashbox_wgo.data.GetParam("total_money"), print_zero: true);
		DrawResultRow(GJL.L("total_money"), string.IsNullOrEmpty(text4) ? "-" : text4);
		MainGame.me.player.data.AddToParams("money", cashbox_wgo.data.GetParam("total_money"));
		cashbox_wgo.data.SetParam("total_crates_sold", 0f);
		cashbox_wgo.data.SetParam("silver_crates_sold", 0f);
		cashbox_wgo.data.SetParam("gold_crates_sold", 0f);
		cashbox_wgo.data.SetParam("goods_crates_sold", 0f);
		cashbox_wgo.data.SetParam("total_money", 0f);
		rows_table.Reposition();
	}

	public void OpenTavernCashbox(WorldGameObject tavern_cashbox)
	{
		if (tavern_cashbox == null)
		{
			Debug.LogError("Can not OpenTavernCashbox: tavern_cashbox is NULL!");
			return;
		}
		OpenOK("traiding_report_header");
		row_prefab.gameObject.SetActive(value: false);
		rows_table.DestroyChildren(new PrayReportItemGUI[1] { row_prefab });
		string[] iTEMS_SELLING_IN_TAVERN = MainGame.me.save.players_tavern_engine.ITEMS_SELLING_IN_TAVERN;
		foreach (string text in iTEMS_SELLING_IN_TAVERN)
		{
			string param_name = text.Replace(":", "_");
			int paramInt = tavern_cashbox.GetParamInt(param_name);
			if (paramInt == 0)
			{
				continue;
			}
			ItemDefinition data = GameBalance.me.GetData<ItemDefinition>(text);
			string value = paramInt + " x " + Trading.FormatMoney(data.base_price * 1f);
			string text2 = data.GetItemName();
			if (data.quality_type == ItemDefinition.QualityType.Stars)
			{
				switch (Mathf.RoundToInt(data.quality))
				{
				case 1:
					text2 += " (s1)";
					break;
				case 2:
					text2 += " (s2)";
					break;
				case 3:
					text2 += " (s3)";
					break;
				}
			}
			DrawResultRow(text2, value);
			tavern_cashbox.SetParam(param_name, 0f);
		}
		float param = tavern_cashbox.data.GetParam("money");
		tavern_cashbox.data.SetParam("money", 0f);
		float num = PlayersTavernEngine.CalculateAlcoholSellingBonus(param);
		if (Mathf.Abs(num) > 0.01f)
		{
			DrawResultRow(GJL.L("tavern_upgrade_bonus"), Trading.FormatMoney(num, print_zero: true));
		}
		DrawResultRow(GJL.L("total_money"), Trading.FormatMoney(param, print_zero: true));
		float num2 = tavern_cashbox.data.GetParam("reputation");
		tavern_cashbox.data.SetParam("reputation", 0f);
		string text3 = string.Empty;
		if (num2 >= 3f)
		{
			text3 = "(tavern_reputation) (tavern_reputation) (tavern_reputation)";
		}
		else
		{
			if (num2 > 2f)
			{
				text3 += " (tavern_reputation) (tavern_reputation)";
				num2 -= 2f;
			}
			else if (num2 > 1f)
			{
				text3 += " (tavern_reputation)";
				num2 -= 1f;
			}
			if (num2 > 0f)
			{
				text3 = text3 + " (tavern_reputation)" + Mathf.RoundToInt(num2 * 100f) + "%";
			}
		}
		DrawResultRow(GJL.L("total_reputation"), text3);
		MainGame.me.player.data.AddToParams("money", param);
		rows_table.Reposition();
	}
}
