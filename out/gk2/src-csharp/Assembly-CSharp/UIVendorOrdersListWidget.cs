using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIVendorOrdersListWidget : LazyWidget<UIVendorOrdersListWidgetData>
{
	[SerializeField]
	private Image vendorIcon;

	[SerializeField]
	private RectTransform elementsContent;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	[SerializeField]
	private LazyButton characterIconBtn;

	private List<UIVendorOrderWidget> drawnOrders = new List<UIVendorOrderWidget>();

	private void Awake()
	{
		if (characterIconBtn != null)
		{
			characterIconBtn.onEnter.AddListener(OnOver);
			characterIconBtn.onExit.AddListener(OnOut);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		ReleaseDrawnOrders();
		int i;
		for (i = 0; i < data.Vendor.Orders.Count; i++)
		{
			UIVendorOrderWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIVendorOrderWidget>(elementsContent);
			bool flag = MainGame.Instance.GameSave.vendorSystem.currentOrders.Find((SGuid o) => o.Guid == data.Vendor.Orders[i].Guid.Guid) != null;
			bool flag2 = data.Vendor.Orders[i].State == VendorOrderState.Finished;
			elementFromPool.Draw(new UIVendorOrderWidgetData(data.Vendor, data.Vendor.Orders[i], isEmpty: false, (hasForceState: data.IsAllNotInteractable || flag || flag2, isInteractable: false), data.OnElementPressed, -1, isRenewableGreen: false));
			elementFromPool.Init();
			drawnOrders.Add(elementFromPool);
		}
		vendorIcon.sprite = data.Vendor.Definition.Icon;
		vendorIcon.enabled = vendorIcon.sprite != null;
		vendorIcon.BlueColorReplace(toReplace);
	}

	public override void Hide()
	{
		ReleaseDrawnOrders();
		base.Hide();
	}

	private void ReleaseDrawnOrders()
	{
		foreach (UIVendorOrderWidget drawnOrder in drawnOrders)
		{
			drawnOrder.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(drawnOrder);
		}
		drawnOrders.Clear();
	}

	private void OnOver()
	{
		UITooltip.ShowSimpleInfo(characterIconBtn.transform, LLBase.L(data.Vendor.id));
	}

	private void OnOut()
	{
		UITooltip.Hide();
	}

	private void OnDisable()
	{
		if (characterIconBtn != null && UITooltip.IsTooltipShowingAtTarget(characterIconBtn.transform as RectTransform))
		{
			UITooltip.HideImmediately();
		}
	}

	protected override void TestDraw()
	{
	}
}
