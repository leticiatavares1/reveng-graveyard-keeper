namespace LazyBearTechnology;

public static class Platform
{
	private static bool debugPlatformForced;

	private static PlatformType forcedPlatformType;

	private const PlatformType CURRENT = PlatformType.PС;

	public static PlatformType Type
	{
		get
		{
			if (debugPlatformForced)
			{
				return forcedPlatformType;
			}
			return PlatformType.PС;
		}
	}

	public static void ForceDebugPlatform(PlatformType platformType)
	{
		debugPlatformForced = true;
		forcedPlatformType = platformType;
	}

	public static void ClearForcedPlatform()
	{
		debugPlatformForced = false;
	}
}
