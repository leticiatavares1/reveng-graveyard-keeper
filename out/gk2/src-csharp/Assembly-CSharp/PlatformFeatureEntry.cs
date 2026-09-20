using System;

[Serializable]
public class PlatformFeatureEntry
{
	public const int DefaultAudioCompression = 100;

	public GamePlatform platform;

	public PlatformShadowMode shadowMode = PlatformShadowMode.NGSS;

	public PlatformCloudAppearance cloudAppearance;

	public PlatformUnityShadowPreset unityShadowPreset = PlatformUnityShadowPreset.Balanced;

	public PlatformPointLightMode pointLightMode;

	public bool backLightEnabled = true;

	public PlatformWaterTier waterTier;

	public PlatformRenderMode renderMode;

	public PlatformHBAOQuality hbaoQuality = PlatformHBAOQuality.Medium;

	public NgssQualityPreset ngssQuality;

	public PlatformTextureCompression textureCompression;

	public int audioCompression = 100;

	public PlatformPhysicsDelta physicsDelta;

	public float customPhysicsDelta = 0.02f;

	public PlatformFeatureEntry Clone()
	{
		return (PlatformFeatureEntry)MemberwiseClone();
	}

	public float GetPhysicsTimestep()
	{
		switch (physicsDelta)
		{
		case PlatformPhysicsDelta.Fps30:
			return 1f / 30f;
		case PlatformPhysicsDelta.Custom:
			if (!(customPhysicsDelta > 0f))
			{
				return 0.02f;
			}
			return customPhysicsDelta;
		default:
			return 0.02f;
		}
	}

	public PlatformSpecificMaterialType GetWaterMaterialType()
	{
		return waterTier switch
		{
			PlatformWaterTier.Medium => PlatformSpecificMaterialType.Switch, 
			PlatformWaterTier.Light => PlatformSpecificMaterialType.SwitchSimple, 
			PlatformWaterTier.Mobile => PlatformSpecificMaterialType.Mobile, 
			PlatformWaterTier.Minimal => PlatformSpecificMaterialType.Minimal, 
			_ => PlatformSpecificMaterialType.Standalone, 
		};
	}

	public MainCamera.RenderMode GetMainCameraRenderMode()
	{
		if (renderMode != PlatformRenderMode.Lightweight)
		{
			return MainCamera.RenderMode.Native;
		}
		return MainCamera.RenderMode.Lightweight;
	}
}
