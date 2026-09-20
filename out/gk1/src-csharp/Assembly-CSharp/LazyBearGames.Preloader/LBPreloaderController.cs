using UnityEngine;

namespace LazyBearGames.Preloader;

public class LBPreloaderController : MonoBehaviour
{
	public void OnAnimationStarted()
	{
		LBPreloader.OnAnimationStarted();
	}

	public void OnAnimationStopped()
	{
		LBPreloader.OnAnimationStopped();
	}
}
