using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.ResourceManagement.AsyncOperations;

[ExecuteInEditMode]
public class EnvironmentEngine : MonoBehaviour, ICustomUpdatable
{
	private class OverrideRequest
	{
		public object requester;

		public string soundId;

		public bool crossfade;
	}

	private const int MINUTES_PER_DAY = 1440;

	private const int SECONDS_PER_MINUTE = 60;

	private const string TIME_OF_DAY_PRESETS_LABEL = "_TimeOfDayPresets";

	private static EnvironmentEngine cachedInstance;

	[SerializeField]
	private TimeOfDayPresets timesOfDay;

	[SerializeField]
	[Range(1f, 10f)]
	public float gameplayDayInMinutes = 5f;

	[Range(0f, 1f)]
	public float timeOfDay;

	[SerializeField]
	private EnvironmentData data = new EnvironmentData();

	[SerializeField]
	private List<LightEnvironmentPreset> presets = new List<LightEnvironmentPreset>();

	[SerializeField]
	private LightEnvironmentPreset presetOverride;

	[Range(0f, 1f)]
	public float presetOverrideIntensity = 1f;

	[SerializeField]
	private bool isPaused;

	[SerializeField]
	[HideInInspector]
	private LightsSystem lightsSystem;

	[SerializeField]
	[HideInInspector]
	private GlobalShaderParameters globalShaderParameters;

	private LightEnvironmentPreset presetTime1;

	private LightEnvironmentPreset presetTime2;

	[Range(0f, 1f)]
	private float presetTimeLerp;

	private float additiveBloomThreshold;

	private LightEnvironmentPreset presetTimeLerped;

	private LightEnvironmentPreset presetOverrideLerped;

	private Dictionary<string, TimeOfDayPresets> availablePresets = new Dictionary<string, TimeOfDayPresets>();

	private AsyncLazy presetsPreloadTask;

	[SerializeField]
	private SoundEnvironmentConfig additionalSoundConfig;

	private readonly AmbientSoundMixer ambientMixer = new AmbientSoundMixer();

	private string soundId1;

	private string soundId2;

	[Range(0f, 1f)]
	private float soundTimeLerp;

	[Range(0f, 10f)]
	private float overrodeAmbientSoundSwitchTime = 5f;

	private string currentOverrodeAmbientSoundId = string.Empty;

	private readonly List<OverrideRequest> overrideRequests = new List<OverrideRequest>();

	private string currentAdditionalSoundId;

	private SoundHandler additionalSoundHandler;

	public float timeOfDayTest;

	public static EnvironmentEngine Instance
	{
		get
		{
			if (cachedInstance == null)
			{
				cachedInstance = UnityEngine.Object.FindObjectOfType<EnvironmentEngine>();
				if (cachedInstance == null)
				{
					Debug.LogError($"Cannot find instance of {typeof(EnvironmentEngine)} on current scene.");
				}
			}
			return cachedInstance;
		}
		set
		{
			cachedInstance = value;
		}
	}

	public string CurrentOverrodeAmbientSoundId => currentOverrodeAmbientSoundId;

	public EnvironmentData Data
	{
		get
		{
			data = ((MainGame.Instance != null) ? MainGame.Instance.GameSave.environmentData : data);
			return data;
		}
	}

	public TimeOfDayPresets TimesOfDay => timesOfDay;

	public bool IsPaused
	{
		get
		{
			return isPaused;
		}
		set
		{
			if (isPaused != value)
			{
				Debug.Log($"Set EnvEng IsPaused to [{value}] ");
				isPaused = value;
			}
		}
	}

	public static event Action<float, bool> OnTimeOfDayChangedEvent;

	public static event Action<int> OnNewDayStarted;

	public static event Action<int> OnNewDayStartedWithDayNumber;

	public void PushOverrodeAmbientSound(object requester, string soundId, bool crossfade = true)
	{
		OverrideRequest overrideRequest = FindOverrideRequest(requester);
		if (overrideRequest != null)
		{
			overrideRequest.soundId = soundId ?? string.Empty;
			overrideRequest.crossfade = crossfade;
		}
		else
		{
			overrideRequests.Add(new OverrideRequest
			{
				requester = requester,
				soundId = (soundId ?? string.Empty),
				crossfade = crossfade
			});
		}
		ApplyTopOverrodeAmbientSound(crossfade);
	}

	public void ClearOverrodeAmbientSound(object requester, bool crossfade = true)
	{
		for (int num = overrideRequests.Count - 1; num >= 0; num--)
		{
			if (overrideRequests[num].requester == requester)
			{
				overrideRequests.RemoveAt(num);
				break;
			}
		}
		ApplyTopOverrodeAmbientSound(crossfade);
	}

