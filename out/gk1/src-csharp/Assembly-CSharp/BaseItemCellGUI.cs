using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class BaseItemCellGUI : MonoBehaviour
{
	public delegate void OnItemAction(BaseItemCellGUI item);

	private GJCommons.VoidDelegate _on_over_void;

	private GJCommons.VoidDelegate _on_out_void;

	private GJCommons.VoidDelegate _on_select_void;

	private OnItemAction _on_over;

	private OnItemAction _on_out;

	private OnItemAction _on_action;

	public BaseItemCellElements x1;

	public BaseItemCellElements x2;

	public ItemCellColors colors;

	public UILabel item_name;

	public UILabel item_description;

	public UILabel price;

	public UI2DSprite additional_icon;

	public UI2DSprite progress;

	public UI2DSprite quality_icon;

	[NonSerialized]
	[HideInInspector]
	public BaseItemCellElements container;

	[HideInInspector]
	public GamepadNavigationItem gamepad_item;

	private ItemDefinition _item_definition;

	private bool _interaction_enabled = true;

	private bool _is_inactive_state;

	private Item _item;

	private string _multiquality_id = "";

	private bool _mouse_overed;

	public UIEventTrigger additional_button;

	public UI2DSprite radial_dim;

	private float _update_period;

	private Item _last_drawitem_item;

	private bool _last_drawitem_tooltip = true;

	private string _last_drawitem_multiquality = "";

	public UIWidget widget => GetComponent<UIWidget>();

	public string item_id
	{
		get
		{
			if (_item != null)
			{
				if (!string.IsNullOrEmpty(_multiquality_id))
				{
					return _multiquality_id;
				}
				return _item.id;
			}
			return "";
		}
	}

	public Item item => _item;

	public bool is_inactive_state => _is_inactive_state;

	public bool id_empty
	{
		get
		{
			if (_item != null)
			{
				return _item.IsEmpty();
			}
			return true;
		}
	}

	public bool interaction_enabled
	{
		get
		{
			return _interaction_enabled;
		}
		set
		{
			_interaction_enabled = value;
			if (container == null)
			{
				container = x1;
			}
			container.collider.enabled = value;
		}
	}

	public void DrawEmpty()
	{
		DrawItem(Item.empty);
	}

	public void DrawItem(Item i, bool init_tooltip = true, string multiquality_id = "", bool try_optimize_redraw = false, bool infinity_counter = false)
	{
		if (i == null)
		{
			i = Item.empty;
		}
		if (try_optimize_redraw && _last_drawitem_item != null && i?.id == _last_drawitem_item?.id && i?.value == _last_drawitem_item?.value && _last_drawitem_multiquality == multiquality_id && _last_drawitem_tooltip == init_tooltip)
		{
			return;
		}
		_last_drawitem_item = i;
		_last_drawitem_multiquality = multiquality_id;
		_last_drawitem_tooltip = init_tooltip;
		_item = i;
		if (radial_dim != null)
		{
			radial_dim.gameObject.SetActive(value: false);
		}
		_multiquality_id = ((!_item.is_multiquality) ? "" : (string.IsNullOrEmpty(multiquality_id) ? _item.multiquality_items[0] : multiquality_id));
		string text = item_id;
		if (text == "" || text == null)
		{
			text = "empty";
		}
		int value = _item.value;
		base.gameObject.SetActive(value: true);
		_item_definition = null;
		if (text != "empty" && !TechDefinition.TECH_POINTS.Contains(text))
		{
			_item_definition = GameBalance.me.GetDataOrNull<ItemDefinition>(text);
			if (_item_definition == null)
			{
				List<string> itemsOfBaseName = GameBalance.me.GetItemsOfBaseName(text);
				if (itemsOfBaseName.Count > 0)
				{
					_item_definition = GameBalance.me.GetData<ItemDefinition>(itemsOfBaseName[0]);
				}
			}
		}
		bool flag = _item_definition == null || _item_definition.is_small;
		bool flag2 = text == "empty" || _item_definition == null;
		if (x1.container == null)
		{
			Debug.LogException(new Exception("No container for item " + base.name), this);
			return;
		}
		x1.container.SetActive(flag);
		if (x2 != null && x2.container != null)
		{
			x2.container.SetActive(!flag);
		}
		container = (flag ? x1 : x2);
		if (container == null && text == "body")
		{
			container = x1;
			x1.container.SetActive(value: true);
			text = "body_shoud";
		}
		if (flag2)
		{
			container.icon.sprite2D = null;
		}
		else
		{
			container.icon.sprite2D = EasySpritesCollection.GetSprite(_item_definition.GetIcon());
		}
		if (container.empty_item_gfx != null)
		{
			container.empty_item_gfx.SetActive(flag2);
		}
		container.icon.color = ((_item.durability_state == Item.DurabilityState.Broken) ? colors.broken : colors.normal);
		if (container.collider != null)
		{
			container.collider.enabled = interaction_enabled;
		}
		if (item_name != null)
		{
			item_name.text = (flag2 ? "" : _item_definition.GetItemName());
		}
		if (item_description != null)
		{
			item_description.text = (flag2 ? "" : _item_definition.GetItemDescription());
		}
		if ((bool)container.counter)
		{
			if (infinity_counter)
			{
				container.counter.text = "∞";
			}
			else
			{
				container.counter.text = ((value <= 1 || flag2) ? "" : value.ToString());
			}
		}
		if (price != null)
		{
			price.text = "";
		}
		if (container.tooltip != null)
		{
			container.tooltip.SetData((init_tooltip && !flag2) ? _item_definition.GetTooltipData(i) : null);
		}
		container.selection.Deactivate();
		container.gamepad_frame.Deactivate();
		if (progress != null)
		{
			if (!flag2 && _item?.definition != null && _item.definition.has_durability)
			{
				if (_item.durability_state == Item.DurabilityState.Full)
				{
					progress.Deactivate();
				}
				else
				{
					progress.Activate();
					progress.fillAmount = _item.durability;
				}
			}
			else
			{
				progress.Deactivate();
			}
		}
		if (additional_icon != null)
		{
			additional_icon.Deactivate();
		}
		DrawQualityIcon(force_disable: false);
		RedrawCooldown();
		_update_period = 0f;
		DrawCapIcon(vis: false);
		DrawGratitudeIcon(vis: false);
	}

	public void DrawEquippedIcons()
	{
		if (additional_icon == null)
		{
			return;
		}
		if (((item == null || item.IsEmpty()) ? null : item.definition) == null)
		{
			additional_icon.sprite2D = null;
		}
		else
		{
			string text = "";
			if (_item.equipped_as == ItemDefinition.EquipmentType.None)
			{
				if (_item.definition.equipment_type == ItemDefinition.EquipmentType.None)
				{
				}
			}
			else
			{
				text = "icon_" + _item.equipped_as.ToString().ToLower() + "_equipped";
			}
			additional_icon.sprite2D = ((text == "") ? null : EasySpritesCollection.GetSprite(text));
		}
		additional_icon.SetActive(additional_icon.sprite2D != null);
	}

	private void DrawQualityIcon(bool force_disable)
	{
		if (!(quality_icon == null))
		{
			if (force_disable || _item_definition == null || _item_definition.quality_type == ItemDefinition.QualityType.Default)
			{
				quality_icon.sprite2D = null;
				quality_icon.gameObject.SetActive(value: false);
			}
			else
			{
				_item_definition.TryDrawQualityOrDisableGameObject(quality_icon);
			}
		}
	}

	public void DrawItem(string id, int value, bool init_tooltip = true, bool try_optimize_redraw = false, bool infinity_counter = false)
	{
		DrawItem(new Item(id, value), init_tooltip, "", try_optimize_redraw, infinity_counter);
	}

	public void RedrawItem()
	{
		DrawItem(_item);
	}

	public void DrawIngredient(Item item, MultiInventory multi_inventory, bool deactivate_colliders = false, bool init_tooltip = false, string multiquality_id = "", Item additional_inventory = null, List<Item> used_items = null, bool item_is_a_group_of_multiquality = false)
	{
		base.gameObject.SetActive(item != null);
		if (item == null)
		{
			return;
		}
		if (additional_inventory != null && multi_inventory != null)
		{
			foreach (Inventory item2 in multi_inventory.all)
			{
				if (item2 != null && item2.data == additional_inventory)
				{
					additional_inventory = null;
					Debug.Log("Additional inventory ignored: duplicate in multi_inventory");
					break;
				}
			}
		}
		DrawItem(item, init_tooltip, multiquality_id);
		if (deactivate_colliders)
		{
			container.collider.enabled = false;
		}
		if (item.value == 0)
		{
			container.counter.text = "";
		}
		else
		{
			int num = 0;
			if (item.id == "gratitude_as_item")
			{
				num = (int)MainGame.me.player.gratitude_points;
			}
			else
			{
				if (item_is_a_group_of_multiquality)
				{
					foreach (string multiquality_item in item.multiquality_items)
					{
						if (multi_inventory != null)
						{
							num += multi_inventory.GetTotalCount(multiquality_item);
						}
						if (additional_inventory != null)
						{
							num += additional_inventory.GetItemsCount(multiquality_item);
						}
					}
				}
				else
				{
					if (multi_inventory != null)
					{
						num += multi_inventory.GetTotalCount(item_id);
					}
					if (additional_inventory != null)
					{
						num += additional_inventory.GetItemsCount(item_id);
					}
				}
				if (used_items != null)
				{
					foreach (Item used_item in used_items)
					{
						if (used_item.id == item_id)
						{
							num -= used_item.value;
						}
					}
					if (num < 0)
					{
						num = 0;
					}
				}
			}
			container.counter.text = num + "/" + item.value;
			container.counter.color = ((num >= item.value) ? colors.enough_res : colors.not_enough_res);
		}
		if (item_name != null)
		{
			item_name.text = item.GetItemName();
		}
	}

	public void DrawIcon(string icon, bool draw_back = true, bool hide_quality_icon = true)
	{
		if (x1 != null && x1.container != null)
		{
			x1.container.SetActive(value: true);
		}
		if (x2 != null && x2.container != null)
		{
			x2.container.SetActive(value: false);
		}
		container = x1;
		if (container == null)
		{
			Debug.LogError("Container is null", this);
			return;
		}
		DrawCapIcon(vis: false);
		DrawGratitudeIcon(vis: false);
		container.icon.sprite2D = EasySpritesCollection.GetSprite(icon);
		if (container.empty_item_gfx != null)
		{
			container.empty_item_gfx.SetActive(string.IsNullOrEmpty(icon));
		}
		if (container.gamepad_frame != null)
		{
			container.gamepad_frame.gameObject.SetActive(value: false);
		}
		if (container.back != null)
		{
			container.back.gameObject.SetActive(draw_back);
		}
		if (price != null)
		{
			price.text = "";
		}
		if (additional_icon != null && hide_quality_icon)
		{
			additional_icon.gameObject.SetActive(value: false);
		}
		if (container != null && container.counter != null)
		{
			container.counter.text = "";
		}
	}

	public void HideBack()
	{
		container.back.gameObject.SetActive(value: false);
	}

	public void ResizeIconByContent()
	{
		container.icon.ResizeByContent();
	}

	public void InitInputBehaviour(int navigation_group = 0, int navigation_sub_group = 0)
	{
		if (gamepad_item == null)
		{
			gamepad_item = GetComponent<GamepadNavigationItem>();
		}
		if (gamepad_item == null)
		{
			return;
		}
		gamepad_item.group = navigation_group;
		gamepad_item.sub_group = navigation_sub_group;
		gamepad_item.active = BaseGUI.for_gamepad;
		_mouse_overed = false;
		if (BaseGUI.for_gamepad)
		{
			gamepad_item.SetCallbacks(delegate
			{
				OnOver(by_gamepad: true);
			}, delegate
			{
				OnOut(by_gamepad: true);
			}, delegate
			{
				OnPressed(by_gamepad: true);
			});
		}
	}

	public void InitTooltips()
	{
		if (x1.tooltip != null)
		{
			x1.tooltip.Init();
		}
		if (x2.tooltip != null)
		{
			x2.tooltip.Init();
		}
	}

	public void OnMouseOvered()
	{
		if (!_mouse_overed)
		{
			_mouse_overed = true;
			if (!BaseGUI.for_gamepad)
			{
				OnOver(by_gamepad: false);
			}
		}
	}

	public void OnMouseOut()
	{
		_mouse_overed = false;
		if (!BaseGUI.for_gamepad)
		{
			OnOut(by_gamepad: false);
		}
	}

	public void ForceMouseOut()
	{
		_mouse_overed = false;
	}

	public void OnMousePress()
	{
		if (!BaseGUI.for_gamepad)
		{
			OnPressed(by_gamepad: false);
		}
	}

	public void OnOver(bool by_gamepad)
	{
		if (!_interaction_enabled || GUIElements.me.context_menu_bubble.is_shown)
		{
			return;
		}
		try
		{
			if (base.gameObject == null)
			{
				return;
			}
		}
		catch (MissingReferenceException)
		{
			return;
		}
		SetVisualyOveredState(overed: true, by_gamepad);
		if (_on_over != null)
		{
			_on_over(this);
		}
		_on_over_void.TryInvoke();
		try
		{
			if (by_gamepad == BaseGUI.for_gamepad && container != null && (by_gamepad ? container.gamepad_frame : container.selection).gameObject.activeSelf)
			{
				Sounds.OnGUIHover(Sounds.ElementType.ItemCell);
			}
		}
		catch (MissingReferenceException ex2)
		{
			Debug.LogError("Missing ref: " + ex2);
		}
		TooltipsManager.Redraw();
	}

	public void OnOut(bool by_gamepad)
	{
		if (_interaction_enabled)
		{
			SetVisualyOveredState(overed: false, by_gamepad);
			if (_on_out != null)
			{
				_on_out(this);
			}
			_on_out_void.TryInvoke();
		}
	}

	public void OnPressed(bool by_gamepad)
	{
		if (_interaction_enabled && !_is_inactive_state)
		{
			_on_select_void.TryInvoke();
			if (_on_action != null)
			{
				_on_action(this);
			}
			if (!Sounds.WasAnySoundPlayedThisFrame() && (by_gamepad ? container.gamepad_frame : container.selection).gameObject.activeSelf)
			{
				Sounds.OnGUIClick();
			}
		}
	}

	public void SetVisualyOveredState(bool overed, bool by_gamepad)
	{
		if (container == null || container.icon == null)
		{
			Debug.LogError("icon is null", this);
		}
		else
		{
			NGUIExtensionMethods.ChangeColor(color: (_item == null || item.durability_state != 0) ? ((!overed) ? colors.normal : (by_gamepad ? colors.gamepad_overed : colors.mouse_overed)) : colors.broken, w: container.icon, duration: 0f, on_complete: null, delay: 0f, ignore_alpha: true);
			(by_gamepad ? container.gamepad_frame : container.selection).SetActive(overed);
		}
	}

	public void SetGrayState(bool set_inactive = true)
	{
		SetInactiveState(set_inactive);
	}

	public void SetInactiveState(bool set_inactive = true)
	{
		_is_inactive_state = set_inactive;
		widget.alpha = (_is_inactive_state ? colors.inactive.a : 1f);
	}

	public void DrawUnknown()
	{
		DrawItem(new Item("unknown", 1));
	}

	public void SetCallbacks(GJCommons.VoidDelegate on_over, GJCommons.VoidDelegate on_out, GJCommons.VoidDelegate on_action)
	{
		_on_over_void = on_over;
		_on_out_void = on_out;
		_on_select_void = on_action;
	}

	public void SetCallbacks(OnItemAction on_over, OnItemAction on_out, OnItemAction on_action)
	{
		_on_over = on_over;
		_on_out = on_out;
		_on_action = on_action;
	}

	public void SetItemPrice(float price)
	{
		if (this.price == null)
		{
			Debug.LogError("Item has no price field", this);
		}
		else
		{
			this.price.text = Trading.FormatMoney(price);
		}
	}

	public void ClearPrice()
	{
		if (price == null)
		{
			Debug.LogError("Item has no price field", this);
		}
		else
		{
			price.text = "";
		}
	}

	public static void DrawIngredients(BaseItemCellGUI[] ingredients, List<Item> items, MultiInventory multi_inventory, List<string> multiquality_ids = null, int amount = 1)
	{
		List<Item> list = new List<Item>();
		for (int i = 0; i < ingredients.Length; i++)
		{
			Item item = ((i >= items.Count) ? null : items[i]);
			ingredients[i].gameObject.SetActive(item != null);
			if (item != null)
			{
				if (amount > 1)
				{
					List<string> multiquality_items = item.multiquality_items;
					item = new Item(item.id, item.value * amount)
					{
						multiquality_items = multiquality_items
					};
				}
				string text = item.id;
				if ((multiquality_ids == null || multiquality_ids[i] == null) && item.multiquality_items.Count > 1)
				{
					ingredients[i].DrawIngredient(item, multi_inventory, deactivate_colliders: false, init_tooltip: true, item.multiquality_items.FirstOrDefault(), null, list, item_is_a_group_of_multiquality: true);
					ingredients[i].quality_icon.sprite2D = null;
				}
				else
				{
					ingredients[i].DrawIngredient(item, multi_inventory, deactivate_colliders: false, init_tooltip: true, multiquality_ids?[i], null, list);
				}
				if (!string.IsNullOrEmpty(ingredients[i]._multiquality_id))
				{
					text = ingredients[i]._multiquality_id;
				}
				list.Add(new Item(text, item.value * amount));
			}
		}
	}

	private void RedrawCooldown()
	{
		if (_item != null && _item_definition != null && _item_definition.can_be_used && _item_definition.cooldown.has_expression && radial_dim != null)
		{
			radial_dim.gameObject.SetActive(value: true);
			radial_dim.fillAmount = (float)_item.GetGrayedCooldownPercent() / 100f;
		}
	}

	public void DrawGratitudeIcon(bool vis, bool enough = true)
	{
		if (container != null && container.gratitude_craft_label != null)
		{
			container.gratitude_craft_label.gameObject.SetActive(vis);
			container.gratitude_craft_label.sprite2D = EasySpritesCollection.GetSprite(enough ? "techpoint_drop_smile" : "techpoint_drop_smile_not_enough");
		}
	}

	public void DrawCapIcon(bool vis)
	{
		if (container.icon_cap_limit != null)
		{
			container.icon_cap_limit.gameObject.SetActive(vis);
		}
	}
}
