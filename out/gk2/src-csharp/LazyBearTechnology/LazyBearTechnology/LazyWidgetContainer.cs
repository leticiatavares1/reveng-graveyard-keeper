using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyWidgetContainer : MonoBehaviour
{
	[SerializeField]
	protected GameObject widgetContainerGO;

	[SerializeField]
	protected List<LazyWidgetBase> usedWidgetsList = new List<LazyWidgetBase>();

	protected List<LazyWidgetBase> displayedWidgets = new List<LazyWidgetBase>();

	protected List<LazyWidgetDataBase> dataList;

	public int DisplayedWidgetsCount => displayedWidgets.Count;

	public virtual void ConstructWidgets(List<LazyWidgetDataBase> dataList, Transform parent = null)
	{
		foreach (LazyWidgetBase usedWidgets in usedWidgetsList)
		{
			usedWidgets.gameObject.SetActive(value: false);
		}
		displayedWidgets.Clear();
		int num = 0;
		foreach (LazyWidgetDataBase data in dataList)
		{
			bool flag = false;
			foreach (LazyWidgetBase usedWidgets2 in usedWidgetsList)
			{
				if (!usedWidgets2.gameObject.activeSelf && usedWidgets2.GetDataType() == data.GetType())
				{
					usedWidgets2.gameObject.SetActive(value: true);
					usedWidgets2.Draw(data);
					usedWidgets2.transform.SetSiblingIndex(num);
					num++;
					flag = true;
					displayedWidgets.Add(usedWidgets2);
					break;
				}
			}
			if (!flag)
			{
				LazyWidgetBase lazyWidgetBase = LazyWidgetPrefabContainer.GetPrefabFromDataObject(data).Copy(parent ? parent : ((widgetContainerGO != null) ? widgetContainerGO.transform : base.transform));
				lazyWidgetBase.transform.SetSiblingIndex(num);
				num++;
				lazyWidgetBase.Draw(data);
				displayedWidgets.Add(lazyWidgetBase);
				usedWidgetsList.Add(lazyWidgetBase);
			}
		}
		this.dataList = dataList;
	}

	public virtual void ConstructWidgetsFromScratch(List<LazyWidgetDataBase> dataList)
	{
		if (usedWidgetsList.Count != 0)
		{
			foreach (LazyWidgetBase usedWidgets in usedWidgetsList)
			{
				Object.Destroy(usedWidgets.gameObject);
			}
			usedWidgetsList.Clear();
		}
		foreach (LazyWidgetDataBase data in dataList)
		{
			LazyWidgetBase lazyWidgetBase = LazyWidgetPrefabContainer.GetPrefabFromDataObject(data).Copy(base.transform);
			lazyWidgetBase.Draw(data);
			usedWidgetsList.Add(lazyWidgetBase);
		}
		this.dataList = dataList;
	}

	public virtual void CustomUpdate()
	{
		for (int i = 0; i < usedWidgetsList.Count; i++)
		{
			if (usedWidgetsList[i].gameObject.activeSelf)
			{
				usedWidgetsList[i].CustomUpdate();
			}
		}
	}
}
