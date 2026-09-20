using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIVendorOrdersSelectionWindowData : LazyWidgetDataBase
{
	public List<Vendor> VendorsToDraw { get; private set; }

	public Action OnClosed { get; private set; }

	public bool IsAllNotInteractable { get; private set; }

	public UIVendorOrdersSelectionWindowData(Action onClosed, bool isAllNotInteractable)
	{
		VendorsToDraw = new List<Vendor>();
		KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
		foreach (Vendor vendor in MainGame.Instance.GameSave.vendorSystem.vendors)
		{
			if (vendor.Definition.townVendor && knowledgeSystem.IsVendorForOrdersUnlocked(vendor.id) && HasOrdersInTierData(vendor))
			{
				VendorsToDraw.Add(vendor);
			}
		}
		IsAllNotInteractable = isAllNotInteractable;
		OnClosed = onClosed;
	}

	private bool HasOrdersInTierData(Vendor vendor)
	{
		for (int i = 0; i < vendor.Definition.tierDataList.Count; i++)
		{
			if (vendor.Definition.tierDataList[i].orders.Count > 0)
			{
				return true;
			}
		}
		return false;
	}
}
