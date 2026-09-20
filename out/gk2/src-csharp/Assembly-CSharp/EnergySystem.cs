using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class EnergySystem
{
	private const float TIME_MULTIPLIER_WHILE_SLEEP = 50f;

	private const int REMOVE_INSANITY_AFTER_SLEEP = -20;

	private const float ENERGY_PER_DELTA_VALUE = 400f;

	public float timeWithoutSleep;

	private Action onSleepEndedByEnergyCallback;

	private bool sleepWithoutSavingGame;

	private bool sleepWithMaxEnergy;

	private float remainingSleepTime;

	private float prevFrameTime;

	private PerkSystemData PerkSystemData => MainGame.Instance.GameSave.perkSystemData;

	public bool IsSleeping { get; private set; }

	public bool IsInTransitionBetweenSleep { get; private set; }

	public void StartSleeping(Action onSleepEndedByEnergyCallback = null, Action onSleepDidNotStartedCallback = null, bool sleepWithoutSavingGame = false, bool sleepWithMaxEnergy = false, SleepAnimType sleepAnimType = SleepAnimType.None, float sleepDuration = 2f)
	{
		this.onSleepEndedByEnergyCallback = onSleepEndedByEnergyCallback;
		this.sleepWithoutSavingGame = sleepWithoutSavingGame;
		remainingSleepTime = sleepDuration;
		if (PlayerEnergyGameResSystem.GetSystem().HasMax() && !sleepWithMaxEnergy && !PerkSystemData.HasPerk("lack_of_sleep_debuff"))
		{
			Debug.Log("[EnergySystem]: player tried started sleeping, but it is not required");
			onSleepDidNotStartedCallback?.Invoke();
			return;
		}
		if (EnvironmentEngine.Instance.IsPaused)
		{
			Debug.LogWarning("Try sleep with EnvironmentEngine paused. Unpausing");
			EnvironmentEngine.Instance.IsPaused = false;
		}
		MainGame.PlayerController.SetControlTakenType(TakenControlType.BySleep, isEnabled: false);
		MainGame.Instance.dropSystem.CollectAllGameResDropsToPlayer(1f);
		IsInTransitionBetweenSleep = true;
		LazyUI.Get<UISleepFade>().FadeIn(delegate
		{
			IsInTransitionBetweenSleep = false;
			IsSleeping = true;
			timeWithoutSleep = 0f;
			prevFrameTime = Time.time;
			MainGame.UpdateManager.SetTimeSpeedMultiplier(50f);
		}, FadeFlag.Common, blockInterceptions: true, sleepAnimType);
		Debug.Log("[EnergySystem]: player started sleeping");
	}

	public void StopSleeping()
	{
		IsInTransitionBetweenSleep = true;
		IsSleeping = false;
		MainGame.UpdateManager.SetTimeSpeedMultiplier(1f);
		LazyUI.Get<UISleepFade>().FadeOut(delegate
		{
			IsInTransitionBetweenSleep = false;
			MainGame.PlayerController.SetControlTakenType(TakenControlType.BySleep, isEnabled: true);
		});
		Debug.Log("[EnergySystem]: player stopped sleeping");
		if (!sleepWithoutSavingGame)
		{
			SaveSystem.Save(MainGame.Instance.SaveSlotData, MainGame.Instance.GameSave);
		}
		else
		{
			sleepWithoutSavingGame = false;
		}
	}

	public void UpdateSleepLogic(float dayDeltaTime)
	{
		if (!IsInTransitionBetweenSleep)
		{
			if (IsSleeping)
			{
				RestoreEnergyWhileSleeping(dayDeltaTime);
			}
			else
			{
				TrackTimeWithoutSleep(dayDeltaTime);
			}
		}
	}

	public void DeactivateLackOfSleep()
	{
		PerkSystemData.RemovePerk("lack_of_sleep_debuff");
		timeWithoutSleep = 0f;
	}

	private void ActivateLackOfSleep()
	{
		PerkSystemData.AddPerk("lack_of_sleep_debuff");
	}

	private void RestoreEnergyWhileSleeping(float dayDeltaTime)
	{
		if (remainingSleepTime >= 0f)
		{
			remainingSleepTime -= Time.time - prevFrameTime;
			prevFrameTime = Time.time;
		}
		if (!PlayerEnergyGameResSystem.GetSystem().HasMax())
		{
			float value = dayDeltaTime * 400f;
			PlayerEnergyGameResSystem.GetSystem().Add(value);
		}
		else if (PerkSystemData.HasPerk("lack_of_sleep_debuff"))
		{
			DeactivateLackOfSleep();
			PlayerInsanityGameResSystem.GetSystem().Add(-20f);
		}
		else if (remainingSleepTime <= 0f)
		{
			StopSleeping();
			onSleepEndedByEnergyCallback?.Invoke();
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.AfterSleep);
		}
	}

	private void TrackTimeWithoutSleep(float dayDeltaTime)
	{
		timeWithoutSleep += dayDeltaTime;
		if (timeWithoutSleep >= 2f && !PerkSystemData.HasPerk("lack_of_sleep_debuff"))
		{
			ActivateLackOfSleep();
		}
	}
}
