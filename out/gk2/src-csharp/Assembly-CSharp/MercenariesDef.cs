using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class MercenariesDef : BalanceBaseObject
{
	[AutoParse("money")]
	public int money;

	[AutoParse("after_pay_expr")]
	public List<LazyExpression> afterPayExpr;

	[AutoParse("after_win_expr")]
	public List<LazyExpression> afterWinExpr;

	[AutoParse("items")]
	public List<NeedItemData> needItems = new List<NeedItemData>();
}
