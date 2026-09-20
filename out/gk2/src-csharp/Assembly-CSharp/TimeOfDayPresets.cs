using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeOfDayPresets", menuName = "GK2/Light/TimeOfDayPresets")]
public class TimeOfDayPresets : ScriptableObject
{
	[Serializable]
	public struct TimeAndPreset
	{
		[Range(0f, 1f)]
		public float time;

		public LightEnvironmentPreset preset;
	}

	public bool indoorPreset;

	public bool applySfxFromOutdoor;

	public List<TimeAndPreset> presets = new List<TimeAndPreset>();

	public SoundEnvironmentConfig soundEnvironmentConfig;
}
