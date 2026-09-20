using System;
using System.Collections.Generic;
using UnityEngine;

public class MixedCraftPresetGUI : MonoBehaviour
{
	[NonSerialized]
	[HideInInspector]
	public BaseItemCellGUI[] items;

	private MixedCraftGUI _mixed_craft_gui;

	private bool _opened;

	private UIWidget _ui_widget;

	public UIWidget ui_widget => _ui_widget;

	public void Init(MixedCraftGUI mixed_craft_gui, BaseItemCellGUI.OnItemAction on_over, BaseItemCellGUI.OnItemAction on_out, BaseItemCellGUI.OnItemAction on_select)
	{
		items = GetComponentsInChildren<BaseItemCellGUI>(includeInactive: true);
		BaseItemCellGUI[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetCallbacks(on_over, on_out, on_select);
		}
		_mixed_craft_gui = mixed_craft_gui;
		_ui_widget = GetComponent<UIWidget>();
		this.Deactivate();
	}

	public void Open(bool for_gamepad)
	{
		if (_opened)
		{
			Debug.LogError("MixCraftGUI preset " + base.name + " allready opened");
			return;
		}
		_opened = true;
		this.Activate();
		BaseItemCellGUI[] array = items;
		foreach (BaseItemCellGUI obj in array)
		{
			obj.DrawEmpty();
			obj.InitInputBehaviour();
		}
	}

	public void ClearItems()
	{
		BaseItemCellGUI[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DrawEmpty();
		}
	}

	public void Hide()
	{
		if (_opened)
		{
			ClearItems();
			_opened = false;
			this.Deactivate();
		}
	}

	public void OnCloseButtonPressed()
	{
		_mixed_craft_gui.OnClosePressed();
	}

	public List<Item> GetSelectedItems()
	{
		List<Item> list = new List<Item>();
		BaseItemCellGUI[] array = items;
		foreach (BaseItemCellGUI baseItemCellGUI in array)
		{
			list.Add(baseItemCellGUI.item);
		}
		return list;
	}
}
