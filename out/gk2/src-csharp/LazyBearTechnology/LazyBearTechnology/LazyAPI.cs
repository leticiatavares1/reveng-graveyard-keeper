namespace LazyBearTechnology;

public static class LazyAPI
{
	private static ILazyPlatform platform;

	private static LazyPlayerPrefs playerPrefs;

	private static LazyFile lazyFile;

	public static ILazyPlatform Platform
	{
		get
		{
			if (platform == null)
			{
				platform = new LazyPlatformDefault();
				LazyPlatformUpdater.Init();
			}
			return platform;
		}
	}

	public static LazyPlayerPrefs PlayerPrefs => playerPrefs ?? (playerPrefs = new LazyPlayerPrefs());

	public static LazyFile LazyFile => lazyFile ?? (lazyFile = new LazyFile());
}
