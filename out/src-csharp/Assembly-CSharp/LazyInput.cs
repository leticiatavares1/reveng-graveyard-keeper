using System;
using System.Collections.Generic;
using UnityEngine;

public class LazyInput : MonoBehaviour
{
	private enum HoldedGroup
	{
		None,
		Navigation,
		Tabs,
		Slider
	}

	public const float HOLD_PRESS_DELAY = 0.3f;

	public const float DIRECTIONS_DELAY = 0.11f;

	public const float TABS_DELAY = 0.35f;

	public const float SLIDER_DELAY = 0.07f;

	public static readonly GameKey[] toolbar_keys = new GameKey[4]
	{
		GameKey.Toolbar1,
		GameKey.Toolbar2,
		GameKey.Toolbar3,
		GameKey.Toolbar4
	};

	private static Dictionary<GameKey, float> _can_be_holded = new Dictionary<GameKey, float>
	{
		{
			GameKey.Left,
			0.11f
		},
		{
			GameKey.Right,
			0.11f
		},
		{
			GameKey.Up,
			0.11f
		},
		{
			GameKey.Down,
			0.11f
		},
		{
			GameKey.PrevTab,
			0.35f
		},
		{
			GameKey.NextTab,
			0.35f
		},
		{
			GameKey.PrevSubTab,
			0.35f
		},
		{
			GameKey.NextSubTub,
			0.35f
		},
		{
			GameKey.SliderDec,
			0.07f
		},
		{
			GameKey.SliderInc,
			0.07f
		}
	};

	private static bool _cached;

	private static bool _gamepad_active = true;

	private static LazyInput _me;

	private static GamePadController _gamepad;

	private static KeyboardController _keyboard;

	private List<GameKey> _holded_keys = new List<GameKey>();

	private List<GameKey> _pressed_keys = new List<GameKey>();

	private List<GameKey> _wait_for_release = new List<GameKey>();

	private Vector2 _direction = Vector2.zero;

	private Vector2 _direction2 = Vector2.zero;

	private List<GameKey> _holded_for_press = new List<GameKey>();

	private List<float> _holded_for_press_delays = new List<float>();

	private Dictionary<HoldedGroup, float> _last_releases_in_group = new Dictionary<HoldedGroup, float>();

	public GameKey simulate_hold_key;

	public Vector2 simulate_direction = Vector2.zero;

	public static bool gamepad_active => _gamepad_active;

	private static LazyInput me
	{
		get
		{
			if (!_cached)
			{
				_me = SingletonGameObjects.FindOrCreate<LazyInput>();
				_gamepad = new GamePadController();
				_keyboard = new KeyboardController();
				_gamepad_active = _gamepad.active_at_start;
				_cached = true;
			}
			return _me;
		}
	}

	public static event Action on_input_changed;

	private void PressEmulateLeft()
	{
		EmulateGamepadPress(GamePadButton.Left);
	}

	private void PressEmulateRight()
	{
		EmulateGamepadPress(GamePadButton.Right);
	}

	private void PressEmulateUp()
	{
		EmulateGamepadPress(GamePadButton.Up);
	}

	private void PressEmulateDown()
	{
		EmulateGamepadPress(GamePadButton.Down);
	}

	private void ResumeTime()
	{
		Time.timeScale = 1f;
	}

	public static void Init()
	{
		Debug.Log(me.name + " started");
	}

	private void Update()
	{
		_gamepad.Update();
		_keyboard.Update();
		_pressed_keys.Clear();
		_holded_keys.Clear();
		bool flag = false;
		if (_gamepad.IsActive())
		{
			flag = !_gamepad_active;
			_gamepad_active = true;
		}
		else if (_keyboard.IsActive())
		{
			flag = _gamepad_active;
			_gamepad_active = false;
		}
		if (flag && LazyInput.on_input_changed != null)
		{
			LazyInput.on_input_changed();
		}
		foreach (GameKey item in _gamepad_active ? _gamepad.pressed_keys : _keyboard.pressed_keys)
		{
			AddPressed(item);
		}
		foreach (GameKey item2 in _gamepad_active ? _gamepad.holded_keys : _keyboard.holded_keys)
		{
			AddHolded(item2);
		}
		if (simulate_hold_key != 0)
		{
			AddHolded(simulate_hold_key);
		}
		_direction = (_gamepad_active ? _gamepad.direction : _keyboard.direction);
		_direction2 = (_gamepad_active ? _gamepad.direction2 : _keyboard.direction2);
		if (simulate_direction.magnitude > 0f)
		{
			_direction = simulate_direction;
		}
		UpdateHolded(Time.deltaTime);
	}

	public static bool GetKey(GameKey key)
	{
		if (!me._wait_for_release.Contains(key))
		{
			return me._holded_keys.Contains(key);
		}
		return false;
	}

