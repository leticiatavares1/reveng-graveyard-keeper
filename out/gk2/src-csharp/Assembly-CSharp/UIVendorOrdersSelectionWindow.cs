using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIVendorOrdersSelectionWindow : LazyWindow<UIVendorOrdersSelectionWindowData>
{
	[SerializeField]
	private ScrollRect vendorsScrollRect;

	[SerializeField]
	private TextMeshProUGUI helperLabel;

	[SerializeField]
	private TextStyle helperStyleGrey;

	[SerializeField]
	private TextStyle helperStyleRed;

	[SerializeField]
	private ScrollRect scrollRect;

	private List<UIVendorOrdersListWidget> drawnListWidgets = new List<UIVendorOrdersListWidget>();

	private UIVendorOrderWidget selectedWidget;

	private List<UICraftsTabSeparatorWidget> displayedTabSeparators = new List<UICraftsTabSeparatorWidget>();

	public UIVendorOrderWidget SelectedWidget => selectedWidget;

	public override void Redraw()
	{
		base.Redraw();
		ReleaseDrawnWidgets();
		bool num = MainGame.PlayerData.GetResInt("chalk_board_enabled") > 0;
		selectedWidget = null;
		helperLabel.text = LLBase.L("ui_order_possible_days", "day_pride".FontIcon());
		if (num)
		{
			helperStyleGrey.ApplyStyle(helperLabel);
		}
		else
		{
			helperStyleRed.ApplyStyle(helperLabel);
		}
		foreach (Vendor item in data.VendorsToDraw)
		{
			UICraftsTabSeparatorWidgetData uICraftsTabSeparatorWidgetData = new UICraftsTabSeparatorWidgetData();
			UICraftsTabSeparatorWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftsTabSeparatorWidget>(vendorsScrollRect.content);
			elementFromPool.Draw(uICraftsTabSeparatorWidgetData);
			displayedTabSeparators.Add(elementFromPool);
			UIVendorOrdersListWidget elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<UIVendorOrdersListWidget>(vendorsScrollRect.content);
			elementFromPool2.Draw(new UIVendorOrdersListWidgetData(item, OnElementPressed, data.IsAllNotInteractable));
			drawnListWidgets.Add(elementFromPool2);
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		scrollRect.verticalNormalizedPosition = 1f;
	}

	private void OnElementPressed(UIVendorOrderWidget orderWidget)
	{
		if (MainGame.PlayerData.GetResInt("chalk_board_enabled") > 0 && orderWidget.Data.VendorOrderData.State == VendorOrderState.Default && orderWidget.Data.Vendor.CurTier >= orderWidget.Data.VendorOrderData.Tier)
		{
			selectedWidget = orderWidget;
			Close();
		}
	}

	public override void Close()
	{
		data.OnClosed?.Invoke();
		base.Close();
	}

	public override void Hide()
	{
		ReleaseDrawnWidgets();
		base.Hide();
	}

	private void ReleaseDrawnWidgets()
	{
		foreach (UIVendorOrdersListWidget drawnListWidget in drawnListWidgets)
		{
			drawnListWidget.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(drawnListWidget);
		}
		drawnListWidgets.Clear();
		foreach (UICraftsTabSeparatorWidget displayedTabSeparator in displayedTabSeparators)
		{
			displayedTabSeparator.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedTabSeparator);
		}
		displayedTabSeparators.Clear();
	}

	protected override void TestDraw()
	{
	}
}
