using System.Collections.Generic;
using DG.Tweening;
using DLCRefugees;
using UnityEngine;

public class InventoryPanelGUI : MonoBehaviour
{
	public UILabel money_label;

	public UILabel panel_title;

	public GameObject bottom_bar;

	public bool draw_equipped_icons;

	public bool dont_show_empty_rows;

	public UI2DSprite spr_head;

	private UIScrollView _scroll_view;

	private UITable _widgets_table;

	private InventoryWidget _inventory_widget_prefab;

	private BagInventoryWidget _bag_inventory_widget_prefab;

	private SoulContainerInventoryWidget _soul_container_widget;

	private Dictionary<string, CustomInventoryWidget> _custom_widgets_presets = new Dictionary<string, CustomInventoryWidget>();

	private BaseInventoryWidget _selected_widget;

	private BaseItemCellGUI _selected_item_gui;

	private Item _selected_item;

	private MultiInventory _multi_inventory;

	private GJCommons.VoidDelegate _on_over;

	private GJCommons.VoidDelegate _on_out;

	private GJCommons.VoidDelegate _on_press;

	private GJCommons.VoidDelegate _on_custom_widget_over;

	private List<InventoryWidget> _widgets = new List<InventoryWidget>();

	private List<UIWidget> _separators = new List<UIWidget>();

	private List<CustomInventoryWidget> _custom_widgets = new List<CustomInventoryWidget>();

	private bool _waiting_for_reposition;

	[SerializeField]
	private GameObject bottom_bar_with_debt;

	[SerializeField]
	private UILabel money_label_2;

	[SerializeField]
	private UILabel debt_label;

	public BaseInventoryWidget selected_widget => _selected_widget;

	public static InventoryPanelGUI last { get; private set; }

	public MultiInventory multi_inventory => _multi_inventory;

	public BaseItemCellGUI selected_item_gui
	{
		get
		{
			return _selected_item_gui;
		}
		private set
		{
			_selected_item_gui = value;
			_selected_item = ((_selected_item_gui == null) ? null : _selected_item_gui.item);
		}
	}

	public Item selected_item => _selected_item;

	public bool selected_item_is_empty
	{
		get
		{
			if (_selected_item != null)
			{
				return _selected_item.IsEmpty();
			}
			return true;
		}
	}

	public bool selected_widget_is_not_main
	{
		get
		{
			if (!(_selected_widget == null))
			{
				return !_selected_widget.IsMain();
			}
			return true;
		}
	}

	public void Init()
	{
		_inventory_widget_prefab = GetComponentInChildren<InventoryWidget>(includeInactive: true);
		_inventory_widget_prefab.Init();
		_bag_inventory_widget_prefab = GetComponentInChildren<BagInventoryWidget>(includeInactive: true);
		if (_bag_inventory_widget_prefab != null)
		{
			_bag_inventory_widget_prefab.Init();
		}
		_soul_container_widget = GetComponentInChildren<SoulContainerInventoryWidget>(includeInactive: true);
		_soul_container_widget?.Init();
		_scroll_view = GetComponentInChildren<UIScrollView>(includeInactive: true);
		_widgets_table = _scroll_view.GetComponentInChildren<UITable>(includeInactive: true);
		CustomInventoryWidget[] componentsInChildren = GetComponentsInChildren<CustomInventoryWidget>(includeInactive: true);
		foreach (CustomInventoryWidget customInventoryWidget in componentsInChildren)
		{
			customInventoryWidget.Init();
			_custom_widgets_presets.Add(customInventoryWidget.name, customInventoryWidget);
			customInventoryWidget.Deactivate();
		}
		base.gameObject.SetActive(value: false);
	}

	public void Open(MultiInventory multi_inventory, int navigation_group = 0, int navigation_sub_group = 0, bool clear_name = false, int custom_line_length = -1, bool is_debt_show = false)
	{
		base.gameObject.SetActive(value: true);
		DoOpening(multi_inventory, navigation_group, navigation_sub_group, clear_name, custom_line_length, is_debt_show);
	}

