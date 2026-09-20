using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class EnvironmentControlClip : PlayableAsset, ITimelineClipAsset
{
	public EnvironmentControlBehaviour template = new EnvironmentControlBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<EnvironmentControlBehaviour>.Create(graph, template);
	}
}
