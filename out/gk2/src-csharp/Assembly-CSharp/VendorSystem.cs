using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class VendorSystem
{
	public const string WAREHOUSE_ZONE_ID = "warehouse";

	public const string WAREHOUSE_CELLAR_ID = "warehouse_cellar";

	public List<SGuid> currentOrders = new List<SGuid>();

	public List<Vendor> vendors = new List<Vendor>();

	public void PrepareForGame()
	{
		if (vendors == null)
		{
			vendors = new List<Vendor>();
		}
		List<VendorDef> list = GameBalance.Me?.vendorDefs;
		if (list == null)
		{
			return;
		}
		KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
		for (int i = 0; i < list.Count; i++)
		{
			VendorDef vendorDef = list[i];
			if (vendorDef != null && !string.IsNullOrEmpty(vendorDef.id) && GetVendor(vendorDef.id) == null)
			{
				vendors.Add(new Vendor(vendorDef.id, 0));
				if (!vendorDef.lockedByDefaultInOrdersWindow)
				{
					knowledgeSystem.UnlockVendorForOrders(vendorDef.id);
				}
				Debug.Log("VendorSystem: add vendor [" + vendorDef.id + "]");
			}
		}
	}

	public Vendor GetVendor(string vendorId)
	{
		return vendors.Find((Vendor v) => v.id == vendorId);
	}

	public List<(VendorOrderData, Vendor)> GetCurrentOrders()
	{
		if (currentOrders.Count == 0)
		{
			for (int i = 0; i < ConstDef.Get("start_orders_count").IntValue; i++)
			{
				currentOrders.Add(SGuid.Empty);
			}
		}
		List<(VendorOrderData, Vendor)> list = new List<(VendorOrderData, Vendor)>();
		for (int j = 0; j < currentOrders.Count; j++)
		{
			SGuid sGuid = currentOrders[j];
			if (sGuid.IsEmpty)
			{
				list.Add((null, null));
				continue;
			}
			for (int k = 0; k < vendors.Count; k++)
			{
				Vendor vendor = vendors[k];
				for (int l = 0; l < vendor.Orders.Count; l++)
				{
					VendorOrderData vendorOrderData = vendor.Orders[l];
					if (sGuid.Guid == vendorOrderData.Guid.Guid)
					{
						list.Add((vendorOrderData, vendor));
					}
				}
			}
		}
		return list;
	}

	public bool IsOrderFinished(string id)
	{
		foreach (Vendor vendor in vendors)
		{
			foreach (VendorOrderData order in vendor.Orders)
			{
				if (!(order.id != id) && (order.IsFinishedOnce || order.State == VendorOrderState.Finished))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void TryResolveOrders()
	{
		WorldZoneData worldZoneDataById = MainGame.WorldData.GetWorldZoneDataById("warehouse");
		WorldZoneData worldZoneDataById2 = MainGame.WorldData.GetWorldZoneDataById("warehouse_cellar");
		List<(VendorOrderData, Vendor)> list = GetCurrentOrders();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			(VendorOrderData, Vendor) tuple = list[num];
			if (tuple.Item1 != null)
			{
				int num2 = worldZoneDataById.CountItemsOnTownPalettes(tuple.Item1.Definition.itemId) + worldZoneDataById2.CountItemsOnTownPalettes(tuple.Item1.Definition.itemId);
				if (tuple.Item1.Definition.isUrgent)
				{
					if (num2 >= tuple.Item1.Definition.count)
					{
						tuple.Item1.State = VendorOrderState.Finished;
					}
				}
				else if (tuple.Item1.Count + num2 >= tuple.Item1.Definition.count)
				{
					tuple.Item1.State = VendorOrderState.Finished;
				}
				else
				{
					tuple.Item1.Count += num2;
				}
			}
		}
	}

	public void AddMoneyToVendor(string vendorId, int money)
	{
		Vendor vendor = GetVendor(vendorId);
		if (vendor == null)
		{
			Debug.LogError("Can't add money to vendor:[" + vendorId + "] no such vendor.");
		}
		else
		{
			vendor.CurMoney += money;
		}
	}

	public void ForceLevelUpVendor(string vendorId)
	{
		Vendor vendor = GetVendor(vendorId);
		if (vendor == null)
		{
			Debug.LogError("Can't level up vendor:[" + vendorId + "] no such vendor.");
		}
		else
		{
			vendor.ForceLevelUp();
		}
	}

	public void UpdateSystemAtTheEndOfDay(int day)
	{
		UpdateVendorsAtTheEndOfDay();
	}

	private void UpdateVendorsAtTheEndOfDay()
	{
		foreach (Vendor vendor in vendors)
		{
			vendor.OnEndOfDay();
		}
		UIVendorWindow window = LazyUI.GetWindow<UIVendorWindow>();
		if (window.IsShown)
		{
			window.RedrawLite();
		}
	}
}
