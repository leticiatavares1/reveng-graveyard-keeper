using System;
using LinqTools;
using UnityEngine;

public class TimeMachineItemGUI : MonoBehaviour
{
	private const string CLOSED_ITEM_LOCALE = "dlc_cutscene_name_locked";

	private const string CLOSED_DATE_LOCALE = "dlc_cutscene_date_locked";

	[SerializeField]
	private UIButton _button;

	[SerializeField]
	private UILabel _scene_name_label;

	[SerializeField]
	private UILabel _scene_date_label;

	[SerializeField]
	private UI2DSprite _icon;

	[SerializeField]
	[Header("Settings")]
	private Color _closed_color;

	[SerializeField]
	private Color _opened_color;

	[SerializeField]
	private Sprite _closed_icon;

	[Space]
	[SerializeField]
	private GameObject _disabled_button_image;

	[SerializeField]
	private GameObject _selection_frame;

	private string _scene_id;

	private Sprite _scene_sprite;

	private string _flow_script;

	private string _name_locale_key;

	private string _date_locale_key;

	private Action<string> _on_pressed;

	private GamepadNavigationItem _gamepad_navigation_item;

	private TimeMachineGUI _time_machine_gui;

	public PanelAutoScroll auto_scroll;

	public GamepadNavigationItem gamepad_navigation_item
	{
		get
		{
			if (_gamepad_navigation_item == null)
			{
				_gamepad_navigation_item = GetComponentInChildren<GamepadNavigationItem>(includeInactive: true);
			}
			return _gamepad_navigation_item;
		}
	}

	public void Initialize(string scene_id, string item_name, string flow_script, Action<string> on_pressed)
	{
		_scene_id = scene_id;
		_scene_sprite = EasySpritesCollection.GetSprite(item_name);
		_flow_script = flow_script;
		_name_locale_key = scene_id + "_name";
		_date_locale_key = scene_id + "_date";
		_on_pressed = on_pressed;
		_time_machine_gui = GUIElements.me.time_machine_gui;
		if (BaseGUI.for_gamepad && gamepad_navigation_item != null)
		{
			gamepad_navigation_item.SetCallbacks(OnOver, OnOut, OnItemAction);
		}
	}

	public void CheckEnabled(bool is_enabled)
	{
		if (is_enabled)
		{
			_scene_name_label.color = _opened_color;
			_scene_date_label.color = _opened_color;
			_scene_name_label.applyGradient = true;
			_scene_date_label.applyGradient = true;
			_icon.sprite2D = _scene_sprite;
			_icon.ResizeByContent();
			_button.enabled = true;
			_disabled_button_image.SetActive(value: false);
			_scene_name_label.text = GJL.L(_name_locale_key);
			_scene_date_label.text = GJL.L(_date_locale_key);
		}
		else
		{
			_scene_name_label.color = _closed_color;
			_scene_date_label.color = _closed_color;
			_scene_name_label.applyGradient = false;
			_scene_date_label.applyGradient = false;
			_icon.sprite2D = _closed_icon;
			_icon.ResizeByContent();
			_button.enabled = false;
			_disabled_button_image.SetActive(value: true);
			_scene_name_label.text = GJL.L("dlc_cutscene_name_locked");
			_scene_date_label.text = GJL.L("dlc_cutscene_date_locked");
		}
	}

	public void OnPlayPressed()
	{
		_on_pressed?.Invoke(_flow_script);
	}

	private void OnOver()
	{
		if (BaseGUI.for_gamepad)
		{
			if (auto_scroll != null && _time_machine_gui.items.Count > 0 && (this == _time_machine_gui.items[0] || this == _time_machine_gui.items.Last()) && auto_scroll != null)
			{
				auto_scroll.Perform();
			}
			_time_machine_gui.button_tips.Print(GameKeyTip.Select("select", _button.enabled), GameKeyTip.Close());
		}
	}

	private void OnOut()
	{
	}

	private void OnItemAction()
	{
		if (_button.enabled)
		{
			OnPlayPressed();
		}
	}

	public void OnMouseOvered()
	{
		if (!BaseGUI.for_gamepad)
		{
			_selection_frame.gameObject.SetActive(value: true);
		}
	}

	public void OnMouseOuted()
	{
		if (!BaseGUI.for_gamepad)
		{
			_selection_frame.gameObject.SetActive(value: false);
		}
	}
}
