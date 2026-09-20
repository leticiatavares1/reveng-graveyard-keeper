using System;
using UnityEngine;

namespace DarkTonic.MasterAudio;

[Serializable]
public class MusicSetting
{
	public string alias = string.Empty;

	public MasterAudio.AudioLocation audLocation;

	public AudioClip clip;

	public string songName = string.Empty;

	public string resourceFileName = string.Empty;

	public float volume = 1f;

	public float pitch = 1f;

	public bool isExpanded = true;

	public bool isLoop;

	public MasterAudio.CustomSongStartTimeMode songStartTimeMode;

	public float customStartTime;

	public float customStartTimeMax;

	public int lastKnownTimePoint;

	public bool wasLastKnownTimePointSet;

	public int songIndex;

	public bool songStartedEventExpanded;

	public string songStartedCustomEvent = string.Empty;

	public bool songChangedEventExpanded;

	public string songChangedCustomEvent = string.Empty;

	public float SongStartTime => songStartTimeMode switch
	{
		MasterAudio.CustomSongStartTimeMode.SpecificTime => customStartTime, 
		MasterAudio.CustomSongStartTimeMode.RandomTime => UnityEngine.Random.Range(customStartTime, customStartTimeMax), 
		_ => 0f, 
	};

	public MusicSetting()
	{
		songChangedEventExpanded = false;
	}

	public static MusicSetting Clone(MusicSetting mus)
	{
		return new MusicSetting
		{
			alias = mus.alias,
			audLocation = mus.audLocation,
			clip = mus.clip,
			songName = mus.songName,
			resourceFileName = mus.resourceFileName,
			volume = mus.volume,
			pitch = mus.pitch,
			isExpanded = mus.isExpanded,
			isLoop = mus.isLoop,
			customStartTime = mus.customStartTime,
			songStartedEventExpanded = mus.songStartedEventExpanded,
			songStartedCustomEvent = mus.songStartedCustomEvent,
			songChangedEventExpanded = mus.songChangedEventExpanded,
			songChangedCustomEvent = mus.songChangedCustomEvent
		};
	}
}