	private OverrideRequest FindOverrideRequest(object requester)
	{
		for (int i = 0; i < overrideRequests.Count; i++)
		{
			if (overrideRequests[i].requester == requester)
			{
				return overrideRequests[i];
			}
		}
		return null;
	}

	private void ApplyTopOverrodeAmbientSound(bool fallbackCrossfade)
	{
		string text = string.Empty;
		bool crossfade = fallbackCrossfade;
		for (int num = overrideRequests.Count - 1; num >= 0; num--)
		{
			if (!string.IsNullOrEmpty(overrideRequests[num].soundId))
			{
				text = overrideRequests[num].soundId;
				crossfade = overrideRequests[num].crossfade;
				break;
			}
		}
		if (!(text == currentOverrodeAmbientSoundId))
		{
			currentOverrodeAmbientSoundId = text;
			ambientMixer.SwitchDuration = overrodeAmbientSoundSwitchTime;
			ambientMixer.SetOverride(string.IsNullOrEmpty(currentOverrodeAmbientSoundId) ? null : currentOverrodeAmbientSoundId, crossfade);
		}
	}

	public void ApplyOverridePreset(LightEnvironmentPreset preset, float intensity = 1f)
	{
		presetOverride = preset;
		presetOverrideIntensity = intensity;
		RecalcLightPresetLerp();
	}

	public void ApplyOverridePreset(string presetName, float intensity = 1f)
	{
		if (string.IsNullOrEmpty(presetName))
		{
			ApplyOverridePreset((LightEnvironmentPreset)null, 0f);
			return;
		}
		LightEnvironmentPreset lightEnvironmentPreset = FindOverridePreset(presetName);
		if (lightEnvironmentPreset == null)
		{
			Debug.LogError("Light environment preset [" + presetName + "] wasn't found");
		}
		else
		{
			ApplyOverridePreset(lightEnvironmentPreset, intensity);
		}
	}

	public void RefreshAppliedLightPreset()
	{
		RecalcLightPresetLerp();
	}

	private LightEnvironmentPreset FindOverridePreset(string presetName)
	{
		if (presets == null)
		{
			return null;
		}
		for (int i = 0; i < presets.Count; i++)
		{
			LightEnvironmentPreset lightEnvironmentPreset = presets[i];
			if (lightEnvironmentPreset != null && lightEnvironmentPreset.name == presetName)
			{
				return lightEnvironmentPreset;
			}
		}
		return null;
	}

	public void SetTimeOfDay(float timeOfDay)
	{
		this.timeOfDay = timeOfDay;
		data.SetTimeOfDay(timeOfDay);
		OnTimeOfDayChanged(timeOfDay);
		EnvironmentEngine.OnTimeOfDayChangedEvent?.Invoke(timeOfDay, arg2: false);
	}

	public void SetTimeOfDayFake(float timeOfDay)
	{
		IsPaused = true;
		this.timeOfDay = timeOfDay;
		OnTimeOfDayChanged(timeOfDay);
		EnvironmentEngine.OnTimeOfDayChangedEvent?.Invoke(timeOfDay, arg2: true);
	}

	public void ResumeTimeOfDayFromData()
	{
		IsPaused = false;
		timeOfDay = data.TimeOfDay;
		OnTimeOfDayChanged(timeOfDay);
		EnvironmentEngine.OnTimeOfDayChangedEvent?.Invoke(timeOfDay, arg2: false);
	}

	public TimeOfDayType GetTimeOfDayType()
	{
		if (timeOfDay >= 0.25f && timeOfDay < 0.75f)
		{
			return TimeOfDayType.Day;
		}
		return TimeOfDayType.Night;
	}

	public void SetTimeOfDay_DEV(float timeOfDay)
	{
		SetTimeOfDay(timeOfDay);
		if (!Application.isPlaying)
		{
			OnTimeOfDayChanged(timeOfDay);
		}
	}

	private void RecalcLightPresetLerp()
	{
		LightEnvironmentPreset.Lerp(presetTimeLerped, presetTime1, presetTime2, presetTimeLerp);
		if (presetOverride == null)
		{
			ApplyPreset(presetTimeLerped);
			return;
		}
		LightEnvironmentPreset.Lerp(presetOverrideLerped, presetTimeLerped, presetOverride, presetOverrideIntensity);
		ApplyPreset(presetOverrideLerped);
	}

	private void RecalcSoundPresetLerp(bool hardCut = false)
	{
		ambientMixer.SetAmbientPair(soundId1, soundId2, soundTimeLerp, !hardCut);
	}

