using UnityEngine;

public class EquipToToolbarGUI : BaseGUI
{
	public GameObject dark_back;

	public BaseItemCellGUI cursor_item;

	private Camera _gui_cam;

	private ToolbarGUI _toolbar;

	private bool _update_icon;

	private Transform _cursor_tf;

	private BaseItemCellGUI _prev_cell_gui;

	public BaseItemCellGUI[] keyboard_cells => _toolbar.keyboard.cells;

	public override void Init()
	{
		_toolbar = GetComponentInChildren<ToolbarGUI>(includeInactive: true);
		_toolbar.Init();
		cursor_item.interaction_enabled = false;
		cursor_item.x1.tooltip.available = false;
		_cursor_tf = cursor_item.transform;
		base.Init();
	}

	public void Open(Item item, bool from_inventory)
	{
		Open();
		_toolbar.Redraw();
		_prev_cell_gui = null;
		if (item.is_equipped_to_toolbar && !from_inventory)
		{
			int toolbar_index = item.toolbar_index;
			if (toolbar_index != -1)
			{
				(BaseGUI.for_gamepad ? _toolbar.gamepad : _toolbar.keyboard).cells[toolbar_index].DrawEmpty();
			}
		}
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.PrintBack();
		}
		dark_back.Activate();
		_update_icon = !BaseGUI.for_gamepad;
		if (_update_icon)
		{
			_gui_cam = MainGame.me.gui_cam;
			cursor_item.Activate();
			cursor_item.DrawItem(item.id, 1, init_tooltip: false);
			Cursor.visible = false;
		}
		else
		{
			cursor_item.Deactivate();
		}
	}

	public override void Update()
	{
		base.Update();
		for (int i = 0; i < 4; i++)
		{
			if (LazyInput.GetKeyDown(LazyInput.toolbar_keys[i]))
			{
				GUIElements.me.inventory.OnToolbarClicked(i);
				return;
			}
		}
		if (BaseGUI.for_gamepad)
		{
			return;
		}
		Collider2D[] collidersUnderMouse = NGUIExtensionMethods.GetCollidersUnderMouse(MainGame.me.gui_cam);
		BaseItemCellGUI baseItemCellGUI = null;
		int num = -1;
		Collider2D[] array = collidersUnderMouse;
		for (int j = 0; j < array.Length; j++)
		{
			baseItemCellGUI = array[j].transform.parent.GetComponent<BaseItemCellGUI>();
			if (!(baseItemCellGUI == null))
			{
				num = GetToolbarIndexByGUI(baseItemCellGUI);
				if (num != -1)
				{
					break;
				}
				baseItemCellGUI = null;
			}
		}
		if (baseItemCellGUI != _prev_cell_gui)
		{
			if (_prev_cell_gui != null)
			{
				_prev_cell_gui.SetVisualyOveredState(overed: false, by_gamepad: false);
			}
			if (baseItemCellGUI != null)
			{
				baseItemCellGUI.SetVisualyOveredState(overed: true, by_gamepad: false);
			}
			_prev_cell_gui = baseItemCellGUI;
		}
		if (Input.GetMouseButtonUp(0))
		{
			if (num == -1)
			{
				OnClickedBack();
			}
			else
			{
				GUIElements.me.inventory.OnToolbarClicked(num);
			}
		}
	}

	private int GetToolbarIndexByGUI(BaseItemCellGUI cell_gui)
	{
		for (int i = 0; i < 4; i++)
		{
			if (!(cell_gui != keyboard_cells[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private new void LateUpdate()
	{
		if (_update_icon)
		{
			Vector3 position = _gui_cam.ScreenToWorldPoint(Input.mousePosition);
			position.z = _cursor_tf.position.z;
			_cursor_tf.position = position;
		}
	}

	public void OnClickedBack()
	{
		GUIElements.me.inventory.OnEquipmentBackClicked();
	}

	protected override bool OnPressedBack()
	{
		Hide();
		return true;
	}

	public override void Hide(bool play_hide_sound = true)
	{
		BaseItemCellGUI[] array = keyboard_cells;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetVisualyOveredState(overed: false, BaseGUI.for_gamepad);
		}
		base.Hide(play_hide_sound);
		Cursor.visible = true;
		Sounds.PlaySound("gui_item_put");
	}
}
