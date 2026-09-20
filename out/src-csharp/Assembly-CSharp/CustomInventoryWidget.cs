public class CustomInventoryWidget : BaseInventoryWidget
{
	private CustomInventoryItem[] _custom_items;

	private PanelAutoScroll _panel_auto_scroll;

	private GJCommons.VoidDelegate _on_over;

	private UIGrid _grid;

	public bool hide_empty_items;

	public override void Init()
	{
		_custom_items = GetComponentsInChildren<CustomInventoryItem>(includeInactive: true);
		CustomInventoryItem[] custom_items = _custom_items;
		for (int i = 0; i < custom_items.Length; i++)
		{
			custom_items[i].Init(this);
		}
		_panel_auto_scroll = GetComponent<PanelAutoScroll>();
		Tooltip[] componentsInChildren = GetComponentsInChildren<Tooltip>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		_grid = GetComponentInChildren<UIGrid>(includeInactive: true);
	}

	public void Open(Inventory inventory, bool for_gamepad, int navigation_group = 0, int navigation_sub_group = 0, GJCommons.VoidDelegate on_over = null)
	{
		base.Open(inventory, for_gamepad, InventoryType.Custom);
		_on_over = on_over;
		CustomInventoryItem[] custom_items = _custom_items;
		foreach (CustomInventoryItem obj in custom_items)
		{
			obj.gamepad_item.group = navigation_group;
			obj.gamepad_item.sub_group = navigation_sub_group;
		}
		Redraw();
	}

	public override void Redraw()
	{
		CustomInventoryItem[] custom_items = _custom_items;
		foreach (CustomInventoryItem customInventoryItem in custom_items)
		{
			customInventoryItem.Redraw(inventory);
			if (hide_empty_items)
			{
				customInventoryItem.gameObject.SetActive(customInventoryItem.total_count != 0);
			}
		}
		if (hide_empty_items && _grid != null)
		{
			_grid.Reposition();
			_grid.repositionNow = true;
		}
	}

	public void OnItemOver(string item_id)
	{
		_panel_auto_scroll.Perform();
		_on_over.TryInvoke();
	}

	public override GamepadNavigationItem GetFirstNavigationItem(Direction dir)
	{
		return _custom_items[0].gamepad_item;
	}
}
