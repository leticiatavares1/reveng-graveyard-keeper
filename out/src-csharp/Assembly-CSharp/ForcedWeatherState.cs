using System;
using UnityEngine;

[Serializable]
public class ForcedWeatherState : WeatherStateBase
{
	public float t_flat;

	public float t_dec;

	public ForcedWeatherState(SmartWeatherState.WeatherType type, Texture2D lut_texture, float value, float t_start, float t_atk, float t_flat, float t_dec)
	{
		base.type = type;
		base.lut_texture = lut_texture;
		base.value = value;
		base.t_start = t_start;
		base.t_atk = t_atk;
		this.t_flat = t_flat;
		this.t_dec = t_dec;
	}
}
