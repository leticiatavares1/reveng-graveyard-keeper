using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using UnityEngine;

public static class PlatformFeatures
{
	private static PlatformFeatureEntry cachedCurrent;

	private static GamePlatform? cachedPlatform;

	private static GraphicsTier? cachedGraphicsTier;

	public static PlatformFeatureEntry Current
	{
		get
		{
			GamePlatform current = GamePlatformResolver.Current;
			GraphicsTier graphicsTier = ResolveGraphicsTier();
			if (cachedCurrent == null || cachedPlatform != current || cachedGraphicsTier != graphicsTier)
			{
				RebuildCurrent(current, graphicsTier);
			}
			return cachedCurrent;
		}
	}

	public static void RebuildCurrent()
	{
		RebuildCurrent(GamePlatformResolver.Current, ResolveGraphicsTier());
	}

	private static void RebuildCurrent(GamePlatform platform, GraphicsTier graphicsTier)
	{
		cachedCurrent = GraphicsTierConfig.ApplyTier(PlatformFeatureConfig.Get(platform), graphicsTier);
		cachedPlatform = platform;
		cachedGraphicsTier = graphicsTier;
	}

	private static GraphicsTier ResolveGraphicsTier()
	{
		if (GamePlatformResolver.Current != 0)
		{
			return GraphicsTier.High;
		}
		if (GameSettings.Instance == null)
		{
			return GraphicsTier.High;
		}
		return GameSettings.Instance.graphicsTier;
	}

	public static void ReapplyAll()
	{
		RebuildCurrent();
		ApplyShadowSettings();
		ApplyNgssQuality();
		ApplyBackLightSettings();
		ApplyRenderMode();
		ApplyHBAO();
		RefreshPlatformDependentLightElements();
		SwitchLightPolicy.ApplyLightRTPolicy();
		RefreshLightFakers();
		RefreshWaterMaterials();
	}

	public static void ApplyShadowSettings()
	{
		PlatformFeatureEntry current = Current;
		PlatformUnityShadowPreset preset = ResolveUnityShadowPreset(current);
		PlatformUnityShadowSettings platformUnityShadowSettings = PlatformUnityShadowPresets.Get(preset);
		Light sunLight = LightsSystem.Instance?.SunLight;
		platformUnityShadowSettings.Apply(sunLight);
		SwitchLightPolicy.NotifyLocalLightShadowPolicyChanged();
		if (!(LightsSystem.Instance == null))
		{
			if (ShouldUseNgss(current, preset))
			{
				LightsSystem.Instance.EnableNGSS();
			}
			else
			{
				LightsSystem.Instance.DisableNGSS();
			}
		}
	}

	public static void ApplyNgssQuality()
	{
		if (!(LightsSystem.Instance == null))
		{
			PlatformFeatureEntry current = Current;
			PlatformUnityShadowPreset preset = ResolveUnityShadowPreset(current);
			if (ShouldUseNgss(current, preset))
			{
				NGSS_Directional directional = ((LightsSystem.Instance.SunLight != null) ? LightsSystem.Instance.SunLight.GetComponent<NGSS_Directional>() : null);
				NGSS_Local componentInChildren = LightsSystem.Instance.GetComponentInChildren<NGSS_Local>(includeInactive: true);
				NgssQualityPresets.Get(current.ngssQuality).Apply(directional, componentInChildren);
			}
		}
	}

	public static PlatformUnityShadowPreset GetActiveUnityShadowPreset()
	{
		return ResolveUnityShadowPreset(Current);
	}

	public static PlatformUnityShadowPreset GetPlatformDefaultUnityShadowPreset(PlatformFeatureEntry features)
	{
		if (features.shadowMode == PlatformShadowMode.Off)
		{
			return PlatformUnityShadowPreset.Off;
		}
		if (features.shadowMode == PlatformShadowMode.Unity)
		{
			return features.unityShadowPreset;
		}
		return PlatformUnityShadowPreset.DesktopLike;
	}

	private static PlatformUnityShadowPreset ResolveUnityShadowPreset(PlatformFeatureEntry features)
	{
		if (features.shadowMode == PlatformShadowMode.Off)
		{
			return PlatformUnityShadowPreset.Off;
		}
		if (features.shadowMode == PlatformShadowMode.Unity)
		{
			return features.unityShadowPreset;
		}
		return PlatformUnityShadowPreset.DesktopLike;
	}

	private static bool ShouldUseNgss(PlatformFeatureEntry features, PlatformUnityShadowPreset preset)
	{
		if (features.shadowMode == PlatformShadowMode.NGSS)
		{
			return preset == PlatformUnityShadowPreset.DesktopLike;
		}
		return false;
	}

	public static void ApplyBackLightSettings()
	{
		if (!(LightsSystem.Instance == null))
		{
			if (Current.backLightEnabled)
			{
				LightsSystem.Instance.EnableBackLight();
			}
			else
			{
				LightsSystem.Instance.DisableBackLight();
			}
		}
	}

	public static void ApplyRenderMode()
	{
		if (!(CameraSystem.Instance == null))
		{
			CameraSystem.Instance.ApplyRenderActiveState();
		}
	}

	public static void ApplyHBAO()
	{
		HBAO hBAO = CameraSystem.Instance?.MainCamera?.GetComponent<HBAO>();
		if (!(hBAO == null))
		{
			PlatformFeatureEntry current = Current;
			hBAO.enabled = current.hbaoQuality != PlatformHBAOQuality.Off;
			if (hBAO.enabled)
			{
				hBAO.SetQuality(MapHBAOQuality(current.hbaoQuality));
			}
		}
	}

	public static bool IsHBAOEnabled()
	{
		return Current.hbaoQuality != PlatformHBAOQuality.Off;
	}

	public static PlatformSpecificMaterialType GetWaterMaterialType()
	{
		return Current.GetWaterMaterialType();
	}

	private static void RefreshWaterMaterials()
	{
		MaterialProvider[] array = Object.FindObjectsByType<MaterialProvider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyMaterial();
		}
	}

	private static void RefreshPlatformDependentLightElements()
	{
		PlatformDependentElementGK2[] array = Object.FindObjectsByType<PlatformDependentElementGK2>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (PlatformDependentElementGK2 platformDependentElementGK in array)
		{
			if (platformDependentElementGK.rtLightInUse != platformDependentElementGK.rtLightNotInUse)
			{
				platformDependentElementGK.Init();
			}
		}
	}

	private static void RefreshLightFakers()
	{
		LightFaker[] array = Object.FindObjectsByType<LightFaker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RefreshPolicyState();
		}
	}

	private static HBAO.Quality MapHBAOQuality(PlatformHBAOQuality quality)
	{
		return quality switch
		{
			PlatformHBAOQuality.Lowest => HBAO.Quality.Lowest, 
			PlatformHBAOQuality.Low => HBAO.Quality.Low, 
			PlatformHBAOQuality.High => HBAO.Quality.High, 
			PlatformHBAOQuality.Highest => HBAO.Quality.Highest, 
			_ => HBAO.Quality.Medium, 
		};
	}
}
