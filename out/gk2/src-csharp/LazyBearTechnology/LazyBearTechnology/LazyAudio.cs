using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace LazyBearTechnology;

public class LazyAudio : MonoBehaviour
{
	[SerializeField]
	private Transform microphone;

	[Header("Settings")]
	[SerializeField]
	private int initialPoolSize = 15;

	[SerializeField]
	private bool dynamic = true;

	[SerializeField]
	private float minDelayTimeBetweenSounds = 15f;

	[Space]
	[SerializeField]
	private List<SoundController> activeSources = new List<SoundController>();

	[SerializeField]
	private Stack<SoundController> inactiveSources = new Stack<SoundController>();

	private List<PlaylistController> playlistControllers = new List<PlaylistController>();

	private Dictionary<AudioSettings3DType, AudioSource> audio3DSettingsPresets = new Dictionary<AudioSettings3DType, AudioSource>();

	[SerializeField]
	private AudioConfig audioConfig;

	[SerializeField]
	private AudioMixerGroup voiceOverAudioMixerGroup;

	private bool isDataCached;

	private List<PlaylistWeight> cachedPlaylistWeights = new List<PlaylistWeight>();

	private static LazyAudio instance;

	private static bool isInitialized;

	private VoiceOverPlayer voiceOverPlayer;

	public static Func<VoiceOverPlayer> VoiceOverPlayerFactory;

	public static bool IsInitialized => isInitialized;

	public static AudioMixerGroup VoiceOverAudioMixerGroup => instance.voiceOverAudioMixerGroup;

	public static Transform Microphone => instance.microphone;

	public static AudioMixer AudioMixer => Instance?.audioConfig?.audioMixer;

