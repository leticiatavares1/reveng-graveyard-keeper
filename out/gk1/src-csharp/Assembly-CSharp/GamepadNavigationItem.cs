using UnityEngine;

[RequireComponent(typeof(PanelAutoScroll))]
public class GamepadNavigationItem : MonoBehaviour
{
	public int group;

	public int sub_group;

	private GJCommons.VoidDelegate _on_focus;

	private GJCommons.VoidDelegate _on_unfocus;

	private GJCommons.VoidDelegate _on_pressed;

	public GameObject focus_frame;

	private Transform _tf;

	private GamepadNavigationController _controller;

	private int _index;

	private bool _is_focused;

	private Vector2 _half_size;

	private PanelAutoScroll _auto_scroll;

	private bool _active = true;

	[Space(5f)]
	[Header("Custom navigation")]
	public GamepadNavigationItem left_item;

	public GamepadNavigationItem right_item;

	public GamepadNavigationItem up_item;

	public GamepadNavigationItem down_item;

	public bool is_focused => _is_focused;

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
		}
	}

	public int index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	public Transform tf => _tf;

	public Vector2 pos => tf.position;

	public GamepadNavigationController controller => _controller;

	public void Init(int i, GamepadNavigationController controller, float gui_scale)
	{
		_index = i;
		_controller = controller;
		_tf = base.transform;
		_auto_scroll = GetComponent<PanelAutoScroll>();
		if (_auto_scroll == null)
		{
			_auto_scroll = base.gameObject.AddComponent<PanelAutoScroll>();
		}
		_auto_scroll.Init();
		UIWidget component = GetComponent<UIWidget>();
		_half_size = ((component == null) ? new Vector2(10f, 10f) : new Vector2(component.width, component.height));
		_half_size *= gui_scale / 2f;
	}

	public void SetCallbacks(GJCommons.VoidDelegate on_focus, GJCommons.VoidDelegate on_unfocus, GJCommons.VoidDelegate on_select)
	{
		_on_focus = on_focus;
		_on_unfocus = on_unfocus;
		_on_pressed = on_select;
	}

	public void Focus(bool animate_auto_scroll = true)
	{
		if (!_is_focused)
		{
			SetFocus(focused: true);
			if (_auto_scroll.avaible)
			{
				_auto_scroll.Perform(animate_auto_scroll);
			}
			_on_focus.TryInvoke();
		}
	}

	public void UnFocus()
	{
		if (_is_focused)
		{
			SetFocus(focused: false);
			_on_unfocus.TryInvoke();
		}
	}

	public void SetFocus(bool focused)
	{
		_is_focused = focused;
		if (focus_frame != null)
		{
			focus_frame.SetActive(focused);
		}
	}

	public void Select()
	{
		_on_pressed.TryInvoke();
	}

	public float CalcDistToCurrentPos(Vector2 current_pos, Direction direction)
	{
		Vector2 vector = pos;
		switch (direction)
		{
		case Direction.Left:
			vector.x += _half_size.x;
			break;
		case Direction.Right:
			vector.x -= _half_size.x;
			break;
		case Direction.Up:
			vector.y -= _half_size.y;
			break;
		case Direction.Down:
			vector.y += _half_size.y;
			break;
		}
		return (vector - current_pos).magnitude;
	}

	public bool CorrectDirection(Vector2 other_pos, Direction direction)
	{
		Vector2 vector = pos - other_pos;
		if (vector.magnitude.EqualsTo(0f))
		{
			return false;
		}
		return direction switch
		{
			Direction.Left => vector.x < 0f - _half_size.x, 
			Direction.Right => vector.x > _half_size.x, 
			Direction.Up => vector.y > _half_size.y, 
			Direction.Down => vector.y < 0f - _half_size.y, 
			_ => false, 
		};
	}

	public bool CorrectGrid(Vector2 other_pos, Direction direction)
	{
		Vector2 vector = pos - other_pos;
		switch (direction)
		{
		case Direction.Right:
		case Direction.Left:
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
			{
				return Mathf.Abs(vector.y) <= _half_size.y;
			}
			return false;
		case Direction.Up:
		case Direction.Down:
			if (Mathf.Abs(vector.y) > Mathf.Abs(vector.x))
			{
				return Mathf.Abs(vector.x) <= _half_size.x;
			}
			return false;
		default:
			return false;
		}
	}

	public GamepadNavigationItem GetCustomDirectionItem(Direction dir)
	{
		GamepadNavigationItem gamepadNavigationItem = null;
		switch (dir)
		{
		case Direction.Left:
			gamepadNavigationItem = left_item;
			break;
		case Direction.Right:
			gamepadNavigationItem = right_item;
			break;
		case Direction.Up:
			gamepadNavigationItem = up_item;
			break;
		case Direction.Down:
			gamepadNavigationItem = down_item;
			break;
		}
		if (!(gamepadNavigationItem == null) && gamepadNavigationItem.isActiveAndEnabled && gamepadNavigationItem.active)
		{
			return gamepadNavigationItem;
		}
		return null;
	}

	public void SetCustomDirectionItem(GamepadNavigationItem custom_item, Direction dir)
	{
		switch (dir)
		{
		case Direction.Left:
			left_item = custom_item;
			break;
		case Direction.Right:
			right_item = custom_item;
			break;
		case Direction.Up:
			up_item = custom_item;
			break;
		case Direction.Down:
			down_item = custom_item;
			break;
		}
	}
}
