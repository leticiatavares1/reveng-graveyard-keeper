using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class AchievementsSystem
{
	[Serializable]
	private class AchievementData : ObjectLinkedToDefinition<AchievementDefinition>
	{
		public int countProgress;

		public bool isCompleted;

		public bool isPlatformProgressCalled;

		public bool isPlatformUnlockCalled;

		public AchievementData(string id)
			: base(id)
		{
		}
	}

	public const string FIRST_TOWN_ORDER_ID = "ach_first_town_order";

	public const string GRAVEYARD_QUALITY_200_ID = "ach_graveyard_quality_200";

	public const string GRAVEYARD_QUALITY_200_ZONE_ID = "graveyard";

	public const int GRAVEYARD_QUALITY_200_THRESHOLD = 200;

	public const string ORDER_DONE_TRIGGER = "order_done";

	public const string SERMON_DONE_TRIGGER = "sermon_done";

	public const string INSPIRATION_UNLOCKED_TRIGGER = "inspiration_unlocked";

	public const string FISH_CAUGHT_TRIGGER = "fish_caught";

	[SerializeField]
	private List<AchievementData> achievementDatas = new List<AchievementData>();

	private Dictionary<string, AchievementData> cacheById;

	private Dictionary<string, List<AchievementData>> cacheByTrigger;

	public static AchievementsSystem Instance => MainGame.Instance.GameSave.achievementsSystem;

	public void PrepareForGame()
	{
		cacheById = new Dictionary<string, AchievementData>();
		foreach (AchievementData achievementData in achievementDatas)
		{
			if (!cacheById.ContainsKey(achievementData.id))
			{
				cacheById.Add(achievementData.id, achievementData);
			}
			else
			{
				Debug.LogError("Duplicated achievement data found for id:[" + achievementData.id + "]");
			}
		}
		cacheByTrigger = new Dictionary<string, List<AchievementData>>();
		foreach (AchievementDefinition achievementDef in GameBalance.Me.achievementDefs)
		{
			if (achievementDef.AchievementType != AchievementType.Countable)
			{
				continue;
			}
			if (string.IsNullOrEmpty(achievementDef.countTrigger))
			{
				Debug.LogError("Countable achievement:[" + achievementDef.id + "] has empty count trigger.");
				continue;
			}
			if (!cacheByTrigger.ContainsKey(achievementDef.countTrigger))
			{
				cacheByTrigger.Add(achievementDef.countTrigger, new List<AchievementData>());
			}
			cacheByTrigger[achievementDef.countTrigger].Add(GetOrCreateAchievementData(achievementDef.id));
		}
		VerifyAndTryUnlockAchievement();
	}

	public void Unlock(string achievementId)
	{
		EnsureCache();
		AchievementData orCreateAchievementData = GetOrCreateAchievementData(achievementId);
		if (orCreateAchievementData.isCompleted)
		{
			return;
		}
		AchievementDefinition definition = orCreateAchievementData.Definition;
		if (definition == null)
		{
			Debug.LogError("No achievement definition found for id:[" + achievementId + "]");
			return;
		}
		if (definition.AchievementType == AchievementType.Countable)
		{
			Debug.LogError("Trying to unlock countable achievement:[" + achievementId + "] with common unlock method.");
			return;
		}
		orCreateAchievementData.isCompleted = true;
		string idForPlatformUnlock = definition.GetIdForPlatformUnlock();
		if (idForPlatformUnlock != "-1")
		{
			TryUnlockAchievementOnPlatform(orCreateAchievementData, idForPlatformUnlock);
		}
	}

	public void TriggerCountable(string trigger, int countProgress = 1)
	{
		EnsureCache();
		if (cacheByTrigger.TryGetValue(trigger, out var value))
		{
			foreach (AchievementData item in value)
			{
				if (!item.isCompleted)
				{
					AchievementDefinition definition = item.Definition;
					if (definition == null)
					{
						Debug.LogError("No achievement definition found for id:[" + item.id + "]");
					}
					else
					{
						item.countProgress = Mathf.Clamp(item.countProgress + countProgress, 0, definition.counter);
						string idForPlatformUnlock = definition.GetIdForPlatformUnlock();
						string platformCounterTrigger = definition.GetPlatformCounterTrigger();
						if (platformCounterTrigger != "-1")
						{
							TrySetAchievementProgressOnPlatform(item, platformCounterTrigger, item.countProgress, definition.counter);
						}
						if (item.countProgress >= definition.counter)
						{
							item.isCompleted = true;
							if (idForPlatformUnlock != "-1")
							{
								TryUnlockAchievementOnPlatform(item, idForPlatformUnlock);
							}
						}
					}
				}
			}
			return;
		}
		Debug.LogError("No achievement data found for trigger:[" + trigger + "]");
	}

	public bool IsCompleted(string achievementId)
	{
		EnsureCache();
		if (!cacheById.TryGetValue(achievementId, out var value))
		{
			return false;
		}
		return value.isCompleted;
	}

	public int GetCountProgress(string achievementId)
	{
		EnsureCache();
		if (!cacheById.TryGetValue(achievementId, out var value))
		{
			return 0;
		}
		return value.countProgress;
	}

	public void VerifyAndTryUnlockAchievement()
	{
	}

	private AchievementData GetOrCreateAchievementData(string id)
	{
		if (!cacheById.TryGetValue(id, out var value))
		{
			value = new AchievementData(id);
			achievementDatas.Add(value);
			cacheById.Add(id, value);
		}
		return value;
	}

	private void EnsureCache()
	{
		if (cacheById == null || cacheByTrigger == null)
		{
			PrepareForGame();
		}
	}

	private void TrySetAchievementProgressOnPlatform(AchievementData achievementData, string statId, int progress, int maxProgress)
	{
	}

	private void TryUnlockAchievementOnPlatform(AchievementData achievementData, string platformId)
	{
	}

	public void VerifyAndSetMissedAchievements()
	{
	}
}
