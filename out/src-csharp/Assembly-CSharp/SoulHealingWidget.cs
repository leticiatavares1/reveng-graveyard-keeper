using System;
using System.Collections.Generic;
using UnityEngine;

public class SoulHealingWidget : BaseGUI
{
	public Action ON_GUI_ITEM_CHANGE;

	public Action<GamepadNavigationItem> ON_FOCUS_GAMEPAD_ITEM;

	private const string INSERT_SOUL_BUTTON_LABEL = "insert_soul";

	private const string TAKE_OUT_SOUL_BUTTON_LABEL = "takeout_soul";

	private const int SOUL_ITEM_IN_INVENTORY_INDEX = 7;

	private const int SIN_SHARD_BONUS_MULTIPLIER = 1;

	[SerializeField]
	private BaseItemCellGUI _soul_item_gui;

	[SerializeField]
	private GameObject _heal_button_object;

	[SerializeField]
	private SoulExtractorPanelBarGUI _bar_gui;

	[Space]
	[SerializeField]
	private GamepadSelectableButton _heal_button_gamepad;

	private WorldGameObject _wgo;

	private int _ordinal_number;

	private List<SinItem> _sin_items;

	private float _sins_heal_rate;

	private bool is_heal_button_interactive;

	public Item inserted_item => _soul_item_gui.item;

	public float sins_heal_rate
	{
		private get
		{
			return _sins_heal_rate;
		}
		set
		{
			_sins_heal_rate = value;
		}
	}

	public GamepadSelectableButton heal_button_gamepad => _heal_button_gamepad;

	public new void Init()
	{
		_heal_button_gamepad.Init();
		_heal_button_gamepad.SetCallbacks(OnStartHealButtonPressed, OnHealButtonOver);
		_soul_item_gui.SetCallbacks(OnItemOver, null, OnItemPress);
		base.Init();
	}

	public void Draw(WorldGameObject container_obj, List<SinItem> sin_items)
	{
		_wgo = container_obj;
		_sin_items = sin_items;
		_bar_gui.SetActive(active: false);
		_soul_item_gui.InitInputBehaviour();
		_soul_item_gui.InitTooltips();
		_soul_item_gui.DrawItem(container_obj.data.GetItemByIndex(7));
		bool num = IsSoulItemInserted();
		UpdateHealButtonInteraction();
		if (num)
		{
			_bar_gui.SetActive(active: true);
			_bar_gui.SetData(_soul_item_gui.item.durability, 0f, sins_heal_rate.EqualsTo(-1f) ? 0f : (0.5f * (1f - sins_heal_rate)));
			_bar_gui.Redraw();
		}
	}

	public void OnStartHealButtonPressed()
	{
		CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>(_wgo.obj_id + ":" + inserted_item.id);
		if (dataOrNull == null)
		{
			return;
		}
		GUIElements.me.soul_healer_gui.Hide();
		Item itemOfType = _wgo.GetItemOfType(ItemDefinition.ItemType.Soul);
		_wgo.data.RemoveItem(itemOfType);
		Item item = new Item("soul_healed", 1)
		{
			durability = itemOfType.durability - (sins_heal_rate.EqualsTo(-1f) ? 0f : (0.5f * (1f - sins_heal_rate)))
		};
		float param = item.GetParam("durability");
		_ = 1f / (_wgo.GetParam("sin_drop_max_count") + 1f);
		int paramInt = itemOfType.GetParamInt("sins_count");
		int num = ((param > 0f) ? paramInt : 0);
		num += MainGame.me.player.GetParamInt("increase_sin_shard_drop") * num;
		Item item2 = new Item("sin_shard", num);
		item.AddToParams("sins_count", paramInt);
		dataOrNull.output = new List<Item> { item, item2 };
		foreach (Item item3 in dataOrNull.output)
		{
			item3.min_value = new SmartExpression();
			item3.max_value = new SmartExpression();
			item3.self_chance = new SmartExpression
			{
				default_value = 1f
			};
		}
		_wgo.components.craft.Craft(dataOrNull);
	}

	private static InventoryWidget.ItemFilterResult SoulItemsFilter(Item item)
	{
		if (item == null || item.IsEmpty())
		{
			return InventoryWidget.ItemFilterResult.Hide;
		}
		if (item.definition.type != ItemDefinition.ItemType.Soul)
		{
			return InventoryWidget.ItemFilterResult.Inactive;
		}
		if (item.id == "soul_healed")
		{
			return InventoryWidget.ItemFilterResult.Inactive;
		}
		return InventoryWidget.ItemFilterResult.Active;
	}

