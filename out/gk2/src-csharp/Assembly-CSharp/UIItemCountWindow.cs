using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemCountWindow : LazyWindow<UIItemCountWindowData>
{
	[SerializeField]
	private TextMeshProUGUI headerLabel;

	[SerializeField]
	private UIDialogWindowButton okBtn;

	[SerializeField]
	private UIDialogWindowButton backBtn;

	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private SmartSlider slider;

	[SerializeField]
	private TextStyle moneyStyle;

	[SerializeField]
	private float priceWindowSize;

	[SerializeField]
	private float noPriceWindowSize;

	private Action<int> onConfirm;

	public TextMeshProUGUI price;

	public RectTransform rectTransform;

	private UIItemCountWindowData.PriceCalculateDelegate priceCalculateDelegate;

	public override void Init()
	{
		slider.Init();
		base.Init();
	}

	public override void Open(UIItemCountWindowData data)
	{
		base.Open(data);
		priceCalculateDelegate = data.PriceCalculateDel;
		itemCell.Draw(data.Item);
		itemCell.ClearCallbacks();
		headerLabel.text = data.Item.Definition.GetHeader();
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, data.IsForVendor ? priceWindowSize : noPriceWindowSize);
		slider.Open(data.IsForVendor ? 1 : data.Max, data.Min, data.Max, delegate
		{
			RedrawPrice();
		}, inputFieldEnabled: true, gameKeysEnabled: true, 1, data.SnapStep);
		onConfirm = data.OnConfirm;
		data.OkBtnData.onPressed = OnConfirm;
		okBtn.Draw(data.OkBtnData);
		if (data.BackBtnData != null)
		{
			data.BackBtnData.onPressed = Close;
			backBtn.Draw(data.BackBtnData);
		}
		else
		{
			backBtn.gameObject.SetActive(value: false);
		}
		RedrawPrice();
		UpdateGamepadDependentStuff();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void RedrawPrice()
	{
		price.text = ((!data.IsForVendor) ? "" : (LLBase.L("price_dialog_total") + "\n " + moneyStyle.ApplyStyleToString(Trading.FormatMoney(priceCalculateDelegate(slider.Value), printZero: false, " ", GameResIconType.MoneyBig))));
	}

	private void OnConfirm()
	{
		onConfirm?.Invoke(slider.Value);
		LazyUI.GetWindow<UIItemCountWindow>().Close();
	}

	private bool OnConfirmKeyPressed()
	{
		if (okBtn.LazyButton.interactable)
		{
			okBtn.LazyButton?.ForceOnClick();
			return true;
		}
		return false;
	}

	protected override bool OnPressedBack()
	{
		if (data.BackBtnData == null)
		{
			Close();
			return true;
		}
		backBtn.LazyButton.ForceOnClick();
		return true;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		gameKeyDelegates.Add(GameKey.Left, () => false);
		gameKeyDelegates.Add(GameKey.Right, () => false);
		gameKeyDelegates.Add(GameKey.DpadLeft, () => false);
		gameKeyDelegates.Add(GameKey.DpadRight, () => false);
		gameKeyDelegates.Add(GameKey.IncSlider, () => false);
		gameKeyDelegates.Add(GameKey.DecSlider, () => false);
		gameKeyDelegates.Add(GameKey.ItemCountWindow_Increase, () => false);
		gameKeyDelegates.Add(GameKey.ItemCountWindow_Decrease, () => false);
		gameKeyDelegates.Add(GameKey.ItemCountWindow_Apply, OnConfirmKeyPressed);
		gameKeyDelegates.Add(GameKey.ItemCountWindow_Cancel, OnPressedBack);
		return gameKeyDelegates;
	}

	protected override void PrintTips()
	{
		if (data != null)
		{
			List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
			list.Add(new LazyGameKeyTip(GameKey.ItemCountWindow_Increase, "+", active: true, gamepadOnly: true, translate: false));
			list.Add(new LazyGameKeyTip(GameKey.ItemCountWindow_Decrease, "-", active: true, gamepadOnly: true, translate: false));
			lazyButtonTips.Print(list);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		UIItemCountWindowData uIItemCountWindowData = new UIItemCountWindowData();
		uIItemCountWindowData.Item = new Item("apple", 25);
		uIItemCountWindowData.Min = 1;
		uIItemCountWindowData.Max = uIItemCountWindowData.Item.Count;
		uIItemCountWindowData.OnConfirm = delegate
		{
		};
		uIItemCountWindowData.PriceCalculateDel = (int _) => 11111;
		uIItemCountWindowData.IsForVendor = true;
		uIItemCountWindowData.OkBtnData = new UIDialogWindowData.ButtonData(null, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
		uIItemCountWindowData.BackBtnData = new UIDialogWindowData.ButtonData(null, LLBase.L("btn_cancel"), null, replaceForGamepad: true, GameKey.Back);
		Open(uIItemCountWindowData);
	}
}
