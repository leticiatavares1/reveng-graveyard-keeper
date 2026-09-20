using UnityEngine;

public readonly struct PlatformUnityShadowSettings
{
	public readonly ShadowQuality shadowQuality;

	public readonly ShadowResolution shadowResolution;

	public readonly float shadowDistance;

	public readonly int shadowCascades;

	public readonly LightShadows sunLightShadows;

	public readonly int pixelLightCount;

	public readonly LightShadows? localLightShadows;

	public bool IsDisabled => shadowQuality == ShadowQuality.Disable;

	public PlatformUnityShadowSettings(ShadowQuality shadowQuality, ShadowResolution shadowResolution, float shadowDistance, int shadowCascades, LightShadows sunLightShadows, int pixelLightCount, LightShadows? localLightShadows = null)
	{
		this.shadowQuality = shadowQuality;
		this.shadowResolution = shadowResolution;
		this.shadowDistance = shadowDistance;
		this.shadowCascades = shadowCascades;
		this.sunLightShadows = sunLightShadows;
		this.pixelLightCount = pixelLightCount;
		this.localLightShadows = localLightShadows;
	}

	public void Apply(Light sunLight)
	{
		if (IsDisabled)
		{
			QualitySettings.shadows = ShadowQuality.Disable;
			if (sunLight != null)
			{
				sunLight.shadows = LightShadows.None;
			}
			return;
		}
		QualitySettings.shadows = shadowQuality;
		QualitySettings.shadowResolution = shadowResolution;
		QualitySettings.shadowDistance = shadowDistance;
		QualitySettings.shadowCascades = shadowCascades;
		QualitySettings.pixelLightCount = pixelLightCount;
		if (sunLight != null)
		{
			sunLight.shadows = sunLightShadows;
		}
	}
}