	private void OnItemPicked(Item item)
	{
		if (item == null || item.IsEmpty())
		{
			if (BaseGUI.for_gamepad)
			{
				ON_FOCUS_GAMEPAD_ITEM?.Invoke(_soul_item_gui.gamepad_item);
			}
			return;
		}
		_wgo.data.AddItemByIndex(item, 7);
		WorldZone myWorldZone = _wgo.GetMyWorldZone();
		WorldGameObject worldGameObject = _wgo;
		if (myWorldZone != null && myWorldZone.IsPlayerInZone())
		{
			worldGameObject = MainGame.me.player;
		}
		worldGameObject.GetMultiInventory(new List<WorldGameObject> { _wgo }, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true, include_bags: true).RemoveItem(item);
		Redraw();
		if (BaseGUI.for_gamepad)
		{
			ON_FOCUS_GAMEPAD_ITEM?.Invoke(_heal_button_gamepad.navigation_item);
		}
		ON_GUI_ITEM_CHANGE?.Invoke();
	}

	public void Redraw()
	{
		Draw(_wgo, _sin_items);
	}

	public void UpdateHealButtonInteraction()
	{
		bool flag = IsSoulItemInserted();
		for (int i = 0; i < _sin_items.Count; i++)
		{
			if (flag)
			{
				if (SinItem.GetSinItemByTypeFromItem(_sin_items[i].item_type, _soul_item_gui.item) != null)
				{
					if (_sin_items[i].item_cell_gui.item == null)
					{
						SetHealButtonInteractionState(is_active: false);
						return;
					}
					if (_sin_items[i].item_cell_gui.item.IsEmpty())
					{
						SetHealButtonInteractionState(is_active: false);
						return;
					}
				}
				continue;
			}
			SetHealButtonInteractionState(is_active: false);
			return;
		}
		SetHealButtonInteractionState(is_active: true);
	}

	public void ClearSoulItemInGUI()
	{
		_soul_item_gui.DrawEmpty();
	}

	private void SetHealButtonInteractionState(bool is_active)
	{
		UIButton[] componentsInChildren = _heal_button_object.GetComponentsInChildren<UIButton>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].isEnabled = is_active;
			is_heal_button_interactive = is_active;
		}
	}

	private void OnItemOver(BaseItemCellGUI item_gui)
	{
		if (item_gui.item == null || item_gui.item.IsEmpty())
		{
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Print(GameKeyTip.Select("select"), GameKeyTip.Close());
			}
		}
		else if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select("take"), GameKeyTip.Close());
		}
	}

	private void OnItemPress(BaseItemCellGUI item_gui)
	{
		if (_soul_item_gui.item != null && !_soul_item_gui.item.IsEmpty())
		{
			WorldZone myWorldZone = _wgo.GetMyWorldZone();
			if (myWorldZone != null && !myWorldZone.IsPlayerInZone())
			{
				MultiInventory multiInventory = _wgo.GetMultiInventory();
				if (multiInventory != null && multiInventory.CanAddItem(_soul_item_gui.item))
				{
					multiInventory.AddItem(_soul_item_gui.item);
				}
				else
				{
					_wgo.DropItem(_soul_item_gui.item);
				}
			}
			else if (MainGame.me.player.data.CanAddItem(_soul_item_gui.item))
			{
				MainGame.me.player.data.AddItem(_soul_item_gui.item);
			}
			else
			{
				_wgo.DropItem(_soul_item_gui.item, Direction.ToPlayer);
			}
			_soul_item_gui.DrawEmpty();
			_wgo.data.RemoveItemByIndex(7);
			Draw(_wgo, _sin_items);
			if (BaseGUI.for_gamepad)
			{
				ON_FOCUS_GAMEPAD_ITEM?.Invoke(item_gui.gamepad_item);
			}
			ON_GUI_ITEM_CHANGE?.Invoke();
		}
		else
		{
			WorldGameObject obj = MainGame.me.player;
			if (GlobalCraftControlGUI.is_global_control_active && _wgo != null)
			{
				obj = _wgo;
			}
			GUIElements.me.resource_picker.Open(obj, (Item itm, InventoryWidget widget) => SoulItemsFilter(itm), OnItemPicked);
		}
	}

	private bool IsSoulItemInserted()
	{
		if (_soul_item_gui.item != null && !_soul_item_gui.item.IsEmpty())
		{
			return _soul_item_gui.item.definition.type == ItemDefinition.ItemType.Soul;
		}
		return false;
	}

	private void OnHealButtonOver()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(is_heal_button_interactive), GameKeyTip.Close());
		}
	}
}
