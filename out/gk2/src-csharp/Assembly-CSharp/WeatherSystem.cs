using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using NodeCanvas.StateMachines;
using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-5)]
public class WeatherSystem : MonoBehaviour
{
	public static readonly int idWindAmp = Shader.PropertyToID("_WindAmplitude");

	public static readonly int idWindConstant = Shader.PropertyToID("_WindConstant");

	public static readonly int idWindGustAmp = Shader.PropertyToID("_WindGustAmp");

	public const string CLEAN_WEATHER_NAME = "CleanWeather";

	public LightsSystem lightsSystem;

	private static WeatherSystem instance;

	[NonSerialized]
	public Dictionary<string, WeatherComponent> components = new Dictionary<string, WeatherComponent>();

	public float fadeTime = 4f;

	public float phaseLength = 0.25f;

	[Space]
	[SerializeField]
	[Range(0f, 1f)]
	private float windValue;

	[Space]
	[Range(-1f, 1f)]
	public float windGrassMultiplier = 1f;

	private FSMOwner fsmOwner;

	private bool stateWasChanged;

	private bool needUpdateState;

	private AudioMixerStateController audioMixerStateController = AudioMixerStateController.Unavailable;

	private MultiFlagOR<WeatherSystemPauseFlag> pauseMultiFlagDisabledState = new MultiFlagOR<WeatherSystemPauseFlag>();

	private const int MAX_GOD_RAYS = 5;

	private const float GOD_RAYS_LIFE_TIME = 10f;

	private const float MAX_RANGE_FROM_ACTIVATION_POINT = 10f;

	private const float GOD_RAY_SPAWN_INTERVAL_MIN = 0.2f;

	private const float GOD_RAY_SPAWN_INTERVAL_MAX = 0.8f;

	[SerializeField]
	private GameObject godRaysPrefab;

	[Header("God Rays Time Window (time of day 0..1)")]
	[SerializeField]
	[Range(0f, 1f)]
	private float godRaysTimeMin = 0.25f;

	[SerializeField]
	[Range(0f, 1f)]
	private float godRaysTimeMax = 0.65f;

	[SerializeField]
	private float godRaysLifeTime = 10f;

	[SerializeField]
	private float godRaysRange = 10f;

	[SerializeField]
	private int godRaysCount = 5;

	private List<GameObject> activeGodRays = new List<GameObject>();

	private List<float> activeGodRaysLifeTimes = new List<float>();

	private List<Vector3> activeGodRaysOffsets = new List<Vector3>();

	private List<GameObject> cachedGodRays = new List<GameObject>();

	private bool isWeatherPaused;

	private string lastWeatherStateName = "";

	private Coroutine godRaysSpawnCoroutine;

