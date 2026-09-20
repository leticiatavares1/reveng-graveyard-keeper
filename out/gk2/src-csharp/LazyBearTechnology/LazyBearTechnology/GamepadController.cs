using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace LazyBearTechnology;

public class GamepadController : BaseInputController
{
	private enum GuiNavigationAxis
	{
		None,
		Vertical,
		Horizontal
	}

	private Dictionary<GamepadButton, int> rewiredBindings = new Dictionary<GamepadButton, int>
	{
		{
			GamepadButton.X,
			2
		},
		{
			GamepadButton.Y,
			3
		},
		{
			GamepadButton.A,
			4
		},
		{
			GamepadButton.B,
			5
		},
		{
			GamepadButton.LB,
			6
		},
		{
			GamepadButton.RB,
			7
		},
		{
			GamepadButton.LT,
			8
		},
		{
			GamepadButton.RT,
			9
		},
		{
			GamepadButton.Back,
			10
		},
		{
			GamepadButton.Start,
			11
		},
		{
			GamepadButton.DUp,
			12
		},
		{
			GamepadButton.DDown,
			13
		},
		{
			GamepadButton.DLeft,
			14
		},
		{
			GamepadButton.DRight,
			15
		},
		{
			GamepadButton.RStick,
			19
		},
		{
			GamepadButton.LStick,
			20
		}
	};

	private List<GamepadBinding> gamepadBindings;

	private GameBindings gameBindings;

	private Stick stick;

	private Stick rightStick;

	private Player rewiredPlayer;

	private NavigationStick verticalNavigation;

	private NavigationStick horizontalNavigation;

	private float guiNavigationDelay;

	private GuiNavigationAxis lastGuiNavigationAxis;

	private const string SonyTouchpadRole = "gamepad/touchpad/press";

	private const string SonyTouchpadKeyDualShock = "touchpad_button";

	private const string SonyTouchpadKeyDualSense = "touchpad/button";

	private List<GameKey> holdedForRepeatPress = new List<GameKey>();

	private List<float> holdedForRepeatPressDelays = new List<float>();

	private Dictionary<HoldedGroupType, float> lastReleasesInGroup = new Dictionary<HoldedGroupType, float>();

	public GamepadController(GameBindings gameBindings)
	{
		this.gameBindings = gameBindings;
		gamepadBindings = gameBindings.gamepadBindings;
		rewiredPlayer = ReInput.players.GetPlayer(0);
		stick = new Stick(0, 1);
		rightStick = new Stick(16, 17);
		verticalNavigation = new NavigationStick(vertical: true, this);
		horizontalNavigation = new NavigationStick(vertical: false, this);
	}

	public override void Update()
	{
		base.Update();
		if (!ReInput.isReady)
		{
			return;
		}
		rewiredPlayer = ReInput.players.GetPlayer(0);
		stick.Update(rewiredPlayer);
		rightStick.Update(rewiredPlayer);
		direction = (stick.HasDirection ? stick.direction : Vector2.zero);
		direction2 = (rightStick.HasDirection ? rightStick.direction : Vector2.zero);
		bool flag = ShouldReplaceBackWithSonyTouchpad();
		bool flag2 = flag && IsSonyTouchpadButtonDown();
		bool flag3 = flag && IsSonyTouchpadButtonHeld();
		for (int i = 0; i < gamepadBindings.Count; i++)
		{
			GameKey gameKey = gamepadBindings[i].gameKey;
			GamepadButton gamepadButton = gamepadBindings[i].gamepadButton;
			int actionId = rewiredBindings[gamepadButton];
			bool flag4;
			bool flag5;
			if (gamepadButton == GamepadButton.Back && flag)
			{
				flag4 = flag2;
				flag5 = flag3;
			}
			else
			{
				flag4 = rewiredPlayer.GetButtonDown(actionId);
				flag5 = rewiredPlayer.GetButton(actionId);
			}
			if (flag4)
			{
				HandlePressing(gameKey);
			}
			if (flag5)
			{
				HandleHolding(gameKey);
			}
		}
		UpdateStickNavigation(direction);
		UpdateHolded(Time.deltaTime);
	}

