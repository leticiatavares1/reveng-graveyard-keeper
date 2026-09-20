using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class GameResSystemDef : BalanceBaseObject
{
	[AutoParse("res_name")]
	public string overrodeResName;

	[AutoParse("start")]
	public LazyExpression start;

	[AutoParse("min")]
	public LazyExpression min;

	[AutoParse("max")]
	public LazyExpression max;

	[AutoParse("expressions_on_increase")]
	public List<LazyExpression> expressionsOnIncrease = new List<LazyExpression>();

	[AutoParse("expressions_on_decrease")]
	public List<LazyExpression> expressionsOnDecrease = new List<LazyExpression>();

	public string ResId
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(overrodeResName))
			{
				return overrodeResName;
			}
			return id;
		}
	}
}
