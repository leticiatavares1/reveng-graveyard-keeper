using System;
using System.Collections.Generic;
using LazyBearTechnology;
using PI.NGSS;
using UnityEngine;

[Serializable]
public class CPDirectShadowsBlur : ControllableParameter
{
	[Range(0f, 3f)]
	public float additionalBlur = 2f;

	private static Dictionary<CPDirectShadowsBlur, float> values = new Dictionary<CPDirectShadowsBlur, float>();

	private static float initialBlurValue;

	private static bool ngssInitialized = false;

	private static NGSS_Directional ngssDirectional;

	private static NGSS_Directional NgssDirectional
	{
		get
		{
			if (!ngssDirectional)
			{
				ngssDirectional = UnityEngine.Object.FindFirstObjectByType<NGSS_Directional>();
				if (!ngssDirectional)
				{
					Debug.LogError("No NGSS Directional component found.");
					return null;
				}
				initialBlurValue = ngssDirectional.NGSS_PCSS_SOFTNESS_NEAR;
			}
			if (!ngssInitialized)
			{
				Initialize();
			}
			return ngssDirectional;
		}
	}

	public static void Initialize()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		ngssInitialized = true;
		if (!LightsSystem.Instance.NgssFeatureEnabled)
		{
			return;
		}
		NGSS_Directional ngss = NgssDirectional;
		if (!(ngss != null))
		{
			return;
		}
		ngss.enabled = false;
		LazyTimer.AddTimer(0f, delegate
		{
			if ((bool)ngss)
			{
				ngss.enabled = true;
			}
		});
	}

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		if (!(NgssDirectional == null))
		{
			float value = v * additionalBlur;
			values[this] = value;
		}
	}

	public static void ApplyParameters()
	{
		if (NgssDirectional == null)
		{
			return;
		}
		_ = NgssDirectional.NGSS_PCSS_SOFTNESS_NEAR;
		float num = 0f;
		foreach (float value in values.Values)
		{
			num = Mathf.Max(num, value);
		}
		NgssDirectional.NGSS_PCSS_SOFTNESS_NEAR = initialBlurValue + num;
		NgssDirectional.UpdateParameters();
	}
}
