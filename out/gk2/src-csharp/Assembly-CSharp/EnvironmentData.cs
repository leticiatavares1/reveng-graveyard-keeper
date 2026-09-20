using System;
using UnityEngine;

[Serializable]
public class EnvironmentData
{
	private const int MINUTES_PER_DAY = 1440;

	private const int SECONDS_PER_MINUTE = 60;

	public const int DAYS_IN_WEEK = 6;

	[Range(1f, 10f)]
	private float gameplayDayInMinutes = 5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float timeOfDay = 0.5f;

	[SerializeField]
	private int day = 1;

	[SerializeField]
	private string overrodeNextDay;

	public string timeOfDayPresetName = "outdoor";

	private static EnvironmentEngine envEngineCache;

	private static WeatherSystem weatherSystemCache;

	private static VendorSystem vendorSystemCache;

	private static TownSystem townSystemCache;

	public int CurrentDayNumber => GetDayNumberFromDay(day);

	public int NextDayNumber
	{
		get
		{
			if (!string.IsNullOrEmpty(overrodeNextDay))
			{
				return ConstDef.Get(overrodeNextDay).IntValue;
			}
			return GetDayNumberFromDay(day + 1);
		}
	}

	public float TimeOfDay => timeOfDay;

	public int Day => day;

	public EnvironmentEngine EnvironmentEngine
	{
		get
		{
			if (envEngineCache != null)
			{
				return envEngineCache;
			}
			envEngineCache = EnvironmentEngine.Instance;
			return envEngineCache;
		}
	}

	public void PrepareForGame(float gameplayDayInMinutes)
	{
		this.gameplayDayInMinutes = gameplayDayInMinutes;
		EnvironmentEngine.Instance.SetTimeOfDay(timeOfDay);
		weatherSystemCache = WeatherSystem.Instance;
		vendorSystemCache = MainGame.Instance.GameSave.vendorSystem;
		townSystemCache = MainGame.Instance.GameSave.townSystem;
	}

	public void SetTimeOfDay(float newTimeOfDay)
	{
		timeOfDay = newTimeOfDay;
	}

	public void HandleTimeOfDayChanged(float gameplayDeltaTime)
	{
		MainGame.PlayerData.energySystem.UpdateSleepLogic(gameplayDeltaTime);
		weatherSystemCache.OnGameTimeChanged(gameplayDeltaTime);
	}

	public int GetDayNumberFromDay(int day)
	{
		int num = day % 6;
		if (num != 0)
		{
			return num;
		}
		return 6;
	}

	public int GetDiffInDaysBetweenCurrentAndNext()
	{
		int currentDayNumber = CurrentDayNumber;
		int nextDayNumber = NextDayNumber;
		if (currentDayNumber >= nextDayNumber)
		{
			return 6 - currentDayNumber + nextDayNumber;
		}
		return nextDayNumber - currentDayNumber;
	}

	public void AddToDay()
	{
		if (!string.IsNullOrEmpty(overrodeNextDay))
		{
			int nextDayNumber = NextDayNumber;
			do
			{
				day++;
			}
			while (CurrentDayNumber != nextDayNumber);
			overrodeNextDay = string.Empty;
		}
		else
		{
			day++;
		}
		vendorSystemCache.UpdateSystemAtTheEndOfDay(day);
		townSystemCache.UpdateSystemAtTheEndOfDay(day);
	}

	public void OverrideNextDayNumber(string dayName)
	{
		overrodeNextDay = dayName;
	}
}
