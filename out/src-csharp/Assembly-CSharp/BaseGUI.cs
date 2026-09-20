using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class BaseGUI : MonoBehaviour
{
	public delegate void OnAnyWindowStateChanged(BaseGUI window_obj);

	protected Dictionary<GameKey, GJCommons.BoolDelegate> gamekey_delegates = new Dictionary<GameKey, GJCommons.BoolDelegate>();

	public static GJCommons.VoidDelegate on_all_closed;

	public static List<BaseGUI> opened_windows = new List<BaseGUI>();

	private static bool _opened_for_gamepad;

	private static BaseGUI _active_gui;

	public bool add_to_opened_stack = true;

	private bool _is_shown;

	private int _open_frame;

	private int _hide_frame;

	private GamepadNavigationController _gamepad_navigation_controller;

	private ButtonTipsStr _button_tips;

	private UIRect _ui_rect;

	private bool _ui_rect_cached;

	private GJCommons.VoidDelegate _on_hide;

	private bool _call_on_hide_only_one_time;

	private bool _need_recalc_anchors;

	private List<ScrollWithKeyboard> _keyboard_scrolls = new List<ScrollWithKeyboard>();

	public static bool for_gamepad
	{
		get
		{
			return _opened_for_gamepad;
		}
		set
		{
			_opened_for_gamepad = value;
		}
	}

	public static bool all_guis_closed => opened_windows.Count == 0;

	public static BaseGUI active_gui => _active_gui;

	public bool is_just_opened => Mathf.Abs(_open_frame - Time.frameCount) <= 1;

	public bool is_just_hided => Mathf.Abs(_hide_frame - Time.frameCount) <= 1;

	public bool is_shown => _is_shown;

	public bool is_shown_and_top
	{
		get
		{
			if (_is_shown && opened_windows.Count > 0)
			{
				return opened_windows.LastElement() == this;
			}
			return false;
		}
	}

	public GamepadNavigationController gamepad_controller => _gamepad_navigation_controller;

	public ButtonTipsStr button_tips => _button_tips;

	public UIRect ui_rect
	{
		get
		{
			if (_ui_rect_cached)
			{
				return _ui_rect;
			}
			_ui_rect_cached = true;
			return _ui_rect = GetComponent<UIRect>();
		}
	}

	private string window_type_name
	{
		get
		{
			Type type = GetType();
			if (!(type == null))
			{
				return type.Name;
			}
			return "[???]";
		}
	}

	public static event OnAnyWindowStateChanged on_window_opened;

	public static event OnAnyWindowStateChanged on_window_closed;

	public virtual void Open()
	{
		Open(play_open_sound: true);
	}

	protected void Open(bool play_open_sound)
	{
		Debug.Log("<color=green>Open GUI:</color> " + window_type_name);
		if (_is_shown)
		{
			Debug.LogWarning("window " + base.name + " is already opened", this);
		}
		UpdateSourceType(force: false);
		_open_frame = Time.frameCount;
		_is_shown = true;
		base.gameObject.SetActive(value: true);
		LazyInput.WaitForReleaseNavigationKeys();
		InitPlatformDependentStuff();
		UpdateLocalizedLabels();
		UpdatePixelPerfect();
		if (add_to_opened_stack)
		{
			if (play_open_sound)
			{
				Sounds.OnWindowOpened();
			}
			BaseGUI baseGUI = opened_windows.LastElement();
			if (baseGUI != null && baseGUI != this)
			{
				baseGUI.OnHiddenByAnotherGUI();
			}
			if (opened_windows.Contains(this) && baseGUI == this)
			{
				opened_windows.Remove(this);
			}
			opened_windows.Add(this);
			_active_gui = this;
		}
		if (BaseGUI.on_window_opened != null)
		{
			BaseGUI.on_window_opened(this);
		}
		if (!for_gamepad)
		{
			_keyboard_scrolls = GetComponentsInChildren<ScrollWithKeyboard>(includeInactive: true).ToList();
		}
	}

	protected void UpdateSourceType(bool force)
	{
		if (force || (add_to_opened_stack && opened_windows.Count == 0))
		{
			_opened_for_gamepad = LazyInput.gamepad_active;
		}
	}

	public static void UpdateSourceType()
	{
		if (opened_windows.Count == 0)
		{
			_opened_for_gamepad = LazyInput.gamepad_active;
		}
	}

	public virtual void UpdateLocalizedLabels()
	{
		LocalizedLabel[] componentsInChildren = GetComponentsInChildren<LocalizedLabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Localize();
		}
		GJL.EnsureChildLabelsHasCorrectFont(base.gameObject);
	}

	protected void UpdatePixelPerfect()
	{
		PixelPerfectGUI[] componentsInChildren = GetComponentsInChildren<PixelPerfectGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].LateUpdate();
		}
	}

	public virtual void Update()
	{
		if (!is_shown_and_top)
		{
			return;
		}
		foreach (GameKey key in gamekey_delegates.Keys)
		{
			if (LazyInput.GetKeyDown(key) && gamekey_delegates[key]())
			{
				LazyInput.ClearKeyDown(key);
			}
		}
		if (CanCloseWithRightClick() && Input.GetMouseButtonDown(1))
		{
			OnRightClick();
		}
		if (for_gamepad || _keyboard_scrolls == null || _keyboard_scrolls.Count <= 0)
		{
			return;
		}
		foreach (ScrollWithKeyboard keyboard_scroll in _keyboard_scrolls)
		{
			if (keyboard_scroll.scroll_view == null)
			{
				continue;
			}
			Vector2 direction = LazyInput.GetDirection();
			float num = 0f;
			switch (keyboard_scroll.scroll_type)
			{
			case ScrollWithKeyboard.KeyboardScrollType.Vertical:
				if (keyboard_scroll.scroll_view.shouldMoveVertically)
				{
					num = direction.y * keyboard_scroll.scroll_sensivity;
				}
				break;
			case ScrollWithKeyboard.KeyboardScrollType.Horizontal:
				if (keyboard_scroll.scroll_view.shouldMoveHorizontally)
				{
					num = direction.x * keyboard_scroll.scroll_sensivity;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (!num.EqualsTo(0f))
			{
				keyboard_scroll.scroll_view.Scroll(num);
			}
		}
	}

	protected virtual bool CanCloseWithRightClick()
	{
		return false;
	}

	public virtual void OnAboveWindowClosed()
	{
	}

	public virtual void OnHiddenByAnotherGUI()
	{
		if ((bool)gamepad_controller)
		{
			gamepad_controller.Disable();
		}
	}

	protected void InitPlatformDependentStuff()
	{
		if (gamepad_controller != null)
		{
			if (for_gamepad)
			{
				gamepad_controller.Enable();
			}
			else if (gamepad_controller.is_enabled)
			{
				gamepad_controller.Disable();
			}
		}
		PlatformDependentElement[] componentsInChildren = GetComponentsInChildren<PlatformDependentElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init(for_gamepad);
		}
	}

	public virtual void Hide(bool play_hide_sound = true)
	{
		if (MainGame.game_started)
		{
			Debug.Log("<color=green>Hide GUI:</color> " + window_type_name);
		}
		_hide_frame = Time.frameCount;
		base.gameObject.SetActive(value: false);
		_is_shown = false;
		LazyInput.ClearAllKeysDown();
		LazyInput.WaitForReleaseMouseKeys();
		if (_gamepad_navigation_controller != null)
		{
			_gamepad_navigation_controller.Disable();
		}
		if (opened_windows.Contains(this))
		{
			opened_windows.Remove(this);
		}
		_active_gui = ((opened_windows.Count == 0) ? null : opened_windows.LastElement());
		if (add_to_opened_stack && BaseGUI.on_window_closed != null)
		{
			BaseGUI.on_window_closed(this);
		}
		if (add_to_opened_stack)
		{
			if (play_hide_sound && MainGame.game_started)
			{
				Sounds.OnClosePressed();
			}
			if (opened_windows.Count == 0)
			{
				on_all_closed.TryInvoke();
			}
			else
			{
				opened_windows.LastElement().OnAboveWindowClosed();
			}
		}
		GJCommons.VoidDelegate on_hide = _on_hide;
		if (_call_on_hide_only_one_time)
		{
			_on_hide = null;
		}
		on_hide.TryInvoke();
		MainGame.me.player.components.interaction.RedrawCurrentInteractiveHint();
		BuffsLogics.CheckBuffsGiveConditions();
	}

	public virtual void Init()
	{
		_gamepad_navigation_controller = GetComponent<GamepadNavigationController>();
		ButtonTipsStr[] componentsInChildren = GetComponentsInChildren<ButtonTipsStr>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			ButtonTipsStr[] array = componentsInChildren;
			foreach (ButtonTipsStr buttonTipsStr in array)
			{
				if (buttonTipsStr.gameObject.activeSelf)
				{
					_button_tips = buttonTipsStr;
					break;
				}
			}
			if (_button_tips == null)
			{
				_button_tips = componentsInChildren[0];
			}
		}
		if (_button_tips != null)
		{
			_button_tips.Clear();
		}
		Tooltip[] componentsInChildren2 = GetComponentsInChildren<Tooltip>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].Init();
		}
		Hide();
		gamekey_delegates = new Dictionary<GameKey, GJCommons.BoolDelegate>
		{
			{
				GameKey.Select,
				OnPressedSelect
			},
			{
				GameKey.Back,
				OnPressedBack
			},
			{
				GameKey.Option1,
				OnPressedOption1
			},
			{
				GameKey.Option2,
				OnPressedOption2
			},
			{
				GameKey.SliderDec,
				OnPressedSliderDec
			},
			{
				GameKey.SliderInc,
				OnPressedSliderInc
			},
			{
				GameKey.PrevTab,
				OnPressedPrevTab
			},
			{
				GameKey.NextTab,
				OnPressedNextTab
			},
			{
				GameKey.PrevSubTab,
				OnPressedPrevSubTab
			},
			{
				GameKey.NextSubTub,
				OnPressedNextSubTab
			},
			{
				GameKey.Left,
				OnPressedLeft
			},
			{
				GameKey.Right,
				OnPressedRight
			},
			{
				GameKey.Up,
				OnPressedUp
			},
			{
				GameKey.Down,
				OnPressedDown
			}
		};
	}

	public void SetOnHide(GJCommons.VoidDelegate on_hide, bool call_only_one_time = true)
	{
		_on_hide = on_hide;
		_call_on_hide_only_one_time = call_only_one_time;
	}

	public virtual void OnClosePressed()
	{
		Hide();
	}

	protected virtual void OnInputSourceChanged()
	{
	}

	public void SoundOnMouseOverCloseButton()
	{
		Sounds.OnGUIHover(Sounds.ElementType.ItemCell);
	}

	public void SoundOnMouseOverButton()
	{
		Sounds.OnGUIHover();
	}

	protected virtual bool OnPressedSelect()
	{
		if (gamepad_controller.is_enabled && gamepad_controller.auto_select)
		{
			gamepad_controller.SelectFocusedItem();
			return true;
		}
		return false;
	}

	protected virtual bool OnPressedBack()
	{
		return false;
	}

	protected virtual bool OnPressedOption1()
	{
		return false;
	}

	protected virtual bool OnPressedOption2()
	{
		return false;
	}

	protected virtual bool OnPressedSliderDec()
	{
		return false;
	}

	protected virtual bool OnPressedSliderInc()
	{
		return false;
	}

	protected virtual bool OnPressedPrevTab()
	{
		return false;
	}

	protected virtual bool OnPressedNextTab()
	{
		return false;
	}

	protected virtual bool OnPressedPrevSubTab()
	{
		return false;
	}

	protected virtual bool OnPressedNextSubTab()
	{
		return false;
	}

	protected virtual bool OnPressedLeft()
	{
		if (!LazyInput.gamepad_active || !gamepad_controller.is_enabled)
		{
			return false;
		}
		gamepad_controller.Navigate(Direction.Left);
		return true;
	}

	protected virtual bool OnPressedRight()
	{
		if (!LazyInput.gamepad_active || !gamepad_controller.is_enabled)
		{
			return false;
		}
		gamepad_controller.Navigate(Direction.Right);
		return true;
	}

	protected virtual bool OnPressedUp()
	{
		if (!LazyInput.gamepad_active || !gamepad_controller.is_enabled)
		{
			return false;
		}
		gamepad_controller.Navigate(Direction.Up);
		return true;
	}

	protected virtual bool OnPressedDown()
	{
		if (!LazyInput.gamepad_active || !gamepad_controller.is_enabled)
		{
			return false;
		}
		gamepad_controller.Navigate(Direction.Down);
		return true;
	}

	public void RecalcAllAnchors()
	{
		DoRecalcAllAnchors();
		_need_recalc_anchors = true;
	}

	private void DoRecalcAllAnchors()
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateAnchors();
		}
		UITable[] componentsInChildren2 = GetComponentsInChildren<UITable>(includeInactive: true);
		foreach (UITable obj in componentsInChildren2)
		{
			obj.repositionNow = true;
			obj.Reposition();
			obj.repositionNow = true;
		}
		componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateAnchors();
		}
	}

	public void LateUpdate()
	{
		if (_need_recalc_anchors)
		{
			DoRecalcAllAnchors();
			_need_recalc_anchors = false;
		}
	}

	public void UpdateAllAnchors()
	{
		BroadcastMessage("UpdateAnchors", SendMessageOptions.DontRequireReceiver);
		GetComponentInParent<UIPanel>().BroadcastMessage("UpdateAnchors", SendMessageOptions.DontRequireReceiver);
	}

	public static bool IsLastClickRightButton()
	{
		return UICamera.currentTouchID == -2;
	}

	protected virtual void OnRightClick()
	{
		OnClosePressed();
	}
}