	public static WeatherSystem Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<WeatherSystem>(includeInactive: true);
			}
			return instance;
		}
	}

	private WeatherData WeatherData => MainGame.Instance.GameSave.weatherData;

	public float WindValue
	{
		get
		{
			return windValue;
		}
		set
		{
			windValue = value;
			OnWindValueChanged();
		}
	}

	public AudioMixerStateController AudioMixerStateController
	{
		get
		{
			EnsureAudioMixerStateController();
			return audioMixerStateController;
		}
	}

	protected void Awake()
	{
		if (!(instance != null) || !(instance != this))
		{
			instance = this;
			fsmOwner = GetComponent<FSMOwner>();
			WeatherComponent[] componentsInChildren = GetComponentsInChildren<WeatherComponent>(includeInactive: true);
			foreach (WeatherComponent weatherComponent in componentsInChildren)
			{
				components.TryAdd(weatherComponent.name, weatherComponent);
			}
			LazyPlatformDependentElement[] componentsInChildren2 = GetComponentsInChildren<LazyPlatformDependentElement>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Init();
			}
			pauseMultiFlagDisabledState.Init(SetPauseState);
			OnWindValueChanged();
		}
	}

	private void EnsureAudioMixerStateController()
	{
		if (audioMixerStateController == AudioMixerStateController.Unavailable && LazyAudio.TryGetAudioMixer(out var audioMixer))
		{
			audioMixerStateController = new AudioMixerStateController(audioMixer);
			SyncAudioMixerLayersFromData();
		}
	}

	private void SyncAudioMixerLayersFromData()
	{
		if (!(MainGame.Instance == null) && MainGame.Instance.GameSave != null)
		{
			audioMixerStateController.SetLayerActive(AudioMixerSnapshotLayer.Indoor, WeatherData.isSoundEnabled);
			audioMixerStateController.SetLayerActive(AudioMixerSnapshotLayer.Cinematics, WeatherData.isWeatherPausedByCinematics);
		}
	}

	public void RestoreFSMStateFromData()
	{
		if (string.IsNullOrEmpty(WeatherData.stateName))
		{
			WeatherData.stateName = "CleanWeather";
		}
		foreach (string enabledWeatherComponent in WeatherData.enabledWeatherComponents)
		{
			SetWeatherComponent(enabledWeatherComponent, isActive: true);
		}
		SetWeatherState(WeatherData.stateName);
		pauseMultiFlagDisabledState.UpdateFlag(WeatherSystemPauseFlag.TimeOfDay, WeatherData.isWeatherPausedByTimeOfDay);
		pauseMultiFlagDisabledState.UpdateFlag(WeatherSystemPauseFlag.Cinematics, WeatherData.isWeatherPausedByCinematics);
		ApplySoundParameters(WeatherData.isSoundEnabled, WeatherData.isIndoorSfxEnabled);
		AudioMixerStateController.SetLayerActive(AudioMixerSnapshotLayer.Indoor, WeatherData.isSoundEnabled);
		AudioMixerStateController.SetLayerActive(AudioMixerSnapshotLayer.Cinematics, WeatherData.isWeatherPausedByCinematics);
	}

	private void SetPauseState(bool isPaused)
	{
		isWeatherPaused = isPaused;
		if (isPaused)
		{
			ForceStopGodRays();
		}
		VerticalFog componentInChildren = GetComponentInChildren<VerticalFog>();
		if (componentInChildren != null)
		{
			componentInChildren.fogEnabled = !isPaused;
			componentInChildren.ApplyFogParameters();
		}
		base.gameObject.SetActive(!isPaused);
	}

	public void SetPauseState(WeatherSystemPauseFlag flag, bool isPaused)
	{
		pauseMultiFlagDisabledState.UpdateFlag(flag, isPaused);
		switch (flag)
		{
		case WeatherSystemPauseFlag.TimeOfDay:
			WeatherData.isWeatherPausedByTimeOfDay = isPaused;
			break;
		case WeatherSystemPauseFlag.Cinematics:
			WeatherData.isWeatherPausedByCinematics = isPaused;
			break;
		}
	}

	public void ClearWeather()
	{
		ForceStopGodRays();
		lastWeatherStateName = "";
		WeatherData.isWeatherPausedByTimeOfDay = false;
		WeatherData.isWeatherPausedByCinematics = false;
		pauseMultiFlagDisabledState.Reset();
		SetPauseState(isPaused: false);
		foreach (WeatherComponent value in components.Values)
		{
			value.ClearState();
		}
		AudioMixerStateController.ResetToDefault();
	}

	public void ApplySoundParameters(bool playSound, bool applyIndoorMod = false)
	{
		WeatherData.isSoundEnabled = playSound;
		WeatherData.isIndoorSfxEnabled = applyIndoorMod;
	}

	public void OnGameTimeChanged(float deltaTime)
	{
		if (!(deltaTime > 0.5f) && !(deltaTime < 0f))
		{
			WeatherData.currentPhaseLen += deltaTime;
			if (WeatherData.currentPhaseLen >= phaseLength)
			{
				WeatherData.currentPhaseLen -= phaseLength;
				RollNextWeatherState();
			}
		}
	}

	public void SetWeatherState(string stateName, bool force = false)
	{
		FSMWeatherState fSMWeatherState = fsmOwner.GetCurrentState() as FSMWeatherState;
		FSMState stateWithName = fSMWeatherState.FSM.GetStateWithName(stateName);
		if (stateWithName == null)
		{
			Debug.LogWarning("Warning: instance name:[" + base.gameObject.name + "] Can't find a weather state with name: " + stateName);
			return;
		}
		string prevStateName = fSMWeatherState.name;
		fSMWeatherState.FSM.EnterState(stateWithName, FSM.TransitionCallMode.Normal);
		Debug.Log($"Set weather state to: {stateName}, force: {force}");
		WeatherData.stateName = stateName;
		if (force)
		{
			WeatherData.hasForceState = true;
		}
		TryTriggerGodRaysOnTransition(prevStateName, stateName);
		lastWeatherStateName = stateName;
	}

	public void ResetWeatherState()
	{
		SetWeatherState("CleanWeather");
		WeatherData.hasForceState = false;
	}

	public void SetWeatherComponent(string componentName, bool isActive)
	{
		if (!components.TryGetValue(componentName, out var value))
		{
			Debug.LogWarning("Can't find weather component with name: " + componentName);
			return;
		}
		if (isActive)
		{
			value.FadeIn();
			if (!WeatherData.enabledWeatherComponents.Contains(componentName))
			{
				WeatherData.enabledWeatherComponents.Add(componentName);
			}
		}
		else
		{
			value.FadeOutIfActive();
			WeatherData.enabledWeatherComponents.Remove(componentName);
		}
		Debug.Log($"Set weather component: {componentName}, active: {isActive}");
	}

	private void RollNextWeatherState()
	{
		if (!WeatherData.hasForceState)
		{
			if (!(fsmOwner.GetCurrentState() is FSMWeatherState fSMWeatherState))
			{
				Debug.Log("Current weather state is null.");
				return;
			}
			fSMWeatherState.FinishWeatherState();
			stateWasChanged = true;
			needUpdateState = false;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying || MainGame.IsGamePaused)
		{
			return;
		}
		foreach (WeatherComponent value in components.Values)
		{
			value.CustomUpdate();
		}
		CPWindValue.ApplyParameters();
		VerticalFog.GlobalInstance.GlobalFogUpdate();
		CPDirectShadowsBlur.ApplyParameters();
		CPCloudsDensity.ApplyParameters();
		if (needUpdateState)
		{
			needUpdateState = false;
			string text = fsmOwner.GetCurrentState().name;
			WeatherData.stateName = text;
			Debug.Log("New weather state: " + WeatherData.stateName);
			TryTriggerGodRaysOnTransition(lastWeatherStateName, text);
			lastWeatherStateName = text;
		}
		else if (stateWasChanged)
		{
			needUpdateState = true;
			stateWasChanged = false;
		}
		UpdateGodRaysPositions();
		UpdateGodRaysLifeTimes();
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			ForceStopGodRays();
		}
	}

	private void TryTriggerGodRaysOnTransition(string prevStateName, string newStateName)
	{
		if (isWeatherPaused || string.IsNullOrEmpty(prevStateName) || prevStateName.IndexOf("rain", StringComparison.OrdinalIgnoreCase) < 0)
		{
			return;
		}
		bool num = newStateName.IndexOf("rain", StringComparison.OrdinalIgnoreCase) >= 0;
		bool flag = newStateName.IndexOf("mist", StringComparison.OrdinalIgnoreCase) >= 0;
		if (!(num || flag))
		{
			float num2 = ((EnvironmentEngine.Instance != null) ? EnvironmentEngine.Instance.timeOfDay : (-1f));
			if (!(num2 < godRaysTimeMin) && !(num2 >= godRaysTimeMax))
			{
				StartGodRaysSequence();
			}
		}
	}

	private void StartGodRaysSequence()
	{
		StopGodRaysSequence();
		int num = Mathf.Max(0, godRaysCount - activeGodRays.Count);
		if (num > 0)
		{
			godRaysSpawnCoroutine = StartCoroutine(GodRaysSpawnRoutine(num));
		}
	}

	private IEnumerator GodRaysSpawnRoutine(int count)
	{
		for (int i = 0; i < count; i++)
		{
			while (MainGame.IsGamePaused)
			{
				yield return null;
			}
			SpawnGodRay();
			if (i >= count - 1)
			{
				continue;
			}
			float wait = UnityEngine.Random.Range(0.2f, 0.8f);
			float elapsed = 0f;
			while (elapsed < wait)
			{
				while (MainGame.IsGamePaused)
				{
					yield return null;
				}
				elapsed += Time.deltaTime;
				yield return null;
			}
		}
		godRaysSpawnCoroutine = null;
	}

	private void StopGodRaysSequence()
	{
		if (godRaysSpawnCoroutine != null)
		{
			StopCoroutine(godRaysSpawnCoroutine);
			godRaysSpawnCoroutine = null;
		}
	}

	private void SpawnGodRay()
	{
		if (!(godRaysPrefab == null))
		{
			Vector3 godRayActivationCenter = GetGodRayActivationCenter();
			Vector2 vector = UnityEngine.Random.insideUnitCircle * godRaysRange;
			Vector3 vector2 = new Vector3(vector.x, 0f, vector.y);
			Vector3 position = godRayActivationCenter + vector2;
			GameObject pooledGodRay = GetPooledGodRay();
			pooledGodRay.transform.SetParent(base.transform, worldPositionStays: false);
			pooledGodRay.transform.position = position;
			pooledGodRay.SetActive(value: true);
			activeGodRays.Add(pooledGodRay);
			activeGodRaysLifeTimes.Add(godRaysLifeTime);
			activeGodRaysOffsets.Add(vector2);
		}
	}

	private Vector3 GetGodRayActivationCenter()
	{
		CameraController cameraController = ((CameraSystem.Instance != null) ? CameraSystem.Instance.ActiveCameraController : null);
		if (cameraController != null && cameraController.Target != null)
		{
			return cameraController.Target.position;
		}
		if (CameraSystem.Instance != null && CameraSystem.Instance.WorldCamera != null)
		{
			return CameraSystem.Instance.WorldCamera.transform.position;
		}
		return base.transform.position;
	}

	private GameObject GetPooledGodRay()
	{
		if (cachedGodRays.Count > 0)
		{
			int index = cachedGodRays.Count - 1;
			GameObject result = cachedGodRays[index];
			cachedGodRays.RemoveAt(index);
			return result;
		}
		return UnityEngine.Object.Instantiate(godRaysPrefab);
	}

	private void ReleaseGodRay(int index)
	{
		GameObject gameObject = activeGodRays[index];
		gameObject.SetActive(value: false);
		activeGodRays.RemoveAt(index);
		activeGodRaysLifeTimes.RemoveAt(index);
		activeGodRaysOffsets.RemoveAt(index);
		cachedGodRays.Add(gameObject);
	}

	private void UpdateGodRaysPositions()
	{
		if (activeGodRays.Count != 0)
		{
			Vector3 godRayActivationCenter = GetGodRayActivationCenter();
			for (int i = 0; i < activeGodRays.Count; i++)
			{
				activeGodRays[i].transform.position = godRayActivationCenter + activeGodRaysOffsets[i];
			}
		}
	}

	private void UpdateGodRaysLifeTimes()
	{
		if (activeGodRaysLifeTimes.Count == 0)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int num = activeGodRaysLifeTimes.Count - 1; num >= 0; num--)
		{
			float num2 = activeGodRaysLifeTimes[num] - deltaTime;
			if (num2 <= 0f)
			{
				ReleaseGodRay(num);
			}
			else
			{
				activeGodRaysLifeTimes[num] = num2;
			}
		}
	}

	private void ForceStopGodRays()
	{
		StopGodRaysSequence();
		for (int num = activeGodRays.Count - 1; num >= 0; num--)
		{
			ReleaseGodRay(num);
		}
	}

	private void OnWindValueChanged()
	{
		Shader.SetGlobalFloat(GlobalShaderParameters.idWindValue, WindValue);
		UpdateDeformMaterialParameters(LazySingletonSO<GlobalResources>.Instance.matObject3DDeforming);
		UpdateDeformMaterialParameters(LazySingletonSO<GlobalResources>.Instance.matDeformingGrass);
		WorldParticleController.UpdateParameters();
		WindDependentSound.UpdateSounds();
	}

	private void UpdateDeformMaterialParameters(Material mat)
	{
		mat.SetFloat(idWindAmp, windGrassMultiplier * WindValue * 0.15f + 0.025f);
		mat.SetFloat(idWindConstant, windGrassMultiplier * WindValue * 0.2f);
		mat.SetFloat(idWindGustAmp, windGrassMultiplier * WindValue * 0.5f);
	}
}
