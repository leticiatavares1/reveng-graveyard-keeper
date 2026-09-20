using UnityEngine;

public class SoulContainerWidget : MonoBehaviour
{
	private const string INSERT_SOUL_BUTTON_LABEL = "insert_soul";

	private const string TAKE_OUT_SOUL_BUTTON_LABEL = "takeout_soul";

	private const string SLOT_SOUL_ITEM_ID_PREFIX = "soul_placed_";

	[SerializeField]
	private UI2DSprite _item_icon;

	[SerializeField]
	private UI2DSprite _no_icon;

	[SerializeField]
	private GameObject _button_obj;

	[SerializeField]
	private UILabel _buttons_label;

	[SerializeField]
	private SoulExtractorPanelBarGUI _bar_gui;

	private bool _is_item_set;

	private Item _inserted_item;

	private WorldGameObject _wgo;

	private int _ordinal_number;

	public void Draw(int ordinal_number, WorldGameObject container_obj)
	{
		_ordinal_number = ordinal_number;
		_inserted_item = container_obj.data.GetItemByIndex(ordinal_number);
		_is_item_set = _inserted_item != null && !_inserted_item.IsEmpty();
		_wgo = container_obj;
		_bar_gui.SetActive(active: false);
		_item_icon.SetActive(active: false);
		_no_icon.SetActive(active: false);
		UIButton[] componentsInChildren = _button_obj.GetComponentsInChildren<UIButton>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].isEnabled = true;
		}
		_buttons_label.text = GJL.L(_is_item_set ? "takeout_soul" : "insert_soul");
		if (!_is_item_set)
		{
			_no_icon.SetActive(active: true);
			return;
		}
		int num = MainGame.me.player.data.CanAddCount(_inserted_item, count_empty: true);
		_ = 0;
		componentsInChildren = _button_obj.GetComponentsInChildren<UIButton>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].isEnabled = num > 0;
		}
		_item_icon.SetActive(active: true);
		_bar_gui.SetActive(active: true);
		_bar_gui.SetData(_inserted_item.durability);
		_bar_gui.Redraw();
	}

	public void OnButtonPressed()
	{
		if (_is_item_set)
		{
			MainGame.me.player.data.AddItem(_inserted_item);
			_wgo.data.RemoveItemByIndex(_ordinal_number);
			Draw(_ordinal_number, _wgo);
			GUIElements.me.soul_container_gui.Redraw(_wgo);
			return;
		}
		WorldGameObject obj = MainGame.me.player;
		if (GlobalCraftControlGUI.is_global_control_active && _wgo != null)
		{
			obj = _wgo;
		}
		GUIElements.me.resource_picker.Open(obj, (Item itm, InventoryWidget widget) => SoulItemsFilter(itm), OnItemPicked);
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
		return InventoryWidget.ItemFilterResult.Active;
	}

	private void OnItemPicked(Item item)
	{
		if (item != null && !item.IsEmpty())
		{
			_wgo.data.AddItemByIndex(item, _ordinal_number);
			MainGame.me.player.data.RemoveItem(item);
			GUIElements.me.soul_container_gui.Redraw(_wgo);
		}
	}
}
