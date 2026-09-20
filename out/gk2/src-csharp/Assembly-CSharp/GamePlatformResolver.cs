public static class GamePlatformResolver
{
	private const string EDITOR_OVERRIDE_KEY = "debug_game_platform_override";

	private const int NO_OVERRIDE = -1;

	private static GamePlatform? resolvedPlatform;

	public static GamePlatform Current
	{
		get
		{
			if (!resolvedPlatform.HasValue)
			{
				resolvedPlatform = ResolveBuildPlatform();
			}
			return resolvedPlatform.Value;
		}
	}

	public static bool HasEditorOverride => false;

	public static GamePlatform ResolveBuildPlatform()
	{
		return GamePlatform.PC;
	}

	public static string ToTextureImporterPlatformName(this GamePlatform platform)
	{
		return platform switch
		{
			GamePlatform.Switch => "Switch", 
			GamePlatform.Switch2 => "Switch2", 
			GamePlatform.PS4 => "PS4", 
			GamePlatform.PS5 => "PS5", 
			GamePlatform.XboxOne => "GameCoreXboxOne", 
			GamePlatform.XboxSeries => "GameCoreScarlett", 
			_ => "Standalone", 
		};
	}
}
