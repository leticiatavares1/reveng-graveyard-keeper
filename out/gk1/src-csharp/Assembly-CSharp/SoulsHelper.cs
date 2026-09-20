public static class SoulsHelper
{
	public const string SOULS_SINS_COUNT_RES = "sins_count";

	public const float BASE_SOUL_REWARD = 5f;

	public const float BASE_SIN_REWARD = 5f;

	public static float CalculatePointsAfterSoulRelease(Item healed_soul)
	{
		float num = healed_soul.durability;
		float param = healed_soul.GetParam("sins_count");
		if (num > 0.9f)
		{
			num = 1f;
		}
		return num * 5f + param * 5f;
	}
}
