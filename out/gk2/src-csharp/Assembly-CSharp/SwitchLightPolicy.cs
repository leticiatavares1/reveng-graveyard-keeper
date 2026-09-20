using System;
using LazyBearTechnology;
using UnityEngine;

public static class SwitchLightPolicy
{
	private const float INTENSITY_EPSILON = 0.001f;

	public static bool IsSwitchPlatform
	{
		get
		{
			RuntimePlatform platform = Application.platform;
			return platform == RuntimePlatform.Switch || platform == RuntimePlatform.Switch2;
		}
	}

	public static bool UseLightRT => PlatformFeatures.Current.pointLightMode == PlatformPointLightMode.Faked;

	public static bool AllowRealPointLights => true;

	public static event Action OnLocalLightShadowPolicyChanged;

	public static bool ShouldDisableRealLight(float intensity)
	{
		return intensity < 0.001f;
	}

	public static void ApplyLightRTPolicy()
	{
		LightRTManager.ApplyPolicy();
	}

	public static void ApplyRealLightEnabled(Light light, float intensity)
	{
		if (!(light == null))
		{
			if (TryGetPlatformDependentPointLightEnabled(light, intensity, out var enabled))
			{
				light.enabled = enabled;
			}
			else
			{
				light.enabled = GetRealLightEnabled(intensity);
			}
		}
	}

	public static void NotifyLocalLightShadowPolicyChanged()
	{
		SwitchLightPolicy.OnLocalLightShadowPolicyChanged?.Invoke();
	}

	public static void ApplyLocalLightShadowPolicy(Light light, LightShadows authoredShadows)
	{
		if (!(light == null) && light.type != LightType.Directional)
		{
			LightShadows lightShadows = PlatformUnityShadowPresets.Get(PlatformFeatures.GetActiveUnityShadowPreset()).localLightShadows ?? authoredShadows;
			if (light.shadows != lightShadows)
			{
				light.shadows = lightShadows;
			}
		}
	}

	private static bool GetRealLightEnabled(float intensity)
	{
		if (UseLightRT)
		{
			return false;
		}
		return !ShouldDisableRealLight(intensity);
	}

	private static bool TryGetPlatformDependentPointLightEnabled(Light light, float intensity, out bool enabled)
	{
		enabled = false;
		if (light.type != LightType.Point)
		{
			return false;
		}
		if (!light.TryGetComponent<LazyPlatformDependentElement>(out var component))
		{
			return false;
		}
		if (!component.IsActive)
		{
			return true;
		}
		enabled = GetRealLightEnabled(intensity);
		return true;
	}
}
