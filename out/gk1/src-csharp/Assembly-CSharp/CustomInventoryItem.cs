using System;
using UnityEngine;

public class CustomInventoryItem : MonoBehaviour
{
	public string item_id;

	[NonSerialized]
	public int total_count;

	public GameObject gamepad_frame;

	private UILabel _counter_label;

	private Tooltip _tooltip;

	private CustomInventoryWidget _inventory_widget;

	private GamepadNavigationItem _gamepad_item;

	public GamepadNavigationItem gamepad_item => _gamepad_item;

	public void Init(CustomInventoryWidget inventory_widget)
	{
		_inventory_widget = inventory_widget;
		_counter_label = GetComponentInChildren<UILabel>(includeInactive: true);
		_tooltip = GetComponentInChildren<Tooltip>();
		gamepad_frame.Deactivate();
		ItemDefinition data = GameBalance.me.GetData<ItemDefinition>(item_id);
		if (data != null && _tooltip != null)
		{
			_tooltip.SetText(data.GetItemName());
		}
		_gamepad_item = GetComponent<GamepadNavigationItem>();
		_gamepad_item.SetCallbacks(OnOver, gamepad_frame.Deactivate, null);
	}

	public void Redraw(Inventory inventory)
	{
		total_count = inventory.data.GetTotalCount(item_id);
		_counter_label.text = "x" + total_count;
	}

	private void OnOver()
	{
		gamepad_frame.Activate();
		_inventory_widget.OnItemOver(item_id);
	}
}
