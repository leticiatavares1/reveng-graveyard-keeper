using UnityEngine;

public class CraftResourcesSelectGUI : BaseGUI
{
	public delegate void ResourceSelectResultDelegate(Item item);

	private InventoryPanelGUI _inventory_panel;

	private ResourceSelectResultDelegate _result_delegate;

	private Item _selected_result;

	private BaseItemCellGUI current_item_gui => _inventory_panel.selected_item_gui;

	private Item current_item => _inventory_panel.selected_item;

	private bool current_item_is_selectable
	{
		get
		{
			if (current_item_gui != null && !current_item_gui.is_inactive_state && current_item != null)
			{
				return current_item.IsNotEmpty();
			}
			return false;
		}
	}

	public override void Init()
	{
		_inventory_panel = GetComponentInChildren<InventoryPanelGUI>(includeInactive: true);
		_inventory_panel.Init();
		_inventory_panel.SetCallbacks(OnItemOver, null, OnItemSelect, OnItemOver);
		_selected_result = null;
		base.Init();
	}

	public void Open(WorldGameObject obj, InventoryWidget.ItemFilterDelegate filter_delegate, ResourceSelectResultDelegate result_delegate, bool force_ignore_toolbelt = false)
	{
		if (base.is_shown)
		{
			Debug.LogError("CraftResourcesSelectGUI is already opened");
			return;
		}
		base.Open();
		_selected_result = null;
		_result_delegate = result_delegate;
		MultiInventory multiInventory = obj.GetMultiInventory(null, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true, include_bags: true);
		bool flag = false;
		if (GlobalCraftControlGUI.is_global_control_active && !obj.is_player)
		{
			WorldZone zoneOfObject = WorldZone.GetZoneOfObject(obj);
			if (zoneOfObject != null && !zoneOfObject.IsPlayerInZone())
			{
				flag = true;
			}
		}
		if (obj.is_player && multiInventory.all.Count > 0 && !flag && !force_ignore_toolbelt)
		{
			Item data = new Item
			{
				inventory_size = 7,
				inventory = obj.data.secondary_inventory
			};
			multiInventory.all.Insert(1, new Inventory(data, "Instruments"));
		}
		_inventory_panel.Open(multiInventory);
		_inventory_panel.FilterItems(filter_delegate);
		_inventory_panel.SetInactiveStateToEmptyCells();
		if (BaseGUI.for_gamepad)
		{
			_inventory_panel.InitGamepad(base.gamepad_controller);
		}
	}

	private void OnItemOver()
	{
		if (!BaseGUI.for_gamepad)
		{
			if (_inventory_panel.selected_item_is_empty)
			{
				_inventory_panel.selected_item_gui.SetVisualyOveredState(overed: false, by_gamepad: false);
			}
		}
		else
		{
			base.button_tips.Print(GameKeyTip.Select(current_item_is_selectable), GameKeyTip.Back());
		}
	}

	private void OnItemSelect()
	{
		if (current_item_is_selectable && !LazyInput.GetKeyDown(GameKey.RightClick))
		{
			_selected_result = current_item;
			OnPressedBack();
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		_inventory_panel.Hide();
		base.Hide(play_hide_sound);
		if (_result_delegate != null)
		{
			_result_delegate(_selected_result);
		}
	}

	public void ClearResultDelegate()
	{
		_result_delegate = null;
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public void OnCloseBtnPressed()
	{
		OnPressedBack();
	}
}
