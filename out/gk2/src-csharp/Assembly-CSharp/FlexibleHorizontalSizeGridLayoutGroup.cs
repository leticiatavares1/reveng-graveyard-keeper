using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class FlexibleHorizontalSizeGridLayoutGroup : MonoBehaviour, ILazyGUIElement
{
	[SerializeField]
	private float width;

	[SerializeField]
	private FlexibleHorizontalSizeGridLayoutLine layoutLinePrefab;

	private Pool linePool;

	private List<FlexibleHorizontalSizeGridLayoutLine> drawnLines = new List<FlexibleHorizontalSizeGridLayoutLine>();

	public void Init()
	{
		layoutLinePrefab.gameObject.SetActive(value: false);
		linePool = LazyPooler.CreatePoolById(string.Format("{0}_{1}", "FlexibleHorizontalSizeGridLayoutGroup", GetHashCode()), layoutLinePrefab);
	}

	public void Draw(List<RectTransform> items)
	{
		float num = 0f;
		foreach (FlexibleHorizontalSizeGridLayoutLine drawnLine in drawnLines)
		{
			linePool.ReleaseObject(drawnLine);
		}
		drawnLines.Clear();
		FlexibleHorizontalSizeGridLayoutLine orCreateObject = linePool.GetOrCreateObject<FlexibleHorizontalSizeGridLayoutLine>();
		drawnLines.Add(orCreateObject);
		orCreateObject.transform.SetAsLastSibling();
		for (int i = 0; i < items.Count; i++)
		{
			RectTransform rectTransform = items[i];
			num += rectTransform.sizeDelta.x;
			if (rectTransform.sizeDelta.x > width)
			{
				Debug.LogError("Too big size of element when draw FlexibleHorizontalSizeGridLayoutGroup");
			}
			if (num > width)
			{
				orCreateObject = linePool.GetOrCreateObject<FlexibleHorizontalSizeGridLayoutLine>();
				drawnLines.Add(orCreateObject);
				orCreateObject.transform.SetAsLastSibling();
				num = rectTransform.sizeDelta.x;
			}
			rectTransform.transform.SetParent(orCreateObject.RectTransform);
		}
	}
}
