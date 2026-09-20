using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class GamePadController : BaseInputController
{
	private enum GuiNavigationAxis
	{
		None,
		Vertical,
		Horizontal
	}

	private Dictionary<GamePadButton, int> _rewired_bindings = new Dictionary<GamePadButton, int>
	{
		{
			GamePadButton.A,
			2
		},
		{
			GamePadButton.B,
			3
		},
		{
			GamePadButton.X,
			4
		},
		{
			GamePadButton.Y,
			5
		},
		{
			GamePadButton.LB,
			6
		},
		{
			GamePadButton.RB,
			7
		},
		{
			GamePadButton.Back,
			8
		},
		{
			GamePadButton.Start,
			9
		},
		{
			GamePadButton.DUp,
			10
		},
		{
			GamePadButton.DDown,
			11
		},
		{
			GamePadButton.DLeft,
			12
		},
		{
			GamePadButton.DRight,
			13
		},
		{
			GamePadButton.RT,
			19
		},
		{
			GamePadButton.LT,
			20
		}
	};

	private Dictionary<GameKey, GamePadButton> _bindings = KeyBindings.gamepad_bindings;

	private Stick _stick;

	private Stick _stick2;

	private Player _rewired_player;

	private NavigationStick vertical_navigation;

	private NavigationStick horizontal_navigation;

	private bool _no_horizontal_dir;

	private bool _no_vertical_dir;

	private GuiNavigationAxis _last_gui_navigation_axis;

	private float _gui_navigation_delay;

	private GamePadButton _emulated_key;

	private float _emulated_key_time_left;

	private bool _emulated_key_just_pressed;

	private bool _pause_after_emulated_key;

	public static bool cheat_combination_pressed;

	public bool active_at_start => false;

	public GamePadController()
	{
		_rewired_player = ReInput.players.GetPlayer(0);
		_stick = new Stick(0, 1);
		_stick2 = new Stick(21, 22);
		vertical_navigation = new NavigationStick(vertical: true, this);
		horizontal_navigation = new NavigationStick(vertical: false, this);
	}

	public override void Update()
	{
		base.Update();
		cheat_combination_pressed = false;
		if (_rewired_player == null)
		{
			Debug.LogError("Fatal error, _rewired_player is null. Trying to fix...");
			_rewired_player = ReInput.players.GetPlayer(0);
			if (_rewired_player == null)
			{
				return;
			}
		}
		_stick.Update(_rewired_player);
		_stick2.Update(_rewired_player);
		dir = (_stick.has_direction ? _stick.direction : Vector2.zero);
		dir2 = (_stick2.has_direction ? _stick2.direction : Vector2.zero);
		foreach (KeyValuePair<GameKey, GamePadButton> binding in _bindings)
		{
			int num = _rewired_bindings[binding.Value];
			if (_rewired_player.GetButtonDown(num) || IsEmulatedPress(binding.Value))
			{
				AddPressed(binding.Key);
			}
			if (_rewired_player.GetButton(num) || IsEmulatedHold(binding.Value))
			{
				AddHolded(binding.Key);
				if (num >= 10 && num <= 13)
				{
					dir = Vector2.zero;
				}
			}
		}
		ResetEmulatePressState();
		if (_emulated_key == GamePadButton.None)
		{
			UpdateStickNavigation(dir);
		}
		else
		{
			switch (_emulated_key)
			{
			case GamePadButton.Left:
				dir = Vector2.left;
				break;
			case GamePadButton.Right:
				dir = Vector2.right;
				break;
			case GamePadButton.Up:
				dir = Vector2.up;
				break;
			case GamePadButton.Down:
				dir = Vector2.down;
				break;
			}
			vertical_navigation.Update(dir);
			horizontal_navigation.Update(dir);
		}
		cheat_combination_pressed = _rewired_player.GetButton(_rewired_bindings[GamePadButton.LB]) && _rewired_player.GetButton(_rewired_bindings[GamePadButton.LT]) && _rewired_player.GetButton(_rewired_bindings[GamePadButton.RB]) && _rewired_player.GetButton(_rewired_bindings[GamePadButton.RT]) && (_rewired_player.GetButtonDown(_rewired_bindings[GamePadButton.LB]) || _rewired_player.GetButtonDown(_rewired_bindings[GamePadButton.LT]) || _rewired_player.GetButtonDown(_rewired_bindings[GamePadButton.RB]) || _rewired_player.GetButtonDown(_rewired_bindings[GamePadButton.RT]));
		if (_emulated_key == GamePadButton.None || !(_emulated_key_time_left > 0f))
		{
			return;
		}
		_emulated_key_time_left -= Time.deltaTime;
		if (_emulated_key_time_left <= 0f)
		{
			_emulated_key = GamePadButton.None;
			if (_pause_after_emulated_key)
			{
				Time.timeScale = 0f;
			}
		}
	}

	private void UpdateStickNavigation(Vector2 gui_navigation)
	{
		if (gui_navigation.magnitude > 0f)
		{
			GuiNavigationAxis guiNavigationAxis;
			if (Mathf.Abs(gui_navigation.x) > Mathf.Abs(gui_navigation.y))
			{
				gui_navigation.y = 0f;
				guiNavigationAxis = GuiNavigationAxis.Horizontal;
			}
			else
			{
				gui_navigation.x = 0f;
				guiNavigationAxis = GuiNavigationAxis.Vertical;
			}
			if (_last_gui_navigation_axis != 0 && _last_gui_navigation_axis != guiNavigationAxis)
			{
				_gui_navigation_delay = 0.11f;
			}
			_last_gui_navigation_axis = guiNavigationAxis;
			if (_gui_navigation_delay > 0f)
			{
				gui_navigation = Vector2.zero;
			}
			_gui_navigation_delay -= Time.deltaTime;
		}
		else
		{
			_last_gui_navigation_axis = GuiNavigationAxis.None;
		}
		vertical_navigation.Update(gui_navigation);
		horizontal_navigation.Update(gui_navigation);
	}

	public void AddPressed(GameKey key)
	{
		pressed_keys.Add(key);
		if (key == GameKey.Right)
		{
			AddPressed(GameKey.SliderInc);
		}
		if (key == GameKey.Left)
		{
			AddPressed(GameKey.SliderDec);
		}
	}

	public void AddHolded(GameKey key)
	{
		if (!holded_keys.Contains(key))
		{
			holded_keys.Add(key);
		}
		if (key == GameKey.Right)
		{
			AddHolded(GameKey.SliderInc);
		}
		if (key == GameKey.Left)
		{
			AddHolded(GameKey.SliderDec);
		}
	}

	public void Vibrate(float value, float duration)
	{
		if (_rewired_player == null)
		{
			return;
		}
		foreach (Joystick joystick in _rewired_player.controllers.Joysticks)
		{
			if (joystick.supportsVibration)
			{
				joystick.SetVibration(value, value, duration, duration);
			}
		}
	}

	private bool IsEmulatedHold(GamePadButton button)
	{
		return _emulated_key == button;
	}

	private bool IsEmulatedPress(GamePadButton button)
	{
		if (_emulated_key == button)
		{
			return _emulated_key_just_pressed;
		}
		return false;
	}

	public void EmulateKeyPress(GamePadButton button, float len = 0.2f, bool pause_after_release = true)
	{
		if (pause_after_release)
		{
			Time.timeScale = 1f;
		}
		_emulated_key = button;
		_emulated_key_time_left = len;
		_emulated_key_just_pressed = true;
		_pause_after_emulated_key = pause_after_release;
	}

	public void ResetEmulatePressState()
	{
		_emulated_key_just_pressed = false;
	}

	public override bool IsActive()
	{
		if (!base.IsActive())
		{
			return _emulated_key != GamePadButton.None;
		}
		return true;
	}
}