	private void UpdateHolded(float deltaTime)
	{
		for (int i = 0; i < holdedForRepeatPress.Count; i++)
		{
			GameKey gameKey = holdedForRepeatPress[i];
			if (!holdedKeys.Contains(gameKey))
			{
				holdedForRepeatPress.RemoveAt(i);
				holdedForRepeatPressDelays.RemoveAt(i);
				i--;
				HoldedGroup group = GetGroup(gameKey);
				if (group != null)
				{
					if (lastReleasesInGroup.ContainsKey(group.groupType))
					{
						lastReleasesInGroup[group.groupType] = Time.time;
					}
					else
					{
						lastReleasesInGroup.Add(group.groupType, Time.time);
					}
				}
			}
			else
			{
				holdedForRepeatPressDelays[i] -= deltaTime;
				if (holdedForRepeatPressDelays[i] <= 0f)
				{
					HandlePressing(gameKey);
					HoldedGroup group2 = GetGroup(gameKey);
					holdedForRepeatPressDelays[i] = group2.timeRepeatPeriod;
				}
			}
		}
	}

	private void UpdateStickNavigation(Vector2 guiNavigation)
	{
		if (guiNavigation.magnitude > 0f)
		{
			GuiNavigationAxis guiNavigationAxis;
			if (Mathf.Abs(guiNavigation.x) > Mathf.Abs(guiNavigation.y))
			{
				guiNavigation.y = 0f;
				guiNavigationAxis = GuiNavigationAxis.Horizontal;
			}
			else
			{
				guiNavigation.x = 0f;
				guiNavigationAxis = GuiNavigationAxis.Vertical;
			}
			if (lastGuiNavigationAxis != 0 && lastGuiNavigationAxis != guiNavigationAxis)
			{
				guiNavigationDelay = 0.11f;
			}
			lastGuiNavigationAxis = guiNavigationAxis;
			if (guiNavigationDelay > 0f)
			{
				guiNavigation = Vector2.zero;
			}
			guiNavigationDelay -= LazyTime.GetUnscaledDeltaTime;
		}
		else
		{
			lastGuiNavigationAxis = GuiNavigationAxis.None;
		}
		verticalNavigation.Update(guiNavigation);
		horizontalNavigation.Update(guiNavigation);
	}

	public void HandlePressing(GameKey key)
	{
		pressedKeys.Add(key);
		List<BindingAlias> bindingAliases = gameBindings.bindingAliases;
		for (int i = 0; i < bindingAliases.Count; i++)
		{
			BindingAlias bindingAlias = bindingAliases[i];
			if (key == bindingAlias.gameKey1)
			{
				pressedKeys.Add(bindingAlias.gameKey2);
			}
		}
	}

	public bool AnyKeyDownExceptSticks()
	{
		bool flag = ShouldReplaceBackWithSonyTouchpad();
		foreach (KeyValuePair<GamepadButton, int> rewiredBinding in rewiredBindings)
		{
			if ((!flag || !(rewiredBinding.Key == GamepadButton.Back)) && rewiredPlayer.GetButtonDown(rewiredBinding.Value))
			{
				return true;
			}
		}
		if (flag && IsSonyTouchpadButtonDown())
		{
			return true;
		}
		return false;
	}

	private bool IsSonyTouchpadButtonDown()
	{
		return IsSonyTouchpadButtonState(justPressed: true);
	}

	private bool IsSonyTouchpadButtonHeld()
	{
		return IsSonyTouchpadButtonState(justPressed: false);
	}

	private bool IsSonyTouchpadButtonState(bool justPressed)
	{
		if (rewiredPlayer == null || !IsSonyTouchpadOverrideEnabled())
		{
			return false;
		}
		foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
		{
			if (!IsSonyDualShockOrDualSense(joystick))
			{
				continue;
			}
			IList<ControllerElementIdentifier> buttonElementIdentifiers = joystick.ButtonElementIdentifiers;
			for (int i = 0; i < buttonElementIdentifiers.Count; i++)
			{
				ControllerElementIdentifier controllerElementIdentifier = buttonElementIdentifiers[i];
				if (IsSonyTouchpadClickElement(controllerElementIdentifier))
				{
					return justPressed ? joystick.GetButtonDownById(controllerElementIdentifier.id) : joystick.GetButtonById(controllerElementIdentifier.id);
				}
			}
		}
		return false;
	}