	private void OnTimeOfDayChanged(float timeOfDay, bool hardCutSound = false)
	{
		if (timesOfDay == null)
		{
			return;
		}
		presetTime1 = (presetTime2 = null);
		for (int i = 0; i < timesOfDay.presets.Count; i++)
		{
			TimeOfDayPresets.TimeAndPreset timeAndPreset = timesOfDay.presets[i];
			if (timeAndPreset.time == timeOfDay)
			{
				presetTime1 = (presetTime2 = timeAndPreset.preset);
				presetTimeLerp = 0f;
				break;
			}
			if (timeOfDay > timeAndPreset.time)
			{
				presetTime1 = timeAndPreset.preset;
				if (i + 1 >= timesOfDay.presets.Count)
				{
					Debug.LogError("Error picking a time of day preset. Probably, last preset time is less then 1.0");
					presetTime2 = presetTime1;
					presetTimeLerp = 0f;
					break;
				}
				TimeOfDayPresets.TimeAndPreset timeAndPreset2 = timesOfDay.presets[i + 1];
				if (timeOfDay <= timeAndPreset2.time)
				{
					presetTime2 = timeAndPreset2.preset;
					presetTimeLerp = (timeOfDay - timeAndPreset.time) / (timeAndPreset2.time - timeAndPreset.time);
					break;
				}
			}
		}
		RecalcLightPresetLerp();
		if (timesOfDay.soundEnvironmentConfig != null)
		{
			float crossfadeDuration = timesOfDay.soundEnvironmentConfig.GetCrossfadeDuration01(gameplayDayInMinutes * 60f);
			SoundEnvironmentConfig.FindPair(timesOfDay.soundEnvironmentConfig.sounds, timeOfDay, crossfadeDuration, out soundId1, out soundId2, out soundTimeLerp);
		}
		else
		{
			soundId1 = (soundId2 = null);
			soundTimeLerp = 0f;
		}
		RecalcSoundPresetLerp(hardCutSound);
		UpdateAdditionalSound(timeOfDay);
	}

