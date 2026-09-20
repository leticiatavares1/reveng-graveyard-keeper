using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[RequireComponent(typeof(GamepadNavigationController))]
[RequireComponent(typeof(Canvas))]
public abstract class LazyWindow<T> : LazyWidget<T> where T : LazyWidgetDataBase
{
	[Space]
	[SerializeField]
	protected bool isModalWindow = true;

	[Space]
	[SerializeField]
	protected LazyButton closeButton;

	[Space]
	[SerializeField]
	private string openSoundId;

	[SerializeField]
	private string closeSoundId;

	private GamepadNavigationController gamepadNavigationController;

	protected Action<T> OnClosed;

	protected LazyButtonTipsStr lazyButtonTips;

	protected LazyWindowInputController lazyWindowInputController;

	protected Canvas canvas;

	private bool isShown;

	protected GamepadNavigationController GamepadNavigationController
	{
		get
		{
			if (gamepadNavigationController == null)
			{
				gamepadNavigationController = GetComponent<GamepadNavigationController>();
			}
			return gamepadNavigationController;
		}
	}

	public Canvas Canvas => canvas;

	public bool IsShown => isShown;

	public bool IsTop => LazyWindowsStackController.IsWindowOnTop(this);

	public bool IsShownAndTop
	{
		get
		{
			if (IsShown)
			{
				return IsTop;
			}
			return false;
		}
	}

	public bool IsModalWindow => isModalWindow;

	public override void Init()
	{
		base.Init();
		if ((bool)closeButton)
		{
			InitCloseButton(closeButton);
		}
		InitInputController();
		InitButtonTipsStr();
		InitCanvas();
		SubscribePermanentEvents();
		HideWindow();
	}

	public virtual void Open(T data)
	{
		ShowWindow();
		Draw(data);
	}

	public void Open(T data, Action<T> onClosed)
	{
		Open(data);
		OnClosed = onClosed;
	}

	public virtual void Close()
	{
		HideWindow();
		OnClosed?.Invoke(data);
		OnClosed = null;
	}

	public virtual void CloseWithoutCallback()
	{
		HideWindow();
		OnClosed = null;
	}

	protected virtual void ShowWindow()
	{
		if (isShown)
		{
			Debug.LogWarning("ShowWindow(): Window " + base.name + " is already shown. Call Redraw.", this);
			return;
		}
		if (!string.IsNullOrEmpty(openSoundId))
		{
			LazyAudio.PlayAndForget(openSoundId);
		}
		isShown = true;
		lazyWindowInputController.Enable(restoreFocused: false);
		LazyInput.OnInputChanged += OnInputChanged;
		UpdateGamepadDependentStuff();
		lazyWindowInputController.UpdateGamepadDependentStuff();
		base.gameObject.SetActive(value: true);
		LazyWindowsStackController.AddToStack(this);
	}

	protected virtual void HideWindow()
	{
		lazyWindowInputController.Disable(rememberFocused: false);
		LazyInput.OnInputChanged -= OnInputChanged;
		if (isShown && !string.IsNullOrEmpty(closeSoundId))
		{
			LazyAudio.PlayAndForget(closeSoundId);
		}
		isShown = false;
		LazyWindowsStackController.RemoveFromStack(this);
		Hide();
	}

	protected virtual void Update()
	{
		lazyWindowInputController.Update();
	}

	protected virtual void InitCloseButton(LazyButton button)
	{
		button.onClick.AddListener(Close);
	}

	protected virtual void SubscribePermanentEvents()
	{
		LazyWindowsStackController.OnWindowBecameVisibleInStack += OnWindowBecameVisibleInStack;
		LazyWindowsStackController.OnWindowBecameHiddenInStack += OnWindowBecameHiddenInStack;
	}

	protected virtual bool OnPressedBack()
	{
		if ((bool)closeButton)
		{
			Close();
			return true;
		}
		return false;
	}

	protected virtual void UpdateGamepadDependentStuff()
	{
		LazyPlatformDependentElement[] componentsInChildren = GetComponentsInChildren<LazyPlatformDependentElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		LazyGamepadDependentElement[] componentsInChildren2 = GetComponentsInChildren<LazyGamepadDependentElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].UpdateState();
		}
		if (LazyInput.IsGamepadActive)
		{
			if ((bool)closeButton)
			{
				closeButton.gameObject.SetActive(value: false);
			}
			PrintTips();
			ChangeTipsState(active: true);
		}
		else
		{
			if ((bool)closeButton)
			{
				closeButton.gameObject.SetActive(value: true);
			}
			ChangeTipsState(active: false);
		}
	}

	protected virtual void PrintTips()
	{
		if ((bool)closeButton)
		{
			lazyButtonTips.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back());
		}
		else
		{
			lazyButtonTips.Print(LazyGameKeyTip.Select());
		}
	}

	protected virtual void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		PrintTips();
	}

	protected virtual void OnFocusedItemChanged(GamepadNavigationItem gamepadNavigationItem)
	{
		PrintTips(gamepadNavigationItem);
	}

	protected virtual void OnBecameVisibleInStack()
	{
		ChangeTipsState(active: true);
		lazyWindowInputController.Enable(restoreFocused: true);
	}

	protected virtual void OnBecameHiddenInStack()
	{
		ChangeTipsState(active: false);
		lazyWindowInputController.Disable(rememberFocused: true);
	}

	protected virtual Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		return new Dictionary<GameKey, Func<bool>> { 
		{
			GameKey.Back,
			OnPressedBack
		} };
	}

	protected virtual GamepadNavigationItem GetFocusedNavigationItem()
	{
		return null;
	}

	private void InitInputController()
	{
		GamepadNavigationController.OnFocusedItemChanged += OnFocusedItemChanged;
		lazyWindowInputController = new LazyWindowInputController(GamepadNavigationController, GetGameKeyDelegates(), GetFocusedNavigationItem);
	}

	private void InitCanvas()
	{
		canvas = GetComponent<Canvas>();
		canvas.overrideSorting = true;
	}

	private void InitButtonTipsStr()
	{
		LazyButtonTipsStr[] componentsInChildren = GetComponentsInChildren<LazyButtonTipsStr>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			LazyButtonTipsStr[] array = componentsInChildren;
			foreach (LazyButtonTipsStr lazyButtonTipsStr in array)
			{
				if (lazyButtonTipsStr.gameObject.activeSelf)
				{
					lazyButtonTips = lazyButtonTipsStr;
					break;
				}
			}
			if (lazyButtonTips == null)
			{
				lazyButtonTips = componentsInChildren[0];
			}
		}
		if (lazyButtonTips != null)
		{
			lazyButtonTips.Clear();
		}
	}

	private void OnInputChanged()
	{
		UpdateGamepadDependentStuff();
		if (IsShownAndTop)
		{
			lazyWindowInputController.UpdateGamepadDependentStuff();
		}
	}

	private void OnWindowBecameVisibleInStack(LazyWidgetBase window)
	{
		if (!(window != this) && isShown)
		{
			OnBecameVisibleInStack();
		}
	}

	private void OnWindowBecameHiddenInStack(LazyWidgetBase window)
	{
		if (!(window != this) && isShown)
		{
			OnBecameHiddenInStack();
		}
	}

	protected void ChangeTipsState(bool active)
	{
		lazyButtonTips?.gameObject.SetActive(active && LazyInput.IsGamepadActive);
	}
}
