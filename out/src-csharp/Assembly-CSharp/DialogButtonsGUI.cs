using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class DialogButtonsGUI : MonoBehaviour
{
	private List<DialogButtonGUI> _buttons;

	private UITable _table;

	private ButtonTipsStr _button_tips;

	private PlatformDependentElement[] platform_dependent_elements;

	private List<string> _texts = new List<string>();

	private List<GJCommons.VoidDelegate> _delegates = new List<GJCommons.VoidDelegate>();

	private List<GameKey> _game_keys = new List<GameKey>();

	private List<bool> _enables = new List<bool>();

	private bool _initialized;

	public void Init()
	{
		if (_initialized)
		{
			return;
		}
		_buttons = GetComponentsInChildren<DialogButtonGUI>(includeInactive: true).ToList();
		foreach (DialogButtonGUI button in _buttons)
		{
			button.Init(this);
		}
		platform_dependent_elements = GetComponentsInChildren<PlatformDependentElement>(includeInactive: true);
		_button_tips = GetComponentInChildren<ButtonTipsStr>(includeInactive: true);
		_table = GetComponentInChildren<UITable>(includeInactive: true);
		_initialized = true;
	}

	private void Update()
	{
		for (int i = 0; i < _game_keys.Count; i++)
		{
			if (_enables[i] && LazyInput.GetKeyDown(_game_keys[i]))
			{
				LazyInput.ClearKeyDown(_game_keys[i]);
				InvokeOption(i);
				break;
			}
		}
	}

	public void Set(string text_1, GJCommons.VoidDelegate delegate_1, string text_2 = null, GJCommons.VoidDelegate delegate_2 = null, string text_3 = null, GJCommons.VoidDelegate delegate_3 = null, GameKey key_1 = GameKey.Select, GameKey key_2 = GameKey.Back)
	{
		Init();
		_texts.Clear();
		_delegates.Clear();
		_game_keys.Clear();
		_texts.Add(text_1);
		if (!string.IsNullOrEmpty(text_2))
		{
			_texts.Add(text_2);
		}
		if (!string.IsNullOrEmpty(text_3))
		{
			_texts.Add(text_3);
		}
		_delegates.Add(delegate_1);
		if (delegate_2 != null)
		{
			_delegates.Add(delegate_2);
		}
		if (delegate_3 != null)
		{
			_delegates.Add(delegate_3);
		}
		_game_keys.Add(key_1);
		_game_keys.Add(key_2);
		SetEnabled();
		Redraw();
	}

	public void SetEnabled(bool enable_1 = true, bool enable_2 = true, bool enable_3 = true)
	{
		Init();
		_enables.Clear();
		_enables.Add(enable_1);
		_enables.Add(enable_2);
		_enables.Add(enable_3);
		Redraw();
		_buttons[0].SetEnabled(enable_1);
		if (_buttons.Count >= 2)
		{
			_buttons[1].SetEnabled(enable_2);
		}
	}

	private void Redraw()
	{
		_button_tips.SetActive(BaseGUI.for_gamepad);
		_table.SetActive(!BaseGUI.for_gamepad);
		if (BaseGUI.for_gamepad)
		{
			List<GameKeyTip> list = new List<GameKeyTip>();
			for (int i = 0; i < _texts.Count; i++)
			{
				list.Add(new GameKeyTip(_game_keys[i], _texts[i], _enables[i]));
			}
			_button_tips.Print(list);
			return;
		}
		for (int j = 0; j < 3; j++)
		{
			if (j < _buttons.Count)
			{
				if (j >= _texts.Count)
				{
					_buttons[j].Deactivate();
					continue;
				}
				_buttons[j].Activate();
				_buttons[j].SetText(_texts[j]);
				_buttons[j].SetEnabled(_enables[j]);
			}
		}
		if (_texts.Count == 1)
		{
			_table.Reposition();
			return;
		}
		if (_buttons == null || _buttons.Count == 0)
		{
			Debug.LogError("No buttons found", this);
			return;
		}
		int num = _buttons.Select((DialogButtonGUI button) => button.GetWidth()).Max();
		foreach (DialogButtonGUI button in _buttons)
		{
			int width = button.GetWidth();
			if (width > num)
			{
				num = width;
			}
		}
		foreach (DialogButtonGUI button2 in _buttons)
		{
			button2.SetWidth(num);
		}
		_table.Reposition();
	}

	public void OnBtnClicked(DialogButtonGUI btn)
	{
		InvokeOption(_buttons.IndexOf(btn));
	}

	public void InvokeOption(int index)
	{
		if (index >= 0)
		{
			if (index >= _delegates.Count)
			{
				Debug.LogError("button delegate index is too large = " + index + ", delegates = " + _delegates.Count, this);
			}
			else
			{
				_delegates[index].TryInvoke();
			}
		}
	}
}
