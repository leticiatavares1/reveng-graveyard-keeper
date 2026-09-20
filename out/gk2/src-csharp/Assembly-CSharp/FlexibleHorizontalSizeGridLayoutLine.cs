using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class FlexibleHorizontalSizeGridLayoutLine : MonoBehaviour
{
	private RectTransform rectTransform;

	public RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = base.transform as RectTransform;
			}
			return rectTransform;
		}
	}
}
