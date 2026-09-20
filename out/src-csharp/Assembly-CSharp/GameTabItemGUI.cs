using UnityEngine;

public class GameTabItemGUI : MonoBehaviour
{
	private UIButton _button;

	private UI2DSprite _sprite;

	private UILabel _label;

	private GameGUI.TabType _tab_type;

	private Color _default_color;

	private BaseGameGUI _gui;

	private bool _selected;

	public GameObject go_active;

	public void Init(GameGUI.TabType tab_type, BaseGameGUI gui)
	{
		_tab_type = tab_type;
		_sprite = GetComponentInChildren<UI2DSprite>(includeInactive: true);
		_button = GetComponentInChildren<UIButton>(includeInactive: true);
		_default_color = _button.defaultColor;
		_label = GetComponentInChildren<UILabel>(includeInactive: true);
		_label.text = GJL.L("tab_" + tab_type);
		base.name = tab_type.ToString();
		(base.gameObject.GetComponent<LocalizedLabel>() ?? base.gameObject.AddComponent<LocalizedLabel>()).token = "tab_" + tab_type;
		_gui = gui;
		_selected = false;
		this.Activate();
	}

	public void OnPressed()
	{
		if (!BaseGUI.IsLastClickRightButton())
		{
			GUIElements.me.game_gui.SelectTab(_tab_type);
			Sounds.OnGUITabClick();
		}
	}

	public void Select()
	{
		if (!_selected)
		{
			_selected = true;
			_button.defaultColor = _button.pressed;
			_button.enabled = false;
			Sounds.ignore_window_sounds = true;
			_gui.OpenFromGameGUI();
			Sounds.ignore_window_sounds = false;
			go_active.SetActive(value: true);
		}
	}

	public void Unselect()
	{
		if (_selected)
		{
			_selected = false;
			_button.defaultColor = _default_color;
			_button.enabled = true;
			Sounds.ignore_window_sounds = true;
			_gui.CloseFromGameGUI();
			Sounds.ignore_window_sounds = false;
			go_active.SetActive(value: false);
		}
	}
}
