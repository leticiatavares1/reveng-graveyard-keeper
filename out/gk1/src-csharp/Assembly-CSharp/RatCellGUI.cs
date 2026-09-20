using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class RatCellGUI : BaseGUI
{
	public const string BUTTON_INSERT_RAT = "button_insert_rat";

	public const string BUTTON_EXTRACT_RAT = "button_extract_rat";

	public const string BUFF_OR_PERK_ITEM = "Buff or Perk item";

	public GameObject no_rat_label;

	public UILabel button_label;

	public UI2DSprite rat_sprite;

	public UITable ui_table;

	public UITable status_table;

	public UITable buffs_table;

	public UIButton train_button;

	public UI2DSprite train_button_inactive;

	public UILabel rat_descr;

	private UniversalObjectInfoGUI _universal_info;

	private WorldGameObject _rat_cell_wgo;

	private Item _rat;

	public override void Init()
	{
		_universal_info = GetComponentInChildren<UniversalObjectInfoGUI>(includeInactive: true);
		RatBuffItemGUI[] componentsInChildren = GetComponentsInChildren<RatBuffItemGUI>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		base.Init();
	}

	public void Open(WorldGameObject rat_cell_wgo)
	{
		base.Open();
		_rat_cell_wgo = rat_cell_wgo;
		_rat = null;
		foreach (Item item in _rat_cell_wgo.data.inventory)
		{
			if (item.definition.type == ItemDefinition.ItemType.Rat)
			{
				_rat = item;
				break;
			}
		}
		_universal_info.Draw(_rat_cell_wgo.GetUniversalObjectInfo());
		if (_rat == null)
		{
			DrawEmpty();
		}
		else
		{
			DrawRat();
		}
	}

	public void Redraw()
	{
		if (_rat == null)
		{
			DrawEmpty();
		}
		else
		{
			DrawRat();
		}
	}

	private void DrawEmpty()
	{
		no_rat_label.SetActive(value: true);
		button_label.text = GJL.L("button_insert_rat");
		UpdateBuffsList(new List<Item>());
		train_button.isEnabled = false;
		train_button_inactive.SetActive(active: true);
		rat_descr.text = string.Empty;
	}

	private void DrawRat()
	{
		no_rat_label.SetActive(value: false);
		button_label.text = GJL.L("button_extract_rat");
		UpdateBuffsList(_rat.GetAllRatBuffs());
		train_button.isEnabled = true;
		train_button_inactive.SetActive(active: false);
		rat_descr.text = _rat.GetRatDescription();
	}

	public void OnBuffInsertionButtonPressed()
	{
		if (_rat != null)
		{
			GUIElements.me.craft.OpenAsRatCell(_rat_cell_wgo, _rat);
		}
	}

	public void OnRatInsertionButtonPressed()
	{
		if (_rat == null)
		{
			WorldGameObject obj = MainGame.me.player;
			if (GlobalCraftControlGUI.is_global_control_active && _rat_cell_wgo != null)
			{
				obj = _rat_cell_wgo;
			}
			GUIElements.me.resource_picker.Open(obj, delegate(Item item, InventoryWidget widget)
			{
				if (item == null || item.IsEmpty())
				{
					return InventoryWidget.ItemFilterResult.Hide;
				}
				return (item.definition.type != ItemDefinition.ItemType.Rat) ? InventoryWidget.ItemFilterResult.Inactive : InventoryWidget.ItemFilterResult.Active;
			}, OnRatForInsertionPicked);
		}
		else
		{
			_rat_cell_wgo.DropItem(_rat);
			_rat_cell_wgo.data.RemoveItem(new Item(_rat.id));
			_rat = null;
			Hide(play_hide_sound: false);
		}
	}

	private void OnRatForInsertionPicked(Item rat)
	{
		if (rat == null)
		{
			DrawEmpty();
			return;
		}
		_rat_cell_wgo.AddToInventory(rat);
		foreach (Item item in _rat_cell_wgo.data.inventory)
		{
			if (item.definition.type == ItemDefinition.ItemType.Rat)
			{
				_rat = item;
				break;
			}
		}
		bool flag = false;
		foreach (Inventory item2 in MainGame.me.player.GetMultiInventory().all)
		{
			if (item2.data.inventory.Contains(rat))
			{
				if (item2.data.inventory.Remove(rat))
				{
					flag = true;
					break;
				}
				Debug.LogError("FATAL ERROR: error while removing rat item from multi inventory\n id=" + rat.id);
			}
		}
		if (!flag)
		{
			Debug.LogError("FATAL ERROR: not found rat item in multi inventory\n id=" + rat.id);
		}
		DrawRat();
	}

	public override void Hide(bool play_hide_sound = true)
	{
		base.Hide(play_hide_sound);
	}

	public override void OnClosePressed()
	{
		Hide();
	}

	private void UpdateBuffsList(List<Item> buffs)
	{
		List<RatBuffItemGUI> list = status_table.GetComponentsInChildren<RatBuffItemGUI>(includeInactive: true).ToList();
		List<RatBuffItemGUI> list2 = buffs_table.GetComponentsInChildren<RatBuffItemGUI>(includeInactive: true).ToList();
		if (list.Count < 1)
		{
			Debug.LogError("FATAL ERROR: Not found PerkBuffItemGUI in RatCellGUI.status_table table! Call Bulat.");
			return;
		}
		if (list2.Count < 1)
		{
			Debug.LogError("FATAL ERROR: Not found PerkBuffItemGUI in RatCellGUI.buffs_table table! Call Bulat.");
			return;
		}
		List<Item> list3 = new List<Item>();
		List<Item> list4 = new List<Item>();
		foreach (Item buff in buffs)
		{
			if (buff.definition.has_durability)
			{
				list3.Add(buff);
			}
			else
			{
				list4.Add(buff);
			}
		}
		if (list3.Count == 0)
		{
			foreach (RatBuffItemGUI item in list)
			{
				item.SetActive(active: false);
			}
		}
		else
		{
			int i;
			for (i = 0; i < list3.Count; i++)
			{
				if (i >= list.Count)
				{
					list.Add(list[list.Count - 1].Copy(status_table.transform, activate: true, "Buff or Perk item"));
				}
				list[i].SetActive(active: true);
				try
				{
					list[i].Draw(list3[i]);
				}
				catch (ArgumentOutOfRangeException ex)
				{
					Debug.LogError($"ArgumentOutOfRangeException: status_gui_items.Count={list.Count}, status_items.Count={list3.Count}, i={i}. Error: {ex}");
					throw;
				}
			}
			for (int j = i + 1; j < list.Count; j++)
			{
				list[j].SetActive(active: false);
			}
		}
		if (list4.Count == 0)
		{
			foreach (RatBuffItemGUI item2 in list2)
			{
				item2.SetActive(active: false);
			}
		}
		else
		{
			int i;
			for (i = 0; i < list4.Count; i++)
			{
				if (i >= list2.Count)
				{
					list2.Add(list2[list2.Count - 1].Copy(buffs_table.transform, activate: true, "Buff or Perk item"));
				}
				list2[i].SetActive(active: true);
				list2[i].Draw(list4[i]);
			}
			for (int k = i + 1; k < list2.Count; k++)
			{
				list2[k].SetActive(active: false);
			}
		}
		status_table.Reposition();
		buffs_table.Reposition();
		ui_table.Reposition();
	}
}
