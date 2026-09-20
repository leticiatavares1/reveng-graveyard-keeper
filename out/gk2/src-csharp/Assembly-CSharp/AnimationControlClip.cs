using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class AnimationControlClip : PlayableAsset, ITimelineClipAsset
{
	public AnimationControlBehaviour template = new AnimationControlBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<AnimationControlBehaviour>.Create(graph, template);
	}
}
