using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GamepadNavigationController : MonoBehaviour
{
	public enum OpenMethod
	{
		None,
		GetAll,
		GetAllAndFocus,
		GetAllAndSelect
	}

	public delegate void ItemInteractionDelegate(GamepadNavigationItem item);

	public const float LOOP_OFFSET = 50f;

	public bool avaible = true;

	[Space]
	[SerializeField]
	[RuntimeValue]
	private List<GamepadNavigationItem> selectable_items = new List<GamepadNavigationItem>();

	public bool auto_select;

	public bool perfect_grid;

	public bool restore_last_in_group;

	public GamepadNavigationSettings vertical_settings;

	public GamepadNavigationSettings horizontal_settings;

	private Dictionary<int, GamepadNavigationItem> _last_focused_items = new Dictionary<int, GamepadNavigationItem>();

	private bool _is_enabled;

	private float _gui_scale;

	public static GamepadNavigationController current
	{
		get
		{
			if (!(BaseGUI.active_gui == null))
			{
				return BaseGUI.active_gui.gamepad_controller;
			}
			return null;
		}
	}

	public GamepadNavigationItem focused_item
	{
		get
		{
			foreach (GamepadNavigationItem selectable_item in selectable_items)
			{
				if (selectable_item.is_focused)
				{
					return selectable_item;
				}
			}
			return null;
		}
	}

	public int focused_item_index
	{
		get
		{
			GamepadNavigationItem gamepadNavigationItem = focused_item;
			if (!(gamepadNavigationItem != null))
			{
				return -1;
			}
			return gamepadNavigationItem.index;
		}
	}

	public bool is_enabled => _is_enabled;

	public void Enable()
	{
		if (!_is_enabled)
		{
			_is_enabled = true;
		}
	}

	public void Enable(OpenMethod method)
	{
		Enable();
		if (focused_item != null)
		{
			focused_item.UnFocus();
		}
		switch (method)
		{
		case OpenMethod.GetAll:
			ReinitItems(focus_on_first_active: false);
			break;
		case OpenMethod.GetAllAndFocus:
			ReinitItems(focus_on_first_active: true);
			break;
		case OpenMethod.GetAllAndSelect:
			ReinitItems(focus_on_first_active: true);
			SelectFocusedItem();
			break;
		}
	}

	public void ReinitItems(bool focus_on_first_active)
	{
		_gui_scale = base.transform.lossyScale.x;
		selectable_items.Clear();
		selectable_items.AddRange(GetComponentsInChildren<GamepadNavigationItem>());
		RemoveNullsAndSetIndexes(selectable_items);
		PrintItemsList(selectable_items, "GetComponentsInChildren() result");
		int num = 0;
		foreach (GamepadNavigationItem selectable_item in selectable_items)
		{
			selectable_item.Init(num++, this, _gui_scale);
		}
		if (focus_on_first_active)
		{
			FocusOnFirstActive();
		}
	}

	private void RemoveNullsAndSetIndexes(List<GamepadNavigationItem> items)
	{
		items.RemoveUnityNulls();
		for (int i = 0; i < items.Count; i++)
		{
			items[i].index = i;
		}
	}

	public void Disable()
	{
		if (!_is_enabled)
		{
			return;
		}
		_is_enabled = false;
		foreach (GamepadNavigationItem selectable_item in selectable_items)
		{
			selectable_item.UnFocus();
		}
		selectable_items.Clear();
	}

	public void RememberFocused(GamepadNavigationItem item)
	{
		if (!(item == null))
		{
			if (_last_focused_items.ContainsKey(item.group))
			{
				_last_focused_items[item.group] = item;
			}
			else
			{
				_last_focused_items.Add(item.group, item);
			}
		}
	}

	public bool FocusOnFirstActive(int group = -1)
	{
		int num = -1;
		bool flag = group >= 0;
		foreach (GamepadNavigationItem selectable_item in selectable_items)
		{
			num++;
			if (selectable_item == null)
			{
				Debug.LogError("Found a null item while selecting group = " + group);
			}
			else if (selectable_item.isActiveAndEnabled && selectable_item.active && (!flag || selectable_item.group == group))
			{
				SetFocusedItem(num);
				return true;
			}
		}
		Debug.LogWarning("no selectable_items", this);
		return false;
	}

	public bool HaveSavedFocusForGroup(int group)
	{
		if (!_last_focused_items.ContainsKey(group))
		{
			return false;
		}
		if (_last_focused_items[group] == null)
		{
			_last_focused_items.Remove(group);
			return false;
		}
		if (_last_focused_items[group].isActiveAndEnabled && _last_focused_items[group].active)
		{
			return true;
		}
		_last_focused_items.Remove(group);
		return false;
	}

	public void RestoreFocus(int group = 0)
	{
		if (HaveSavedFocusForGroup(group))
		{
			SetFocusedItem(_last_focused_items[group]);
			return;
		}
		Debug.LogWarning("no saved last focus for group " + group + ", trying to select any in this group");
		FocusOnFirstActive(group);
	}

	public void RestoreSelection(int group)
	{
		RestoreFocus(group);
		SelectFocusedItem();
	}

	public void SetFocusedItem(GamepadNavigationItem item, bool animate_auto_scroll = true)
	{
		SetFocusedItem(selectable_items.IndexOf(item), animate_auto_scroll);
	}

	public int GetFocusedItemIndex(GamepadNavigationItem item)
	{
		return selectable_items.IndexOf(item);
	}

	public void SetFocusedItem(int focus_index, bool animate_auto_scroll = true)
	{
		if (focus_index < 0 || focus_index >= selectable_items.Count)
		{
			focus_index = 0;
		}
		GamepadNavigationItem gamepadNavigationItem = null;
		for (int i = 0; i < selectable_items.Count; i++)
		{
			if (i == focus_index)
			{
				if (selectable_items[i].active)
				{
					gamepadNavigationItem = selectable_items[i];
				}
			}
			else
			{
				selectable_items[i].UnFocus();
			}
		}
		if (gamepadNavigationItem != null)
		{
			gamepadNavigationItem.Focus(animate_auto_scroll);
			RememberFocused(gamepadNavigationItem);
		}
	}

	public void SelectFocusedItem()
	{
		GamepadNavigationItem gamepadNavigationItem = focused_item;
		if (gamepadNavigationItem != null)
		{
			gamepadNavigationItem.Select();
		}
	}

	public void Navigate(Direction direction)
	{
		GamepadNavigationSettings gamepadNavigationSettings;
		switch (direction)
		{
		case Direction.None:
			return;
		default:
			gamepadNavigationSettings = horizontal_settings;
			break;
		case Direction.Up:
		case Direction.Down:
			gamepadNavigationSettings = vertical_settings;
			break;
		}
		GamepadNavigationSettings gamepadNavigationSettings2 = gamepadNavigationSettings;
		RemoveNullsAndSetIndexes(selectable_items);
		if (selectable_items.Count == 0)
		{
			return;
		}
		GamepadNavigationItem gamepadNavigationItem = focused_item;
		if (gamepadNavigationItem == null)
		{
			SetFocusedItem(0);
			return;
		}
		GamepadNavigationItem customDirectionItem = gamepadNavigationItem.GetCustomDirectionItem(direction);
		if (customDirectionItem != null && selectable_items.Contains(customDirectionItem))
		{
			if (customDirectionItem != gamepadNavigationItem)
			{
				SetFocusedItem(customDirectionItem);
			}
			return;
		}
		int group = gamepadNavigationItem.group;
		int sub_group = gamepadNavigationItem.sub_group;
		Vector2 pos = gamepadNavigationItem.pos;
		List<GamepadNavigationItem> list = new List<GamepadNavigationItem>();
		foreach (GamepadNavigationItem selectable_item in selectable_items)
		{
			if (!(selectable_item == gamepadNavigationItem) && !SkipItemBecauseOfState(selectable_item) && (!gamepadNavigationSettings2.stay_in_group || !SkipItemBecauseOfGroup(selectable_item, group)) && (!gamepadNavigationSettings2.stay_in_sub_group || !SkipItemBecauseOfSubGroup(selectable_item, sub_group)) && !SkipItemBecauseOfDirection(selectable_item, pos, direction))
			{
				list.Add(selectable_item);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		List<GamepadNavigationItem> list2 = new List<GamepadNavigationItem>(list);
		List<GamepadNavigationItem> list3 = new List<GamepadNavigationItem>();
		List<GamepadNavigationItem> list4 = new List<GamepadNavigationItem>();
		for (int i = 0; i < list2.Count; i++)
		{
			bool flag = false;
			if (gamepadNavigationSettings2.try_stay_in_sub_group && SkipItemBecauseOfGroup(list2[i], group))
			{
				list3.Add(list2[i]);
				flag = true;
			}
			if (!flag && SkipItemBecauseOfGrid(list2[i], pos, direction))
			{
				list4.Add(list2[i]);
				flag = true;
			}
			if (flag)
			{
				list2.RemoveAt(i);
				i--;
			}
		}
		if (list2.Count == 0 && list4.Count > 0)
		{
			list2.AddRange(list4);
		}
		if (list2.Count == 0)
		{
			if (list3.Count == 0)
			{
				return;
			}
			list2.AddRange(list3);
		}
		GamepadNavigationItem gamepadNavigationItem3 = list2[0];
		float num = gamepadNavigationItem3.CalcDistToCurrentPos(pos, direction);
		for (int j = 1; j < list2.Count; j++)
		{
			float num2 = list2[j].CalcDistToCurrentPos(pos, direction);
			if (num2 < num)
			{
				num = num2;
				gamepadNavigationItem3 = list2[j];
			}
		}
		if (restore_last_in_group)
		{
			int group2 = gamepadNavigationItem3.group;
			if (group2 != group && HaveSavedFocusForGroup(group2))
			{
				RestoreFocus(group2);
				return;
			}
		}
		SetFocusedItem(gamepadNavigationItem3);
	}

	private bool SkipItemBecauseOfState(GamepadNavigationItem item)
	{
		if (!item.is_focused && item.isActiveAndEnabled)
		{
			return !item.active;
		}
		return true;
	}

	private bool SkipItemBecauseOfGroup(GamepadNavigationItem item, int needed_group)
	{
		return item.group != needed_group;
	}

	private bool SkipItemBecauseOfSubGroup(GamepadNavigationItem item, int needed_group)
	{
		return item.sub_group != needed_group;
	}

	private bool SkipItemBecauseOfDirection(GamepadNavigationItem item, Vector2 current_pos, Direction direction)
	{
		return !item.CorrectDirection(current_pos, direction);
	}

	private bool SkipItemBecauseOfGrid(GamepadNavigationItem item, Vector2 current_pos, Direction direction)
	{
		return !item.CorrectGrid(current_pos, direction);
	}

	private void PrintItemsList(List<GamepadNavigationItem> items, string prefix)
	{
		StringBuilder stringBuilder = new StringBuilder(prefix + ": ");
		if (items.Count == 0)
		{
			stringBuilder.Append("empty");
			return;
		}
		foreach (GamepadNavigationItem item in items)
		{
			stringBuilder.Append(item.name);
			stringBuilder.Append(" ");
		}
	}
}
