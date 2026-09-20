using System;
using System.Collections.Generic;
using UnityEngine;

public class BubbleWidgetAlchemyItem : BubbleWidget<BubbleWidgetAlchemyItemData>
{
	public UILabel lbl_header;

	public UILabel lbl_info;

	public UILabel lbl_info_l;

	public UILabel lbl_info_r;

	public Color clr_normal;

	public Color clr_not_complete;

	public override void Init()
	{
		base.Init();
	}

	public override void Draw(BubbleWidgetAlchemyItemData data)
	{
		if (!initialized)
		{
			Init();
		}
		base.data = data;
		if (string.IsNullOrEmpty(data.item_id))
		{
			this.Deactivate();
			return;
		}
		bool flag = false;
		if (MainGame.me != null && MainGame.me.save != null)
		{
			flag = MainGame.me.save.IsSurveyComplete(CraftDefinition.CraftSubType.Alchemy, data.item_id);
		}
		if (!Application.isPlaying && UnityEngine.Random.Range(0, 100) > 50)
		{
			flag = true;
		}
		UILabel uILabel = lbl_header;
		Color color2 = (lbl_info.color = (flag ? clr_normal : clr_not_complete));
		uILabel.color = color2;
		if (!flag)
		{
			lbl_header.text = GJL.L("alch_survey_hdr") + GJL.L(":") + "\n" + GJL.L("alch_not_complete");
			return;
		}
		lbl_info.text = "";
		ItemDefinition itemDefinition = GameBalance.me.GetData<ItemDefinition>(data.item_id);
		if (itemDefinition == null)
		{
			return;
		}
		ItemDefinition.ItemDetails itemDetails = itemDefinition.GetItemDetails();
		if (itemDetails.alchemy != null)
		{
			switch (itemDetails.alchemy.details_type)
			{
			case ItemDefinition.ItemDetailsAlchemy.DetailsType.Decompose:
				DrawDecomposeInfo(itemDetails);
				break;
			case ItemDefinition.ItemDetailsAlchemy.DetailsType.Slots:
				DrawSlotsInfo(itemDetails);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			ui_widget.Update();
		}
	}

	private void DrawDecomposeInfo(ItemDefinition.ItemDetails details)
	{
		lbl_header.text = GJL.L("alch_decompose") + GJL.L(":") + " ";
		string text = "";
		foreach (int decompose in details.alchemy.decomposes)
		{
			text = text + "(alc" + decompose + ")";
		}
		lbl_header.text += (string.IsNullOrEmpty(text) ? "-" : text);
	}

	private void DrawSlotsInfo(ItemDefinition.ItemDetails details)
	{
		lbl_header.text = GJL.L("alch_slots") + GJL.L(":");
		string text = "";
		string text2 = "";
		int num = 0;
		foreach (List<bool> slot in details.alchemy.slots)
		{
			num++;
			if (num > 1)
			{
				text += "\n";
				text2 += "\n";
				lbl_info.text += "\n";
			}
			text2 = text2 + GJL.L("alc_d_tier") + " " + GJCommons.GetRomeNumber(num);
			foreach (bool item in slot)
			{
				text += (item ? "(alc+)" : "(alc-)");
			}
		}
		lbl_info_l.text = text2;
		lbl_info_r.text = text;
		UILabel uILabel = lbl_info_l;
		Color color2 = (lbl_info_r.color = clr_normal);
		uILabel.color = color2;
	}
}
