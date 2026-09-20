using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class ToolTypeDef : BalanceBaseObject
{
	[AutoParse("talent_ids")]
	public List<string> talentIds;
}
