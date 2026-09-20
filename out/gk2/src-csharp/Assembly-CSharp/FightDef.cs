using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class FightDef : BalanceBaseObject
{
	[AutoParse("squads")]
	public int squads;

	[AutoParse("defence_power_lock")]
	public int defencePowerLock;

	[AutoParse("rewards")]
	public List<NeedItemData> rewards = new List<NeedItemData>();

	[AutoParse("on_debug_start")]
	public List<LazyExpression> onDebugStartExpressions = new List<LazyExpression>();

	[AutoParse("on_win_next_fight_id")]
	public string onWinNextFightId;

	[AutoParse("is_barricades_unavailable")]
	public bool isBarricadesUnavailable;

	[AutoParse("is_towers_unavailable")]
	public bool isTowersUnavailable;
}
