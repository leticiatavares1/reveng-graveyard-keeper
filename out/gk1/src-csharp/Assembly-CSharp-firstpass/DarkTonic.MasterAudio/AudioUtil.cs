using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class AudioUtil
{
	public const float DefaultMinOcclusionCutoffFrequency = 22000f;

	public const float DefaultMaxOcclusionCutoffFrequency = 0f;

	private const float SemitonePitchChangeAmt = 1.0594635f;

	public static float FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;

	public static float FrameTime => UnityEngine.Time.unscaledDeltaTime;

	public static float Time => UnityEngine.Time.unscaledTime;

	public static int FrameCount => UnityEngine.Time.frameCount;

	private static float CutoffRange(SoundGroupVariationUpdater updater)
	{
		return updater.MinOcclusionFreq - updater.MaxOcclusionFreq;
	}

	private static float MaxCutoffFreq(SoundGroupVariationUpdater updater)
	{
		return updater.MaxOcclusionFreq;
	}

	public static float MinCutoffFreq(SoundGroupVariationUpdater updater)
	{
		return updater.MinOcclusionFreq;
	}

	public static float GetOcclusionCutoffFrequencyByDistanceRatio(float distRatio, SoundGroupVariationUpdater updater)
	{
		return MaxCutoffFreq(updater) + distRatio * CutoffRange(updater);
	}

	public static float GetSemitonesFromPitch(float pitch)
	{
		if (pitch < 1f && pitch > 0f)
		{
			return Mathf.Log(1f / pitch, 1.0594635f) * -1f;
		}
		return Mathf.Log(pitch, 1.0594635f);
	}

	public static float GetPitchFromSemitones(float semitones)
	{
		if (semitones >= 0f)
		{
			return Mathf.Pow(1.0594635f, semitones);
		}
		return 1f / Mathf.Pow(1.0594635f, Mathf.Abs(semitones));
	}

	public static float GetDbFromFloatVolume(float vol)
	{
		return Mathf.Log10(vol) * 20f;
	}

	public static float GetFloatVolumeFromDb(float db)
	{
		return Mathf.Pow(10f, db / 20f);
	}

	public static float GetAudioPlayedPercentage(AudioSource source)
	{
		if (source.clip == null || source.time == 0f)
		{
			return 0f;
		}
		return source.time / source.clip.length * 100f;
	}

	public static bool IsAudioPaused(AudioSource source)
	{
		if (!source.isPlaying)
		{
			return GetAudioPlayedPercentage(source) > 0f;
		}
		return false;
	}

	public static void ClipPlayed(AudioClip clip, GameObject actor)
	{
		if (!AudioClipWillPreload(clip))
		{
			AudioLoaderOptimizer.AddNonPreloadedPlayingClip(clip, actor);
		}
	}

	public static void UnloadNonPreloadedAudioData(AudioClip clip, GameObject actor)
	{
		if (!(clip == null) && !AudioClipWillPreload(clip))
		{
			AudioLoaderOptimizer.RemoveNonPreloadedPlayingClip(clip, actor);
			if (!AudioLoaderOptimizer.IsAnyOfNonPreloadedClipPlaying(clip))
			{
				clip.UnloadAudioData();
			}
		}
	}

	public static bool AudioClipWillPreload(AudioClip clip)
	{
		if (clip == null)
		{
			return false;
		}
		return clip.preloadAudioData;
	}

	public static bool IsClipReadyToPlay(this AudioClip clip)
	{
		if (clip != null)
		{
			return clip.loadType != AudioClipLoadType.Streaming;
		}
		return false;
	}

	private static float GetPositiveUsablePitch(AudioSource source)
	{
		return GetPositiveUsablePitch(source.pitch);
	}

	private static float GetPositiveUsablePitch(float pitch)
	{
		if (!(pitch > 0f))
		{
			return 1f;
		}
		return pitch;
	}

	public static float AdjustAudioClipDurationForPitch(float duration, AudioSource sourceWithPitch)
	{
		return AdjustAudioClipDurationForPitch(duration, sourceWithPitch.pitch);
	}

	public static float AdjustAudioClipDurationForPitch(float duration, float pitch)
	{
		return duration / GetPositiveUsablePitch(pitch);
	}

	public static float AdjustEndLeadTimeForPitch(float duration, AudioSource sourceWithPitch)
	{
		return duration * GetPositiveUsablePitch(sourceWithPitch);
	}
}
