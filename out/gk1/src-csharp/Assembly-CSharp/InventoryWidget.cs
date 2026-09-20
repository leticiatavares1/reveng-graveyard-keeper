using System.Collections.Generic;
using UnityEngine;

public class InventoryWidget : BaseInventoryWidget
{
	public enum ItemFilterResult
	{
		Active,
		Inactive,
		Hide,
		Unknown
	}

	public delegate void ItemDelegate(BaseItemCellGUI item_gui);

	public delegate ItemFilterResult ItemFilterDelegate(Item item, InventoryWidget widget);

	public delegate float ItemPriceDelegate(Item item, int count_modificator);

	protected Item inventory_item;

	protected UIGrid items_table;

	protected BaseItemCellGUI item_prefab;

	protected List<BaseItemCellGUI> items = new List<BaseItemCellGUI>();

	protected ItemDelegate on_over;

	protected ItemDelegate on_out;

	protected ItemDelegate on_press;

	public bool auto_height = true;

	public bool auto_width;

	public bool dont_show_empty_rows;

	public bool custom_size;

	protected PanelAutoScroll header_auto_scroll;

	protected PanelAutoScroll bottom_auto_scroll;

	private bool _interaction_enabled = true;

	private const int HEADER_HEIGHT = 46;

	private bool _moved_contents;

	private int _default_table_y;

	private int _default_back_anchor_y;

	public UIWidget inventory_lock_widget;

	public UIWidget inventory_lock_progressbar_w;

	public UIProgressBar lock_progress_bar;

	public UILabel lock_descr_text;

	public UILabel lock_tier_1;

	public UILabel lock_tier_2;

	public bool can_show_2h_items;

	public bool sort = true;

	public bool show_empty_as_invisible;

	public int auto_height_offset;

	public int auto_width_offset;

	public bool interaction_enabled
	{
		get
		{
			return _interaction_enabled;
		}
		set
		{
			_interaction_enabled = value;
			foreach (BaseItemCellGUI item in items)
			{
				item.interaction_enabled = _interaction_enabled;
			}
		}
	}

	public override void Init()
	{
		if (!initialized)
		{
			item_prefab = GetComponentInChildren<BaseItemCellGUI>(includeInactive: true);
			items.AddRange(GetComponentsInChildren<BaseItemCellGUI>(includeInactive: true));
			custom_size = items.Count > 1;
			if (!custom_size)
			{
				items.Clear();
				item_prefab.Deactivate();
			}
			FindItemsTable();
			if (header_label != null)
			{
				header_auto_scroll = header_label.transform.parent.GetComponentInChildren<PanelAutoScroll>();
			}
			Transform transform = base.transform.Find("background");
			bottom_auto_scroll = ((transform != null) ? transform.GetComponentInChildren<PanelAutoScroll>() : GetComponentInChildren<PanelAutoScroll>());
			Hide();
			initialized = true;
			if (inventory_lock_widget != null)
			{
				inventory_lock_widget.gameObject.SetActive(value: false);
			}
		}
	}

	public virtual void Open(Inventory inventory, bool for_gamepad, int navigation_group = 0, int navigation_sub_group = 0, bool dont_show_empty_rows = false, int custom_line_length = -1)
	{
		if (!initialized)
		{
			Init();
		}
		base.gameObject.SetActive(value: true);
		if (inventory == null)
		{
			inventory = new Inventory(MainGame.me.player);
		}
		base.Open(inventory, for_gamepad);
		inventory_item = inventory.data;
		this.dont_show_empty_rows = dont_show_empty_rows;
		if (custom_line_length > 0)
		{
			if (items_table == null)
			{
				FindItemsTable();
			}
			items_table.maxPerLine = custom_line_length;
			auto_width = true;
		}
		if (!custom_size)
		{
			int size = inventory.size;
			while (size-- > 0)
			{
				BaseItemCellGUI baseItemCellGUI = item_prefab.Copy();
				baseItemCellGUI.interaction_enabled = interaction_enabled;
				items.Add(baseItemCellGUI);
			}
		}
		foreach (BaseItemCellGUI item in items)
		{
			item.SetCallbacks(OnItemOver, OnItemOut, OnItemPress);
			item.InitInputBehaviour(navigation_group, navigation_sub_group);
			item.InitTooltips();
		}
		Redraw();
	}

