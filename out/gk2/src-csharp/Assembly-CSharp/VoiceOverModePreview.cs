using System.Collections;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class VoiceOverModePreview
{
	private const string VoiceOverPreviewId = "153_crossroad_dark_come_1";

	private static readonly VoiceID VoiceOverPreviewVoiceId = VoiceID.Larry;

	private const float MumblingPreviewDuration = 1.5f;

	private static Coroutine previewRoutine;

	private static AudioSource previewSource;

	private static AudioClip cachedClip;

	private static AsyncOperationHandle<AudioClip> clipHandle;

	public static void Warmup()
	{
		EnsureClipLoadStarted();
	}

	public static void Play(VoiceOverMode mode, MonoBehaviour coroutineHost)
	{
		Stop(coroutineHost);
		if (mode == VoiceOverMode.VoiceOver)
		{
			PlayVoiceOverPreview();
		}
		else if (!(coroutineHost == null) && !(LazySpeechEngine.Instance == null))
		{
			previewRoutine = coroutineHost.StartCoroutine(PlayMumblingPreview());
		}
	}

	public static void Stop(MonoBehaviour coroutineHost)
	{
		if (previewRoutine != null && coroutineHost != null)
		{
			coroutineHost.StopCoroutine(previewRoutine);
			previewRoutine = null;
		}
		if (previewSource != null)
		{
			previewSource.Stop();
		}
		LazySpeechEngine.Instance?.Stop(VoiceOverPreviewVoiceId);
	}

	private static void PlayVoiceOverPreview()
	{
		AudioClip clip = GetClip();
		if (!(clip == null))
		{
			AudioSource audioSource = GetPreviewSource();
			audioSource.clip = clip;
			audioSource.loop = false;
			audioSource.outputAudioMixerGroup = GetSpeechMixerGroup();
			audioSource.Play();
		}
	}

	private static void EnsureClipLoadStarted()
	{
		if (!(cachedClip != null) && !clipHandle.IsValid())
		{
			clipHandle = Addressables.LoadAssetAsync<AudioClip>("153_crossroad_dark_come_1.wav");
		}
	}

	private static AudioClip GetClip()
	{
		if (cachedClip != null)
		{
			return cachedClip;
		}
		EnsureClipLoadStarted();
		if (!clipHandle.IsValid())
		{
			return null;
		}
		cachedClip = clipHandle.WaitForCompletion();
		return cachedClip;
	}

	private static IEnumerator PlayMumblingPreview()
	{
		float remaining = 1.5f;
		LazySpeechEngine engine = LazySpeechEngine.Instance;
		while (remaining > 0f && engine != null)
		{
			engine.Play(VoiceOverPreviewVoiceId, remaining);
			remaining -= Time.unscaledDeltaTime;
			yield return null;
		}
		engine?.Stop(VoiceOverPreviewVoiceId);
		previewRoutine = null;
	}

	private static AudioSource GetPreviewSource()
	{
		if (previewSource != null)
		{
			return previewSource;
		}
		GameObject gameObject = new GameObject("VoiceOverModePreview");
		Object.DontDestroyOnLoad(gameObject);
		previewSource = gameObject.AddComponent<AudioSource>();
		previewSource.playOnAwake = false;
		previewSource.spatialBlend = 0f;
		return previewSource;
	}

	private static AudioMixerGroup GetSpeechMixerGroup()
	{
		if (LazySingletonSO<AudioConfig>.Instance != null && LazySingletonSO<AudioConfig>.Instance.voiceOverGroup != null)
		{
			return LazySingletonSO<AudioConfig>.Instance.voiceOverGroup;
		}
		if (!LazyAudio.IsInitialized)
		{
			return null;
		}
		return LazyAudio.VoiceOverAudioMixerGroup;
	}
}