	private bool ShouldReplaceBackWithSonyTouchpad()
	{
		if (!IsSonyTouchpadOverrideEnabled() || rewiredPlayer == null)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
		{
			if (IsSonyDualShockOrDualSense(joystick))
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (!flag2)
		{
			return true;
		}
		return IsSonyDualShockOrDualSense(rewiredPlayer.controllers.GetLastActiveController() as Joystick);
	}

	private static bool IsSonyTouchpadOverrideEnabled()
	{
		return true;
	}

	private static bool IsSonyDualShockOrDualSense(Joystick joystick)
	{
		if (joystick == null)
		{
			return false;
		}
		GamepadTypeData instance = LazySingletonSO<GamepadTypeData>.Instance;
		if (instance == null)
		{
			return false;
		}
		GamepadType typeByGuid = instance.GetTypeByGuid(joystick.hardwareTypeGuid);
		if (!(typeByGuid == GamepadType.Sony_DualShock))
		{
			return typeByGuid == GamepadType.Sony_DualSense;
		}
		return true;
	}

	private static bool IsSonyTouchpadClickElement(ControllerElementIdentifier identifier)
	{
		if (identifier == null)
		{
			return false;
		}
		if (identifier.role == "gamepad/touchpad/press")
		{
			return true;
		}
		string key = identifier.key;
		if (!(key == "touchpad_button"))
		{
			return key == "touchpad/button";
		}
		return true;
	}

	public void HandleHolding(GameKey key)
	{
		if (!holdedKeys.Contains(key))
		{
			holdedKeys.Add(key);
		}
		List<BindingAlias> bindingAliases = gameBindings.bindingAliases;
		for (int i = 0; i < bindingAliases.Count; i++)
		{
			BindingAlias bindingAlias = bindingAliases[i];
			if (key == bindingAlias.gameKey1)
			{
				holdedKeys.Add(bindingAlias.gameKey2);
			}
		}
		if (!CanBeHoldedForRepeatPress(key) || holdedForRepeatPress.Contains(key))
		{
			return;
		}
		holdedForRepeatPress.Add(key);
		HoldedGroup group = GetGroup(key);
		float num = 0f;
		foreach (float value in lastReleasesInGroup.Values)
		{
			if (num == 0f)
			{
				num = value;
			}
			else if (value > num)
			{
				num = value;
			}
		}
		holdedForRepeatPressDelays.Add((Time.time - num < group.timeBeforeRepeat) ? group.timeRepeatPeriod : group.timeBeforeRepeat);
	}

	private bool CanBeHoldedForRepeatPress(GameKey key)
	{
		List<HoldableElement> canBeHoldedForRepeatPress = gameBindings.canBeHoldedForRepeatPress;
		for (int i = 0; i < canBeHoldedForRepeatPress.Count; i++)
		{
			if (canBeHoldedForRepeatPress[i].gameKey == key)
			{
				return true;
			}
		}
		return false;
	}

	private HoldedGroup GetGroup(GameKey key)
	{
		List<HoldableElement> canBeHoldedForRepeatPress = gameBindings.canBeHoldedForRepeatPress;
		for (int i = 0; i < canBeHoldedForRepeatPress.Count; i++)
		{
			if (!(canBeHoldedForRepeatPress[i].gameKey == key))
			{
				continue;
			}
			HoldedGroupType groupType = canBeHoldedForRepeatPress[i].groupType;
			List<HoldedGroup> holdedGroups = gameBindings.holdedGroups;
			for (int j = 0; j < holdedGroups.Count; j++)
			{
				if (holdedGroups[j].groupType == groupType)
				{
					return holdedGroups[j];
				}
			}
		}
		return null;
	}

	public void Vibrate(float value, float duration)
	{
		if (!ReInput.isReady)
		{
			return;
		}
		rewiredPlayer = ReInput.players.GetPlayer(0);
		if (rewiredPlayer == null)
		{
			return;
		}
		foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
		{
			if (joystick.supportsVibration)
			{
				joystick.SetVibration(value, value, duration, duration);
			}
		}
	}
}