	private void DoOpening(MultiInventory multi_inventory, int navigation_group = 0, int navigation_sub_group = 0, bool clear_name = false, int custom_line_length = -1, bool is_debt_show = false)
	{
		_multi_inventory = multi_inventory;
		BaseInventoryWidget baseInventoryWidget = null;
		int num = -1;
		Transform parent = _inventory_widget_prefab.transform.parent;
		bool flag = _bag_inventory_widget_prefab != null;
		bool flag2 = multi_inventory.all.Find((Inventory i) => i.data == MainGame.me.player.data) == null && GlobalCraftControlGUI.is_global_control_active;
		bool flag3 = _soul_container_widget != null;
		if (flag2)
		{
			clear_name = false;
			flag = false;
		}
		foreach (Inventory item in _multi_inventory.all)
		{
			if (item == null || (item.data.inventory_size <= 0 && GlobalCraftControlGUI.is_global_control_active))
			{
				continue;
			}
			if (++num == 0 && clear_name)
			{
				item.ClearName();
			}
			if (_widgets.Count > 0 || _custom_widgets.Count > 0)
			{
				GameObject obj = new GameObject("separator");
				obj.transform.SetParent(parent, worldPositionStays: false);
				UIWidget uIWidget = obj.AddComponent<UIWidget>();
				uIWidget.height = 6;
				_separators.Add(uIWidget);
			}
			string text = item.preset;
			if (!string.IsNullOrEmpty(text) && !_custom_widgets_presets.ContainsKey(text))
			{
				if (text != "soul_container_widget")
				{
					Debug.LogError("No custom preset: " + text);
				}
				text = "";
			}
			if (string.IsNullOrEmpty(text))
			{
				InventoryWidget inventoryWidget = null;
				if (flag3 && item.data != null && item.preset == "soul_container_widget")
				{
					SoulContainerInventoryWidget cont_w = _soul_container_widget.Copy();
					cont_w.SetActive(active: true);
					cont_w.Open(item, BaseGUI.for_gamepad, navigation_group, navigation_sub_group++, dont_show_empty_rows, custom_line_length);
					cont_w.SetCallbacks(delegate(BaseItemCellGUI i)
					{
						OnItemOver(cont_w, i);
					}, OnItemOut, delegate(BaseItemCellGUI i)
					{
						OnItemPress(cont_w, i);
					});
					inventoryWidget = cont_w;
				}
				else if (flag && item.data != null && item.data.is_bag)
				{
					BagInventoryWidget bag_w = _bag_inventory_widget_prefab.Copy();
					bag_w.SetActive(active: true);
					bag_w.Open(item, BaseGUI.for_gamepad, navigation_group, navigation_sub_group++, dont_show_empty_rows, custom_line_length);
					bag_w.SetCallbacks(delegate(BaseItemCellGUI i)
					{
						OnItemOver(bag_w, i);
					}, OnItemOut, delegate(BaseItemCellGUI i)
					{
						OnItemPress(bag_w, i);
					});
					inventoryWidget = bag_w;
				}
				else
				{
					InventoryWidget w2 = _inventory_widget_prefab.Copy();
					w2.Open(item, BaseGUI.for_gamepad, navigation_group, navigation_sub_group++, dont_show_empty_rows, custom_line_length);
					w2.SetCallbacks(delegate(BaseItemCellGUI i)
					{
						OnItemOver(w2, i);
					}, OnItemOut, delegate(BaseItemCellGUI i)
					{
						OnItemPress(w2, i);
					});
					inventoryWidget = w2;
				}
				if (_widgets.Count == 0 && !flag2)
				{
					inventoryWidget.SetMain();
				}
				_widgets.Add(inventoryWidget);
				if (baseInventoryWidget != null && baseInventoryWidget.IsCustom())
				{
					inventoryWidget.SetCustomNavigationTarget(baseInventoryWidget, Direction.Up);
				}
				baseInventoryWidget = inventoryWidget;
			}
			else
			{
				CustomInventoryWidget w = _custom_widgets_presets[text].Copy();
				w.Open(item, BaseGUI.for_gamepad, navigation_group, navigation_sub_group++, delegate
				{
					OnCustomItemOver(w);
				});
				_custom_widgets.Add(w);
				if (baseInventoryWidget != null && !baseInventoryWidget.IsCustom())
				{
					baseInventoryWidget.SetCustomNavigationTarget(w, Direction.Down);
				}
				baseInventoryWidget = w;
			}
		}
		_widgets_table.Reposition();
		_widgets_table.repositionNow = true;
		_waiting_for_reposition = true;
		_scroll_view.transform.localPosition = Vector3.zero;
		_scroll_view.RestrictWithinBounds(instant: false);
		if (draw_equipped_icons)
		{
			DrawEquippedIcons();
		}
		RedrawMoney();
		if (bottom_bar_with_debt != null && is_debt_show)
		{
			float playersDebt = RefugeesCampEngine.GetPlayersDebt();
			if (!playersDebt.Equals(0f))
			{
				RedrawMoneyWithDebt(playersDebt);
			}
		}
		_selected_widget = null;
	}

