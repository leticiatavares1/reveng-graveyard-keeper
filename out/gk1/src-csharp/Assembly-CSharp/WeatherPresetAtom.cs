using System;
using UnityEngine;

[Serializable]
public class WeatherPresetAtom
{
	public SmartWeatherState.WeatherType type;

	public Texture2D lut_texture;

	public float value;

	public float t_atk_in_secs;

	public float t_atk => TimeOfDay.FromSecondsToTimeK(t_atk_in_secs);

	public override string ToString()
	{
		return type.ToString() + "=" + value + "(" + t_atk_in_secs + " sec)";
	}
}
