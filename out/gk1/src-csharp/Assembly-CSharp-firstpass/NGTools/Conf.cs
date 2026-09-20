using System;

namespace NGTools;

public static class Conf
{
	public enum DebugModes
	{
		None,
		Active,
		Verbose
	}

	public const string DebugModeKeyPref = "NGTools_DebugMode";

	public static Action DebugModeChanged;

	private static DebugModes debugMode;

	public static DebugModes DebugMode
	{
		get
		{
			return debugMode;
		}
		set
		{
			if (debugMode != value)
			{
				debugMode = value;
				if (DebugModeChanged != null)
				{
					DebugModeChanged();
				}
			}
		}
	}
}
