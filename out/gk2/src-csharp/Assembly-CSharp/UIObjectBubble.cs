using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectBubble : UIWidgetContainer
{
	[SerializeField]
	private RectTransform rectTransform;

	private IBubbleDrawable target;

	private bool isOutOfScreen;

	public IBubbleDrawable Target => target;

	public bool IsOutOfScreen => isOutOfScreen;

	public long DepthKey { get; private set; }

	internal void SetDepthKey(long depthKey)
	{
		DepthKey = depthKey;
	}

	public void TryCompareDataAndRedrawExistingOrDisplay(IBubbleDrawable target, List<LazyWidgetDataBase> dataList)
	{
		if (dataList.Count != base.dataList.Count)
		{
			Display(target, dataList);
			return;
		}
		for (int i = 0; i < dataList.Count; i++)
		{
			if (dataList[i].GetType() != base.dataList[i].GetType())
			{
				Display(target, dataList);
				return;
			}
		}
		RedrawExistingWidgetsOnly(target, dataList);
	}

	public void Display(IBubbleDrawable target, List<LazyWidgetDataBase> dataList)
	{
		this.target = target;
		if (HasDelayedHide())
		{
			ConstructWidgetsWithoutTouchingDelayed(dataList);
		}
		else
		{
			ConstructWidgets(dataList);
		}
		if (rectTransform.GetComponentInChildren<IBubbleLayoutAlwaysActive>() != null)
		{
			rectTransform.EnableLayoutGroupsAndRefreshContentFitter();
		}
		else
		{
			rectTransform.RefreshContentFitterAndDisable();
		}
		UpdatePos();
		base.gameObject.SetActive(value: true);
	}

	public void RedrawExistingWidgetsOnly(IBubbleDrawable target, List<LazyWidgetDataBase> dataList)
	{
		bool flag = false;
		List<LazyWidgetBase> list = new List<LazyWidgetBase>();
		for (int i = 0; i < displayedWidgets.Count; i++)
		{
			LazyWidgetBase lazyWidgetBase = displayedWidgets[i];
			if (!(lazyWidgetBase is IDelayedUIHide { ShouldDelayHide: not false }))
			{
				lazyWidgetBase.Draw(dataList[i]);
				if (!(lazyWidgetBase is IUIObjectBubbleWidgetWithoutRebuildingLayout))
				{
					flag = true;
				}
				if (lazyWidgetBase is IBubbleLayoutAlwaysActive)
				{
					list.Add(lazyWidgetBase);
				}
			}
		}
		if (flag)
		{
			rectTransform.RefreshContentFitterAndDisable();
			UpdatePos();
		}
		foreach (LazyWidgetBase item in list)
		{
			((RectTransform)item.transform).EnableLayoutGroupsAndRefreshContentFitter();
		}
	}

	private void ConstructWidgetsWithoutTouchingDelayed(List<LazyWidgetDataBase> dataList)
	{
		foreach (LazyWidgetBase usedWidgets in usedWidgetsList)
		{
			if (!(usedWidgets is IDelayedUIHide { ShouldDelayHide: not false }))
			{
				usedWidgets.Hide();
				usedWidgets.gameObject.SetActive(value: false);
			}
		}
		displayedWidgets.RemoveAll((LazyWidgetBase widget) => !(widget is IDelayedUIHide delayedUIHide3) || !delayedUIHide3.ShouldDelayHide);
		int num = 0;
		foreach (LazyWidgetDataBase data in dataList)
		{
			bool flag = false;
			foreach (LazyWidgetBase usedWidgets2 in usedWidgetsList)
			{
				if (usedWidgets2 is IDelayedUIHide { ShouldDelayHide: not false })
				{
					if (usedWidgets2.GetDataType() == data.GetType())
					{
						usedWidgets2.transform.SetSiblingIndex(num);
						num++;
						flag = true;
						break;
					}
				}
				else if (!usedWidgets2.gameObject.activeSelf && usedWidgets2.GetDataType() == data.GetType())
				{
					usedWidgets2.Draw(data);
					usedWidgets2.gameObject.SetActive(value: true);
					usedWidgets2.transform.SetSiblingIndex(num);
					num++;
					flag = true;
					displayedWidgets.Add(usedWidgets2);
					break;
				}
			}
			if (!flag)
			{
				LazyWidgetBase lazyWidgetBase = LazyWidgetPrefabContainer.GetPrefabFromDataObject(data).Copy((widgetContainerGO != null) ? widgetContainerGO.transform : base.transform);
				lazyWidgetBase.transform.SetSiblingIndex(num);
				num++;
				lazyWidgetBase.Draw(data);
				displayedWidgets.Add(lazyWidgetBase);
				usedWidgetsList.Add(lazyWidgetBase);
			}
		}
		base.dataList = dataList;
	}

	public void HideParticularWidget<T>() where T : LazyWidgetDataBase
	{
		for (int i = 0; i < displayedWidgets.Count; i++)
		{
			LazyWidgetBase lazyWidgetBase = displayedWidgets[i];
			if (lazyWidgetBase.GetDataType() == typeof(T))
			{
				lazyWidgetBase.Hide();
				lazyWidgetBase.gameObject.SetActive(value: false);
				displayedWidgets.RemoveAt(i);
				rectTransform.RefreshContentFitterAndDisable();
			}
		}
	}

	public bool TryDelayHide(Action onReadyToHide)
	{
		foreach (LazyWidgetBase displayedWidget in displayedWidgets)
		{
			if (displayedWidget is IDelayedUIHide { ShouldDelayHide: not false } delayedUIHide)
			{
				delayedUIHide.AddHideAfterDelayCallback(onReadyToHide);
				return true;
			}
		}
		return false;
	}

	public void ForceCancelDelayedHides()
	{
		foreach (LazyWidgetBase displayedWidget in displayedWidgets)
		{
			if (displayedWidget is IDelayedUIHide delayedUIHide)
			{
				delayedUIHide.ForceCancelDelayedHide();
			}
		}
	}

	public bool HasDelayedHide()
	{
		foreach (LazyWidgetBase displayedWidget in displayedWidgets)
		{
			if (displayedWidget is IDelayedUIHide { ShouldDelayHide: not false })
			{
				return true;
			}
		}
		return false;
	}

	public bool TryDelayHideWidget<T>(Action onReadyToHide) where T : LazyWidgetDataBase
	{
		foreach (LazyWidgetBase displayedWidget in displayedWidgets)
		{
			if (displayedWidget.GetDataType() == typeof(T) && displayedWidget is IDelayedUIHide { ShouldDelayHide: not false } delayedUIHide)
			{
				delayedUIHide.AddHideAfterDelayCallback(onReadyToHide);
				return true;
			}
		}
		return false;
	}

	public bool TryDelayHideWidget(Type widgetDataType, Action onReadyToHide)
	{
		foreach (LazyWidgetBase displayedWidget in displayedWidgets)
		{
			if (displayedWidget.GetDataType() == widgetDataType && displayedWidget is IDelayedUIHide { ShouldDelayHide: not false } delayedUIHide)
			{
				delayedUIHide.AddHideAfterDelayCallback(onReadyToHide);
				return true;
			}
		}
		return false;
	}

	public void HideParticularWidget(LazyWidgetDataBase widgetData)
	{
		for (int i = 0; i < displayedWidgets.Count; i++)
		{
			LazyWidgetBase lazyWidgetBase = displayedWidgets[i];
			if (lazyWidgetBase.GetDataType() == widgetData.GetType())
			{
				lazyWidgetBase.Hide();
				lazyWidgetBase.gameObject.SetActive(value: false);
				displayedWidgets.RemoveAt(i);
				rectTransform.RefreshContentFitterAndDisable();
			}
		}
	}

	public void Hide()
	{
		foreach (LazyWidgetBase usedWidgets in usedWidgetsList)
		{
			usedWidgets.Hide();
			usedWidgets.gameObject.SetActive(value: false);
		}
		displayedWidgets.Clear();
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(value: false);
		}
		target = null;
	}

	public override void CustomUpdate()
	{
		if (!isOutOfScreen)
		{
			base.CustomUpdate();
		}
	}

	public void UpdatePos()
	{
		base.transform.position = CameraSystem.WorldToScreenPoint(target.BubbleDrawablePosition);
		if (HasDelayedHide())
		{
			isOutOfScreen = false;
		}
		else
		{
			isOutOfScreen = rectTransform.IsRectOutOfScreen(LazyUI.GetScreenBounds());
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.1f, 0.8f, 0.4f, 0.4f);
		Rect worldRect = rectTransform.GetWorldRect();
		Gizmos.DrawCube(worldRect.center, new Vector3(worldRect.width, worldRect.height, 0.4f));
	}
}
