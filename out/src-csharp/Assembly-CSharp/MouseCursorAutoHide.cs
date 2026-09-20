using UnityEngine;

public class MouseCursorAutoHide : MonoBehaviour
{
	private const float MOUSE_AUTOHIDE_TIME = 4f;

	private Vector2 _last_mouse_pos = Vector2.zero;

	private float _last_move_time;

	private float _dtime;

	private bool _mouse_shown = true;

	public void Awake()
	{
		_last_move_time = Time.time;
	}

	public void Update()
	{
		Vector2 vector = Input.mousePosition;
		float magnitude = (_last_mouse_pos - vector).magnitude;
		_last_mouse_pos = vector;
		bool flag = true;
		if (magnitude > 0.1f)
		{
			_last_move_time = Time.time;
			_dtime = 0f;
		}
		else
		{
			_dtime = Time.time - _last_move_time;
			if (_dtime > 4f)
			{
				flag = false;
			}
		}
		if (!MainGame.game_started || (!BaseGUI.all_guis_closed && !BaseGUI.for_gamepad))
		{
			flag = true;
			_last_move_time = Time.time;
		}
		if (_mouse_shown != flag)
		{
			Cursor.visible = flag;
			_mouse_shown = flag;
		}
	}
}
