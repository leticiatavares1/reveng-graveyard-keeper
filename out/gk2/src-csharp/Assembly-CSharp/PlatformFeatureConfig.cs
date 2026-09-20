using System.Collections.Generic;
using UnityEngine;

public static class PlatformFeatureConfig
{
	public static readonly PlatformFeatureEntry PC = new PlatformFeatureEntry
	{
		platform = GamePlatform.PC,
		shadowMode = PlatformShadowMode.NGSS,
		pointLightMode = PlatformPointLightMode.Realtime,
		waterTier = PlatformWaterTier.High,
		renderMode = PlatformRenderMode.Native,
		hbaoQuality = PlatformHBAOQuality.Highest,
		textureCompression = PlatformTextureCompression.Disabled,
		audioCompression = 100,
		physicsDelta = PlatformPhysicsDelta.Default
	};

	public static readonly PlatformFeatureEntry Switch = new PlatformFeatureEntry
	{
		platform = GamePlatform.Switch,
		shadowMode = PlatformShadowMode.Off,
		cloudAppearance = PlatformCloudAppearance.RenderTexture,
		pointLightMode = PlatformPointLightMode.Faked,
		backLightEnabled = false,
		waterTier = PlatformWaterTier.Light,
		renderMode = PlatformRenderMode.Lightweight,
		hbaoQuality = PlatformHBAOQuality.Off,
		textureCompression = PlatformTextureCompression.ASTC_6x6,
		audioCompression = 50,
		physicsDelta = PlatformPhysicsDelta.Fps30
	};

	public static readonly PlatformFeatureEntry Switch2 = new PlatformFeatureEntry
	{
		platform = GamePlatform.Switch2,
		shadowMode = PlatformShadowMode.Off,
		cloudAppearance = PlatformCloudAppearance.RenderTexture,
		pointLightMode = PlatformPointLightMode.Realtime,
		backLightEnabled = true,
		waterTier = PlatformWaterTier.Light,
		renderMode = PlatformRenderMode.Native,
		hbaoQuality = PlatformHBAOQuality.Lowest,
		textureCompression = PlatformTextureCompression.ASTC_4x4,
		audioCompression = 60,
		physicsDelta = PlatformPhysicsDelta.Default
	};

	public static readonly PlatformFeatureEntry PS4 = new PlatformFeatureEntry
	{
		platform = GamePlatform.PS4,
		shadowMode = PlatformShadowMode.Off,
		cloudAppearance = PlatformCloudAppearance.RenderTexture,
		pointLightMode = PlatformPointLightMode.Faked,
		waterTier = PlatformWaterTier.Light,
		renderMode = PlatformRenderMode.Native,
		hbaoQuality = PlatformHBAOQuality.Lowest,
		textureCompression = PlatformTextureCompression.BC7,
		audioCompression = 100,
		physicsDelta = PlatformPhysicsDelta.Default,
		backLightEnabled = false
	};

	public static readonly PlatformFeatureEntry PS5 = new PlatformFeatureEntry
	{
		platform = GamePlatform.PS5,
		shadowMode = PlatformShadowMode.Unity,
		cloudAppearance = PlatformCloudAppearance.Replaced,
		unityShadowPreset = PlatformUnityShadowPreset.Console,
		pointLightMode = PlatformPointLightMode.Realtime,
		waterTier = PlatformWaterTier.High,
		renderMode = PlatformRenderMode.Native,
		hbaoQuality = PlatformHBAOQuality.High,
		textureCompression = PlatformTextureCompression.Disabled,
		audioCompression = 100,
		physicsDelta = PlatformPhysicsDelta.Default
	};

	public static readonly PlatformFeatureEntry XboxOne = new PlatformFeatureEntry
	{
		platform = GamePlatform.XboxOne,
		shadowMode = PlatformShadowMode.Off,
		cloudAppearance = PlatformCloudAppearance.RenderTexture,
		pointLightMode = PlatformPointLightMode.Faked,
		waterTier = PlatformWaterTier.Light,
		renderMode = PlatformRenderMode.Native,
		hbaoQuality = PlatformHBAOQuality.Lowest,
		textureCompression = PlatformTextureCompression.BC7,
		audioCompression = 100,
		physicsDelta = PlatformPhysicsDelta.Default
	};

	public static readonly PlatformFeatureEntry XboxSeries = new PlatformFeatureEntry
	{
		platform = GamePlatform.XboxSeries,
		shadowMode = PlatformShadowMode.Unity,
		cloudAppearance = PlatformCloudAppearance.Replaced,
		unityShadowPreset = PlatformUnityShadowPreset.Console,
		pointLightMode = PlatformPointLightMode.Realtime,
		waterTier = PlatformWaterTier.High,
		renderMode = PlatformRenderMode.Native,
		hbaoQuality = PlatformHBAOQuality.High,
		textureCompression = PlatformTextureCompression.Disabled,
		audioCompression = 100,
		physicsDelta = PlatformPhysicsDelta.Default
	};

	public static readonly IReadOnlyList<PlatformFeatureEntry> Entries = new PlatformFeatureEntry[7] { PC, Switch, Switch2, PS4, PS5, XboxOne, XboxSeries };

	private static readonly Dictionary<GamePlatform, PlatformFeatureEntry> entriesByPlatform = BuildLookup();

	private static Dictionary<GamePlatform, PlatformFeatureEntry> BuildLookup()
	{
		Dictionary<GamePlatform, PlatformFeatureEntry> dictionary = new Dictionary<GamePlatform, PlatformFeatureEntry>();
		for (int i = 0; i < Entries.Count; i++)
		{
			dictionary[Entries[i].platform] = Entries[i];
		}
		return dictionary;
	}

	public static PlatformFeatureEntry Get(GamePlatform platform)
	{
		if (entriesByPlatform.TryGetValue(platform, out var value))
		{
			return value;
		}
		Debug.LogWarning($"[PlatformFeatureConfig] Missing entry for {platform}, using PC defaults.");
		return PC;
	}

	public static Dictionary<string, PlatformTextureCompression> GetTextureCompressionSettings()
	{
		Dictionary<string, PlatformTextureCompression> dictionary = new Dictionary<string, PlatformTextureCompression>();
		for (int i = 0; i < Entries.Count; i++)
		{
			PlatformFeatureEntry platformFeatureEntry = Entries[i];
			if (platformFeatureEntry.textureCompression != 0)
			{
				dictionary[platformFeatureEntry.platform.ToTextureImporterPlatformName()] = platformFeatureEntry.textureCompression;
			}
		}
		return dictionary;
	}

	public static Dictionary<string, int> GetAudioCompressionSettings()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 0; i < Entries.Count; i++)
		{
			PlatformFeatureEntry platformFeatureEntry = Entries[i];
			if (platformFeatureEntry.audioCompression > 0 && platformFeatureEntry.audioCompression < 100)
			{
				dictionary[platformFeatureEntry.platform.ToTextureImporterPlatformName()] = platformFeatureEntry.audioCompression;
			}
		}
		return dictionary;
	}
}