	public static bool AnyKeyDown()
	{
		return me._pressed_keys.Count > 0;
	}

	public static bool AnyClick()
	{
		if (!GetKeyDown(GameKey.LeftClick))
		{
			return GetKeyDown(GameKey.RightClick);
		}
		return true;
	}

	public static bool GetKeyDown(GameKey key)
	{
		if (!me._wait_for_release.Contains(key))
		{
			return me._pressed_keys.Contains(key);
		}
		return false;
	}

	public static Vector2 GetDirection()
	{
		return me._direction;
	}

	public static Vector2 GetDirection2()
	{
		return me._direction2;
	}

	public static void ClearKey(GameKey key)
	{
		me._holded_keys.Remove(key);
	}

	public static void ClearKeyDown(GameKey key)
	{
		me._pressed_keys.Remove(key);
	}

	public static void ClearAllKeysDown()
	{
		me._pressed_keys.Clear();
	}

	public static void WaitForRelease(GameKey key)
	{
		if (key != 0 && !me._wait_for_release.Contains(key))
		{
			me._wait_for_release.Add(key);
		}
	}

	public static void WaitForReleaseNavigationKeys()
	{
		WaitForRelease(GameKey.Left);
		WaitForRelease(GameKey.Right);
		WaitForRelease(GameKey.Up);
		WaitForRelease(GameKey.Down);
	}

	public static void WaitForReleaseMouseKeys()
	{
		WaitForRelease(GameKey.LeftClick);
		WaitForRelease(GameKey.RightClick);
		GameKey[] array = KeyBindings.mouse_bindings[GameKey.LeftClick];
		for (int i = 0; i < array.Length; i++)
		{
			WaitForRelease(array[i]);
		}
		array = KeyBindings.mouse_bindings[GameKey.RightClick];
		for (int i = 0; i < array.Length; i++)
		{
			WaitForRelease(array[i]);
		}
	}

	public static bool IsNavigationKey(GameKey key)
	{
		if (key != GameKey.Up && key != GameKey.Down && key != GameKey.Up)
		{
			return key == GameKey.Up;
		}
		return true;
	}

	private void AddPressed(GameKey key)
	{
		if (key != 0 && !_pressed_keys.Contains(key) && !_wait_for_release.Contains(key))
		{
			_pressed_keys.Add(key);
		}
	}

	private HoldedGroup GetGroup(GameKey key)
	{
		switch (key)
		{
		case GameKey.Left:
		case GameKey.Right:
		case GameKey.Up:
		case GameKey.Down:
			return HoldedGroup.Navigation;
		case GameKey.PrevTab:
		case GameKey.NextTab:
		case GameKey.PrevSubTab:
		case GameKey.NextSubTub:
			return HoldedGroup.Tabs;
		case GameKey.SliderDec:
		case GameKey.SliderInc:
			return HoldedGroup.Slider;
		default:
			return HoldedGroup.None;
		}
	}

	private void AddHolded(GameKey key)
	{
		if (key != 0 && !_holded_keys.Contains(key))
		{
			_holded_keys.Add(key);
			if (_can_be_holded.ContainsKey(key) && !_holded_for_press.Contains(key) && !_wait_for_release.Contains(key))
			{
				_holded_for_press.Add(key);
				_holded_for_press_delays.Add(0.3f);
			}
		}
	}

	private void UpdateHolded(float delta_time)
	{
		for (int i = 0; i < _wait_for_release.Count; i++)
		{
			if (_holded_keys.Contains(_wait_for_release[i]))
			{
				_holded_keys.Remove(_wait_for_release[i]);
				continue;
			}
			_wait_for_release.RemoveAt(i);
			i--;
		}
		for (int j = 0; j < _holded_for_press.Count; j++)
		{
			GameKey gameKey = _holded_for_press[j];
			if (!_holded_keys.Contains(gameKey))
			{
				_holded_for_press.RemoveAt(j);
				_holded_for_press_delays.RemoveAt(j);
				j--;
				HoldedGroup group = GetGroup(gameKey);
				if (group != 0)
				{
					if (_last_releases_in_group.ContainsKey(group))
					{
						_last_releases_in_group[group] = Time.time;
					}
					else
					{
						_last_releases_in_group.Add(group, Time.time);
					}
				}
			}
			else
			{
				_holded_for_press_delays[j] -= delta_time;
				if (_holded_for_press_delays[j] <= 0f)
				{
					AddPressed(gameKey);
					_holded_for_press_delays[j] = _can_be_holded[gameKey];
				}
			}
		}
	}

	public static void Vibrate(float value, float duration)
	{
		_gamepad.Vibrate(value, duration);
	}

	public static void EmulateGamepadPress(GamePadButton button)
	{
		_gamepad.EmulateKeyPress(button);
	}
}
