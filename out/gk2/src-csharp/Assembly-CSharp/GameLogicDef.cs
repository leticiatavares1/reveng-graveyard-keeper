using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class GameLogicDef : BalanceBaseObject
{
	public GameLogicStartType gameLogicStartType;

	[AutoParse("start_time")]
	public float startTime;

	[AutoParse("period_time")]
	public float periodTime;

	[AutoParse("day_number")]
	public string dayNumber;

	[AutoParse("day_time")]
	public float dayTime;

	[AutoParse("condition")]
	public LazyExpression condition;

	[AutoParse("exec_expressions")]
	public List<LazyExpression> execExpressions = new List<LazyExpression>();

	[AutoParse("exec_fs")]
	public string execFlowscriptName;
}
