using UnityEngine;

public class MixerLightIntegration : MonoBehaviour
{
	private static MixerLightIntegration _me;

	private bool _enabled;

	private static bool _started;

	public static int enable_mode;

	public static bool IsAvailable()
	{
		return false;
	}

	public static void Init()
	{
	}

	private static void StartMixer()
	{
	}

	private static void StopMixer()
	{
	}

	public static void OnStartPlayingGame()
	{
	}

	public static void OnStopPlayingGame()
	{
	}

	public static void ProcessBody(Item body)
	{
	}

	public static void ApplyEnableMode(int mode)
	{
	}
}
