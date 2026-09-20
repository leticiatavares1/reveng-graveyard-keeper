using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class AudioTrack2Clip : PlayableAsset, ITimelineClipAsset
{
	[SerializeField]
	private AudioClip clip;

	[SerializeField]
	private bool loop;

	[SerializeField]
	private AudioTrack2Behaviour template = new AudioTrack2Behaviour();

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

	public bool Loop
	{
		get
		{
			return loop;
		}
		set
		{
			loop = value;
		}
	}

	public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Blending | (loop ? ClipCaps.Looping : ClipCaps.None);

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
		if (clip == null)
		{
			return Playable.Null;
		}
		AudioTrack2Behaviour audioTrack2Behaviour = (AudioTrack2Behaviour)template.Clone();
		audioTrack2Behaviour.clip = clip;
		audioTrack2Behaviour.loop = loop;
		return ScriptPlayable<AudioTrack2Behaviour>.Create(graph, audioTrack2Behaviour);
	}
}
