using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class UniversalObjectInfoGUI : MonoBehaviour
{
	public UITable table;

	public UILabel text_header;

	public UILabel text_descr;

	public UI2DSprite spr_icon;

	public UITableOrGrid right_items;

	public BaseItemCellGUI right_item_prefab;

	public UI2DSprite spr_icon_back;

	public GameObject top_separator;

	public void Draw(UniversalObjectInfo data)
	{
		if (text_header != null)
		{
			text_header.text = data.header;
		}
		if (text_descr != null)
		{
			text_descr.text = data.descr;
			text_descr.gameObject.SetActive(!string.IsNullOrEmpty(data.descr));
		}
		if (spr_icon != null)
		{
			spr_icon.sprite2D = EasySpritesCollection.GetSprite(data.icon, not_found_is_valid: true);
			spr_icon.color = NGUIMath.HexToColor(uint.Parse(data.icon_color, NumberStyles.HexNumber));
			if (spr_icon_back != null)
			{
				string text = "craft_icon";
				if (data.icon.Contains("_grn_"))
				{
					text += "_grn";
				}
				else if (data.icon.Contains("_b_goc_"))
				{
					text += "_goc";
				}
				else if (data.icon.Contains("_b_sls_"))
				{
					text += "_sls";
				}
				spr_icon_back.sprite2D = EasySpritesCollection.GetSprite(text);
			}
		}
		if (top_separator != null && text_descr != null)
		{
			top_separator.SetActive(string.IsNullOrEmpty(text_header.text) || string.IsNullOrEmpty(text_descr.text));
		}
		if (right_item_prefab != null)
		{
			right_item_prefab.gameObject.SetActive(value: false);
			right_items.gameObject.SetActive(data.right_items.Count > 0);
			right_items.DestroyChildren(new BaseItemCellGUI[1] { right_item_prefab });
			foreach (KeyValuePair<string, int> right_item in data.right_items)
			{
				BaseItemCellGUI baseItemCellGUI = right_item_prefab.Copy();
				baseItemCellGUI.DrawItem(right_item.Key, right_item.Value, init_tooltip: false);
				if (string.IsNullOrEmpty(baseItemCellGUI.container.counter.text))
				{
					baseItemCellGUI.container.counter.text = "0";
				}
			}
			right_items.Reposition();
		}
		if (table != null)
		{
			table.Reposition();
			table.repositionNow = true;
		}
	}
}
