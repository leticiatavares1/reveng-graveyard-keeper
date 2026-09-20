using UnityEngine;

namespace LazyBearTechnology.Preloader;

public class LazyPreloaderController : MonoBehaviour
{
	public void OnAnimationStarted()
	{
		LazyPreloader.OnAnimationStarted();
	}

	public void OnAnimationStopped()
	{
		LazyPreloader.OnAnimationStopped();
	}
}
