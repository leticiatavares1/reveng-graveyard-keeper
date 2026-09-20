using System;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	[Serializable]
	public class LightFlickerStep
	{
		public int len;

		public int len2;

		public Color c;
	}

	public int fps = 30;

	public List<LightFlickerStep> steps = new List<LightFlickerStep>();

	private float _last_time;

	private float _dt;

	private int _cur_frame_counter;

	private int _cur_step;

	private int _cur_step_len;

	private Light _light;

	public float trans_time;

	private bool _is_first = true;

	private bool _is_transing;

	private float _trans_t0;

	private Color c0;

	private Color c1;

	public void Start()
	{
		_last_time = Time.realtimeSinceStartup;
		_dt = 0f;
		_cur_frame_counter = 0;
		_cur_step = 0;
		_light = GetComponent<Light>();
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
			LightFlickerStep lightFlickerStep = steps[_cur_step];
			if (lightFlickerStep.len2 == 0)
			{
				_cur_step_len = lightFlickerStep.len;
			}
			else
			{
				_cur_step_len = UnityEngine.Random.Range(lightFlickerStep.len, lightFlickerStep.len2);
			}
			_cur_step_len += Mathf.RoundToInt(trans_time * (float)fps);
			_trans_t0 = 0f;
			if (_is_first)
			{
				_light.color = lightFlickerStep.c;
				_is_first = false;
			}
			else
			{
				_is_transing = true;
				c1 = lightFlickerStep.c;
				c0 = _light.color;
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
			float t = _trans_t0 / trans_time;
			_light.color = new Color(Mathf.Lerp(c0.r, c1.r, t), Mathf.Lerp(c0.g, c1.g, t), Mathf.Lerp(c0.b, c1.b, t));
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
