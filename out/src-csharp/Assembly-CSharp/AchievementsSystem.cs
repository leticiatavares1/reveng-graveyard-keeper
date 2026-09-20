using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

[Serializable]
public class AchievementsSystem
{
	[SerializeField]
	private List<string> _completed_fully = new List<string>();

	[SerializeField]
	private List<string> _completed = new List<string>();

	[SerializeField]
	private List<int> _completed_n = new List<int>();

	public void CheckKeyQuests(string key, int increment = 1)
	{
		foreach (AchievementDefinition achievements_datum in GameBalance.me.achievements_data)
		{
			if (achievements_datum.start_key.Contains(key) && !_completed_fully.Contains(achievements_datum.id) && achievements_datum.IsSucceed())
			{
				if (!_completed.Contains(achievements_datum.id))
				{
					_completed.Add(achievements_datum.id);
					_completed_n.Add(increment);
				}
				else
				{
					_completed_n[_completed.IndexOf(achievements_datum.id)] += increment;
				}
				if (_completed_n[_completed.IndexOf(achievements_datum.id)] >= achievements_datum.counter)
				{
					_completed_fully.Add(achievements_datum.id);
					PlatformSpecific.OnAchievementComplete(achievements_datum);
				}
			}
		}
	}

	public void VerifyAndSetMissedAchievements()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		foreach (AchievementDefinition achievements_datum in GameBalance.me.achievements_data)
		{
			if (_completed_fully.Contains(achievements_datum.id))
			{
				SteamUserStats.GetAchievement(achievements_datum.id, out var pbAchieved);
				Debug.Log($"#MSA# Ach:[{achievements_datum.id}], counter:[{achievements_datum.counter}], achieved:[{pbAchieved}]");
				if (!pbAchieved)
				{
					PlatformSpecific.OnAchievementComplete(achievements_datum);
				}
			}
		}
	}
}