	public void FullRedraw(MultiInventory multi_inventory, int navigation_group = 0, int navigation_sub_group = 0, bool clear_name = false, int custom_line_length = -1)
	{
		Clear();
		_scroll_view.StopScrolling();
		_scroll_view.transform.DOKill();
		_scroll_view.RestrictWithinBounds(instant: false);
		_scroll_view.transform.localPosition = Vector3.zero;
		DoOpening(multi_inventory, navigation_group, navigation_sub_group, clear_name, custom_line_length);
	}

	public void SetGrayToNotMainWidgets(bool do_not_gray_bags = false)
	{
		foreach (InventoryWidget widget in _widgets)
		{
			if (!widget.IsMain() && (!do_not_gray_bags || !widget.inventory_data.is_bag))
			{
				widget.GetComponent<UIWidget>().alpha = 0.5f;
			}
		}
		foreach (CustomInventoryWidget custom_widget in _custom_widgets)
		{
			custom_widget.GetComponent<UIWidget>().alpha = 0.5f;
		}
	}

	private void LateUpdate()
	{
		if (_waiting_for_reposition)
		{
			_widgets_table.Reposition();
			_waiting_for_reposition = false;
		}
		if (!(_scroll_view == null) && _scroll_view.RestrictWithinBounds(instant: false) && DOTween.IsTweening(_scroll_view.transform))
		{
			_scroll_view.transform.DOKill();
		}
	}

	public void InitGamepad(GamepadNavigationController controller)
	{
		if (!(controller == null))
		{
			controller.ReinitItems(focus_on_first_active: false);
			if (selected_item == null)
			{
				controller.FocusOnFirstActive();
				return;
			}
			BaseItemCellGUI itemCellGuiForItem = GetItemCellGuiForItem(selected_item);
			controller.SetFocusedItem((itemCellGuiForItem == null) ? null : itemCellGuiForItem.gamepad_item, animate_auto_scroll: false);
		}
	}

	public void Redraw()
	{
		foreach (InventoryWidget widget in _widgets)
		{
			widget.Redraw();
		}
		foreach (CustomInventoryWidget custom_widget in _custom_widgets)
		{
			custom_widget.Redraw();
		}
		if (draw_equipped_icons)
		{
			DrawEquippedIcons();
		}
		RedrawMoney();
		_widgets_table.Reposition();
		_widgets_table.repositionNow = true;
	}

	private void DrawEquippedIcons()
	{
		foreach (InventoryWidget widget in _widgets)
		{
			widget.DrawEquippedIcons();
		}
	}

	private void RedrawMoney()
	{
		if (money_label != null)
		{
			money_label.text = Trading.FormatMoney(_multi_inventory.money, print_zero: true);
		}
	}

	private void RedrawMoneyWithDebt(float debt_value)
	{
		bottom_bar_with_debt.SetActive(value: true);
		if (money_label_2 != null)
		{
			money_label_2.text = Trading.FormatMoney(_multi_inventory.money, print_zero: true);
		}
		if (debt_label != null)
		{
			debt_label.text = GJL.L("debt_label") + ": ";
			debt_label.text += Trading.FormatMoney(debt_value, print_zero: true);
		}
	}

