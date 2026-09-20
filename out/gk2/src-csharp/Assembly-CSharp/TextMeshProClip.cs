using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TextMeshProClip : PlayableAsset, ITimelineClipAsset
{
	public TextMeshProBehaviour template = new TextMeshProBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<TextMeshProBehaviour>.Create(graph, template);
	}
}
