using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TimeOfDay : MonoBehaviour
{
	public enum TimeOfDayEnum
	{
		Night,
		Morning,
		Day,
		Evening
	}

	[Serializable]
	public class SerializedTimeOfDay
	{
		public float time_of_day;

		public float prev_time_of_day;
	}

	private readonly Color WARMING_LIGHT = new Color(0.09671455f, 0.06246148f, 0f);

	public const float MORNING = 0.15f;

	public const float DAYTIME = 0.35f;

	public const float EVENING = 0.7f;

	public const float NIGHT = 0f;

	public Gradient light_grd = new Gradient();

	public Gradient ambient_grd = new Gradient();

	public Gradient light_sprites = new Gradient();

	public AnimationCurve lut_amount = new AnimationCurve();

	public AnimationCurve shadow_alpha = new AnimationCurve();

	public AnimationCurve light_intensity = new AnimationCurve();

	public AnimationCurve grain_intensity = new AnimationCurve();

	public static Color light_sprites_color = Color.white;

	public List<float> lut_times = new List<float>();

	public List<Texture> lut_textures = new List<Texture>();

	[NonSerialized]
	private static TimeOfDay _me = null;

	[NonSerialized]
	private static bool _me_is_set = false;

	[Range(-1f, 1f)]
	public float time_of_day;

	private float _prev_time_of_day;

	public static float shadow_alpha_k = 1f;

	public static float light_intensity_k = 1f;

	public static float global_shadows_alpha = 1f;

	public const float SECONDS_IN_DAY = 450f;

	public TimeOfDayEnum time_of_day_enum
	{
		get
		{
			float timeK = GetTimeK();
			if (timeK < 0.15f)
			{
				return TimeOfDayEnum.Night;
			}
			if (timeK < 0.35f)
			{
				return TimeOfDayEnum.Morning;
			}
			if (timeK < 0.7f)
			{
				return TimeOfDayEnum.Day;
			}
			return TimeOfDayEnum.Evening;
		}
	}

	public bool is_night
	{
		get
		{
			float timeK = GetTimeK();
			if (!(timeK < 0.25f))
			{
				return timeK > 0.75f;
			}
			return true;
		}
	}

	public static TimeOfDay me
	{
		get
		{
			if (_me_is_set)
			{
				return _me;
			}
			_me = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
			_me_is_set = true;
			return _me;
		}
	}

	protected AmplifyColorEffect lut_effect => EnvironmentEngine.me.lut_effect_timeofday;

	public float GetTimeK()
	{
		return (time_of_day + 1f) / 2f;
	}

	public void SetTimeK(float k)
	{
		time_of_day = k * 2f - 1f;
		EnvironmentEngine.SetTime(time_of_day);
	}

	public void Update()
	{
		if (EnvironmentEngine.me == null || !MainGame.game_started)
		{
			return;
		}
		float num = GetTimeK();
		global_shadows_alpha = 1f;
		Gradient gradient = light_grd;
		Gradient gradient2 = ambient_grd;
		Gradient gradient3 = light_sprites;
		EnvironmentPreset cur_preset = EnvironmentEngine.cur_preset;
		if (cur_preset != null)
		{
			if (cur_preset.light_override)
			{
				gradient = cur_preset.light_grd;
			}
			if (cur_preset.ambient_light_override)
			{
				gradient2 = cur_preset.ambient_grd;
			}
			if (cur_preset.light_sprites_override)
			{
				gradient3 = cur_preset.light_sprites;
			}
			if (cur_preset.force_static_time)
			{
				num = cur_preset.static_time_value;
			}
			if (cur_preset.force_global_shadows_alpha)
			{
				global_shadows_alpha = cur_preset.global_shadows_alpha;
			}
		}
		gradient.Evaluate(num);
		Color color = gradient2.Evaluate(num);
		light_sprites_color = gradient3.Evaluate(num);
		RenderSettings.ambientLight = color + WARMING_LIGHT;
		shadow_alpha_k = shadow_alpha.Evaluate(num);
		light_intensity_k = light_intensity.Evaluate(num);
		if (Application.isPlaying && MainGame.me.grain_fx_component != null)
		{
			MainGame.me.grain_fx_component.intensityMultiplier = grain_intensity.Evaluate((cur_preset != null && cur_preset.force_light_intensity) ? 0.5f : num);
		}
		if (cur_preset != null)
		{
			if (cur_preset.force_shadows_alpha)
			{
				shadow_alpha_k = cur_preset.shadows_alpha;
			}
			if (cur_preset.force_light_intensity)
			{
				light_intensity_k = cur_preset.light_intensity;
			}
		}
		if (lut_effect != null)
		{
			CalculateLutBlend(num, out lut_effect.BlendAmount, out lut_effect.LutTexture, out lut_effect.LutBlendTexture);
		}
	}

	private void CalculateLutBlend(float time_k, out float blend, out Texture t1, out Texture t2)
	{
		blend = 0f;
		t1 = (t2 = null);
		if (lut_times.Count == 0)
		{
			return;
		}
		for (int i = 0; i < lut_times.Count; i++)
		{
			float num = lut_times[i];
			t1 = lut_textures[i];
			float num2;
			if (i == lut_times.Count - 1)
			{
				num2 = 1f;
				t2 = lut_textures[0];
			}
			else
			{
				num2 = lut_times[i + 1];
				t2 = lut_textures[i + 1];
			}
			if (!((double)Math.Abs(num2 - num) < 0.001) && (double)time_k >= (double)num - 0.001 && time_k < num2)
			{
				if ((double)Math.Abs(time_k - num) < 0.001)
				{
					blend = 0f;
				}
				else
				{
					blend = (time_k - num) / (num2 - num);
				}
				return;
			}
		}
		blend = 1f;
	}

	public static float FromTimeKToSeconds(float time_in_time_k)
	{
		return time_in_time_k * 450f;
	}

	public static float FromSecondsToTimeK(float time_in_secs)
	{
		return time_in_secs / 450f;
	}

	public SerializedTimeOfDay ToSerialized()
	{
		return new SerializedTimeOfDay
		{
			time_of_day = time_of_day,
			prev_time_of_day = _prev_time_of_day
		};
	}

	public void FromSerialized(SerializedTimeOfDay data)
	{
		if (data != null)
		{
			time_of_day = data.time_of_day;
			_prev_time_of_day = data.prev_time_of_day;
		}
	}

	public float GetSecondsToTheMidnight()
	{
		return (1f - GetTimeK()) * 450f;
	}

	public float GetSecondsToTheMorning()
	{
		float num = GetTimeK() - 0.15f;
		if (num < 0f)
		{
			return num * -1f * 450f;
		}
		return (1f - GetTimeK() + 0.15f) * 450f;
	}
}
