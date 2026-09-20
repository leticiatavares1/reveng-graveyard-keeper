using System;
using UnityEngine;

[Serializable]
public class WeatherState : MonoBehaviour
{
	public enum WeatherType
	{
		Rain,
		Fog,
		Wind
	}

	public enum State
	{
		Off,
		FadeIn,
		On,
		FadeOut
	}

	public enum SetMethod
	{
		Fade,
		ImmidiateOff,
		ImmidiateOn
	}

	public WeatherType type;

	private State _state;

	private float _time_left;

	private float _time_total = 10f;

	private float _cur_stay_time;

	private const float FADE_IN_TIME = 10f;

	private const float FADE_OUT_TIME = 10f;

	private const float STAY_TIME = 10f;

	private float _amount;

	public float amount => _amount;

	public void StateUpdate(float delta_time)
	{
		_time_left -= delta_time;
		float num = 0f;
		if (_time_total > 0f)
		{
			num = _time_left / _time_total;
		}
		switch (_state)
		{
		case State.Off:
			break;
		case State.FadeIn:
			if (_time_left <= 0f)
			{
				_state = State.On;
				SetWeatherAmount(1f);
				_time_left = (_time_total = _cur_stay_time);
			}
			else
			{
				SetWeatherAmount(1f - num);
			}
			break;
		case State.On:
			if (_time_left <= 0f)
			{
				DoFadeOut(10f);
			}
			break;
		case State.FadeOut:
			if (_time_left <= 0f)
			{
				_state = State.Off;
				SetWeatherAmount(0f);
			}
			else
			{
				SetWeatherAmount(num);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void DoFadeIn(float fade_time)
	{
		_time_left = (_time_total = fade_time);
		_state = State.FadeIn;
	}

	private void DoFadeOut(float fade_time)
	{
		_time_left = (_time_total = fade_time);
		_state = State.FadeOut;
	}

	public void Set(SetMethod method = SetMethod.Fade)
	{
		_cur_stay_time = 10f;
		switch (method)
		{
		case SetMethod.ImmidiateOff:
			SetWeatherAmount(0f);
			_state = State.Off;
			return;
		case SetMethod.ImmidiateOn:
			SetWeatherAmount(1f);
			_state = State.On;
			return;
		}
		switch (_state)
		{
		case State.Off:
			DoFadeIn(10f);
			break;
		case State.FadeIn:
			break;
		case State.On:
			_time_left = 10f;
			break;
		case State.FadeOut:
		{
			float value = _time_left / _time_total;
			value = 1f - Mathf.Clamp01(value);
			DoFadeIn(10f);
			_time_left = 10f * value;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetWeatherAmount(float a)
	{
		_amount = a;
		WeatherAmountDelegate(Mathf.Clamp01(a));
	}

	protected virtual void WeatherAmountDelegate(float a)
	{
	}
}
