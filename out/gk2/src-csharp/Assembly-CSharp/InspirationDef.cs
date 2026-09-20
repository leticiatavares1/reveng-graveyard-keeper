using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class InspirationDef : BalanceBaseObject
{
	public string idWithoutLvl;

	[AutoParse("level")]
	public int lvl;

	[AutoParse("lvl_frame")]
	public int lvlFrame;

	[AutoParse("talent")]
	public string talentId;

	[AutoParse("completion_price")]
	public int completionPrice;

	[AutoParse("completion_goal_value")]
	public int completionGoalValue;

	[AutoParse("exp_to_talent")]
	public int completionExp;

	[AutoParse("unlock_after_inspiration")]
	public List<string> inpsirationLocks = new List<string>();

	[AutoParse("unlock_after_tech")]
	public List<string> techLocks = new List<string>();

	[SerializeField]
	[AutoParse("icon")]
	private string icon = string.Empty;

	[AutoParse("unlock_after_quest")]
	public List<string> questLocks = new List<string>();

	public Sprite Icon => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(icon, "i_inspiration_anatomy_lvl_01");

	public static InspirationDef GetDataForLevel(string inspirationId, int level)
	{
		return GameBalance.Me.inspirationLevelsCache[inspirationId].GetDataForLevel(level);
	}
}
