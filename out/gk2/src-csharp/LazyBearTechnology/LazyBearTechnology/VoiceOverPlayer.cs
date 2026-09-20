using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LazyBearTechnology;

public class VoiceOverPlayer
{
	private class VoiceOverTailStopper : MonoBehaviour
	{
		private Coroutine stopRoutine;

		public void SchedulePlayAndStop(SoundController controller, AudioSource source, float playDelay, float playDuration)
		{
			Cancel();
			stopRoutine = StartCoroutine(PlayAndStopAfterDelay(controller, source, playDelay, playDuration));
		}

		public void ScheduleStop(AudioSource source, float delay)
		{
			Cancel();
			stopRoutine = StartCoroutine(StopAfterDelay(source, delay));
		}

		public void Cancel()
		{
			if (stopRoutine != null)
			{
				StopCoroutine(stopRoutine);
				stopRoutine = null;
			}
		}

		private IEnumerator PlayAndStopAfterDelay(SoundController controller, AudioSource source, float playDelay, float playDuration)
		{
			yield return new WaitForSeconds(playDelay);
			if (controller != null && source != null && source.clip != null)
			{
				controller.Play();
			}
			if (playDuration > 0f)
			{
				yield return new WaitForSeconds(playDuration);
				if (source != null && source.isPlaying)
				{
					source.Stop();
				}
			}
			stopRoutine = null;
		}

		private IEnumerator StopAfterDelay(AudioSource source, float delay)
		{
			yield return new WaitForSeconds(delay);
			if (source != null && source.isPlaying)
			{
				source.Stop();
			}
			stopRoutine = null;
		}
	}

	private static SoundController voiceOverSoundController;

	private static VoiceOverTailStopper voiceOverTailStopper;

	private static readonly Dictionary<string, IResourceLocation> clipLocations = new Dictionary<string, IResourceLocation>();

	private static readonly Dictionary<string, IResourceLocation> dataLocations = new Dictionary<string, IResourceLocation>();

	private static bool areLocationsBuilt;

	protected AudioClip audioClip;

	protected VoiceClipData voiceClipData;

	protected AsyncOperationHandle<AudioClip> handler;

	protected AsyncOperationHandle<VoiceClipData> voiceClipDataHandler;

	protected AudioMixerGroup outputAudioMixerGroup;

	protected VoiceID currentVoiceId;

	protected string currentLocalKey;

	public static Func<string, bool> MuteCheck;

	public bool HasVoiceOver => audioClip != null;

	public float ClipLength
	{
		get
		{
			if (!(audioClip != null))
			{
				return 0f;
			}
			return GetPlayableClipLength(audioClip, voiceClipData) + VoiceOverSettings.StartPause + VoiceOverSettings.AdditionalClipLength;
		}
	}

	public bool IsPlaying
	{
		get
		{
			if (voiceOverSoundController != null && voiceOverSoundController.audioSource != null)
			{
				return voiceOverSoundController.audioSource.isPlaying;
			}
			return false;
		}
	}

