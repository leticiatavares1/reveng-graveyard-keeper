using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class VendorDef : BalanceBaseObject
{
	[AutoParse("town_vendor")]
	public bool townVendor;

	[AutoParse("start_tier")]
	public int startTier;

	[AutoParse("start_money")]
	public int startMoney;

	[SerializeField]
	[AutoParse("icon")]
	private string icon = string.Empty;

	[AutoParse("locked_by_default_in_orders_window")]
	public bool lockedByDefaultInOrdersWindow;

	public List<VendorTierData> tierDataList;

	public Sprite Icon => GameBalance.Me.GetDataOrNull<WGODef>(icon)?.Portrait;
}
