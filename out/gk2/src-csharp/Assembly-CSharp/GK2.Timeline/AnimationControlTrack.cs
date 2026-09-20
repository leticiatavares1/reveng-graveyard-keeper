using UnityEngine.Timeline;

namespace GK2.Timeline;

[TrackColor(0.9f, 0.5f, 0.1f)]
[TrackClipType(typeof(AnimationControlClip))]
[TrackBindingType(typeof(TimelineAnimator))]
public class AnimationControlTrack : TrackAsset
{
}
