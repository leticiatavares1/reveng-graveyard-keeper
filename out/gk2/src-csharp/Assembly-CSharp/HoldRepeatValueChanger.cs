using System;
using LazyBearTechnology;
using UnityEngine;

public class HoldRepeatValueChanger
{
	private enum SpeedMode
	{
		None,
		Low,
		Medium,
		High
	}

	public const float AxisDeadZone = 0.4f;

	public float ChangeValueTime = 0.2f;

	public float HoldLowChangeValueTime = 0.5f;

	public float HoldMediumChangeValueTime = 2f;

	public float HoldHighChangeValueTime = 4f;

	public int InitialStep = 1;

	public int LowSpeedChangeValue = 1;

	public int MediumSpeedChangeValue = 5;

	public int HighSpeedChangeValue = 10;

	public int ShiftStep = 10;

	private float changeValueTimer;

	private float holdTimer;

	private SpeedMode speedMode;

	private int currentDirection;

	public bool IsActive => currentDirection != 0;

	public void Press(int direction, Action<int> onDelta)
	{
		if (onDelta != null && direction != 0)
		{
			int num = ((direction > 0) ? 1 : (-1));
			if (currentDirection != num)
			{
				Start(num, onDelta);
			}
		}
	}

	public void Tick(int direction, Action<int> onDelta)
	{
		if (onDelta == null || direction == 0)
		{
			Reset();
			return;
		}
		int num = ((direction > 0) ? 1 : (-1));
		if (currentDirection != num)
		{
			Start(num, onDelta);
		}
		else
		{
			UpdateHold(onDelta);
		}
	}

	public void Reset()
	{
		holdTimer = 0f;
		changeValueTimer = 0f;
		speedMode = SpeedMode.None;
		currentDirection = 0;
	}

	public static int GetPointerHoldDirection(LazyButton plusButton, LazyButton minusButton)
	{
		bool flag = IsButtonHeld(plusButton);
		bool flag2 = IsButtonHeld(minusButton);
		if (flag == flag2)
		{
			return 0;
		}
		if (!flag)
		{
			return -1;
		}
		return 1;
	}

	public static bool IsKeyHeld(GameKey key)
	{
		if ((object)key != null)
		{
			if (!LazyInput.GetKey(key))
			{
				return LazyInput.GetKeyDown(key);
			}
			return true;
		}
		return false;
	}

	public static bool IsAnyKeyHeld(GameKey key1, GameKey key2)
	{
		if (!IsKeyHeld(key1))
		{
			return IsKeyHeld(key2);
		}
		return true;
	}

	public static bool IsShiftHeld()
	{
		if (!Input.GetKey(KeyCode.LeftShift))
		{
			return Input.GetKey(KeyCode.RightShift);
		}
		return true;
	}

	public static int GetAxisHoldDirection(bool vertical)
	{
		float num = (vertical ? LazyInput.GetDirection().y : LazyInput.GetDirection().x);
		if (num > 0.4f)
		{
			return 1;
		}
		if (num < -0.4f)
		{
			return -1;
		}
		return 0;
	}

	private static bool IsButtonHeld(LazyButton button)
	{
		if (button != null && button.isActiveAndEnabled && button.interactable)
		{
			return button.IsPointerHeld;
		}
		return false;
	}

	private void Start(int sign, Action<int> onDelta)
	{
		holdTimer = 0f;
		changeValueTimer = 0f;
		speedMode = SpeedMode.None;
		currentDirection = sign;
		onDelta(GetStartStep() * sign);
	}

	private void UpdateHold(Action<int> onDelta)
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (speedMode != SpeedMode.High)
		{
			holdTimer += unscaledDeltaTime;
			if (holdTimer.EqualsOrMore(HoldHighChangeValueTime))
			{
				speedMode = SpeedMode.High;
			}
			else if (holdTimer.EqualsOrMore(HoldMediumChangeValueTime))
			{
				speedMode = SpeedMode.Medium;
			}
			else if (holdTimer.EqualsOrMore(HoldLowChangeValueTime))
			{
				speedMode = SpeedMode.Low;
			}
		}
		changeValueTimer += unscaledDeltaTime;
		if (changeValueTimer.EqualsOrMore(ChangeValueTime))
		{
			int num = speedMode switch
			{
				SpeedMode.Low => LowSpeedChangeValue, 
				SpeedMode.Medium => MediumSpeedChangeValue, 
				SpeedMode.High => HighSpeedChangeValue, 
				_ => 0, 
			};
			if (IsShiftHeld())
			{
				num = Math.Max(num, ShiftStep);
			}
			if (num > 0)
			{
				onDelta(num * currentDirection);
			}
			changeValueTimer = 0f;
		}
	}

	private int GetStartStep()
	{
		if (!IsShiftHeld())
		{
			return InitialStep;
		}
		return Math.Max(InitialStep, ShiftStep);
	}
}
