using UnityEngine;

public class CraftQueueItemGUI : MonoBehaviour
{
	public BaseItemCellGUI icon;

	public UILabel counter;

	public Color default_color;

	public Color gratitude_color;

	private bool _is_current;

	public GameObject amount_buttons;

	public bool is_gratitude_points_element;

	public GameObject selection_frame_default;

	public GameObject selection_frame_gratitude;

	private bool _overed_item;

	private bool _overed_side_buttons;

	private bool _overed_infinity_buttons;

	private GamepadNavigationItem _gamepad_navigation_item;

	private CraftComponent.CraftQueueItem _ci;

	public static CraftQueueItemGUI current_over;

	private WorldGameObject _craftery_wgo;

	public GameObject focus_frame;

	public void Draw(CraftComponent.CraftQueueItem ci, WorldGameObject craftery_wgo, bool add_one_current = false)
	{
		_ci = ci;
		_craftery_wgo = craftery_wgo;
		_is_current = add_one_current;
		Redraw();
		if (current_over == this)
		{
			OnOver();
		}
		focus_frame.SetActive(value: false);
		_gamepad_navigation_item = GetComponentsInChildren<GamepadNavigationItem>(includeInactive: true)[0];
		Update();
	}

	private void Redraw()
	{
		if (_ci?.craft == null)
		{
			return;
		}
		Sounds.OnGUIClick();
		Item item = ((_ci.craft.output.Count > 0) ? _ci.craft.output[0] : null);
		icon.DrawItem(item);
		if (!string.IsNullOrEmpty(_ci.craft.icon) && (item?.definition == null || !item.definition.is_big))
		{
			icon.DrawIcon(_ci.craft.icon, draw_back: true, _ci.craft.hide_quality_icon);
			if (item != null)
			{
				icon.container.counter.text = _craftery_wgo.GetCraftAmountCounter(_ci.craft);
			}
		}
		int num = _ci.n;
		if (_is_current)
		{
			num++;
		}
		if (is_gratitude_points_element)
		{
			counter.color = gratitude_color;
			selection_frame_gratitude.SetActive(value: true);
			selection_frame_default.SetActive(value: false);
			icon.x1.counter.color = gratitude_color;
			icon.x2.counter.color = gratitude_color;
		}
		else
		{
			counter.color = default_color;
			selection_frame_gratitude.SetActive(value: false);
			selection_frame_default.SetActive(value: true);
			icon.x1.counter.color = default_color;
			icon.x2.counter.color = default_color;
		}
		counter.text = (_ci.infinite ? "∞" : ("x" + num));
	}

	public void OnDeletePressed()
	{
		Sounds.OnGUIClick();
		CraftQueueGUI.current_instance.OnDeleteItemPressed(_ci, _is_current);
	}

	public void ForceRemoveSelectionFrame()
	{
		_overed_item = (_overed_side_buttons = (_overed_infinity_buttons = false));
		Update();
		current_over = null;
	}

	public void OnOver()
	{
		_overed_item = true;
	}

	public void OnOut()
	{
		_overed_item = false;
	}

	public void OnOverSideButton()
	{
		_overed_side_buttons = true;
	}

	public void OnOutSideButton()
	{
		_overed_side_buttons = false;
	}

	public void OnOverInfinityButton()
	{
		_overed_infinity_buttons = true;
	}

	public void OnOutInfinityButton()
	{
		_overed_infinity_buttons = false;
	}

	public void Update()
	{
		bool flag = _overed_item || _overed_side_buttons || _overed_infinity_buttons;
		if (BaseGUI.for_gamepad && CraftQueueGUI.current_instance.gamepad_controller.focused_item == _gamepad_navigation_item)
		{
			flag = true;
		}
		if (!(amount_buttons == null))
		{
			amount_buttons.SetActive(flag);
			if (!BaseGUI.for_gamepad)
			{
				focus_frame.SetActive(flag);
			}
			if (flag)
			{
				current_over = this;
			}
			else if (current_over == this)
			{
				current_over = null;
			}
		}
	}

	public void OnIncreasePressed()
	{
		_ci.infinite = false;
		if (_is_current && _ci.n == 0)
		{
			CraftQueueGUI.current_instance.AddANewItemForCurrent(ref _ci);
		}
		else if (++_ci.n > 100)
		{
			_ci.n = 100;
		}
		Redraw();
	}

	public void OnDecreasePressed()
	{
		_ci.infinite = false;
		if ((_is_current && _ci.n == 0) || (!_is_current && _ci.n == 1))
		{
			OnDeletePressed();
			return;
		}
		_ci.n--;
		Redraw();
	}

	public void OnInfinityButtonPressed()
	{
		bool flag = false;
		if (!_ci.infinite && _is_current && _ci.n == 0)
		{
			_ci.infinite = true;
			OnIncreasePressed();
			flag = true;
		}
		_ci.infinite = !_ci.infinite;
		Redraw();
		if (flag)
		{
			CraftQueueGUI.current_instance.Redraw();
		}
	}
}
