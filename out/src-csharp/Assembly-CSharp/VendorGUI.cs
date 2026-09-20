using System.Collections.Generic;
using UnityEngine;

public class VendorGUI : BaseGUI
{
	private const int OFFER_SIZE = 6;

	public UILabel result_money;

	public InventoryPanelGUI player_panel;

	public InventoryPanelGUI vendor_panel;

	public InventoryWidget player_offer_widget;

	public InventoryWidget vendor_offer_widget;

	private List<InventoryPanelGUI> _panels;

	private List<InventoryWidget> _widgets;

	private MultiInventory _player;

	private MultiInventory _vendor;

	private MultiInventory _vendor_real;

	private MultiInventory _player_offer;

	private MultiInventory _vendor_offer;

	private InventoryPanelGUI _selected_panel;

	private InventoryWidget _selected_widget;

	private BaseItemCellGUI _selected_item_gui;

	private Item _selected_item;

	public Trading trading;

	private WorldGameObject _last_vendor_obj;

	private WorldGameObject _vendor_obj;

	public UIButton btn_confirm;

	public UIButton btn_cancel;

	private bool _enable_time_after_close = true;

	public override void Init()
	{
		_panels = new List<InventoryPanelGUI> { player_panel, vendor_panel };
		_widgets = new List<InventoryWidget> { player_offer_widget, vendor_offer_widget };
		player_panel.Init();
		player_offer_widget.Init();
		vendor_panel.Init();
		vendor_offer_widget.Init();
		player_panel.SetCallbacks(delegate
		{
			OnItemOver(null, players: true, offer: false);
		}, null, OnItemSelect, OnCustomItemOver);
		player_offer_widget.SetCallbacks(delegate(BaseItemCellGUI item)
		{
			OnItemOver(item, players: true, offer: true);
		}, null, delegate
		{
			OnItemSelect();
		});
		vendor_panel.SetCallbacks(delegate
		{
			OnItemOver(null, players: false, offer: false);
		}, null, OnItemSelect, OnCustomItemOver);
		vendor_offer_widget.SetCallbacks(delegate(BaseItemCellGUI item)
		{
			OnItemOver(item, players: false, offer: true);
		}, null, delegate
		{
			OnItemSelect();
		});
		base.Init();
	}

