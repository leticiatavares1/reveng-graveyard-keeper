using UnityEngine;

namespace LazyBearTechnology;

public class NavigationStick
{
	private const float MIN_DEAD_ZONE = 0.4f;

	private const float DELTA = 0.2f;

	private bool vertical;

	private GamepadController controller;

	private float maxValue;

	private float minValue;

	private bool waitForPress;

	public NavigationStick(bool vertical, GamepadController controller)
	{
		this.vertical = vertical;
		this.controller = controller;
	}

	public void Update(Vector2 dir)
	{
		float num = (vertical ? dir.y : dir.x);
		float num2 = Mathf.Abs(num);
		if (num2 <= 0.4f)
		{
			minValue = (maxValue = 0f);
			waitForPress = false;
			return;
		}
		GameKey key = GetKey(num);
		if (maxValue.EqualsTo(0f))
		{
			maxValue = num2;
			minValue = maxValue - 0.2f;
			waitForPress = false;
			controller.HandlePressing(key);
			controller.HandleHolding(key);
		}
		else if (num2 < minValue)
		{
			waitForPress = true;
			minValue = num2;
			maxValue = minValue + 0.2f;
		}
		else if (!(num2 < minValue + 0.1f))
		{
			if (waitForPress)
			{
				controller.HandlePressing(key);
				controller.HandleHolding(key);
			}
			if (num2 > minValue + 0.2f)
			{
				maxValue = num2;
				minValue = maxValue - 0.2f;
			}
			if (waitForPress)
			{
				waitForPress = false;
			}
			else
			{
				controller.HandleHolding(key);
			}
		}
	}

	private GameKey GetKey(float value)
	{
		if (!vertical)
		{
			if (!(value > 0f))
			{
				return GameKey.Left;
			}
			return GameKey.Right;
		}
		if (!(value > 0f))
		{
			return GameKey.Down;
		}
		return GameKey.Up;
	}
}
