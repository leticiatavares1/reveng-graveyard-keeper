using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class VoiceOverBehaviour : PlayableBehaviour
{
	[NonSerialized]
	public AudioClip clip;

	[NonSerialized]
	public string voiceOverId;

	private AudioSource previewSource;

	private bool isPlaying;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (info.effectiveWeight <= 0f)
		{
			return;
		}
		if (Application.isPlaying)
		{
			if (!isPlaying && !string.IsNullOrEmpty(voiceOverId) && LazyAudio.IsInitialized)
			{
				LazyAudio.VoiceOverPlayer.Play(voiceOverId);
				isPlaying = true;
			}
			return;
		}
		previewSource = playerData as AudioSource;
		if (!(previewSource == null) && !(clip == null))
		{
			if (!isPlaying)
			{
				previewSource.clip = clip;
				previewSource.loop = false;
				previewSource.time = Mathf.Clamp((float)playable.GetTime(), 0f, clip.length);
				previewSource.Play();
				isPlaying = true;
			}
			previewSource.volume = Mathf.Clamp01(info.effectiveWeight);
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
		if (isPlaying)
		{
			if (!Application.isPlaying && previewSource != null)
			{
				previewSource.Stop();
			}
			isPlaying = false;
		}
	}
}
