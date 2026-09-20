public static class Flow_TownBuildingConsts
{
	public const string TOWN_BUILDING_TIME_AVAILABILITY_KEY = "available_by_time";

	public const string PLAYER_TP_DURING_FADE_POSTFIX = "_player_tp_during_fade";

	public static string GetPlayerTpDuringFadeGdPointId(string gdPointTent)
	{
		if (string.IsNullOrEmpty(gdPointTent))
		{
			return null;
		}
		return gdPointTent + "_player_tp_during_fade";
	}
}
