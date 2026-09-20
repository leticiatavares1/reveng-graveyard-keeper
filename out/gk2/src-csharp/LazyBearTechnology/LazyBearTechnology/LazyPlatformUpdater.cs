using UnityEngine;

namespace LazyBearTechnology;

public class LazyPlatformUpdater : MonoBehaviour
{
	private static bool isInited;

	private void Update()
	{
		LazyAPI.Platform.Update();
	}

	public static void Init()
	{
		if (!isInited && Application.isPlaying)
		{
			isInited = true;
			GameObject obj = new GameObject("LazyPlatformUpdater");
			obj.AddComponent<LazyPlatformUpdater>();
			Object.DontDestroyOnLoad(obj);
		}
	}
}
