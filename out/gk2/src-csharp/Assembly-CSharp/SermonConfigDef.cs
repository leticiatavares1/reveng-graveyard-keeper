using System;
using LazyBearTechnology;

[Serializable]
public class SermonConfigDef : BalanceBaseObject
{
	[AutoParse("cur_zone_id")]
	public string curWorldZoneId;

	[AutoParse("attached_zone_id")]
	public string attachedWorldZoneId;

	[AutoParse("reward_box_id")]
	public string rewardBoxId;
}
