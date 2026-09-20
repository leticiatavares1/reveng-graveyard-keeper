using System;
using UnityEngine;

public class SmartSlider : MonoBehaviour
{
	public const int GAMEPAD_FAST_SLIDER_CHANGE_VALUE = 10;

	public UILabel min_counter;

	public UILabel max_counter;

	private UILabel _input_label;

	private UIInput _input_field;

	private Collider2D _input_collider;

	private UISlider _slider;

	private int _min;

	private int _max;

	private int _step_for_game_keys = 1;

	private int _prev_value;

	private bool _input_field_enabled;

	private bool _game_keys_enabled;

	private Action<int> _on_value_changed;

	public bool auto_steps;

	public int value => _min + Mathf.RoundToInt((float)(_max - _min) * _slider.value);

	public bool input_field_enabled
	{
		get
		{
			return _input_field_enabled;
		}
		set
		{
			_input_field_enabled = value;
		}
	}

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

	public int number_of_steps
	{
		get
		{
			return _slider.numberOfSteps;
		}
		set
		{
			_slider.numberOfSteps = value;
		}
	}

	public void Init()
	{
		_slider = GetComponentInChildren<UISlider>(includeInactive: true);
		_slider.onChange.Add(new EventDelegate(OnSliderChanged));
		_input_field = GetComponentInChildren<UIInput>(includeInactive: true);
		_input_label = _input_field.GetComponent<UILabel>();
		_input_collider = _input_field.GetComponent<Collider2D>();
		_input_field.onChange.Add(new EventDelegate(OnInputFieldChanged));
		_input_field.onSubmit.Add(new EventDelegate(OnInputFieldSubmit));
	}

	public void Open(int value, int min, int max, Action<int> on_value_changed, bool input_field_enabled = false, bool game_keys_enabled = false, int step_for_game_keys = 1)
	{
		_min = min;
		_max = max;
		_prev_value = -9999;
		_slider.numberOfSteps = (auto_steps ? (_max - _min) : 0);
		if (min_counter != null)
		{
			min_counter.text = _min.ToString();
		}
		if (max_counter != null)
		{
			max_counter.text = _max.ToString();
		}
		_on_value_changed = on_value_changed;
		_input_collider.enabled = (_input_field_enabled = input_field_enabled);
		_game_keys_enabled = game_keys_enabled;
		_step_for_game_keys = ((step_for_game_keys == 0) ? 1 : step_for_game_keys);
		SetValue(value, invoke_callback: false);
		UIInput input_field = _input_field;
		string text2 = (_input_label.text = value.ToString());
		input_field.value = text2;
	}

	private void SetValue(int new_value, bool invoke_callback = true)
	{
		if (new_value < _min)
		{
			new_value = _min;
		}
		if (new_value > _max)
		{
			new_value = _max;
		}
		_slider.value = (float)(new_value - _min) / (float)(_max - _min);
		if (invoke_callback && _on_value_changed != null)
		{
			_on_value_changed(value);
		}
	}

	public void OnSliderChanged()
	{
		if (_prev_value == value)
		{
			return;
		}
		_prev_value = value;
		string text = value.ToString();
		if (_input_field_enabled)
		{
			if (_input_field.value != text && !_input_field.isSelected)
			{
				_input_field.value = text;
			}
		}
		else
		{
			_input_label.text = text;
		}
		if (_on_value_changed != null)
		{
			_on_value_changed(value);
		}
		if (!Sounds.WasAnySoundPlayedThisFrame())
		{
			Sounds.OnGUIClick();
		}
	}

	public void OnInputFieldChanged()
	{
		if (!_input_field_enabled)
		{
			return;
		}
		int result = 0;
		if (int.TryParse(_input_field.value, out result))
		{
			if (result < _min)
			{
				result = _min;
			}
			if (result > _max)
			{
				result = _max;
			}
			if (result != value)
			{
				SetValue(result);
			}
		}
	}

	public void OnInputFieldSubmit()
	{
		if (_input_field_enabled)
		{
			_input_field.isSelected = false;
			OnSliderChanged();
		}
	}

	private void Update()
	{
		if (_game_keys_enabled && !_input_field.isSelected)
		{
			if (LazyInput.GetKeyDown(GameKey.SliderInc))
			{
				ChangeValue(_step_for_game_keys);
			}
			else if (LazyInput.GetKeyDown(GameKey.SliderDec))
			{
				ChangeValue(-_step_for_game_keys);
			}
			else if (LazyInput.GetKeyDown(GameKey.PrevTab))
			{
				ChangeValue(-10);
			}
			else if (LazyInput.GetKeyDown(GameKey.NextTab))
			{
				ChangeValue(10);
			}
		}
	}

	private void ChangeValue(int delta)
	{
		SetValue(value + delta);
	}
}
