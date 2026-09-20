using DG.Tweening;
using Rewired;
using UnityEngine;

namespace LazyBearTechnology;

internal class LazyConsolesManagerExample : LazySingleton<LazyConsolesManagerExample>
{
	private const string PS5_ACTIVITY_ID = "continue";

	private static bool showNoControllerWarning;

	private static float showNoControllerWarningDelay;

	private static float frozenTimeScale;

	private static bool unfreezeGame;

	private static bool isFrozen;

	private bool isUserInitializationFinished;

	private bool triggerUserAdded;

	public static bool IsUserInitializationFinished => LazySingleton<LazyConsolesManagerExample>.Instance.isUserInitializationFinished;

	protected override void Awake()
	{
		Init();
	}

	private void Init()
	{
		base.Awake();
		Object.DontDestroyOnLoad(base.gameObject);
		LazyAPI.Platform.Init();
		isUserInitializationFinished = true;
		OnUserAdded();
		LazyAPI.Platform.AddUser();
		LazyAPI.Platform.OnAllControllersDisabled += ShowNoControllersWarning;
	}

	private void OnUserAdded()
	{
		if (!triggerUserAdded)
		{
			triggerUserAdded = true;
		}
	}

	private void Update()
	{
		if (showNoControllerWarning)
		{
			showNoControllerWarningDelay -= LazyTime.GetUnscaledDeltaTime;
			if (showNoControllerWarningDelay <= 0f)
			{
				showNoControllerWarning = false;
				if (ReInput.controllers.joystickCount == 0)
				{
					ShowNoControllersWarning();
				}
			}
		}
		LazyAPI.Platform.Update();
		if (unfreezeGame)
		{
			unfreezeGame = false;
			UnfreezeGame();
		}
	}

	private bool IsGameFrozen()
	{
		return isFrozen;
	}

	private void FreezeGame()
	{
		isFrozen = true;
		LazyTime.OverrideUnscaledDeltaTime(0f);
		DOTween.PauseAll();
		frozenTimeScale = Time.timeScale;
		Time.timeScale = 0f;
	}

	private void UnfreezeGame()
	{
		isFrozen = false;
		LazyTime.CancelOverrideUnscaledDeltaTime();
		DOTween.PlayAll();
		Time.timeScale = frozenTimeScale;
	}

	public void ShowNoControllersWarning()
	{
	}
}
