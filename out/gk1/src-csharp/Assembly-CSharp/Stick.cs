using Rewired;
using UnityEngine;

public class Stick
{
	private const float NEW_DIR_DELAY = 0.05f;

	private const float MIN_MAGNITUDE = 0.1f;

	private const float OPPOSITE_DIR_MIN_MAGNITUDE = 0.5f;

	public Vector2 direction;

	private int _h_axis_id;

	private int _v_axis_id;

	private float _prev_h;

	private float _prev_v;

	private float _current_h;

	private float _current_v;

	private float _stick_delay;

	public bool has_direction => direction.magnitude > 0f;

	public Stick(int horizontal_axis_id, int vertical_axis_id)
	{
		_h_axis_id = horizontal_axis_id;
		_v_axis_id = vertical_axis_id;
	}

	public void Update(Player player)
	{
		_current_h = player.GetAxis(_h_axis_id);
		_current_v = player.GetAxis(_v_axis_id);
		direction = new Vector2(_current_h, _current_v);
		if (direction.magnitude < 0.1f)
		{
			direction = Vector2.zero;
			_current_h = (_current_v = 0f);
		}
		_stick_delay -= Time.deltaTime;
		if (direction.magnitude > 0f)
		{
			if ((_prev_h < 0f && _current_h > 0f) || (_prev_h > 0f && _current_h < 0f) || (_prev_v < 0f && _current_v > 0f) || (_prev_v > 0f && _current_v < 0f))
			{
				_stick_delay = 0.05f;
			}
		}
		else
		{
			_stick_delay = 0f;
		}
		if (_stick_delay > 0f && direction.magnitude < 0.5f)
		{
			direction = Vector2.zero;
			_current_h = (_current_v = 0f);
		}
		_prev_h = direction.x;
		_prev_v = direction.y;
	}
}
