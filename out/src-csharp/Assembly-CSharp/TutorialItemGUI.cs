using System;
using UnityEngine;

[ExecuteInEditMode]
public class TutorialItemGUI : MonoBehaviour
{
	public UIWidget gamepad_frame;

	public string locale_token;

	public GameObject tutorial_window_obj;

	public EventDelegate on_pressed;

	public DLCEngine.DLCVersion dlc_version;

	private BaseTutorialGUI _menu;

	private GamepadNavigationItem _gamepad_item;

	private SmartSlider _slider;

	private SimpleOptionsSwitcher _options_switcher;

	public GameObject additional_go;

	private bool _gamepad_active;

	private bool _overed;

	private const float GAMEPAD_SELECTION_ANIM_TIME = 0f;

	public GamepadNavigationItem gamepad_item => _gamepad_item;

	public bool gamepad_active
	{
		get
		{
			return _gamepad_active;
		}
		set
		{
			_gamepad_active = value;
			_gamepad_item.active = _gamepad_active;
		}
	}

	public void Init(BaseTutorialGUI menu)
	{
		if (!DLCEngine.IsDLCAvailable(dlc_version))
		{
			GUIElements.me.tutorial_windows_gui.RemoveNotAvailableTutorialItem(this);
			return;
		}
		_menu = menu;
		_gamepad_item = GetComponent<GamepadNavigationItem>();
		_gamepad_item.SetCallbacks(OnOver, delegate
		{
			OnOut();
		}, OnTutorialItemSelect);
		_slider = GetComponentInChildren<SmartSlider>(includeInactive: true);
		if (_slider != null)
		{
			_slider.Init();
		}
		_options_switcher = GetComponentInChildren<SimpleOptionsSwitcher>(includeInactive: true);
		LocalizedLabel componentInChildren = GetComponentInChildren<LocalizedLabel>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.token = locale_token;
			componentInChildren.Localize();
		}
		NGUIExtensionMethods.InitEventTriggers(this, OnMouseOvered, OnMouseOuted, OnMousePressed, clear_previous: true);
	}

	public void Show()
	{
		_overed = false;
		gamepad_frame.Deactivate();
		gamepad_frame.alpha = 0f;
	}

	public void SetupSlider(int value, int min, int max, Action<int> on_value_changed, int game_key_step = 1, int number_of_steps = 0)
	{
		if (_slider == null)
		{
			Debug.Log("Cannot setup slider because slider doesn't exist", this);
			return;
		}
		_slider.Open(value, min, max, on_value_changed, input_field_enabled: false, game_keys_enabled: false, game_key_step);
		_slider.number_of_steps = number_of_steps;
	}

	public void SetupOptions(int current_option_index, int max_option_index, string current_option_name, Action<int, UILabel> on_changed, bool call_onchanged_on_init = false)
	{
		if (_options_switcher == null)
		{
			Debug.Log("Cannot setup slider because slider doesn't exist", this);
		}
		else
		{
			_options_switcher.Init(current_option_index, max_option_index, current_option_name, on_changed, call_onchanged_on_init);
		}
	}

	public void OnMousePressed()
	{
		if (!BaseGUI.for_gamepad)
		{
			OnTutorialItemSelect();
		}
	}

	public void OnMouseOvered()
	{
		if (!BaseGUI.for_gamepad)
		{
			OnOver();
		}
	}

	public void OnMouseOuted()
	{
		if (!BaseGUI.for_gamepad)
		{
			OnOut();
		}
	}

	public void OnTutorialItemSelect()
	{
		on_pressed.TryExecute();
		if (tutorial_window_obj != null && !string.IsNullOrEmpty(tutorial_window_obj.name))
		{
			GUIElements.me.tutorial.Open(tutorial_window_obj.name);
		}
		Sounds.OnGUIClick();
	}

	public void OnOver()
	{
		gamepad_frame.Activate();
		gamepad_frame.ChangeAlpha(0f, 1f, 0f);
		_overed = true;
		if (_menu != null)
		{
			_menu.UpdatTip(select_active: true);
		}
		if (_slider != null)
		{
			_slider.game_keys_enabled = true;
		}
		if (_options_switcher != null)
		{
			_options_switcher.game_keys_enabled = true;
		}
		if (!Sounds.WasAnySoundPlayedThisFrame())
		{
			Sounds.OnGUIHover(Sounds.ElementType.Button);
		}
	}

	public void OnOut(bool animate = true)
	{
		_overed = false;
		if (animate)
		{
			gamepad_frame.ChangeAlpha(gamepad_frame.alpha, 0f, 0f, delegate
			{
				if (!_overed)
				{
					gamepad_frame.Deactivate();
				}
			});
		}
		else
		{
			gamepad_frame.Deactivate();
			gamepad_frame.alpha = 0f;
		}
		if (_slider != null)
		{
			_slider.game_keys_enabled = false;
		}
		if (_options_switcher != null)
		{
			_options_switcher.game_keys_enabled = false;
		}
	}

	private void OnValidate()
	{
		if (!Application.isPlaying && !string.IsNullOrEmpty(locale_token) && !(GetComponentInParent<UIRoot>() == null))
		{
			if (base.name != locale_token)
			{
				base.name = locale_token;
			}
			if (GetComponentInChildren<UILabel>(includeInactive: true).text != locale_token)
			{
				GetComponentInChildren<UILabel>(includeInactive: true).text = locale_token;
			}
			if (GetComponentInChildren<LocalizedLabel>(includeInactive: true).token != locale_token)
			{
				GetComponentInChildren<LocalizedLabel>(includeInactive: true).token = locale_token;
			}
		}
	}
}
