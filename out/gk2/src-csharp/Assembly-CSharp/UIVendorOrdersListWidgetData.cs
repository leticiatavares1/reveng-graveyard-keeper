using System;
using LazyBearTechnology;

public class UIVendorOrdersListWidgetData : LazyWidgetDataBase
{
	public Vendor Vendor { get; private set; }

	public Action<UIVendorOrderWidget> OnElementPressed { get; private set; }

	public bool IsAllNotInteractable { get; private set; }

	public UIVendorOrdersListWidgetData(Vendor vendor, Action<UIVendorOrderWidget> onElementPressed, bool isAllNotInteractable)
	{
		Vendor = vendor;
		OnElementPressed = onElementPressed;
		IsAllNotInteractable = isAllNotInteractable;
	}
}
