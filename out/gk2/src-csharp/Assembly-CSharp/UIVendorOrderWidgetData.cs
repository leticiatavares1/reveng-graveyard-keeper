using System;
using LazyBearTechnology;

public class UIVendorOrderWidgetData : LazyWidgetDataBase
{
	public VendorOrderData VendorOrderData { get; private set; }

	public Vendor Vendor { get; private set; }

	public bool IsEmpty { get; private set; }

	public Action<UIVendorOrderWidget> OnPress { get; private set; }

	public (bool hasForceState, bool isInteractable) ForcedInteractableState { get; private set; }

	public int IndexInOrders { get; private set; }

	public bool IsRenewableGreen { get; private set; }

	public UIVendorOrderWidgetData(Vendor vendor, VendorOrderData vendorOrderData, bool isEmpty, (bool hasForceState, bool isInteractable) forcedInteractableState, Action<UIVendorOrderWidget> onPress = null, int indexInOrders = -1, bool isRenewableGreen = true)
	{
		IndexInOrders = indexInOrders;
		VendorOrderData = vendorOrderData;
		Vendor = vendor;
		IsEmpty = isEmpty;
		OnPress = onPress;
		ForcedInteractableState = forcedInteractableState;
		IsRenewableGreen = isRenewableGreen;
	}
}
