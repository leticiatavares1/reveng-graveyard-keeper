using System;
using UnityEngine;

[ExecuteAlways]
public class DayNightLight : DayNightLightBase
{
	public Action<float> OnLightIntensityChanged;

	public float intensity = 1f;

	private Light light;

	private Func<float> lightFunc;

	private LightShadows authoredShadows;

	private bool authoredShadowsCached;

	private float NightLight()
	{
		return Mathf.Lerp(intensity, 0f, Shader.GetGlobalFloat(GlobalShaderParameters.idSunLight));
	}

	private float DayLight()
	{
		return Mathf.Lerp(0f, intensity, Shader.GetGlobalFloat(GlobalShaderParameters.idSunLight));
	}

	private float StaticLight()
	{
		return intensity;
	}

	private void Awake()
	{
		light = GetComponent<Light>();
		CacheAuthoredShadows();
		ApplyLightMode(mode);
	}

	private void OnEnable()
	{
		if (light == null)
		{
			light = GetComponent<Light>();
		}
		CacheAuthoredShadows();
		if (Application.isPlaying)
		{
			SwitchLightPolicy.OnLocalLightShadowPolicyChanged += ApplyLocalShadowPolicy;
			ApplyLocalShadowPolicy();
		}
		ApplyLightMode(mode);
	}

	private void OnDisable()
	{
		SwitchLightPolicy.OnLocalLightShadowPolicyChanged -= ApplyLocalShadowPolicy;
		RestoreAuthoredShadowsIfNeeded();
	}

	private void RestoreAuthoredShadowsIfNeeded()
	{
		if (authoredShadowsCached && !(light == null) && (!base.gameObject.activeInHierarchy || !PlatformUnityShadowPresets.Get(PlatformFeatures.GetActiveUnityShadowPreset()).localLightShadows.HasValue))
		{
			light.shadows = authoredShadows;
		}
	}

	private void Update()
	{
		if (lightFunc != null && (bool)light)
		{
			float num = lightFunc();
			SwitchLightPolicy.ApplyRealLightEnabled(light, num);
			if (Mathf.Abs(light.intensity - num) > 0.0001f)
			{
				light.intensity = num;
			}
			OnLightIntensityChanged?.Invoke(num / intensity);
		}
	}

	protected override void ApplyLightMode(LightMode mode)
	{
		base.mode = mode;
		switch (mode)
		{
		case LightMode.Night:
			lightFunc = NightLight;
			break;
		case LightMode.Day:
			lightFunc = DayLight;
			break;
		case LightMode.Static:
			lightFunc = StaticLight;
			break;
		}
		Update();
		SyncLightFakerMode();
	}

	private void CacheAuthoredShadows()
	{
		if (!authoredShadowsCached && !(light == null))
		{
			authoredShadows = light.shadows;
			authoredShadowsCached = true;
		}
	}

	private void ApplyLocalShadowPolicy()
	{
		if (!(light == null))
		{
			SwitchLightPolicy.ApplyLocalLightShadowPolicy(light, authoredShadows);
		}
	}

	private void SyncLightFakerMode()
	{
		if (TryGetComponent<LightFaker>(out var component) && component.mode != mode)
		{
			component.ApplyLightModeInt((int)mode);
		}
	}
}
