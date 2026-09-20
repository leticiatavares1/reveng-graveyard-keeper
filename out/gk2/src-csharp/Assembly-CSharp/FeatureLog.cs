using System.Diagnostics;
using UnityEngine;

public static class FeatureLog
{
	[Conditional("UNITY_EDITOR")]
	[Conditional("DEV_BUILD")]
	public static void Log(LogChannel channel, string message)
	{
		if (DevUtils.IsLogChannelEnabled(channel))
		{
			UnityEngine.Debug.Log("#" + Tag(channel) + "# " + message);
		}
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEV_BUILD")]
	public static void Log(LogChannel channel, string message, object context)
	{
		if (DevUtils.IsLogChannelEnabled(channel))
		{
			UnityEngine.Debug.Log($"#{Tag(channel)}# {message}. Context: {context}");
		}
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEV_BUILD")]
	public static void Warn(LogChannel channel, string message)
	{
		if (DevUtils.IsLogChannelEnabled(channel))
		{
			UnityEngine.Debug.LogWarning("#" + Tag(channel) + "# " + message);
		}
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEV_BUILD")]
	public static void Error(LogChannel channel, string message)
	{
		if (DevUtils.IsLogChannelEnabled(channel))
		{
			UnityEngine.Debug.LogError("#" + Tag(channel) + "# " + message);
		}
	}

	private static string Tag(LogChannel channel)
	{
		return channel.ToString().ToLowerInvariant();
	}
}
