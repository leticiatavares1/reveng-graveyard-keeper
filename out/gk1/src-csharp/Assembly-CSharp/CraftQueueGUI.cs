using UnityEngine;

public class CraftQueueGUI : MonoBehaviour
{
	private CraftGUI _craft_gui;

	public CraftQueueItemGUI prefab;

	private CraftComponent _craft_component;

	public UITableOrGrid table;

	public UILabel gamepad_hint;

	private bool _is_empty;

	private WorldGameObject _craftery_wgo;

	public static CraftQueueGUI current_instance;

	public GamepadNavigationController gamepad_controller => _craft_gui.gamepad_controller;

	public void Init(CraftGUI craft_gui)
	{
		_craft_gui = craft_gui;
		prefab.gameObject.SetActive(value: false);
	}

	public void Redraw()
	{
		Draw(_craft_component);
	}

	public void Draw(CraftComponent craft_component)
	{
		_craftery_wgo = craft_component.wgo;
		current_instance = this;
		_craft_component = craft_component;
		table.DestroyChildren(new CraftQueueItemGUI[1] { prefab });
		bool emptyState = true;
		if (craft_component?.craft_queue != null)
		{
			emptyState = craft_component.IsCraftQueueEmpty() && !craft_component.is_crafting;
			CraftDefinition craftDefinition = (craft_component.is_crafting ? craft_component.current_craft : null);
			bool flag = false;
			CraftQueueItemGUI craftQueueItemGUI = prefab.Copy();
			craftQueueItemGUI.gameObject.SetActive(value: false);
			foreach (CraftComponent.CraftQueueItem item in craft_component.craft_queue)
			{
				if (item.n != 0 && item.craft != null)
				{
					CraftQueueItemGUI craftQueueItemGUI2 = prefab.Copy();
					craftQueueItemGUI2.is_gratitude_points_element = item.is_gratitude_points_craft;
					bool add_one_current = false;
					if (!flag && craftDefinition != null && item.craft.id == craftDefinition.id)
					{
						flag = true;
						add_one_current = true;
					}
					craftQueueItemGUI2.Draw(item, _craftery_wgo, add_one_current);
				}
			}
			if (!flag && craftDefinition != null)
			{
				craftQueueItemGUI.gameObject.SetActive(value: true);
				craftQueueItemGUI.Draw(new CraftComponent.CraftQueueItem
				{
					id = craftDefinition.id,
					n = 0,
					is_gratitude_points_craft = GlobalCraftControlGUI.is_global_control_active
				}, _craftery_wgo, add_one_current: true);
			}
		}
		Sounds.OnGUIClick();
		SetEmptyState(emptyState);
		table.Reposition();
		_craft_gui.gamepad_controller.ReinitItems(focus_on_first_active: false);
	}

	public bool IsEmpty()
	{
		return _is_empty;
	}

	private void SetEmptyState(bool is_empty)
	{
		base.gameObject.SetActive(!is_empty);
		_is_empty = is_empty;
	}

	public void OnDeleteItemPressed(CraftComponent.CraftQueueItem ci, bool is_current_craft)
	{
		if (_craft_component?.craft_queue == null)
		{
			return;
		}
		_craft_component?.craft_queue.Remove(ci);
		if (ci.is_gratitude_points_craft)
		{
			_craft_component.ReturnPlayerGratitudePoints();
		}
		if (is_current_craft)
		{
			_craft_component.Cancel();
		}
		Redraw();
		if (BaseGUI.for_gamepad)
		{
			CraftComponent craft_component = _craft_component;
			if (craft_component != null && craft_component.craft_queue.Count == 0)
			{
				_craft_gui.ExitFromQueueArea();
			}
			else
			{
				_craft_gui.gamepad_controller.FocusOnFirstActive(5);
			}
		}
	}

	public void AddANewItemForCurrent(ref CraftComponent.CraftQueueItem ci)
	{
		if (_craft_component?.craft_queue != null)
		{
			CraftComponent.CraftQueueItem craftQueueItem = new CraftComponent.CraftQueueItem
			{
				id = ci.id,
				n = 1,
				is_gratitude_points_craft = ci.is_gratitude_points_craft
			};
			_craft_component.craft_queue.Insert(0, craftQueueItem);
			if (GlobalCraftControlGUI.is_global_control_active)
			{
				_craft_component.TryStartCraftFromQueue(can_use_player_inventory: false, start_by_player: false);
			}
			ci = craftQueueItem;
			Redraw();
		}
	}
}
