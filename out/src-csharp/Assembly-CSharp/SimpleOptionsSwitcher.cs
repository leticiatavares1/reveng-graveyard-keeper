using System;
using UnityEngine;

public class SimpleOptionsSwitcher : MonoBehaviour
{
	[HideInInspector]
	public UILabel label;

	private int _current_option_index;

	private int _max_option_index;

	private bool _game_keys_enabled;

	private Action<int, UILabel> _on_changed;

	public bool game_keys_enabled
	{
		get
		{
			return _game_keys_enabled;
		}
		set
		{
			_game_keys_enabled = value;
		}
	}

	public void Init(int current_option_index, int max_option_index, string current_option_name, Action<int, UILabel> on_changed, bool call_onchanged_on_init = false)
	{
		if (label == null)
		{
			label = GetComponentInChildren<UILabel>(includeInactive: true);
		}
		label.text = current_option_name;
		_current_option_index = current_option_index;
		_max_option_index = max_option_index;
		_on_changed = on_changed;
		if (call_onchanged_on_init)
		{
			_on_changed(_current_option_index, label);
		}
	}

	public void Dec()
	{
		if (--_current_option_index < 0)
		{
			_current_option_index = _max_option_index;
		}
		if (_on_changed != null)
		{
			_on_changed(_current_option_index, label);
		}
		Debug.Log("new index = " + _current_option_index);
	}

	public void Inc()
	{
		if (++_current_option_index > _max_option_index)
		{
			_current_option_index = 0;
		}
		if (_on_changed != null)
		{
			_on_changed(_current_option_index, label);
		}
		Debug.Log("new index = " + _current_option_index);
	}

	private void Update()
	{
		if (_game_keys_enabled)
		{
			if (LazyInput.GetKeyDown(GameKey.SliderInc))
			{
				Inc();
			}
			else if (LazyInput.GetKeyDown(GameKey.SliderDec))
			{
				Dec();
			}
		}
	}
}
