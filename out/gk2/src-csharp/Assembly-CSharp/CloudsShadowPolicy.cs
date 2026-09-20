public static class CloudsShadowPolicy
{
	public static PlatformCloudAppearance ActiveAppearance => PlatformFeatures.Current.cloudAppearance;

	public static bool UseCloudsRTOverlay => ActiveAppearance == PlatformCloudAppearance.RenderTexture;

	public static bool UseReplacedCloudMaterial => ActiveAppearance == PlatformCloudAppearance.Replaced;
}
