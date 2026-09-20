using UnityEngine;
using UnityEngine.Timeline;

namespace GK2.Timeline;

[TrackColor(0.4f, 0.8f, 0.4f)]
[TrackClipType(typeof(WorldFXSpawnClip))]
[TrackBindingType(typeof(Transform))]
public class WorldFXSpawnTrack : TrackAsset
{
}
