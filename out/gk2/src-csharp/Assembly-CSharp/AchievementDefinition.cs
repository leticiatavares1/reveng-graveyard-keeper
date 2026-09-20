using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class AchievementDefinition : BalanceBaseObject
{
	[AutoParse("count_trigger")]
	public string countTrigger = string.Empty;

	[AutoParse("counter")]
	public int counter = 1;

	[AutoParse("xbox_id")]
	public int xboxId = -1;

	[AutoParse("playstation4_id")]
	public int playStation4Id = -1;

	[AutoParse("playstation5_id")]
	public int playStation5Id = -1;

	[SerializeField]
	private AchievementType achievementType;

	public AchievementType AchievementType => achievementType;

	public string GetIdForPlatformUnlock()
	{
		return id;
	}

	public string GetPlatformCounterTrigger()
	{
		return countTrigger;
	}

	public bool IsAvailableOnCurrentPlatform()
	{
		return true;
	}
}
