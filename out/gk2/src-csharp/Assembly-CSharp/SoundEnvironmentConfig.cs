using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEnvironmentConfig", menuName = "GK2/Sound/SoundEnvironmentConfig")]
public class SoundEnvironmentConfig : ScriptableObject
{
	[Serializable]
	public struct SoundAndTime
	{
		public string soundId;

		[Range(0f, 1f)]
		public float time;
	}

	public List<SoundAndTime> sounds = new List<SoundAndTime>();

	[Tooltip("Duration (in real-time seconds) of the volume crossfade at the end of each segment, right before switching to the next sound. Outside this window only the current sound is audible. 0 = no crossfade window; the previous sound is released with the standard fade-out at the boundary.")]
	[Min(0f)]
	public float crossfadeSeconds = 5f;

	public float GetCrossfadeDuration01(float secondsPerCycle)
	{
		if (!(secondsPerCycle > 0f))
		{
			return 0f;
		}
		return crossfadeSeconds / secondsPerCycle;
	}

	public static void FindPair(List<SoundAndTime> sounds, float timeOfDay, float crossfadeDuration, out string id1, out string id2, out float lerp)
	{
		id1 = null;
		id2 = null;
		lerp = 0f;
		if (sounds == null || sounds.Count == 0)
		{
			return;
		}
		for (int i = 0; i < sounds.Count; i++)
		{
			SoundAndTime soundAndTime = sounds[i];
			if (soundAndTime.time == timeOfDay)
			{
				id1 = (id2 = soundAndTime.soundId);
				lerp = 0f;
				break;
			}
			if (!(timeOfDay > soundAndTime.time))
			{
				continue;
			}
			id1 = soundAndTime.soundId;
			if (i + 1 >= sounds.Count)
			{
				Debug.LogError("Error picking a sound environment entry. Probably, last sound time is less than 1.0");
				id2 = id1;
				lerp = 0f;
				break;
			}
			SoundAndTime soundAndTime2 = sounds[i + 1];
			if (!(timeOfDay <= soundAndTime2.time))
			{
				continue;
			}
			id2 = soundAndTime2.soundId;
			float num = soundAndTime2.time - soundAndTime.time;
			if (num <= 0f)
			{
				lerp = 0f;
				break;
			}
			float num2 = Mathf.Min(crossfadeDuration, num);
			if (num2 <= 0f)
			{
				lerp = 0f;
				break;
			}
			float num3 = soundAndTime2.time - num2;
			lerp = Mathf.Clamp01((timeOfDay - num3) / num2);
			break;
		}
	}

	public static string FindActiveSegment(List<SoundAndTime> sounds, float timeOfDay)
	{
		if (sounds == null || sounds.Count == 0)
		{
			return null;
		}
		string result = null;
		for (int i = 0; i < sounds.Count && sounds[i].time <= timeOfDay; i++)
		{
			result = sounds[i].soundId;
		}
		return result;
	}
}
