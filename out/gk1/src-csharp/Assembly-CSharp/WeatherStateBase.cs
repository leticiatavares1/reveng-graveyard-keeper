using System;
using UnityEngine;

[Serializable]
public class WeatherStateBase
{
	public float t_start;

	public float t_atk;

	public SmartWeatherState.WeatherType type;

	public float value;

	private Texture2D _lut_texture;

	[SerializeField]
	private string _lut_texture_name;

	private string _cached_lut_texture_name = string.Empty;

	public Texture2D lut_texture
	{
		get
		{
			if (_cached_lut_texture_name != _lut_texture_name)
			{
				_cached_lut_texture_name = _lut_texture_name;
				_lut_texture = (string.IsNullOrEmpty(_lut_texture_name) ? null : Resources.Load<Texture2D>(_lut_texture_name));
			}
			return _lut_texture;
		}
		set
		{
			_lut_texture = value;
			_cached_lut_texture_name = (_lut_texture_name = ((value == null) ? string.Empty : value.name));
		}
	}
}
