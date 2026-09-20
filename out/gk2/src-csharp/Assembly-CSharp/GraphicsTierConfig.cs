public static class GraphicsTierConfig
{
	public static PlatformFeatureEntry ApplyTier(PlatformFeatureEntry baseEntry, GraphicsTier tier)
	{
		if (baseEntry == null)
		{
			return null;
		}
		if (baseEntry.platform != 0 || tier == GraphicsTier.High)
		{
			return baseEntry;
		}
		PlatformFeatureEntry platformFeatureEntry = baseEntry.Clone();
		switch (tier)
		{
		case GraphicsTier.Medium:
			platformFeatureEntry.hbaoQuality = PlatformHBAOQuality.Medium;
			platformFeatureEntry.backLightEnabled = false;
			platformFeatureEntry.waterTier = PlatformWaterTier.High;
			platformFeatureEntry.ngssQuality = NgssQualityPreset.Medium;
			break;
		case GraphicsTier.Low:
			platformFeatureEntry.pointLightMode = PlatformPointLightMode.Faked;
			platformFeatureEntry.backLightEnabled = false;
			platformFeatureEntry.hbaoQuality = PlatformHBAOQuality.Lowest;
			platformFeatureEntry.waterTier = PlatformWaterTier.Light;
			platformFeatureEntry.ngssQuality = NgssQualityPreset.Low;
			platformFeatureEntry.cloudAppearance = PlatformCloudAppearance.Replaced;
			break;
		case GraphicsTier.Lowest:
			platformFeatureEntry.pointLightMode = PlatformPointLightMode.Faked;
			platformFeatureEntry.backLightEnabled = false;
			platformFeatureEntry.hbaoQuality = PlatformHBAOQuality.Lowest;
			platformFeatureEntry.waterTier = PlatformWaterTier.Light;
			platformFeatureEntry.shadowMode = PlatformShadowMode.Off;
			platformFeatureEntry.renderMode = PlatformRenderMode.Lightweight;
			platformFeatureEntry.cloudAppearance = PlatformCloudAppearance.Replaced;
			break;
		}
		return platformFeatureEntry;
	}
}
