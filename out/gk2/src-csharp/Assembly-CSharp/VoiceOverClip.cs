using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class VoiceOverClip : PlayableAsset, ITimelineClipAsset
{
	[SerializeField]
	private AudioClip clip;

	[SerializeField]
	private string voiceOverId;

	[SerializeField]
	private VoiceOverBehaviour template = new VoiceOverBehaviour();

	public AudioClip Clip
	{
		get
		{
			return clip;
		}
		set
		{
			clip = value;
		}
	}

	public string VoiceOverId
	{
		get
		{
			return voiceOverId;
		}
		set
		{
			voiceOverId = value;
		}
	}

	public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.Blending;

	public override double duration
	{
		get
		{
			if (clip == null)
			{
				return base.duration;
			}
			return (double)clip.samples / (double)clip.frequency;
		}
	}

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		VoiceOverBehaviour voiceOverBehaviour = (VoiceOverBehaviour)template.Clone();
		voiceOverBehaviour.clip = clip;
		voiceOverBehaviour.voiceOverId = voiceOverId;
		return ScriptPlayable<VoiceOverBehaviour>.Create(graph, voiceOverBehaviour);
	}

	public void ApplyFromClipAsset()
	{
		if (!(clip == null))
		{
			voiceOverId = clip.name;
		}
	}
}
