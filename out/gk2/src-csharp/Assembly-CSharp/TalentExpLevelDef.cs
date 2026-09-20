using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class TalentExpLevelDef : BalanceBaseObject
{
	public const string YELLOW_TALENT_ID = "talent_yellow";

	public const string GREEN_TALENT_ID = "talent_green";

	public const string RED_TALENT_ID = "talent_red";

	public const string ORANGE_TALENT_ID = "talent_orange";

	public const string BLUE_TALENT_ID = "talent_blue";

	[AutoParse("talent_yellow")]
	public int yellow;

	[AutoParse("talent_green")]
	public int green;

	[AutoParse("talent_red")]
	public int red;

	[AutoParse("talent_orange")]
	public int orange;

	[AutoParse("talent_blue")]
	public int blue;

	public static TalentExpLevelBalanceData GetTalentLevelData(string talentId)
	{
		return GameBalance.Me.talentExpLevelsCache.GetValueOrDefault(talentId);
	}
}
