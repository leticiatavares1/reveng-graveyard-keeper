using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class WorldFXSpawnClip : PlayableAsset, ITimelineClipAsset
{
	public WorldFXSpawnBehaviour template = new WorldFXSpawnBehaviour();

	public override double duration => 0.0;

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<WorldFXSpawnBehaviour>.Create(graph, template);
	}
}
