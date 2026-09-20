using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIVendorOrdersWindow : LazyWindow<UIVendorOrdersWindowData>
{
	[SerializeField]
	private TextMeshProUGUI helperLabel;

	[SerializeField]
	private TextStyle helperStyleGrey;

	[SerializeField]
	private TextStyle helperStyleRed;

	[SerializeField]
	private UIDialogWindowButton tradersButton;

	[SerializeField]
	private UIVendorOrderWidget prefab;

	private List<UIVendorOrderWidget> drawnWidgets = new List<UIVendorOrderWidget>();

	public override void Init()
	{
		base.Init();
		prefab.gameObject.SetActive(value: false);
	}

	public override void Redraw()
	{
		base.Redraw();
		UIDialogWindowData.ButtonData buttonData = new UIDialogWindowData.ButtonData(OpenSelectionWindow, LLBase.L("ui_traders"), null, replaceForGamepad: true, GameKey.OrderWindowTraders);
		tradersButton.Draw(buttonData);
		foreach (UIVendorOrderWidget drawnWidget in drawnWidgets)
		{
			drawnWidget.gameObject.SetActive(value: false);
		}
		bool flag = MainGame.PlayerData.GetResInt("chalk_board_enabled") > 0;
		helperLabel.text = LLBase.L("ui_order_possible_days", "day_pride".FontIcon());
		if (flag)
		{
			helperStyleGrey.ApplyStyle(helperLabel);
		}
		else
		{
			helperStyleRed.ApplyStyle(helperLabel);
		}
		List<(VendorOrderData, Vendor)> currentOrders = MainGame.Instance.GameSave.vendorSystem.GetCurrentOrders();
		for (int i = 0; i < currentOrders.Count; i++)
		{
			(VendorOrderData, Vendor) tuple = currentOrders[i];
			UIVendorOrderWidget uIVendorOrderWidget = null;
			foreach (UIVendorOrderWidget drawnWidget2 in drawnWidgets)
			{
				if (!drawnWidget2.gameObject.activeSelf)
				{
					uIVendorOrderWidget = drawnWidget2;
					break;
				}
			}
			if (uIVendorOrderWidget == null)
			{
				uIVendorOrderWidget = Object.Instantiate(prefab, prefab.transform.parent);
				drawnWidgets.Add(uIVendorOrderWidget);
			}
			uIVendorOrderWidget.Init();
			uIVendorOrderWidget.gameObject.SetActive(value: true);
			if (tuple.Item1 == null || tuple.Item1.Guid == SGuid.Empty)
			{
				if (flag)
				{
					uIVendorOrderWidget.Draw(new UIVendorOrderWidgetData(null, null, isEmpty: true, (hasForceState: false, isInteractable: false), OpenSelectionWindow, i));
				}
				else
				{
					uIVendorOrderWidget.Draw(new UIVendorOrderWidgetData(null, null, isEmpty: true, (hasForceState: true, isInteractable: false), null, i));
				}
			}
			else if (tuple.Item1.State == VendorOrderState.Finished)
			{
				uIVendorOrderWidget.Draw(new UIVendorOrderWidgetData(currentOrders[i].Item2, currentOrders[i].Item1, isEmpty: false, (hasForceState: false, isInteractable: false), OnGetOrderRewardPress, i));
			}
			else if (flag)
			{
				uIVendorOrderWidget.Draw(new UIVendorOrderWidgetData(currentOrders[i].Item2, currentOrders[i].Item1, isEmpty: false, (hasForceState: false, isInteractable: false), OpenSelectionWindow, i));
			}
			else
			{
				uIVendorOrderWidget.Draw(new UIVendorOrderWidgetData(currentOrders[i].Item2, currentOrders[i].Item1, isEmpty: false, (hasForceState: false, isInteractable: false), null, i));
			}
		}
		helperLabel.gameObject.SetActive(!flag);
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	private void OpenSelectionWindow()
	{
		LazyUI.GetWindow<UIVendorOrdersSelectionWindow>().Open(new UIVendorOrdersSelectionWindowData(null, isAllNotInteractable: true));
	}

	private void OpenSelectionWindow(UIVendorOrderWidget orderWidget)
	{
		if (MainGame.PlayerData.GetResInt("chalk_board_enabled") <= 0)
		{
			return;
		}
		UIVendorOrdersSelectionWindow window = LazyUI.GetWindow<UIVendorOrdersSelectionWindow>();
		window.Open(new UIVendorOrdersSelectionWindowData(delegate
		{
			if (orderWidget != null && window.SelectedWidget != null)
			{
				orderWidget.Draw(new UIVendorOrderWidgetData(window.SelectedWidget.Data.Vendor, window.SelectedWidget.Data.VendorOrderData, isEmpty: false, (hasForceState: false, isInteractable: false), OpenSelectionWindow, orderWidget.Data.IndexInOrders));
				MainGame.Instance.GameSave.vendorSystem.currentOrders[orderWidget.Data.IndexInOrders] = window.SelectedWidget.Data.VendorOrderData.Guid;
			}
		}, isAllNotInteractable: false));
	}

	private void OnGetOrderRewardPress(UIVendorOrderWidget orderWidget)
	{
		if (orderWidget.Data.VendorOrderData.State == VendorOrderState.Finished)
		{
			MainGame.Instance.GameSave.vendorSystem.currentOrders[orderWidget.Data.IndexInOrders] = SGuid.Empty;
			int num = orderWidget.Data.VendorOrderData.Definition.happinessReward.EvaluateInt();
			AchievementsSystem.Instance.Unlock("ach_first_town_order");
			AchievementsSystem.Instance.TriggerCountable("order_done");
			if (orderWidget.Data.VendorOrderData.Definition.isRenewable)
			{
				orderWidget.Data.VendorOrderData.State = VendorOrderState.Default;
			}
			for (int i = 0; i < num; i++)
			{
				FlyingTechPoint flyingTechPoint = FlyingTechPoint.Drop(MainGame.PlayerController.transform.position, TechDef.FlyingReses[3], null, canvas.sortingOrder + 1);
				Debug.Log(flyingTechPoint.transform.position, flyingTechPoint);
				LazyAudio.Play("tech_point_collect");
			}
			if (MainGame.PlayerData.GetResInt("chalk_board_enabled") > 0)
			{
				orderWidget.Draw(new UIVendorOrderWidgetData(null, null, isEmpty: true, (hasForceState: false, isInteractable: false), OpenSelectionWindow, orderWidget.Data.IndexInOrders));
			}
			else
			{
				orderWidget.Draw(new UIVendorOrderWidgetData(null, null, isEmpty: true, (hasForceState: true, isInteractable: false), null, orderWidget.Data.IndexInOrders));
			}
			if (LazyInput.IsGamepadActive)
			{
				base.GamepadNavigationController.SetFocusedItem(orderWidget.BtnGrey.GetComponent<GamepadNavigationItem>());
			}
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		LazyUI.GetWindow<UIVendorOrdersWindow>().Open(new UIVendorOrdersWindowData());
	}
}
