using UnityEngine;
using UnityEngine.Timeline;

namespace GK2.Timeline;

[TrackColor(0.2f, 0.6f, 0.9f)]
[TrackClipType(typeof(AudioTrack2Clip))]
[TrackBindingType(typeof(AudioSource))]
public class AudioTrack2 : TrackAsset
{
	public TimelineClip CreateClip(AudioClip clip)
	{
		if (clip == null)
		{
			return null;
		}
		TimelineClip timelineClip = CreateDefaultClip();
		if (timelineClip.asset is AudioTrack2Clip audioTrack2Clip)
		{
			audioTrack2Clip.Clip = clip;
		}
		timelineClip.duration = clip.length;
		timelineClip.displayName = clip.name;
		return timelineClip;
	}
}
