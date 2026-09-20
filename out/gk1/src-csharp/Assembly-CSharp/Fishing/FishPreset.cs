using UnityEngine;

namespace Fishing;

[CreateAssetMenu(fileName = "FishPreset", menuName = "Mini Games/Fish Preset", order = 1)]
public class FishPreset : ScriptableObject
{
	public enum BehaviourType
	{
		Default,
		FastDown,
		FastUp
	}

	public float catch_time = 0.8f;

	public float progress_k_in_zone = 3f;

	public float progress_k_out_of_zone = 1.5f;

	public float zero_progress_fail_time = 1f;

	private float _t;

	private float _ta;

	private float _tb;

	[Header("Curve setup")]
	[Space(10f)]
	public BehaviourType behaviour_type;

	public float drop_k = 1f;

	public float frequency = 1f;

	[Space(10f)]
	[Header("Amplitude setup")]
	public float amplitude = 1f;

	[Range(0f, 10f)]
	public float amp_offset_x;

	[Range(-1f, 1f)]
	public float amp_offset_y;

	[Range(0f, 10f)]
	public float amp_freq = 1f;

	[Range(0f, 10f)]
	public float amp_mod;

	[Range(0f, 10f)]
	[Space(10f)]
	public float amp_offset_2_x;

	[Range(-1f, 1f)]
	public float amp_offset_2_y;

	[Range(0f, 10f)]
	public float amp_freq_2 = 1f;

	[Range(0f, 10f)]
	public float amp_mod_2;

	[Space(10f)]
	[Header("View")]
	public float inspector_period = 1f;

	public float inspector_shift;

	public float max_amp = 1f;

	public void InitTimeCalculation(float shift = 0f)
	{
		_ta = shift;
		_tb = shift;
		_t = shift;
	}

	public float CalculateFishPos(float delta_time, bool normalize = false)
	{
		_t += delta_time * frequency;
		_ta += delta_time * amp_freq * frequency;
		_tb += delta_time * amp_freq_2 * frequency;
		float num = Mathf.Sin(_t) * amplitude + (amp_offset_y + Mathf.Sin(_ta + amp_offset_x) * amp_mod) + (amp_offset_2_y + Mathf.Sin(_tb + amp_offset_2_x) * amp_mod_2);
		if (Mathf.Abs(num) > max_amp)
		{
			max_amp = Mathf.Abs(num);
		}
		if (normalize)
		{
			num = (num / max_amp + 1f) / 2f;
		}
		return num;
	}
}
