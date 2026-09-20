using System;
using UnityEngine;

public class SmartWeatherState : MonoBehaviour
{
	[Serializable]
	public enum WeatherType
	{
		Rain,
		Fog,
		Wind,
		LUT
	}

	[Serializable]
	public struct SerializedWeatherState
	{
		public string name;

		public float controller_value;

		public float value;

		public float forced_value;

		public float nature_value;

		public float cur_amount;

		public float speed;

		public float max;

		public WeatherType type;

		public bool enabled;

		public bool previously_enabled;
	}

	public SmartController controller;

	[Range(0f, 5f)]
	public float value;

	[Range(0f, 5f)]
	public float forced_value;

	public float nature_value;

	[SerializeField]
	private float _cur_amount;

	public float speed = 0.25f;

	public float max = 5f;

	public WeatherType type;

	[SerializeField]
	private bool _enabled = true;

	[SerializeField]
	private bool _previously_enabled = true;

	public void Update()
	{
		if (controller == null)
		{
			return;
		}
		if (value > max)
		{
			value = max;
		}
		float f = value - _cur_amount;
		int num = (int)Mathf.Sign(f);
		if ((double)Mathf.Abs(f) < 0.01)
		{
			if (_previously_enabled == _enabled)
			{
				return;
			}
			num = 0;
		}
		float num2 = speed * Time.deltaTime;
		_cur_amount += num2 * (float)num;
		float f2 = value - _cur_amount;
		if (num != (int)Mathf.Sign(f2))
		{
			_cur_amount = value;
		}
		_previously_enabled = _enabled;
		if (!_enabled)
		{
			controller.value = 0f;
			UpdateWeatherVolume(controller.value);
		}
		else
		{
			controller.value = _cur_amount;
			UpdateWeatherVolume(controller.value);
		}
	}

	public void SetValueImmediate(float v)
	{
		controller.value = (_cur_amount = (value = v));
		UpdateWeatherVolume(controller.value);
	}

	public void SetEnabled(bool state_enabled)
	{
		_enabled = state_enabled;
		SetWeatherSoundEnable(_enabled);
		Update();
	}

	public SerializedWeatherState Serialize()
	{
		SerializedWeatherState result = default(SerializedWeatherState);
		result.value = value;
		result.name = base.name;
		result.type = type;
		result.enabled = _enabled;
		result.max = max;
		result.controller_value = controller.value;
		result.cur_amount = _cur_amount;
		result.forced_value = forced_value;
		result.nature_value = nature_value;
		result.previously_enabled = _previously_enabled;
		result.speed = speed;
		return result;
	}

	public void Deserialize(SerializedWeatherState state)
	{
		value = state.value;
		base.name = state.name;
		type = state.type;
		_enabled = state.enabled;
		max = state.max;
		controller.value = state.controller_value;
		_cur_amount = state.cur_amount;
		forced_value = state.forced_value;
		nature_value = state.nature_value;
		_previously_enabled = state.previously_enabled;
		speed = state.speed;
	}

	private void SetWeatherSoundEnable(bool is_enable)
	{
		string weatherMusicId = GetWeatherMusicId();
		if (!string.IsNullOrEmpty(weatherMusicId))
		{
			if (is_enable)
			{
				SmartAudioEngine.me.SetSoundVolume(weatherMusicId, 0f);
				SmartAudioEngine.me.SetSoundWeight(weatherMusicId, 1f);
				SmartAudioEngine.me.PlaySound(weatherMusicId);
			}
			else
			{
				SmartAudioEngine.me.StopSoundWithFade(weatherMusicId);
			}
		}
	}

	private void UpdateWeatherVolume(float weather_value)
	{
		if (!_enabled)
		{
			return;
		}
		string weatherMusicId = GetWeatherMusicId();
		float num = 0f;
		if (!string.IsNullOrEmpty(weatherMusicId))
		{
			WeatherType weatherType = type;
			if (weatherType == WeatherType.Rain || weatherType == WeatherType.Wind)
			{
				num = weather_value / max;
				SmartAudioEngine.me.SetSoundVolume(weatherMusicId, num);
			}
		}
	}

	private string GetWeatherMusicId()
	{
		string result = string.Empty;
		switch (type)
		{
		case WeatherType.Rain:
			result = "rain_environment";
			break;
		case WeatherType.Wind:
			result = "wind_environment";
			break;
		}
		return result;
	}
}
