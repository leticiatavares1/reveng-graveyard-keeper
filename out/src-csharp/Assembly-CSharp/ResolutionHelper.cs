using UnityEngine;

public class ResolutionHelper : MonoBehaviour
{
	private static int _w;

	private static int _h;

	public void Update()
	{
		if (_w != Screen.width || _h != Screen.height)
		{
			Debug.Log($"Resolution change detected [{_w}x{_h}] ==> [{Screen.width}x{Screen.height}]");
			Awake();
		}
	}

	public void Awake()
	{
		_w = Screen.width;
		_h = Screen.height;
	}

	private void OnApplicationFocus(bool focus)
	{
		Debug.Log($"OnApplicationFocus {focus}");
		if (!(GameSettings.me.screen_mode == 1 && focus))
		{
			return;
		}
		GJTimer.AddTimer(0f, delegate
		{
			Debug.Log($"Checking resolution on re-focus. Current: {Screen.width}x{Screen.height}");
			if (Screen.width != GameSettings.current_resolution.x || Screen.height != GameSettings.current_resolution.y)
			{
				Debug.Log("Restoring resolution...");
				GameSettings.me.ApplyScreenMode();
			}
		});
	}

	public static void OnResolutionChanged(int w, int h)
	{
		Debug.Log($"OnResolutionChanged {w}x{h}");
		_w = w;
		_h = h;
	}
}
