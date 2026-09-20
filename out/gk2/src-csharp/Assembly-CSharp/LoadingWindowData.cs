using System;
using LazyBearTechnology;

public class LoadingWindowData : LazyWidgetDataBase
{
	public Action OnAnimationComplete { get; private set; }

	public string SceneId { get; private set; }

	public bool IsCrossSceneLoading { get; private set; }

	public LoadingWindowData(string sceneId, Action onAnimationComplete, bool isCrossSceneLoading = false)
	{
		SceneId = sceneId;
		OnAnimationComplete = onAnimationComplete;
		IsCrossSceneLoading = isCrossSceneLoading;
	}
}