	private void UpdateAdditionalSound(float timeOfDay)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (additionalSoundConfig == null || additionalSoundConfig.sounds == null || additionalSoundConfig.sounds.Count == 0)
		{
			StopAdditionalSound();
			return;
		}
		string text = SoundEnvironmentConfig.FindActiveSegment(additionalSoundConfig.sounds, timeOfDay);
		if (text == currentAdditionalSoundId)
		{
			return;
		}
		StopAdditionalSound();
		currentAdditionalSoundId = text;
		if (!string.IsNullOrEmpty(text))
		{
			additionalSoundHandler = LazyAudio.Play(text, checkDelay: false);
			if (additionalSoundHandler == null)
			{
				Debug.LogWarning("EnvironmentEngine: failed to play additional sound [" + text + "]");
			}
		}
	}

	private void StopAdditionalSound()
	{
		additionalSoundHandler?.Stop();
		additionalSoundHandler = null;
		currentAdditionalSoundId = null;
	}

	public UniTask PreloadTimeOfDayPresets()
	{
		presetsPreloadTask = UniTask.Lazy(LoadTimeOfDayPresetsAsync);
		return presetsPreloadTask.Task;
	}

	private async UniTask LoadTimeOfDayPresetsAsync()
	{
		AsyncOperationHandle<IList<TimeOfDayPresets>> handle = Addressables.LoadAssetsAsync<TimeOfDayPresets>("_TimeOfDayPresets");
		await handle.ToUniTask();
		if (handle.Status == AsyncOperationStatus.Succeeded)
		{
			foreach (TimeOfDayPresets item in handle.Result)
			{
				availablePresets[item.name] = item;
			}
			Debug.Log($"Loaded {handle.Result.Count} presets.");
		}
		else
		{
			Debug.LogError("Failed to load TimeOfDayPresets from Addressables.");
		}
	}

	public void SetTimeOfDayPreset(string presetName)
	{
		SetTimeOfDayPresetAsync(presetName).Forget();
	}

	private async UniTaskVoid SetTimeOfDayPresetAsync(string presetName)
	{
		if (presetsPreloadTask != null)
		{
			await presetsPreloadTask.Task;
		}
		if (this == null)
		{
			return;
		}
		if (timesOfDay != null && timesOfDay.name == presetName)
		{
			Debug.Log("Preset [" + presetName + "] was already loaded");
			SyncIndoorAudioSnapshot();
			return;
		}
		if (!availablePresets.TryGetValue(presetName, out var value))
		{
			Debug.LogError("Preset [" + presetName + "] wasn't found");
			return;
		}
		Debug.Log("SetTimeOfDayPreset: [" + presetName + "]");
		timesOfDay = value;
		data.timeOfDayPresetName = presetName;
		RecalcLightPresetLerp();
		OnTimeOfDayChanged(data.TimeOfDay, hardCutSound: true);
		if (WeatherSystem.Instance != null)
		{
			WeatherSystem.Instance.SetPauseState(WeatherSystemPauseFlag.TimeOfDay, timesOfDay.indoorPreset);
			WeatherSystem.Instance.ApplySoundParameters(timesOfDay.indoorPreset, timesOfDay.applySfxFromOutdoor);
		}
		else
		{
			Debug.LogWarning("SetTimeOfDayPresetAsync: WeatherSystem.Instance is missing, skipping weather sync.");
		}
		SyncIndoorAudioSnapshot();
	}

	private void SyncIndoorAudioSnapshot()
	{
		if (!(timesOfDay == null) && !(WeatherSystem.Instance == null))
		{
			WeatherSystem.Instance.AudioMixerStateController.SetLayerActive(AudioMixerSnapshotLayer.Indoor, timesOfDay.indoorPreset);
		}
	}

	public void CustomUpdate(float deltaTime)
	{
		if (!Instance.isPaused)
		{
			float num = ConvertDeltaTimeToGameplayTime01(deltaTime);
			float num2 = timeOfDay + num;
			SetTimeOfDay(MathUtilities.ClampCycleWithinRange(num2, 0f, 1f));
			Data.HandleTimeOfDayChanged(num);
			if (num2 >= 1f)
			{
				Data.AddToDay();
				EnvironmentEngine.OnNewDayStarted?.Invoke(Data.Day);
				EnvironmentEngine.OnNewDayStartedWithDayNumber?.Invoke(Data.CurrentDayNumber);
			}
		}
	}

	public float ConvertDeltaTimeToGameplayTime01(float deltaTime)
	{
		return deltaTime / (gameplayDayInMinutes * 60f);
	}

	private void Awake()
	{
		Debug.Log("EnvironmentEngine.Awake()");
		presetTimeLerped = ScriptableObject.CreateInstance<LightEnvironmentPreset>();
		presetOverrideLerped = ScriptableObject.CreateInstance<LightEnvironmentPreset>();
		CameraSystem.Instance.MainCamera.AddAdditionalBloomThresholdGetter(() => additiveBloomThreshold);
		ambientMixer.SwitchDuration = overrodeAmbientSoundSwitchTime;
		TryInitDependencies();
		ApplyPreset(presetTimeLerped);
		OnTimeOfDayChanged(data.TimeOfDay);
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			ambientMixer.SwitchDuration = overrodeAmbientSoundSwitchTime;
			ambientMixer.Update(Time.deltaTime);
		}
	}

	private void OnDestroy()
	{
		ambientMixer.StopAll();
		StopAdditionalSound();
	}

	private static void ApplyPreset(LightEnvironmentPreset preset)
	{
		if (!(preset == null))
		{
			Light sunLight = Instance.lightsSystem.SunLight;
			sunLight.color = preset.sunLightColor;
			sunLight.intensity = preset.sunLightIntensity;
			sunLight.shadowStrength = preset.sunLightShadowStrength;
			sunLight.transform.rotation = Quaternion.Euler(preset.sunLightRotation);
			Light backLight = Instance.lightsSystem.BackLight;
			backLight.color = preset.backLightColor;
			backLight.intensity = preset.backLightIntensity;
			Instance.globalShaderParameters.backlightColor = preset.gspBacklightColor;
			Instance.globalShaderParameters.lightMaxBurn = preset.gspMaxLightBurn;
			Instance.globalShaderParameters.sunLight = preset.sunLightAmount;
			LazySingleton<GlobalShaderParameters>.Instance.ApplyShaderParameters();
			RenderSettings.ambientLight = preset.ambientLightColor;
			ColorGrading setting = CameraSystem.Instance.MainCamera.PostProcessVolume.profile.GetSetting<ColorGrading>();
			if (setting != null)
			{
				setting.ldrLut.value = preset.LutTexture;
			}
			Instance.additiveBloomThreshold = preset.additiveBloomThreshold;
			CameraSystem.Instance.MainCamera.UpdateBloomThreshold();
		}
	}

	private void TryInitDependencies()
	{
		GameObject gameObject = base.transform.parent.gameObject;
		if (lightsSystem == null)
		{
			lightsSystem = gameObject.GetComponentInChildren<LightsSystem>();
		}
		if (globalShaderParameters == null)
		{
			globalShaderParameters = gameObject.GetComponentInChildren<GlobalShaderParameters>();
		}
	}

	private void SetTimeOfDayTest(float timeOfDay)
	{
		SetTimeOfDay(timeOfDay);
	}

	private void SetTimeOfDayFakeTest(float timeOfDay)
	{
		SetTimeOfDayFake(timeOfDay);
	}

	private void ResumeTimeOfDatFromDataTest(float timeOfDay)
	{
		ResumeTimeOfDayFromData();
	}
}
