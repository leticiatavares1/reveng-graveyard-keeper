using UnityEngine;
using UnityEngine.Timeline;

namespace GK2.Timeline;

[TrackColor(0.85f, 0.45f, 0.2f)]
[TrackClipType(typeof(VoiceOverClip))]
[TrackBindingType(typeof(AudioSource))]
public class VoiceOverTrack : TrackAsset
{
	protected override void OnCreateClip(TimelineClip clip)
	{
		base.OnCreateClip(clip);
		SyncClipFromAudio(clip);
	}

	public TimelineClip CreateClip(AudioClip audioClip)
	{
		if (audioClip == null)
		{
			return null;
		}
		TimelineClip timelineClip = CreateDefaultClip();
		if (timelineClip.asset is VoiceOverClip voiceOverClip)
		{
			voiceOverClip.Clip = audioClip;
			ApplyAudioToTimelineClip(timelineClip, voiceOverClip);
		}
		return timelineClip;
	}

	public static void SyncClipFromAudio(TimelineClip timelineClip)
	{
		if (timelineClip?.asset is VoiceOverClip voiceOverClip && !(voiceOverClip.Clip == null))
		{
			ApplyAudioToTimelineClip(timelineClip, voiceOverClip);
		}
	}

	public static void ApplyAudioToTimelineClip(TimelineClip timelineClip, VoiceOverClip voiceOverClip)
	{
		voiceOverClip.ApplyFromClipAsset();
		timelineClip.duration = voiceOverClip.Clip.length;
		timelineClip.displayName = voiceOverClip.VoiceOverId;
	}
}
