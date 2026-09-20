using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlicker : MonoBehaviour
{
	[Serializable]
	public class SpriteFlickerStep
	{
		public int len;

		public int len2;

		public float a;
	}

	public int fps = 30;

	public List<SpriteFlickerStep> steps = new List<SpriteFlickerStep>();

	private float _last_time;

	private float _dt;

	private int _cur_frame_counter;

	private int _cur_step;

	private int _cur_step_len;

	private SpriteRenderer _spr;

	public float trans_time;

	private bool _is_first = true;

	private bool _is_transing;

	private float _trans_t0;

	private float a0;

	private float a1;

	public void Start()
	{
		_last_time = Time.realtimeSinceStartup;
		_dt = 0f;
		_cur_frame_counter = 0;
		_cur_step = 0;
		_spr = GetComponent<SpriteRenderer>();
		_is_first = true;
		GenerateStep();
	}

	private void GenerateStep()
	{
		int count = steps.Count;
		if (count != 0)
		{
			if (_cur_step >= count)
			{
				_cur_step = 0;
			}
			SpriteFlickerStep spriteFlickerStep = steps[_cur_step];
			if (spriteFlickerStep.len2 == 0)
			{
				_cur_step_len = spriteFlickerStep.len;
			}
			else
			{
				_cur_step_len = UnityEngine.Random.Range(spriteFlickerStep.len, spriteFlickerStep.len2);
			}
			_cur_step_len += Mathf.RoundToInt(trans_time * (float)fps);
			_trans_t0 = 0f;
			if (_is_first)
			{
				_spr.color = new Color(_spr.color.r, _spr.color.g, _spr.color.b, spriteFlickerStep.a);
				_is_first = false;
			}
			else
			{
				_is_transing = true;
				a1 = spriteFlickerStep.a;
				a0 = _spr.color.a;
			}
		}
	}

	public void Update()
	{
		if (steps.Count == 0)
		{
			return;
		}
		float num = Time.realtimeSinceStartup - _last_time;
		_trans_t0 += Time.deltaTime;
		if (_is_transing)
		{
			float a = ((trans_time > 0f) ? Mathf.Lerp(a0, a1, _trans_t0 / trans_time) : a1);
			_spr.color = new Color(_spr.color.r, _spr.color.g, _spr.color.b, a);
			if (_trans_t0 > trans_time)
			{
				_is_transing = false;
				_is_first = false;
			}
		}
		_last_time = Time.realtimeSinceStartup;
		_dt += num;
		float num2 = 1f / (float)fps;
		if (_dt > num2)
		{
			_dt -= num2;
			_cur_frame_counter++;
			if (_cur_step >= steps.Count)
			{
				_cur_step = 0;
			}
			else if (_cur_frame_counter >= _cur_step_len)
			{
				_cur_step++;
				_cur_frame_counter = 0;
				GenerateStep();
			}
		}
	}
}
