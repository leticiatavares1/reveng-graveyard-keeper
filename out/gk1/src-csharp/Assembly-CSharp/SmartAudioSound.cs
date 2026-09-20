using System;
using DarkTonic.MasterAudio;
using UnityEngine;

[Serializable]
public class SmartAudioSound
{
	public enum TimeLimitation
	{
		AllDayLong,
		OnlyAtNight,
		OnlyAtDay
	}

	public string id;

	[SoundGroup]
	public string sound_group = "[None]";

	public TimeLimitation time_limitation;

	private float _cur_weight = 1f;

	private float _target_weight;

	private float _volume = 1f;

	private bool _weight_animating;

	public float fade_speed = 0.3f;

	public float fade_out_speed_k = 2f;

	public void SetSoundWeight(float weight, bool is_weight_animate = true)
	{
		_target_weight = weight;
		_weight_animating = is_weight_animate;
	}

	public void CustomUpdate(float delta_time)
	{
		if (_weight_animating)
		{
			float num = _target_weight - _cur_weight;
			float num2 = delta_time * fade_speed;
			if (num < 0f)
			{
				num2 *= fade_out_speed_k;
			}
			if (Mathf.Abs(num) < num2)
			{
				_cur_weight = _target_weight;
				_weight_animating = false;
			}
			else
			{
				_cur_weight += num2 * Mathf.Sign(num);
				if ((int)Mathf.Sign(_target_weight - _cur_weight) != (int)Mathf.Sign(num))
				{
					_cur_weight = _target_weight;
					_weight_animating = false;
				}
			}
		}
		else
		{
			_cur_weight = _target_weight;
		}
		OnWeightChanged();
	}

	private void OnWeightChanged()
	{
		float num = 1f;
		float num2 = Mathf.Abs(TimeOfDay.me.time_of_day);
		switch (time_limitation)
		{
		case TimeLimitation.OnlyAtNight:
			num *= num2;
			break;
		case TimeLimitation.OnlyAtDay:
			num *= 1f - num2;
			break;
		}
		if (!(sound_group == "[None]"))
		{
			MasterAudio.SetGroupVolume(sound_group, _cur_weight * num * _volume);
		}
	}

	public void Play()
	{
		MasterAudio.PlaySound(sound_group);
	}

	public void PlayWithFade()
	{
		MasterAudio.PlaySound(sound_group);
		SetSoundWeight(1f);
	}

	public void StopWithFade()
	{
		SetSoundWeight(0f);
	}

	public void Stop()
	{
		SetSoundWeight(0f, is_weight_animate: false);
	}

	public void SetSoundVolume(float volume)
	{
		_volume = volume;
		CustomUpdate(Time.deltaTime);
	}
}
