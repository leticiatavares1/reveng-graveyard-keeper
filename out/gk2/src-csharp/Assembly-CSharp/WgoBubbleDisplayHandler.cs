using LazyBearTechnology;
using UnityEngine;

public class WgoBubbleDisplayHandler : MonoBehaviour
{
	public static void Display(Wgo wgo)
	{
		if (!wgo.IsDespawning)
		{
			UIObjectBubbleManager.Instance.RequestDisplay(wgo);
		}
	}

	public static void Hide(Wgo wgo)
	{
		UIObjectBubbleManager.Instance.Hide(wgo);
	}

	public static void HideWidget<T>(Wgo wgo) where T : LazyWidgetDataBase
	{
		UIObjectBubbleManager.Instance.HideWidget<T>(wgo);
	}

	public static void HideWidget(Wgo wgo, LazyWidgetDataBase widgetData)
	{
		UIObjectBubbleManager.Instance.HideWidget(wgo, widgetData);
	}
}