	public void UpdateItemsCallbacksAndStuff(int navigation_group = 0, int navigation_sub_group = 0)
	{
		foreach (BaseItemCellGUI item in items)
		{
			item.SetCallbacks(OnItemOver, OnItemOut, OnItemPress);
			item.InitInputBehaviour(navigation_group, navigation_sub_group);
			item.InitTooltips();
		}
	}

	public override void Redraw()
	{
		if (inventory_lock_widget != null)
		{
			inventory_lock_widget.gameObject.SetActive(inventory.is_locked);
			lock_progress_bar.gameObject.SetActive(inventory.vendor_tier_info != null);
			lock_descr_text.gameObject.SetActive(inventory.vendor_tier_info != null);
			if (inventory.vendor_tier_info == null)
			{
				UILabel uILabel = lock_tier_1;
				string text2 = (lock_tier_2.text = "");
				uILabel.text = text2;
			}
			else
			{
				inventory_lock_progressbar_w.gameObject.SetActive(inventory.vendor_tier_info.progressbar_visible);
				lock_progress_bar.value = inventory.vendor_tier_info.progress;
				lock_tier_1.text = "(tr" + inventory.vendor_tier_info.tier_1 + ")";
				lock_tier_2.text = "(tr" + inventory.vendor_tier_info.tier_2 + ")";
				lock_descr_text.text = GJL.L((lock_progress_bar.value >= 1f) ? "vendor_tier_lock_next" : "vendor_tier_lock_info", GJCommons.GetRomeNumber(inventory.vendor_tier_info.tier_2));
				inventory_lock_widget.GetComponentInChildren<UITableOrGrid>().Reposition();
			}
		}
		if (inventory_item == null)
		{
			return;
		}
		if (sort)
		{
			inventory_item.Sort();
		}
		int num = 0;
		foreach (Item item in inventory_item.inventory)
		{
			ItemDefinition definition = item.definition;
			if ((definition == null || !definition.is_big || can_show_2h_items) && num < items.Count)
			{
				items[num].DrawItem(item);
				if (show_empty_as_invisible)
				{
					items[num].container.container.gameObject.SetActive(!item.IsEmpty());
				}
				num++;
			}
		}
		for (int i = num; i < items.Count; i++)
		{
			items[i].DrawItem(Item.empty);
		}
		if (!custom_size)
		{
			RecalculateWidgetSize();
		}
	}

	public void DrawEquippedIcons()
	{
		foreach (BaseItemCellGUI item in items)
		{
			item.DrawEquippedIcons();
		}
	}

