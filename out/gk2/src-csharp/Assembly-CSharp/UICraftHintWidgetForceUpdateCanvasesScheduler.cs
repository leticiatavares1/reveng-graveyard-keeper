using LazyBearTechnology;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class UICraftHintWidgetForceUpdateCanvasesScheduler : LazySingleton<UICraftHintWidgetForceUpdateCanvasesScheduler>
{
	private static bool isUpdateRequested;

	public void RequestUpdate()
	{
		isUpdateRequested = true;
	}

	private void LateUpdate()
	{
		if (isUpdateRequested)
		{
			isUpdateRequested = false;
			Canvas.ForceUpdateCanvases();
		}
	}
}