	private void OnItemOver(InventoryWidget widget, BaseItemCellGUI item_gui)
	{
		if (!GUIElements.me.context_menu_bubble.is_shown && !GUIElements.me.dialog.is_shown)
		{
			last = this;
			_selected_widget = widget;
			selected_item_gui = item_gui;
			_on_over.TryInvoke();
		}
	}

	private void OnItemOut(BaseItemCellGUI item_gui)
	{
		_on_out.TryInvoke();
	}

	private void OnCustomItemOver(CustomInventoryWidget widget)
	{
		_selected_widget = widget;
		selected_item_gui = null;
		_on_custom_widget_over.TryInvoke();
	}

	private void OnItemPress(InventoryWidget widget, BaseItemCellGUI item_gui)
	{
		last = this;
		_selected_widget = widget;
		selected_item_gui = item_gui;
		_on_press.TryInvoke();
	}

	public void SetCallbacks(GJCommons.VoidDelegate on_over, GJCommons.VoidDelegate on_out, GJCommons.VoidDelegate on_press, GJCommons.VoidDelegate on_custom_widget_over = null)
	{
		_on_over = on_over;
		_on_out = on_out;
		_on_press = on_press;
		_on_custom_widget_over = on_custom_widget_over;
	}

	public void Hide()
	{
		Clear();
		_scroll_view.StopScrolling();
		_scroll_view.transform.DOKill();
		_scroll_view.RestrictWithinBounds(instant: false);
		_scroll_view.transform.localPosition = Vector3.zero;
		if (bottom_bar_with_debt != null)
		{
			bottom_bar_with_debt.SetActive(value: false);
		}
		base.gameObject.SetActive(value: false);
	}

	private void Clear()
	{
		foreach (InventoryWidget widget in _widgets)
		{
			widget.Deactivate();
			widget.DestroyGO();
		}
		foreach (CustomInventoryWidget custom_widget in _custom_widgets)
		{
			custom_widget.Deactivate();
			custom_widget.DestroyGO();
		}
		foreach (UIWidget separator in _separators)
		{
			separator.Deactivate();
			separator.DestroyGO();
		}
		_widgets.Clear();
		_custom_widgets.Clear();
		_separators.Clear();
		_widgets_table.Reposition();
		_scroll_view.ResetPosition();
	}

	public void UpdateSelection(bool after_item_count_gui = false)
	{
		if (selected_item_gui != null && !selected_item_gui.id_empty)
		{
			if (BaseGUI.for_gamepad)
			{
				selected_item_gui.OnOver(by_gamepad: true);
			}
			else if (!after_item_count_gui)
			{
				selected_item_gui.OnOver(by_gamepad: false);
			}
		}
	}

	public BaseItemCellGUI GetItemCellGuiForItem(Item item)
	{
		if (item == null || _widgets.Count == 0)
		{
			return null;
		}
		foreach (InventoryWidget widget in _widgets)
		{
			BaseItemCellGUI itemCellGuiForItem = widget.GetItemCellGuiForItem(item);
			if (itemCellGuiForItem != null)
			{
				return itemCellGuiForItem;
			}
		}
		return null;
	}

	public void UpdatePrices(InventoryWidget.ItemPriceDelegate price_delegate, int count_modificator)
	{
		if (price_delegate == null)
		{
			return;
		}
		foreach (InventoryWidget widget in _widgets)
		{
			widget.UpdatePrices(price_delegate, count_modificator);
		}
	}

	public void FilterItems(InventoryWidget.ItemFilterDelegate filter_delegate)
	{
		foreach (InventoryWidget widget in _widgets)
		{
			widget.FilterItems(filter_delegate);
		}
	}

	public void SetInactiveStateToEmptyCells()
	{
		foreach (InventoryWidget widget in _widgets)
		{
			widget.SetInactiveStateToEmptyCells();
		}
	}

	public void ClearSelection()
	{
		selected_item_gui = null;
	}
}
