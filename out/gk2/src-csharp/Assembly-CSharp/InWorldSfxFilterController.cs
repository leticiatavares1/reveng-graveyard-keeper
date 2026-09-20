using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Audio;

public static class InWorldSfxFilterController
{
	private sealed class Driver : MonoBehaviour
	{
		private void LateUpdate()
		{
			SyncToGameplayUiState();
			if (!Mathf.Approximately(currentValue, targetValue))
			{
				float num = Mathf.Abs(80f);
				float num2 = ((num > 0f) ? (num / 0.12f) : 0f);
				currentValue = Mathf.MoveTowards(currentValue, targetValue, num2 * Time.unscaledDeltaTime);
			}
			Apply(currentValue);
		}
	}

	public const string MixerParameter = "inworld_sfx_filter";

	private const float DisabledValue = -80f;

	private const float EnabledValue = 0f;

	private const float TransitionDuration = 0.12f;

	private static float currentValue = -80f;

	private static float targetValue = -80f;

	private static bool isInitialized;

	private static bool loggedMissingParameter;

	private static AudioMixer cachedMixer;

	public static void Init()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			GameObject gameObject = new GameObject("InWorldSfxFilterController");
			if (MainGame.Instance != null)
			{
				gameObject.transform.SetParent(MainGame.Instance.transform);
			}
			gameObject.AddComponent<Driver>();
			LazyWindowsStackController.OnWindowOpened += OnWindowStackChanged;
			LazyWindowsStackController.OnWindowClosed += OnWindowStackChanged;
			MainGame.OnGoToMainMenu = (Action)Delegate.Combine(MainGame.OnGoToMainMenu, new Action(OnGoToMainMenu));
			MainGame.OnGameStarted = (Action)Delegate.Combine(MainGame.OnGameStarted, new Action(OnGameStarted));
			SetEnabledImmediate(enabled: false);
		}
	}

	public static void Shutdown()
	{
		if (isInitialized)
		{
			LazyWindowsStackController.OnWindowOpened -= OnWindowStackChanged;
			LazyWindowsStackController.OnWindowClosed -= OnWindowStackChanged;
			MainGame.OnGoToMainMenu = (Action)Delegate.Remove(MainGame.OnGoToMainMenu, new Action(OnGoToMainMenu));
			MainGame.OnGameStarted = (Action)Delegate.Remove(MainGame.OnGameStarted, new Action(OnGameStarted));
			isInitialized = false;
			cachedMixer = null;
		}
	}

	public static void SetEnabled(bool enabled)
	{
		targetValue = (enabled ? 0f : (-80f));
	}

	public static void SetEnabledImmediate(bool enabled)
	{
		targetValue = (enabled ? 0f : (-80f));
		currentValue = targetValue;
		Apply(currentValue);
	}

	public static void Reapply()
	{
		if (isInitialized)
		{
			Apply(currentValue);
		}
	}

	public static void SyncToGameplayUiState()
	{
		SetEnabled(ShouldEnable());
	}

	private static bool ShouldEnable()
	{
		if (MainGame.Instance != null && MainGame.Instance.gameState == MainGame.GameState.InGame && LazyWindowsStackController.HasAnyModalWindowOpened)
		{
			return !IsFishingMinigameSuppressingDuck();
		}
		return false;
	}

	private static bool IsFishingMinigameSuppressingDuck()
	{
		if (LazyWindowsStackController.ActiveWindow is UIFishingWindow { IsShown: not false } uIFishingWindow)
		{
			return !uIFishingWindow.IsBaitSelectionVisible;
		}
		return false;
	}

	private static void OnWindowStackChanged(LazyWidgetBase _)
	{
		SyncToGameplayUiState();
	}

	private static void OnGoToMainMenu()
	{
		SetEnabledImmediate(enabled: false);
	}

	private static void OnGameStarted()
	{
		SyncToGameplayUiState();
	}

	private static void Apply(float value)
	{
		if (TryGetMixer(out var mixer) && !mixer.SetFloat("inworld_sfx_filter", value) && !loggedMissingParameter)
		{
			loggedMissingParameter = true;
			Debug.LogError("InWorldSfxFilterController: mixer parameter [inworld_sfx_filter] is not exposed");
		}
	}

	private static bool TryGetMixer(out AudioMixer mixer)
	{
		if (cachedMixer == null)
		{
			LazyAudio.TryGetAudioMixer(out cachedMixer);
		}
		mixer = cachedMixer;
		return mixer != null;
	}
}
