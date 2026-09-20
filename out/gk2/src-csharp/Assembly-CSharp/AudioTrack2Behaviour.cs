using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class AudioTrack2Behaviour : PlayableBehaviour
{
	[NonSerialized]
	public AudioClip clip;

	[NonSerialized]
	public bool loop;

	[Range(0f, 1f)]
	public float volume = 1f;

	public float fadeDuration;

	public AnimationCurve fadeInEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve fadeOutEase = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	private AudioSource audioSource;

	private bool isPlaying;

	public float GetFadeMultiplier(Playable playable)
	{
		if (fadeDuration <= 0f)
		{
			return 1f;
		}
		double time = playable.GetTime();
		double duration = playable.GetDuration();
		if (time < (double)fadeDuration)
		{
			return fadeInEase.Evaluate(Mathf.Clamp01((float)(time / (double)fadeDuration)));
		}
		if (duration > (double)fadeDuration && time > duration - (double)fadeDuration)
		{
			return fadeOutEase.Evaluate(Mathf.Clamp01((float)((time - duration + (double)fadeDuration) / (double)fadeDuration)));
		}
		return 1f;
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		audioSource = playerData as AudioSource;
		if (!(audioSource == null) && !(clip == null) && !(info.effectiveWeight <= 0f))
		{
			if (!isPlaying)
			{
				audioSource.clip = clip;
				audioSource.loop = loop;
				audioSource.time = Mathf.Clamp((float)playable.GetTime(), 0f, clip.length);
				audioSource.Play();
				isPlaying = true;
			}
			audioSource.volume = Mathf.Clamp01(volume * GetFadeMultiplier(playable) * info.effectiveWeight);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		StopPlayback();
	}

	public override void OnGraphStop(Playable playable)
	{
		StopPlayback();
	}

	private void StopPlayback()
	{
		if (isPlaying && !(audioSource == null))
		{
			audioSource.Stop();
			isPlaying = false;
		}
	}
}
