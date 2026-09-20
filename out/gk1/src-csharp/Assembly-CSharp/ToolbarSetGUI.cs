using System;
using UnityEngine;

public class ToolbarSetGUI : MonoBehaviour
{
	public BaseItemCellGUI[] cells;

	private bool _initialized;

	private Item _equiped_item;

	public void Init(bool interaction_enabled)
	{
		if (!_initialized)
		{
			_initialized = true;
			_equiped_item = new Item();
			BaseItemCellGUI[] array = cells;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].interaction_enabled = interaction_enabled;
			}
		}
	}

	public void Redraw()
	{
		if (!_initialized)
		{
			Init(interaction_enabled: true);
		}
		GameSave save = MainGame.me.save;
		Item data = MainGame.me.player.data;
		for (int i = 0; i < cells.Length; i++)
		{
			_equiped_item.SetItemID(save.equipped_items[i]);
			int totalCount = data.GetTotalCount(_equiped_item.id);
			_equiped_item.value = totalCount;
			cells[i].DrawItem(_equiped_item);
			if (!string.IsNullOrEmpty(_equiped_item.id))
			{
				cells[i].container.counter.text = totalCount.ToString();
				cells[i].container.icon.alpha = ((totalCount > 0) ? 1f : 0.5f);
			}
		}
	}

	public void SetClickCallback(Action<int> callback)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			int index = i;
			cells[i].SetCallbacks((GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)delegate
			{
				callback(index);
				cells[index].SetVisualyOveredState(overed: true, by_gamepad: false);
			});
		}
	}
}
