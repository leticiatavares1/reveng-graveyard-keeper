using UnityEngine;
using UnityEngine.Timeline;

namespace GK2.Timeline;

[TrackColor(0.2f, 0.8f, 0.8f)]
[TrackClipType(typeof(AddWgoDataClip))]
[TrackBindingType(typeof(Transform))]
public class AddWgoDataTrack : TrackAsset
{
}
