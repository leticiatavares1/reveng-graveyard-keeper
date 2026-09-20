using LazyBearTechnology;
using UnityEngine;

public static class UISliderClickSound
{
	private const float MinInterval = 0.045f;

	private static float lastPlayTime = -1f;

	public static void Play()
	{
		float unscaledTime = Time.unscaledTime;
		if (!(unscaledTime - lastPlayTime < 0.045f))
		{
			lastPlayTime = unscaledTime;
			LazyAudio.PlayAndForget("gui_hover");
		}
	}
}