	private static LazyAudio Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindAnyObjectByType<LazyAudio>(FindObjectsInactive.Exclude);
				if (instance == null)
				{
					Debug.LogError("LazyAudio.Instance error: couldn't find a LazyAudio object.");
					return null;
				}
				if (instance.audioConfig == null)
				{
					Debug.Log("#aud# AudioConfig instancing");
					instance.audioConfig = LazySingletonSO<AudioConfig>.Instance;
				}
				if (instance.voiceOverPlayer == null)
				{
					Debug.Log("#aud# VoiceOverPlayer creating");
					instance.voiceOverPlayer = ((VoiceOverPlayerFactory != null) ? VoiceOverPlayerFactory() : new VoiceOverPlayer(instance.voiceOverAudioMixerGroup));
				}
				instance.InitializeAudio3dPresets();
				isInitialized = true;
			}
			return instance;
		}
	}

	public static VoiceOverPlayer VoiceOverPlayer => instance.voiceOverPlayer;

	public static bool TryGetAudioMixer(out AudioMixer audioMixer)
	{
		audioMixer = AudioMixer;
		return audioMixer != null;
	}

	public void SetConfigurationReference(AudioConfig config)
	{
		audioConfig = config;
	}

	private void Awake()
	{
		if (Application.isPlaying && instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (Application.isPlaying)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		_ = Instance;
		InitializeAudio3dPresets();
		InitializeStartPool();
		InitializePlaylists();
	}

	private void InitializePlaylists()
	{
		if (audioConfig == null)
		{
			audioConfig = LazySingletonSO<AudioConfig>.Instance;
		}
		if (audioConfig == null)
		{
			Debug.LogError("Audio System Error: AudioConfig for AudioSystem not found.");
			return;
		}
		for (int i = 0; i < audioConfig.playlists.Count; i++)
		{
			PlaylistController playlistController = new GameObject().AddComponent<PlaylistController>();
			playlistController.Initialize(audioConfig.playlists[i]);
			playlistController.transform.SetParent(base.gameObject.transform);
			playlistControllers.Add(playlistController);
		}
	}

	private void InitializeStartPool()
	{
		for (int i = 0; i < initialPoolSize; i++)
		{
			SoundController soundController = CreateSoundController();
			soundController.gameObject.SetActive(value: false);
			inactiveSources.Push(soundController);
		}
	}

	private void InitializeAudio3dPresets()
	{
		audio3DSettingsPresets.Clear();
		AudioSettings3DPreset[] componentsInChildren = GetComponentsInChildren<AudioSettings3DPreset>(includeInactive: true);
		if (componentsInChildren.Length == 0)
		{
			Debug.LogError($"Audio System Error: Cannot find any audio settings 3d presets [{typeof(AudioSettings3DPreset)}]");
		}
		bool flag = false;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			audio3DSettingsPresets.Add(componentsInChildren[i].type, componentsInChildren[i].GetComponent<AudioSource>());
			if (!flag && componentsInChildren[i].type == AudioSettings3DType.Default)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.LogError($"Audio System Error: Cannot find default audio settings 3d preset [{typeof(AudioSettings3DPreset)}]");
		}
	}

	private SoundController CreateSoundController()
	{
		SoundController soundController = new GameObject().AddComponent<SoundController>();
		AudioSource audioSource = soundController.gameObject.AddComponent<AudioSource>();
		soundController.audioSource = audioSource;
		soundController.transform.SetParent(base.gameObject.transform);
		return soundController;
	}

	private void Update()
	{
		for (int i = 0; i < activeSources.Count; i++)
		{
			SoundController soundController = activeSources[i];
			if (soundController.IsAlive)
			{
				soundController.UpdateVolumeWhilePlaying();
				soundController.UpdatePosition(microphone);
				continue;
			}
			soundController.gameObject.SetActive(value: false);
			soundController.Reset();
			activeSources.RemoveAt(i);
			i--;
			inactiveSources.Push(soundController);
		}
	}

	public static void PlayAndForget(string id)
	{
		SoundController soundController = Instance.PlaySound(id, playAndForget: true);
		if (!(soundController == null))
		{
			soundController.audioSource.spatialBlend = 0f;
		}
	}

	public static SoundHandler Play(string id)
	{
		return Play(id, checkDelay: true);
	}

	public static SoundHandler Play(string id, bool checkDelay)
	{
		SoundController soundController = Instance.PlaySound(id, playAndForget: false, checkDelay);
		if (soundController == null)
		{
			return null;
		}
		soundController.audioSource.spatialBlend = 0f;
		return soundController.soundHandler;
	}

	public static SoundHandler PlayAtPos(string id, Vector3 position)
	{
		SoundController soundController = Instance.PlaySound(id, playAndForget: false);
		if (soundController == null)
		{
			return null;
		}
		soundController.transform.position = position;
		soundController.audioSource.spatialBlend = 1f;
		return soundController.soundHandler;
	}

	public static SoundHandler PlayAtGameObject(string id, Transform obj, SpatialType spatial = SpatialType.sound1D, bool checkDelay = true)
	{
		SoundController soundController = Instance.PlaySound(id, playAndForget: false, checkDelay);
		if (soundController == null)
		{
			return null;
		}
		soundController.target = obj;
		soundController.spatial = spatial;
		soundController.UpdatePosition(Instance.microphone);
		return soundController.soundHandler;
	}

	public static void Stop(string id)
	{
		if (Instance == null)
		{
			return;
		}
		foreach (SoundController activeSource in Instance.activeSources)
		{
			if (activeSource.id == id)
			{
				activeSource.Stop();
			}
		}
	}

	public static void Stop(SoundHandler handler)
	{
		handler.Stop();
	}

	public static void Pause(SoundHandler handler)
	{
		handler.Pause();
	}

	public static void UnPause(SoundHandler handler)
	{
		handler.UnPause();
	}

	public static void StopAll()
	{
		foreach (SoundController activeSource in Instance.activeSources)
		{
			activeSource.Stop();
		}
	}

	public static PlaylistController PlayPlaylist(string playlistId)
	{
		List<PlaylistController> list = Instance.playlistControllers;
		PlaylistController playlistController = null;
		foreach (PlaylistController item in list)
		{
			if (item.Id == playlistId)
			{
				playlistController = item;
				break;
			}
		}
		if (playlistController == null)
		{
			Debug.LogError("Audio System Error: playlist with key " + playlistId + " not found.");
			return null;
		}
		playlistController.Play();
		return playlistController;
	}

	public static PlaylistController PlayPlaylist(string playlistId, float fadeDuration)
	{
		List<PlaylistController> list = Instance.playlistControllers;
		PlaylistController playlistController = null;
		foreach (PlaylistController item in list)
		{
			if (item.Id == playlistId)
			{
				playlistController = item;
				break;
			}
		}
		if (playlistController == null)
		{
			Debug.LogError("Audio System Error: playlist with key " + playlistId + " not found.");
			return null;
		}
		playlistController.Play(fadeDuration);
		return playlistController;
	}

	public static void PausePlaylist(string playlistId)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.Pause();
			}
		}
	}

	public static void PausePlaylist(string playlistId, float duration)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.Pause(duration);
			}
		}
	}

	public static void PausePlaylist(string playlistId, float duration, Ease ease)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.Pause(duration, ease);
			}
		}
	}

	public static void UnPausePlaylist(string playlistId)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.UnPause();
			}
		}
	}

	public static void UnPausePlaylist(string playlistId, float duration)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.UnPause(duration);
			}
		}
	}

	public static void UnPausePlaylist(string playlistId, float duration, Ease ease)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.UnPause(duration, ease);
			}
		}
	}

	public static void StopPlaylist(string playlistId)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.Stop();
			}
		}
	}

	public static void StopPlaylist(string playlistId, float fadeDuration)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.Stop(fadeDuration);
			}
		}
	}

	public static void PlayNextTrack(string playlistId)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.NextTrack();
			}
		}
	}

	public static void PlayTrackInPlaylist(string trackId, string playlistId)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.PlayTrack(trackId);
			}
		}
	}

	public static void PlayTrackInPlaylist(string trackId, string playlistId, float fadeDuration)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.PlayTrack(trackId, fadeDuration);
			}
		}
	}

	public static void StopAllPlaylistsImmediately()
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			playlistController.StopImmediately();
		}
	}

	public static void SetWeightTrackInPlaylist(string trackId, string playlistId, float weight)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.SetWeightTrack(trackId, weight);
			}
		}
	}

	public static void AddWeightTrackInPlaylist(string trackId, string playlistId, float weight)
	{
		foreach (PlaylistController playlistController in Instance.playlistControllers)
		{
			if (playlistController.Id == playlistId)
			{
				playlistController.AddWeightTrack(trackId, weight);
			}
		}
	}

	public static List<PlaylistController> GetPlaylistControllers()
	{
		return new List<PlaylistController>(Instance.playlistControllers);
	}

	public static void SetChannelVolume(string channel, float volume)
	{
		if (volume <= 0f)
		{
			Instance.audioConfig.audioMixer.SetFloat(channel, -80f);
		}
		else
		{
			Instance.audioConfig.audioMixer.SetFloat(channel, Mathf.Log10(volume) * 20f);
		}
	}

	public static void UpdateMicrophone(Transform microphone)
	{
		Instance.microphone = microphone;
	}

	private SoundController PlaySound(string id, bool playAndForget, bool checkDelay = true)
	{
		if (checkDelay && !IsValidTimePlaybackSound(id))
		{
			return null;
		}
		SoundController freeSoundController = GetFreeSoundController();
		if (freeSoundController == null)
		{
			return null;
		}
		Sound sound = audioConfig.Get(id);
		if (sound == null)
		{
			if (!audioConfig.defaultClip)
			{
				Debug.LogError("Audio System Error: sound with key [" + id + "] not found.");
				return null;
			}
			sound = new Sound();
			Sample sample = new Sample();
			sample.clip = audioConfig.defaultClip;
			sample.volume = 0.3f;
			sound.samples.Add(sample);
			sound.audioSettings3DType = AudioSettings3DType.Default;
			Debug.LogWarning("Audio System Warning: sound with key [" + id + "] not found. Playing [defaultClip]");
		}
		if (!audio3DSettingsPresets.TryGetValue(sound.audioSettings3DType, out var value))
		{
			Debug.LogError("Audio System Error: cannot find audio 3d settings preset for Sound [" + sound.id + "]");
			return null;
		}
		ApplySettingFromSample(sound.RandomSample, freeSoundController.audioSource, sound.volume, sound.panning, sound.loop, sound.group, value);
		activeSources.Add(freeSoundController);
		freeSoundController.id = id;
		freeSoundController.Play();
		if (!playAndForget)
		{
			freeSoundController.soundHandler = new SoundHandler(freeSoundController);
		}
		return freeSoundController;
	}

	private void ApplySettingFromSample(Sample sample, AudioSource audioSource, float soundVolume, float soundPanning, bool soundLooped, AudioMixerGroup group, AudioSource settings3DPreset)
	{
		audioSource.volume = sample.volume * soundVolume;
		audioSource.panStereo = sample.panning * soundPanning;
		audioSource.pitch = sample.Pitch;
		audioSource.outputAudioMixerGroup = group;
		audioSource.loop = soundLooped;
		audioSource.clip = sample.clip;
		audioSource.rolloffMode = settings3DPreset.rolloffMode;
		audioSource.minDistance = settings3DPreset.minDistance;
		audioSource.maxDistance = settings3DPreset.maxDistance;
		audioSource.dopplerLevel = settings3DPreset.dopplerLevel;
		audioSource.spread = settings3DPreset.spread;
		audioSource.spatialBlend = settings3DPreset.spatialBlend;
		audioSource.reverbZoneMix = settings3DPreset.reverbZoneMix;
		audioSource.SetCustomCurve(AudioSourceCurveType.Spread, settings3DPreset.GetCustomCurve(AudioSourceCurveType.Spread));
		audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, settings3DPreset.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
		audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, settings3DPreset.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
		audioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, settings3DPreset.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
	}

	private SoundController GetFreeSoundController()
	{
		SoundController soundController;
		if (inactiveSources.Count != 0)
		{
			soundController = inactiveSources.Pop();
		}
		else
		{
			if (!dynamic)
			{
				Debug.LogError("Audio System Error: No free AudioSource available");
				return null;
			}
			soundController = CreateSoundController();
		}
		soundController.gameObject.SetActive(value: true);
		return soundController;
	}

	private bool IsValidTimePlaybackSound(string id)
	{
		foreach (SoundController activeSource in activeSources)
		{
			if (activeSource.id == id && activeSource.startTime + minDelayTimeBetweenSounds > Time.time)
			{
				return false;
			}
		}
		return true;
	}
}