	public void Open(WorldGameObject vendor_obj, GJCommons.VoidDelegate on_hide)
	{
		SetOnHide(on_hide);
		base.Open();
		trading = new Trading(vendor_obj);
		if (trading.trader == null)
		{
			Debug.LogError("Can not open VendorGUI: vendor of " + vendor_obj.obj_id + " is null!");
			return;
		}
		_player = trading.player_inventory;
		_vendor = trading.trader.drawing_inventory;
		_vendor_real = trading.trader.inventory;
		_vendor_obj = vendor_obj;
		if (_vendor_obj != null)
		{
			SmartAudioEngine.me.OnStartNPCInteraction(_vendor_obj.obj_def);
		}
		Item player_offer = trading.player_offer;
		player_offer.SetInventorySize(6);
		Item cur_offer = trading.trader.cur_offer;
		cur_offer.SetInventorySize(6);
		_player_offer = new MultiInventory(new Inventory(player_offer, ">>> Your offer >>>"));
		_vendor_offer = new MultiInventory(new Inventory(cur_offer, "<<< Vendor's offer <<<"));
		player_panel.Open(_player, 1);
		vendor_panel.Open(_vendor, 2, 0, clear_name: true);
		player_panel.SetGrayToNotMainWidgets(do_not_gray_bags: true);
		player_offer_widget.Open(_player_offer.all[0], BaseGUI.for_gamepad, 3);
		vendor_offer_widget.Open(_vendor_offer.all[0], BaseGUI.for_gamepad, 3);
		vendor_panel.panel_title.text = GJL.L(vendor_obj.obj_id);
		player_panel.panel_title.text = GJL.L("player_inventory");
		player_panel.spr_head.sprite2D = EasySpritesCollection.GetSprite("000_hed_down");
		vendor_panel.spr_head.sprite2D = vendor_obj.GetHeadSprite();
		Redraw();
		if (_last_vendor_obj != vendor_obj)
		{
			_selected_panel = null;
			_selected_item = null;
		}
		_last_vendor_obj = vendor_obj;
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			if (_selected_panel == null || _selected_item == null)
			{
				base.gamepad_controller.FocusOnFirstActive();
			}
			else
			{
				BaseItemCellGUI itemCellGuiForItem = _selected_panel.GetItemCellGuiForItem(_selected_item);
				base.gamepad_controller.SetFocusedItem((itemCellGuiForItem == null) ? null : itemCellGuiForItem.gamepad_item);
			}
		}
		_enable_time_after_close = EnvironmentEngine.me.auto_adjust_time;
		if (_enable_time_after_close)
		{
			EnvironmentEngine.me.EnableTime(enable: false);
		}
	}

	private void OnItemOver(BaseItemCellGUI item_gui, bool players, bool offer)
	{
		if (offer)
		{
			player_panel.ClearSelection();
			vendor_panel.ClearSelection();
			_selected_panel = null;
			_selected_widget = (players ? player_offer_widget : vendor_offer_widget);
			_selected_item_gui = item_gui;
			_selected_item = _selected_item_gui.item;
		}
		else
		{
			(players ? vendor_panel : player_panel).ClearSelection();
			_selected_panel = (players ? player_panel : vendor_panel);
			_selected_widget = null;
			_selected_item_gui = _selected_panel.selected_item_gui;
			_selected_item = _selected_panel.selected_item;
		}
		if (BaseGUI.for_gamepad)
		{
			UpdateTips("put");
		}
	}

	private void OnCustomItemOver()
	{
		PrintTipForCustomWidget();
	}

	private void PrintTipForCustomWidget()
	{
		base.button_tips.Print(GameKeyTip.Option2("finish offer", !OfferIsEmpty()), GameKeyTip.Close());
	}

	private int GetMaxMoveCount(Item item, MultiInventory from, MultiInventory to)
	{
		int totalCount = from.GetTotalCount(item.id);
		int b = to.CanAddCount(item.id, count_bags: true);
		return Mathf.Min(totalCount, b);
	}

	private void UpdateTips(string tip)
	{
		List<GameKeyTip> list = new List<GameKeyTip>();
		int num;
		int num2;
		if (_selected_item != null)
		{
			num = (_selected_item.IsEmpty() ? 1 : 0);
			if (num == 0)
			{
				num2 = ((!_selected_item_gui.is_inactive_state) ? 1 : 0);
				goto IL_0030;
			}
		}
		else
		{
			num = 1;
		}
		num2 = 0;
		goto IL_0030;
		IL_0030:
		bool flag = (byte)num2 != 0;
		MultiInventory from;
		MultiInventory to;
		if (_selected_panel != null)
		{
			from = _selected_panel.multi_inventory;
			to = ((_selected_panel == player_panel) ? _player_offer : _vendor_offer);
		}
		else
		{
			from = ((_selected_widget == player_offer_widget) ? _player_offer : _vendor_offer);
			to = ((_selected_widget == player_offer_widget) ? _player : _vendor);
		}
		int num3 = ((num == 0) ? GetMaxMoveCount(_selected_item, from, to) : 0);
		if (num3 <= 1)
		{
			list.Add(GameKeyTip.Select(tip, flag && num3 > 0));
			list.Add(GameKeyTip.Option1(GJL.L(tip) + " 1", flag && num3 > 0, gamepad_only: true, translate: false));
		}
		else if (_selected_item != null)
		{
			list.Add(GameKeyTip.Select(tip, flag));
			list.Add(GameKeyTip.Option1(GJL.L(tip) + " 1", flag, gamepad_only: true, translate: false));
		}
		list.Add(GameKeyTip.Option2("finish offer", !OfferIsEmpty()));
		list.Add(GameKeyTip.Close());
		base.button_tips.Print(list);
	}

	private void MoveItem(int count)
	{
		if ((_selected_panel == null && _selected_widget == null) || _selected_item == null || _selected_item.IsEmpty() || _selected_item_gui == null || _selected_item_gui.is_inactive_state)
		{
			return;
		}
		MultiInventory multiInventory = null;
		MultiInventory multiInventory2 = null;
		MultiInventory multiInventory3;
		MultiInventory multiInventory4;
		if (_selected_panel != null)
		{
			if (_selected_panel == player_panel)
			{
				multiInventory3 = _player;
				multiInventory4 = _player_offer;
			}
			else
			{
				multiInventory3 = _vendor;
				multiInventory = _vendor_real;
				multiInventory4 = _vendor_offer;
			}
		}
		else if (_selected_widget == player_offer_widget)
		{
			multiInventory3 = _player_offer;
			multiInventory4 = _player;
		}
		else
		{
			multiInventory3 = _vendor_offer;
			multiInventory4 = _vendor;
			multiInventory2 = _vendor_real;
		}
		int maxMoveCount = GetMaxMoveCount(_selected_item, multiInventory3, multiInventory4);
		if (count == 0)
		{
			count = maxMoveCount;
		}
		int value = _selected_item_gui.item.value;
		bool flag = true;
		if (count == 0 || maxMoveCount == 0)
		{
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.MoveAllStack))
		{
			if (multiInventory4 == _vendor)
			{
				int num = _selected_item.definition.product_tier - 1;
				if (num >= trading.trader.max_tier)
				{
					num = trading.trader.max_tier;
				}
				multiInventory3.MoveItemTo(multiInventory4.all[num].data, _selected_item, maxMoveCount);
			}
			else if (!multiInventory3.MoveItemTo(multiInventory4, _selected_item, maxMoveCount))
			{
				return;
			}
			if (multiInventory != null)
			{
				multiInventory.RemoveItem(_selected_item, maxMoveCount, MultiInventory.DestinationType.AllFromFirst);
			}
			else
			{
				multiInventory2?.AddItem(_selected_item.id, maxMoveCount);
			}
		}
		else if (count == 1 || maxMoveCount == 1)
		{
			if (multiInventory4 == _vendor)
			{
				int num2 = _selected_item.definition.product_tier - 1;
				if (num2 >= trading.trader.max_tier)
				{
					num2 = trading.trader.max_tier;
				}
				multiInventory3.MoveItemTo(multiInventory4.all[num2].data, _selected_item, 1);
			}
			else if (!multiInventory3.MoveItemTo(multiInventory4, _selected_item, 1))
			{
				return;
			}
			if (multiInventory != null)
			{
				multiInventory.RemoveItem(_selected_item, 1, MultiInventory.DestinationType.AllFromFirst);
			}
			else
			{
				multiInventory2?.AddItem(_selected_item.id, 1);
			}
		}
		else
		{
			OpenItemCountWidnow(_selected_item.id, count, _selected_panel != null, _selected_panel == player_panel || _selected_widget == player_offer_widget);
			flag = false;
		}
		if (flag)
		{
			Sounds.PlaySound("item_put");
		}
		Redraw();
		_selected_item_gui.OnOver(BaseGUI.for_gamepad);
		if (count >= value)
		{
			TooltipsManager.Redraw();
		}
	}

	private void OpenItemCountWidnow(string item_id, int can_move, bool from_inventory, bool from_player)
	{
		base.button_tips.Deactivate();
		GUIElements.me.item_count.Open(item_id, 1, can_move, delegate(int chosen)
		{
			MultiInventory multiInventory = null;
			MultiInventory multiInventory2 = null;
			MultiInventory multiInventory3;
			MultiInventory multiInventory4;
			if (from_inventory)
			{
				if (from_player)
				{
					multiInventory3 = _player;
					multiInventory4 = _player_offer;
				}
				else
				{
					multiInventory3 = _vendor;
					multiInventory = _vendor_real;
					multiInventory4 = _vendor_offer;
				}
			}
			else if (from_player)
			{
				multiInventory3 = _player_offer;
				multiInventory4 = _player;
			}
			else
			{
				multiInventory3 = _vendor_offer;
				multiInventory4 = _vendor;
				multiInventory2 = _vendor_real;
			}
			if (multiInventory4 == _vendor)
			{
				int num3 = _selected_item.definition.product_tier - 1;
				if (num3 >= trading.trader.max_tier)
				{
					num3 = trading.trader.max_tier;
				}
				multiInventory3.MoveItemTo(multiInventory4.all[num3].data, _selected_item, chosen);
			}
			else
			{
				multiInventory3.MoveItemTo(multiInventory4, _selected_item, chosen);
			}
			if (multiInventory != null)
			{
				multiInventory.RemoveItem(_selected_item, chosen, MultiInventory.DestinationType.AllFromFirst);
			}
			else
			{
				multiInventory2?.AddItem(_selected_item.id, chosen);
			}
		}, 1, delegate(int amount)
		{
			float num = 0f;
			for (int i = 0; i < amount; i++)
			{
				float num2 = (from_player ? trading.GetSingleItemCostInPlayerInventory(item_id, from_inventory ? (i + 1) : (-i)) : trading.GetSingleItemCostInTraderInventory(item_id, from_inventory ? (-i) : (i + 1)));
				num += Mathf.Round(num2 * 100f) / 100f;
			}
			return num;
		});
		GUIElements.me.item_count.SetOnHide(delegate
		{
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Activate();
			}
			Redraw();
			RestoreFocusAfterDialog();
			TooltipsManager.Redraw();
		});
	}

	private void OnItemSelect()
	{
		if (!BaseGUI.for_gamepad)
		{
			MoveItem(LazyInput.GetKeyDown(GameKey.RightClick) ? 1 : 0);
		}
	}

	private void RefreshButtonsState()
	{
		bool flag = !OfferIsEmpty() && trading.CanAcceptOffer();
		bool flag2 = !OfferIsEmpty();
		UIButton[] componentsInChildren = btn_confirm.gameObject.GetComponentsInChildren<UIButton>(includeInactive: true);
		foreach (UIButton uIButton in componentsInChildren)
		{
			uIButton.isEnabled = flag;
			if (uIButton != btn_confirm)
			{
				uIButton.SetState(flag ? UIButtonColor.State.Pressed : UIButtonColor.State.Disabled, immediate: true);
			}
		}
		componentsInChildren = btn_cancel.gameObject.GetComponentsInChildren<UIButton>(includeInactive: true);
		foreach (UIButton uIButton2 in componentsInChildren)
		{
			uIButton2.isEnabled = flag2;
			if (uIButton2 != btn_cancel)
			{
				uIButton2.SetState(flag2 ? UIButtonColor.State.Pressed : UIButtonColor.State.Disabled, immediate: true);
			}
		}
	}

	public void FinishOffer()
	{
		if (trading.CanAcceptOffer())
		{
			trading.DoAcceptOffer(need_check: false);
			vendor_panel.Hide();
			vendor_panel.Open(_vendor, 2, 0, clear_name: true);
			Redraw();
			TooltipsManager.Redraw();
			if (BaseGUI.for_gamepad)
			{
				base.gamepad_controller.ReinitItems(focus_on_first_active: true);
				_selected_item_gui.OnOver(BaseGUI.for_gamepad);
			}
			Sounds.PlaySound("coins_sound");
		}
		else
		{
			GUIElements.me.dialog.OpenOK("cant_accept_offer", RestoreFocusAfterDialog);
			Debug.Log("Can not accept offer!");
		}
	}

	private void Redraw()
	{
		player_panel.Redraw();
		player_panel.FilterItems(trading.BuyableItemsFilter);
		vendor_panel.Redraw();
		vendor_panel.FilterItems(trading.SellableItemsFilter);
		player_offer_widget.Redraw();
		vendor_offer_widget.Redraw();
		player_panel.UpdatePrices(trading.GetSingleItemCostInPlayerInventory, 1);
		player_offer_widget.UpdatePrices(trading.GetSingleItemCostInPlayerInventory, 0);
		vendor_panel.UpdatePrices(trading.GetSingleItemCostInTraderInventory, 0);
		vendor_offer_widget.UpdatePrices(trading.GetSingleItemCostInTraderInventory, 1);
		Trading.DrawMoneyOnLabel(player_panel.money_label, trading.player_money, print_zero: true);
		Trading.DrawMoneyOnLabel(vendor_panel.money_label, trading.trader.cur_money, print_zero: true);
		Trading.DrawMoneyOnLabel(result_money, trading.GetTotalBalance());
		RefreshButtonsState();
	}

	private void RestoreFocusAfterDialog()
	{
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.SetFocusedItem((_selected_item_gui != null) ? _selected_item_gui.gamepad_item : null);
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		ResetOrder();
		_selected_widget = null;
		_selected_item_gui = null;
		foreach (InventoryPanelGUI panel in _panels)
		{
			panel.Hide();
		}
		foreach (InventoryWidget widget in _widgets)
		{
			widget.Hide();
		}
		base.Hide(play_hide_sound);
		if (_vendor_obj != null)
		{
			SmartAudioEngine.me.OnEndNPCInteraction(_vendor_obj.obj_def);
		}
		EnvironmentEngine.me.EnableTime(_enable_time_after_close);
	}

	protected override bool OnPressedSelect()
	{
		MoveItem(0);
		return true;
	}

	protected override bool OnPressedOption1()
	{
		MoveItem(1);
		return true;
	}

	protected override bool OnPressedOption2()
	{
		FinishOffer();
		return true;
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		if (OfferIsEmpty())
		{
			base.OnClosePressed();
			return;
		}
		GUIElements.me.dialog.OpenYesNo("cancel_offer", delegate
		{
			Hide();
		}, RestoreFocusAfterDialog);
	}

	private bool OfferIsEmpty()
	{
		if (trading.player_offer.inventory.Count == 0)
		{
			return trading.trader.cur_offer.inventory.Count == 0;
		}
		return false;
	}

	private void ResetOrder()
	{
		if (_player_offer != null)
		{
			_player.AddItems(_player_offer.all[0].data.inventory, allow_bags: true);
			_player_offer.all[0].data.inventory.Clear();
		}
		if (_vendor_offer != null)
		{
			_vendor_real.AddItems(_vendor_offer.all[0].data.inventory);
			_vendor.AddItems(_vendor_offer.all[0].data.inventory);
			_vendor_offer.all[0].data.inventory.Clear();
		}
		if (trading != null && trading.trader != null)
		{
			trading.trader.FillDrawingMultiInventory();
			vendor_panel.Hide();
			vendor_panel.Open(_vendor, 2, 0, clear_name: true);
			Redraw();
			TooltipsManager.Redraw();
			if (BaseGUI.for_gamepad)
			{
				base.gamepad_controller.ReinitItems(focus_on_first_active: false);
				_selected_item_gui.OnOver(BaseGUI.for_gamepad);
			}
		}
	}

	public void ResetOrderAndRedraw()
	{
		Sounds.OnGUIClick();
		ResetOrder();
		Redraw();
	}
}
