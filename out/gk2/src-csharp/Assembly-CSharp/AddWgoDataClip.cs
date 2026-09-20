using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class AddWgoDataClip : PlayableAsset, ITimelineClipAsset
{
	public AddWgoDataBehaviour template = new AddWgoDataBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<AddWgoDataBehaviour>.Create(graph, template);
	}
}
