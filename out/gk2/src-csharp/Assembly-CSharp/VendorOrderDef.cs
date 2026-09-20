using System;
using LazyBearTechnology;

[Serializable]
public class VendorOrderDef : BalanceBaseObject
{
	[AutoParse("item_id")]
	public string itemId;

	[AutoParse("count")]
	public int count;

	[AutoParse("is_urgent")]
	public bool isUrgent;

	[AutoParse("is_renewable")]
	public bool isRenewable;

	[AutoParse("happiness_reward")]
	public LazyExpression happinessReward;
}
