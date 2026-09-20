using System.Collections.Generic;
using UnityEngine;

public class BagInventoryWidget : InventoryWidget
{
	public UI2DSprite bag_type_sprite;

	public UIGrid inventory_table;

	private static List<string> bag_type_sprites;

	public override void Init()
	{
		base.Init();
		bag_type_sprites = new List<string>
		{
			string.Empty,
			string.Empty,
			"char_wndw_bag_inventory_bag_i_01",
			"char_wndw_bag_inventory_bag_i_02",
			"char_wndw_bag_inventory_bag_i_04",
			"char_wndw_bag_inventory_bag_i_03",
			"char_wndw_bag_inventory_bag_i_05",
			"char_wndw_bag_inventory_bag_i_06",
			"char_wndw_bag_inventory_bag_i_07"
		};
	}

	public override void Open(Inventory inventory, bool for_gamepad, int navigation_group = 0, int navigation_sub_group = 0, bool dont_show_empty_rows = false, int custom_line_length = -1)
	{
		base.Open(inventory, for_gamepad, navigation_group, navigation_sub_group, dont_show_empty_rows, custom_line_length);
		ItemDefinition itemDefinition = inventory?.data?.definition;
		if (itemDefinition == null)
		{
			Debug.LogError("Bag definition is null!");
		}
		else
		{
			SetBagTypeSprite(itemDefinition.bag_type);
		}
	}

	private void SetBagTypeSprite(ItemDefinition.BagType bag_type)
	{
		if ((int)bag_type >= bag_type_sprites.Count)
		{
			Debug.LogError("Bag types more than bag icons!");
		}
		string text = bag_type_sprites[(int)bag_type];
		Sprite sprite2D = null;
		if (!string.IsNullOrEmpty(text))
		{
			sprite2D = EasySpritesCollection.GetSprite(text);
		}
		bag_type_sprite.sprite2D = sprite2D;
	}

	public override void Redraw()
	{
		base.Redraw();
		RecalculatWidgetSizeAndPosition();
	}

	private void RecalculatWidgetSizeAndPosition()
	{
		if (inventory_table == null)
		{
			Debug.LogError("Inventory table is null!");
			return;
		}
		if (inventory_item == null || inventory_item.IsEmpty() || inventory_item.definition == null)
		{
			Debug.LogError("BagInventoryWidget error: wrong inventory_item!");
			return;
		}
		ItemDefinition definition = inventory_item.definition;
		int bag_size_x = definition.bag_size_x;
		int bag_size_y = definition.bag_size_y;
		inventory_table.maxPerLine = bag_size_x;
		float x = 110f - (float)(bag_size_x - 1) * 21f;
		Transform obj = inventory_table.transform;
		obj.localPosition = new Vector2(x, ((Vector2)obj.localPosition).y);
		inventory_table.Reposition();
		GetComponent<UIWidget>().height = 29 + bag_size_y * 42 + auto_height_offset;
	}
}