	public bool IsPlayingByLoud(VoiceID voiceId, string localKey)
	{
		if (!IsCurrentPlayback(voiceId, localKey))
		{
			return false;
		}
		if (!IsPlaying)
		{
			return false;
		}
		LoudInterval[] array = ((voiceClipData != null) ? voiceClipData.loudIntervals : null);
		if (array == null || array.Length == 0)
		{
			return true;
		}
		float time = voiceOverSoundController.audioSource.time;
		for (int i = 0; i < array.Length; i++)
		{
			if (time >= array[i].start && time <= array[i].end)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsMuted(string id)
	{
		if (MuteCheck != null)
		{
			return MuteCheck(id);
		}
		return false;
	}

	public VoiceOverPlayer(AudioMixerGroup outputAudioMixerGroup)
	{
		this.outputAudioMixerGroup = outputAudioMixerGroup;
	}

	public virtual void Play(string id, VoiceID voiceId = null)
	{
		if (!VoiceOverSettings.IsEnabled || IsMuted(id))
		{
			return;
		}
		Stop();
		EnsureLocationsCache();
		try
		{
			if (!TryGetLocation(clipLocations, id, out var location))
			{
				Debug.LogWarning("Failed to find VoiceOver by id '" + id + "' in Addressables label '" + VoiceOverSettings.VoiceOversLabel + "'.");
				return;
			}
			handler = Addressables.LoadAssetAsync<AudioClip>(location);
			audioClip = handler.WaitForCompletion();
		}
		catch (Exception)
		{
			Debug.LogWarning("Failed to load VoiceOver: " + id);
		}
		voiceClipData = null;
		try
		{
			if (TryGetLocation(dataLocations, id, out var location2))
			{
				voiceClipDataHandler = Addressables.LoadAssetAsync<VoiceClipData>(location2);
				voiceClipData = voiceClipDataHandler.WaitForCompletion();
			}
		}
		catch (Exception)
		{
		}
		if (audioClip != null)
		{
			currentLocalKey = id;
			currentVoiceId = voiceId;
		}
		PlayLoadedClip();
	}

	public virtual void Stop()
	{
		if (voiceOverTailStopper != null)
		{
			voiceOverTailStopper.Cancel();
		}
		if (voiceOverSoundController != null)
		{
			voiceOverSoundController.audioSource.clip = null;
			if (handler.IsValid())
			{
				Addressables.Release(handler);
			}
			if (voiceClipDataHandler.IsValid())
			{
				Addressables.Release(voiceClipDataHandler);
			}
			voiceOverSoundController.Stop();
		}
		currentLocalKey = null;
		currentVoiceId = null;
		audioClip = null;
		voiceClipData = null;
	}

	private bool IsCurrentPlayback(VoiceID voiceId, string localKey)
	{
		if (string.IsNullOrEmpty(currentLocalKey) || string.IsNullOrEmpty(localKey))
		{
			return false;
		}
		if (currentLocalKey != localKey)
		{
			return false;
		}
		return currentVoiceId == voiceId;
	}

	protected void PlayLoadedClip()
	{
		if (audioClip == null)
		{
			return;
		}
		if (voiceOverSoundController == null)
		{
			voiceOverSoundController = new GameObject().AddComponent<SoundController>();
			AudioSource audioSource = voiceOverSoundController.gameObject.AddComponent<AudioSource>();
			audioSource.loop = false;
			audioSource.outputAudioMixerGroup = LazySingletonSO<AudioConfig>.Instance.voiceOverGroup;
			audioSource.outputAudioMixerGroup = outputAudioMixerGroup;
			voiceOverSoundController.audioSource = audioSource;
			voiceOverSoundController.spatial = SpatialType.sound1D;
		}
		if (voiceOverTailStopper == null)
		{
			voiceOverTailStopper = voiceOverSoundController.GetComponent<VoiceOverTailStopper>();
			if (voiceOverTailStopper == null)
			{
				voiceOverTailStopper = voiceOverSoundController.gameObject.AddComponent<VoiceOverTailStopper>();
			}
		}
		voiceOverSoundController.audioSource.clip = audioClip;
		float startSilence = GetStartSilence(audioClip, voiceClipData);
		float playableClipLength = GetPlayableClipLength(audioClip, voiceClipData);
		float startPause = VoiceOverSettings.StartPause;
		voiceOverSoundController.audioSource.time = startSilence;
		voiceOverTailStopper.Cancel();
		if (startPause > 0f)
		{
			voiceOverTailStopper.SchedulePlayAndStop(voiceOverSoundController, voiceOverSoundController.audioSource, startPause, playableClipLength);
			return;
		}
		voiceOverSoundController.Play();
		if (playableClipLength > 0f)
		{
			voiceOverTailStopper.ScheduleStop(voiceOverSoundController.audioSource, playableClipLength);
		}
	}

	private static float GetStartSilence(AudioClip clip, VoiceClipData data)
	{
		if (clip == null)
		{
			return 0f;
		}
		if (!(data != null))
		{
			return 0f;
		}
		return Mathf.Clamp(data.startSilence, 0f, clip.length);
	}

	private static float GetPlayableClipLength(AudioClip clip, VoiceClipData data)
	{
		if (clip == null)
		{
			return 0f;
		}
		float startSilence = GetStartSilence(clip, data);
		float num = ((data != null) ? Mathf.Clamp(data.endSilence, 0f, clip.length) : 0f);
		return Mathf.Max(0.01f, clip.length - startSilence - num);
	}

	private static void EnsureLocationsCache()
	{
		if (!areLocationsBuilt)
		{
			clipLocations.Clear();
			dataLocations.Clear();
			BuildLocationsForType<AudioClip>(clipLocations);
			BuildLocationsForType<VoiceClipData>(dataLocations);
			areLocationsBuilt = true;
		}
	}

	private static void BuildLocationsForType<T>(Dictionary<string, IResourceLocation> targetMap)
	{
		AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(VoiceOverSettings.VoiceOversLabel, typeof(T));
		IList<IResourceLocation> list = handle.WaitForCompletion();
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				IResourceLocation resourceLocation = list[i];
				if (resourceLocation == null || string.IsNullOrEmpty(resourceLocation.PrimaryKey))
				{
					continue;
				}
				string text = NormalizePathForKey(Path.ChangeExtension(resourceLocation.PrimaryKey, null));
				if (!string.IsNullOrEmpty(text))
				{
					string text2 = NormalizePathForKey(Path.GetFileNameWithoutExtension(resourceLocation.PrimaryKey));
					if (!targetMap.ContainsKey(text))
					{
						targetMap.Add(text, resourceLocation);
					}
					if (!string.IsNullOrEmpty(text2) && !targetMap.ContainsKey(text2))
					{
						targetMap.Add(text2, resourceLocation);
					}
				}
			}
		}
		if (handle.IsValid())
		{
			Addressables.Release(handle);
		}
	}

	private static bool TryGetLocation(Dictionary<string, IResourceLocation> map, string id, out IResourceLocation location)
	{
		string text = NormalizePathForKey(Path.ChangeExtension(id, null));
		if (!string.IsNullOrEmpty(text) && map.TryGetValue(text, out location))
		{
			return true;
		}
		string text2 = NormalizePathForKey(Path.GetFileNameWithoutExtension(id));
		if (!string.IsNullOrEmpty(text2) && map.TryGetValue(text2, out location))
		{
			return true;
		}
		location = null;
		return false;
	}

	private static string NormalizePathForKey(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return string.Empty;
		}
		return path.Replace('\\', '/').TrimStart('/');
	}
}
