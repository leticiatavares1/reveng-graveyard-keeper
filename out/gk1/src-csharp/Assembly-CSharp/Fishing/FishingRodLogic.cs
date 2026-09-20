using UnityEngine;

namespace Fishing;

public class FishingRodLogic
{
	private FishingRodPreset _rod_preset;

	private float _pos;

	private float _max_height;

	private float _screen_k;

	private float _t;

	private float _y0;

	private float _v0;

	private float _v;

	private bool _pulling_fish;

	public float rect_size => _rod_preset.rect_size;

	public FishingRodLogic(FishingRodPreset preset)
	{
		_rod_preset = preset;
		_max_height = 100f - (float)_rod_preset.rect_size;
		_pulling_fish = false;
		_t = (_y0 = (_v0 = (_v = 0f)));
	}

	public float CalculateRodPos()
	{
		bool key = LazyInput.GetKey(GameKey.MiniGameAction);
		if (LazyInput.GetKeyDown(GameKey.MiniGameAction))
		{
			Sounds.PlaySound("fishing_reel_short");
		}
		float num = 0f - _rod_preset.gravity;
		if (key)
		{
			num += _rod_preset.force / _rod_preset.mass;
			if (!_pulling_fish)
			{
				_v += _rod_preset.impulse / _rod_preset.mass;
			}
		}
		if (_pos > 0f)
		{
			_v += num * Time.deltaTime;
		}
		_pos += _v * Time.deltaTime;
		if (_pos > _max_height)
		{
			_v = 0f;
			_pos = _max_height;
		}
		else if (_pos < 0f)
		{
			_pos = 0f;
			_v = (0f - _v) / 4f;
			if (Mathf.Abs(_v) < 0.05f * _rod_preset.impulse / _rod_preset.mass)
			{
				_v = 0f;
			}
		}
		_pulling_fish = key;
		return _pos;
	}
}
