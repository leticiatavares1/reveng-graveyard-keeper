using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class LazyScrollRect : ScrollRect
{
	private Func<LazyScrollableElement, LazyWidgetBase> widgetGetterFunc;

	private Action<LazyScrollableElement> releaseWidgetAction;

	private Action onGamepadReInit;

	private Pool elementsPool;

	private bool isUpdatingVisibilityForElements;

	private bool anyElementUpdatedVisibility;

	[SerializeField]
	private LayoutGroup contentLayoutGroup;

	private LazyScrollableElement elementPrefab;

	private List<LazyScrollableElement> displayingElements = new List<LazyScrollableElement>();

	private LazyScrollableElement ElementPrefab
	{
		get
		{
			if (elementPrefab == null)
			{
				elementPrefab = GetComponentInChildren<LazyScrollableElement>();
				if (elementPrefab == null)
				{
					GameObject gameObject = new GameObject();
					elementPrefab = gameObject.AddComponent<LazyScrollableElement>();
					gameObject.AddComponent<RectTransform>();
					gameObject.name = "LazyScrollableElementPrefab";
					gameObject.transform.SetParent(base.content);
				}
				elementPrefab.gameObject.SetActive(value: false);
			}
			return elementPrefab;
		}
	}

	public List<LazyScrollableElement> DisplayingElements => displayingElements;

	public void Init(Func<LazyScrollableElement, LazyWidgetBase> widgetGetterFunc, Action<LazyScrollableElement> releaseWidgetAction, GamepadNavigationController gamepadNavigationController, Action onGamepadReInit = null, Func<bool> customVisibilityCondition = null, Action onGamepadFocus = null, Action onGamepadUnFocus = null, Action onGamepadSelect = null)
	{
		this.widgetGetterFunc = widgetGetterFunc;
		this.releaseWidgetAction = releaseWidgetAction;
		this.onGamepadReInit = onGamepadReInit;
		if (contentLayoutGroup == null)
		{
			contentLayoutGroup = base.content.GetComponent<LayoutGroup>();
		}
		gamepadNavigationController.OnFocusedItemChanged += delegate
		{
			CheckVisibility();
		};
		base.onValueChanged.AddListener(OnValueChanged);
		elementsPool = LazyPooler.CreatePoolById("LazyScrollableElement " + ElementPrefab.GetHashCode(), ElementPrefab, 0, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: false, forceActivateAllNewObjects: true, delegate(MonoBehaviour behaviour)
		{
			LazyScrollableElement lazyScrollableElement = behaviour as LazyScrollableElement;
			if (lazyScrollableElement != null)
			{
				lazyScrollableElement.Init(base.viewport, OnUIScrollElementBecameVisible, OnUIScrollElementBecameInvisible, customVisibilityCondition);
				if (lazyScrollableElement.GamepadNavigationItem != null)
				{
					LazyWidgetBase widget = lazyScrollableElement.ForceVisibilityAndGetWidget();
					lazyScrollableElement.GamepadNavigationItem.OnFocus.AddListener(delegate
					{
						if (widget != null)
						{
							onGamepadFocus?.Invoke();
						}
					});
					lazyScrollableElement.GamepadNavigationItem.OnUnfocus.AddListener(delegate
					{
						if (widget != null)
						{
							onGamepadUnFocus?.Invoke();
						}
					});
					lazyScrollableElement.GamepadNavigationItem.OnSelect.AddListener(delegate
					{
						if (widget != null)
						{
							onGamepadSelect?.Invoke();
						}
					});
				}
			}
		});
	}

	public LazyScrollableElement AddScrollableElement(LazyWidgetDataBase data)
	{
		LazyScrollableElement orCreateObject = elementsPool.GetOrCreateObject<LazyScrollableElement>();
		orCreateObject.RectTransform.localScale = Vector3.one;
		orCreateObject.Data = data;
		displayingElements.Add(orCreateObject);
		return orCreateObject;
	}

	public void RemoveScrollableElementAt(int index)
	{
		if (index < 0 || index >= displayingElements.Count)
		{
			Debug.LogWarning($"Can't remove ScrollableElement at index [{index}] because index is out of range.");
			return;
		}
		LazyScrollableElement lazyScrollableElement = displayingElements[index];
		if (lazyScrollableElement != null)
		{
			if (lazyScrollableElement.Widget != null)
			{
				OnReleaseWidget(lazyScrollableElement);
			}
			elementsPool.ReleaseObject(lazyScrollableElement);
			OnGamepadReInit();
			displayingElements.RemoveAt(index);
		}
	}

	public void CheckVisibility()
	{
		isUpdatingVisibilityForElements = true;
		foreach (LazyScrollableElement displayingElement in displayingElements)
		{
			displayingElement.CheckVisibility();
		}
		if (anyElementUpdatedVisibility)
		{
			OnGamepadReInit();
			anyElementUpdatedVisibility = false;
		}
		isUpdatingVisibilityForElements = false;
	}

	public void ClearDisplayingScrollableElements()
	{
		bool flag = false;
		for (int num = displayingElements.Count - 1; num >= 0; num--)
		{
			LazyScrollableElement lazyScrollableElement = displayingElements[num];
			if (lazyScrollableElement.Widget != null)
			{
				OnReleaseWidget(lazyScrollableElement);
				flag = true;
			}
			elementsPool.ReleaseObject(lazyScrollableElement);
		}
		if (flag)
		{
			OnGamepadReInit();
		}
		displayingElements.Clear();
	}

	private void OnUIScrollElementBecameVisible(LazyScrollableElement element)
	{
		if (element.Widget == null && element.Data != null)
		{
			LazyWidgetBase lazyWidgetBase = widgetGetterFunc(element);
			RectTransform component = lazyWidgetBase.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0.5f, 0.5f);
			component.anchorMax = new Vector2(0.5f, 0.5f);
			element.SetElement(lazyWidgetBase);
			element.UpdateSize();
			if (contentLayoutGroup != null)
			{
				contentLayoutGroup.CalculateLayoutInputHorizontal();
				contentLayoutGroup.CalculateLayoutInputVertical();
				contentLayoutGroup.SetLayoutHorizontal();
				contentLayoutGroup.SetLayoutVertical();
			}
			if (isUpdatingVisibilityForElements)
			{
				anyElementUpdatedVisibility = true;
			}
			else
			{
				OnGamepadReInit();
			}
		}
	}

	private void OnUIScrollElementBecameInvisible(LazyScrollableElement element)
	{
		if (element.Widget != null)
		{
			OnReleaseWidget(element);
			if (isUpdatingVisibilityForElements)
			{
				anyElementUpdatedVisibility = true;
			}
			else
			{
				OnGamepadReInit();
			}
		}
	}

	private void OnReleaseWidget(LazyScrollableElement element)
	{
		releaseWidgetAction?.Invoke(element);
		element.UnsetElement();
	}

	private void OnValueChanged(Vector2 value)
	{
		CheckVisibility();
	}

	private void OnGamepadReInit()
	{
		if (LazyInput.IsGamepadActive && base.gameObject.activeSelf)
		{
			onGamepadReInit?.Invoke();
		}
	}
}
