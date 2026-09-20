using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class PorterStationDef : BalanceBaseObject
{
	[AutoParse("items")]
	public List<NeedItemData> items = new List<NeedItemData>();

	[AutoParse("target_world_zone")]
	public string targetWorldZone;

	[AutoParse("custom_targets")]
	public List<string> customTargets = new List<string>();
}
