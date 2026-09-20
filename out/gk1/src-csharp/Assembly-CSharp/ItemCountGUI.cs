using System;
using UnityEngine;

public class ItemCountGUI : BaseGUI
{
	public delegate float PriceCalculateDelegate(int amount);

	public UILabel header_label;

	private DialogButtonsGUI _dialog_buttons;

	private BaseItemCellGUI _item_gui;

	private SmartSlider _slider;

	private Action<int> _on_confirm;

	public UILabel price;

	public UIWidget window;

	public int height_with_price = 144;

	public int height_without_price = 116;

	private PriceCalculateDelegate _price_calculate_delegate;

	public override void Init()
	{
		_dialog_buttons = GetComponentInChildren<DialogButtonsGUI>(includeInactive: true);
		_dialog_buttons.Init();
		_item_gui = GetComponentInChildren<BaseItemCellGUI>();
		_slider = GetComponentInChildren<SmartSlider>(includeInactive: true);
		_slider.Init();
		base.Init();
	}

	public void Open(string item_id, int min, int max, Action<int> on_confirm, int slider_step_for_keyboard = 1, PriceCalculateDelegate price_calculate_delegate = null)
	{
		ItemDefinition data = GameBalance.me.GetData<ItemDefinition>(item_id);
		_price_calculate_delegate = price_calculate_delegate;
		if (data == null)
		{
			Debug.LogError("Cannot open ItemCountGUI because of null definition", this);
			return;
		}
		base.Open();
		bool flag = price_calculate_delegate != null;
		_item_gui.DrawItem(item_id, 1);
		_item_gui.interaction_enabled = false;
		header_label.text = data.GetItemName();
		window.height = (flag ? height_with_price : height_without_price);
		_slider.Open(flag ? 1 : max, min, max, delegate
		{
			RedrawPrice();
		}, input_field_enabled: true, game_keys_enabled: true);
		_on_confirm = on_confirm;
		_dialog_buttons.Set("ok", OnConfirm, BaseGUI.for_gamepad ? "back" : null, delegate
		{
			OnPressedBack();
		});
		RedrawPrice();
	}

	private void RedrawPrice()
	{
		price.text = ((_price_calculate_delegate == null) ? "" : (GJL.L("price_dialog_total") + "\n" + Trading.FormatMoney(_price_calculate_delegate(_slider.value))));
	}

	private void OnConfirm()
	{
		if (_on_confirm != null)
		{
			_on_confirm(_slider.value);
		}
		Hide();
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}
}
