using UnityEngine;

public class NavigationStick
{
	private const float MIN_DEAD_ZONE = 0.4f;

	private const float DELTA = 0.2f;

	private bool _vertical;

	private GamePadController _controller;

	private float _max_value;

	private float _min_value;

	private bool _wait_for_press;

	public NavigationStick(bool vertical, GamePadController controller)
	{
		_vertical = vertical;
		_controller = controller;
	}

	public void Update(Vector2 dir)
	{
		float num = (_vertical ? dir.y : dir.x);
		float num2 = Mathf.Abs(num);
		if (num2 <= 0.4f)
		{
			_min_value = (_max_value = 0f);
			_wait_for_press = false;
			return;
		}
		GameKey key = GetKey(num);
		if (_max_value.EqualsTo(0f))
		{
			_max_value = num2;
			_min_value = _max_value - 0.2f;
			_wait_for_press = false;
			_controller.AddPressed(key);
			_controller.AddHolded(key);
		}
		else if (num2 < _min_value)
		{
			_wait_for_press = true;
			_min_value = num2;
			_max_value = _min_value + 0.2f;
		}
		else if (!(num2 < _min_value + 0.1f))
		{
			if (_wait_for_press)
			{
				_controller.AddPressed(key);
				_controller.AddHolded(key);
			}
			if (num2 > _min_value + 0.2f)
			{
				_max_value = num2;
				_min_value = _max_value - 0.2f;
			}
			if (_wait_for_press)
			{
				_wait_for_press = false;
			}
			else
			{
				_controller.AddHolded(key);
			}
		}
	}

	private GameKey GetKey(float value)
	{
		if (!_vertical)
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