	[ContextMenu("Recalculate widget size")]
	private void RecalculateWidgetSize()
	{
		if (items_table == null)
		{
			FindItemsTable();
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (BaseItemCellGUI item in items)
		{
			if (item.gameObject.activeSelf)
			{
				num++;
				if (!item.id_empty)
				{
					num2 += item.item.definition.item_size;
				}
			}
			else if (!item.id_empty)
			{
				num3++;
			}
		}
		int num4 = Mathf.CeilToInt((float)num / (float)items_table.maxPerLine);
		if (dont_show_empty_rows)
		{
			num4 = Mathf.CeilToInt((float)num2 / (float)items_table.maxPerLine);
			if (num4 == 0)
			{
				num4 = 1;
			}
			int num5 = num4 * items_table.maxPerLine + num3;
			for (int i = 0; i < items.Count; i++)
			{
				if (i < num5)
				{
					if (items[i].id_empty)
					{
						items[i].Activate();
					}
				}
				else
				{
					items[i].Deactivate();
				}
			}
		}
		items_table.Reposition();
		if (!auto_height)
		{
			return;
		}
		int num6 = 0;
		UIWidget component = GetComponent<UIWidget>();
		component.height = 29 + num4 * 42 - num6 + auto_height_offset;
		if (auto_width)
		{
			component.width = 10 + items_table.maxPerLine * 42 + auto_width_offset;
			UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].ResetAndUpdateAnchors();
			}
		}
	}

	private void FindItemsTable()
	{
		UIGrid[] componentsInChildren = GetComponentsInChildren<UIGrid>(includeInactive: true);
		foreach (UIGrid uIGrid in componentsInChildren)
		{
			if (uIGrid != null)
			{
				items_table = uIGrid;
			}
		}
		if (items_table == null)
		{
			Debug.LogError("Couldn't find items table (UIGrid)", this);
		}
	}

	public BaseItemCellGUI ItemAt(int index)
	{
		if (index > items.Count)
		{
			return null;
		}
		return items[index];
	}

	public void FilterItems(ItemFilterDelegate filter_delegate)
	{
		foreach (BaseItemCellGUI item in items)
		{
			switch (filter_delegate(item.item, this))
			{
			case ItemFilterResult.Active:
				item.SetGrayState(set_inactive: false);
				break;
			case ItemFilterResult.Inactive:
				item.SetGrayState();
				break;
			case ItemFilterResult.Hide:
				item.Deactivate();
				break;
			case ItemFilterResult.Unknown:
				item.DrawUnknown();
				break;
			}
		}
		if (!custom_size)
		{
			RecalculateWidgetSize();
		}
	}

	public void SetInactiveStateToEmptyCells()
	{
		foreach (BaseItemCellGUI item in items)
		{
			if (item.id_empty)
			{
				item.SetInactiveState();
			}
		}
	}

	public void UpdatePrices(ItemPriceDelegate price_delegate, int count_modificator)
	{
		if (price_delegate == null)
		{
			return;
		}
		foreach (BaseItemCellGUI item in items)
		{
			if (item.id_empty || item.is_inactive_state)
			{
				item.ClearPrice();
			}
			else
			{
				item.SetItemPrice(price_delegate(item.item, count_modificator));
			}
		}
	}

	public void InitForCraft(List<string> item_types)
	{
		foreach (BaseItemCellGUI item in items)
		{
			if (!item_types.Contains(item.item_id))
			{
				item.widget.alpha = item.colors.inactive.a;
			}
		}
	}

	public void SetCallbacks(ItemDelegate on_over, ItemDelegate on_out, ItemDelegate on_press)
	{
		this.on_over = on_over;
		this.on_out = on_out;
		this.on_press = on_press;
	}

	public void Hide()
	{
		if (custom_size)
		{
			foreach (BaseItemCellGUI item in items)
			{
				item.DrawEmpty();
			}
			return;
		}
		Clear();
		base.gameObject.SetActive(value: false);
	}

	protected virtual void Clear()
	{
		foreach (BaseItemCellGUI item in items)
		{
			item.DestroyGO();
		}
		items.Clear();
	}

	public BaseItemCellGUI GetItemCellGuiForItem(Item item)
	{
		if (item == null)
		{
			return null;
		}
		foreach (BaseItemCellGUI item2 in items)
		{
			if (item2.item == item)
			{
				return item2;
			}
		}
		return null;
	}

	private void OnItemOver(BaseItemCellGUI item_gui)
	{
		if (on_over != null)
		{
			on_over(item_gui);
		}
		if (!for_gamepad)
		{
			return;
		}
		int maxPerLine = items_table.maxPerLine;
		int num = items.IndexOf(item_gui) / maxPerLine + 1;
		int num2 = items.Count / maxPerLine;
		if (items.Count % maxPerLine > 0)
		{
			num2++;
		}
		if (num == 1)
		{
			if (header_auto_scroll != null)
			{
				header_auto_scroll.Perform(!base.just_opened);
			}
		}
		else if (num == num2 && bottom_auto_scroll != null)
		{
			bottom_auto_scroll.Perform(!base.just_opened);
		}
	}

	private void OnItemOut(BaseItemCellGUI item_gui)
	{
		if (on_out != null)
		{
			on_out(item_gui);
		}
	}

	private void OnItemPress(BaseItemCellGUI item_gui)
	{
		if (on_press != null)
		{
			on_press(item_gui);
		}
	}

	public override void SetCustomNavigationTarget(BaseInventoryWidget widget, Direction direction)
	{
		GamepadNavigationItem firstNavigationItem = widget.GetFirstNavigationItem(direction);
		int maxPerLine = items_table.maxPerLine;
		if (items.Count <= maxPerLine)
		{
			foreach (BaseItemCellGUI item in items)
			{
				item.gamepad_item.SetCustomDirectionItem(firstNavigationItem, direction);
			}
			return;
		}
		int num = items.Count / maxPerLine;
		if (items.Count % maxPerLine == 0)
		{
			num--;
		}
		int num2 = ((direction != Direction.Up) ? (num * maxPerLine) : 0);
		int num3 = ((direction == Direction.Up) ? maxPerLine : items.Count);
		for (int i = num2; i < num3; i++)
		{
			items[i].gamepad_item.SetCustomDirectionItem(firstNavigationItem, direction);
		}
	}

	public void ClearItems()
	{
		Clear();
	}
}
