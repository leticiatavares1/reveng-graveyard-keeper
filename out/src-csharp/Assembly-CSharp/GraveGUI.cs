using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GraveGUI : BaseGUI, CraftInterface
{
	public UI2DSprite icon_cross;

	public UI2DSprite icon_fence;

	public UI2DSprite icon_body;

	public UILabel txt_cross;

	public UILabel txt_cross_q;

	public UILabel txt_fence;

	public UILabel txt_fence_q;

	public UILabel txt_grave;

	public UILabel txt_grave_total;

	public UILabel txt_grave_total_q;

	public UILabel txt_grave_condition_v;

	public UILabel txt_fix_grave_descr;

	public UIButton btn_cross_extract;

	public UIButton btn_fence_extract;

	public UIButton btn_body_extract;

	public UIButton btn_grave_fix;

	[NonSerialized]
	public BaseItemCellGUI[] fix_ingredients;

	public UITable ingredients_table;

	private Item _body;

	private Item _fence;

	private Item _cross;

	private WorldGameObject _grave_wgo;

	public BodyPanelGUI body_panel;

	private List<Item> _fix_price = new List<Item>();

	public BaseItemCellGUI prefab_grave_item;

	public UITableOrGrid grave_inventory_table;

	public UIWidget red_vertical_bar;

	public UITableOrGrid tb_skulls;

	public UITableOrGrid tb_slots;

	public GameObject prefab_w;

	public GameObject prefab_w_red;

	public GameObject prefab_slot_red;

	public GameObject prefab_slot_grey;

	public GameObject go_result_negative;

	public Tooltip exhume_tooltip;

	public GamepadNavigationItem middle_bar_item;

	private bool button_enabled_for_tip;

	public UIScrollView _scroll_view;

	public UI2DSprite frame;

	public int scroll_skulls_limit = 10;

	public override void Init()
	{
		base.Init();
		if (middle_bar_item != null)
		{
			middle_bar_item.SetCallbacks(EnableSelectionFrame, DisableSelectionFrame, null);
		}
		fix_ingredients = ingredients_table.GetComponentsInChildren<BaseItemCellGUI>(includeInactive: true);
		GamepadSelectableButton[] componentsInChildren = GetComponentsInChildren<GamepadSelectableButton>(includeInactive: true);
		foreach (GamepadSelectableButton gamepadSelectableButton in componentsInChildren)
		{
			GamepadSelectableButton button = gamepadSelectableButton;
			button.Init();
			button.SetCallbacks(delegate
			{
				OnGamepadSelect(button);
			}, delegate
			{
				OnGamepadOver(button);
			});
		}
		prefab_slot_red.SetActive(value: false);
		prefab_slot_grey.SetActive(value: false);
		prefab_w.SetActive(value: false);
		prefab_w_red.SetActive(value: false);
		body_panel.skull_bar.on_enable_skulls_frame += OnSkullsOver;
		body_panel.skull_bar.on_disable_skulls_frame += OnSkullsOut;
	}

	public void Open(WorldGameObject grave_wgo)
	{
		if (!grave_wgo.components.craft.is_crafting)
		{
			base.Open();
			_grave_wgo = grave_wgo;
			_body = grave_wgo.GetBodyFromInventory();
			_cross = grave_wgo.data.GetItemOfType(ItemDefinition.ItemType.GraveStone);
			_fence = grave_wgo.data.GetItemOfType(ItemDefinition.ItemType.GraveFence);
			Redraw();
			if (BaseGUI.for_gamepad)
			{
				base.gamepad_controller.ReinitItems(focus_on_first_active: true);
			}
		}
	}

	private void Redraw()
	{
		UIButton uIButton = btn_cross_extract;
		UIButton uIButton2 = btn_fence_extract;
		bool flag2 = (btn_body_extract.isEnabled = true);
		bool isEnabled = (uIButton2.isEnabled = flag2);
		uIButton.isEnabled = isEnabled;
		body_panel.Draw(_body);
		if (_body == null)
		{
			btn_body_extract.isEnabled = false;
		}
		txt_grave.text = _grave_wgo.GetObjectConditionString();
		txt_grave_total.text = GJL.L("grave_total_q");
		float quality = _grave_wgo.quality;
		txt_grave_total_q.text = quality.ToString("0");
		txt_grave_condition_v.text = _grave_wgo.GetObjectConditionString("\n");
		go_result_negative.gameObject.SetActive(quality < 0f);
		if (exhume_tooltip != null)
		{
			exhume_tooltip.available = true;
			exhume_tooltip.SetText(GJL.L("cant_exhume"));
		}
		if (_grave_wgo.components.craft.is_crafting)
		{
			UIButton uIButton3 = btn_body_extract;
			UIButton uIButton4 = btn_fence_extract;
			flag2 = (btn_body_extract.isEnabled = false);
			isEnabled = (uIButton4.isEnabled = flag2);
			uIButton3.isEnabled = isEnabled;
		}
		if (_fence != null || _cross != null)
		{
			btn_body_extract.isEnabled = false;
		}
		if (exhume_tooltip != null && (btn_body_extract.isEnabled || _body == null))
		{
			exhume_tooltip.available = false;
		}
		MultiInventory.PlayerMultiInventory player_mi = MultiInventory.PlayerMultiInventory.DontChange;
		if (GlobalCraftControlGUI.is_global_control_active && !WorldZone.GetZoneOfObject(_grave_wgo).IsPlayerInZone())
		{
			player_mi = MultiInventory.PlayerMultiInventory.ExcludePlayer;
		}
		MultiInventory multiInventory = MainGame.me.player.GetMultiInventory(null, "", player_mi, include_toolbelt: false, sortWGOS: true);
		bool flag6 = false;
		for (int i = 0; i < fix_ingredients.Length; i++)
		{
			if (i >= _fix_price.Count)
			{
				fix_ingredients[i].DrawEmpty();
				continue;
			}
			fix_ingredients[i].DrawIngredient(_fix_price[i], multiInventory, deactivate_colliders: false, init_tooltip: true);
			flag6 = true;
		}
		btn_cross_extract.isEnabled &= !flag6;
		btn_fence_extract.isEnabled &= !flag6;
		btn_grave_fix.isEnabled = flag6;
		RedrawGraveInventory();
	}

	public static void FillBodyDescription(Item body, UI2DSprite icon, UILabel hdr, UILabel descr, UILabel value = null)
	{
		if (body == null)
		{
			icon.sprite2D = null;
			descr.text = "";
			hdr.text = GJL.L("none");
			return;
		}
		icon.sprite2D = EasySpritesCollection.GetSprite("i_" + body.definition.id);
		hdr.text = GJL.L(body.definition.id);
		if (value == null)
		{
			hdr.text = hdr.text + " " + body.GetItemQualityString();
		}
		else
		{
			value.text = body.GetItemQualityString();
		}
		descr.text = "(" + body.GetItemConditionString() + ")";
	}

	private void OnGravePartPressed(UIButton button, Item item, ItemDefinition.ItemType type)
	{
		Debug.Log("OnGravePartPressed: " + type);
		if (button != null && !button.isEnabled)
		{
			return;
		}
		if (item == null)
		{
			if (_grave_wgo.components.craft.is_crafting)
			{
				Debug.LogError("Cancel craft not implemented");
			}
			else
			{
				WorldGameObject obj = MainGame.me.player;
				if (GlobalCraftControlGUI.is_global_control_active && _grave_wgo != null)
				{
					obj = _grave_wgo;
				}
				GUIElements.me.resource_picker.Open(obj, (Item itm, InventoryWidget widget) => GravePartsFilter(itm, type), OnResourcePickerClosed);
				if (BaseGUI.for_gamepad)
				{
					base.button_tips.Deactivate();
				}
			}
			Redraw();
		}
		else
		{
			GUIElements.me.craft.OpenAsGrave(_grave_wgo, item, type);
		}
	}

	private void OnResourcePickerClosed(Item item)
	{
		if (item != null)
		{
			CraftDefinition craftDefinition = FindGravePartPutCraft(item);
			if (craftDefinition != null)
			{
				_grave_wgo.components.craft.CraftAsPlayer(craftDefinition, item);
			}
			Hide();
		}
	}

	private CraftDefinition FindGravePartPutCraft(Item item)
	{
		foreach (CraftDefinition craft_datum in GameBalance.me.craft_data)
		{
			if (craft_datum.craft_in.Contains(_grave_wgo.obj_id) && craft_datum.craft_type == CraftDefinition.CraftType.ResourcesBasedCraft && craft_datum.transfer_needs_to_wgo && craft_datum.needs.Count > 0 && craft_datum.needs[0].id == item.id)
			{
				return craft_datum;
			}
		}
		Debug.LogError("Couldn't find a put craft for item: " + item.id);
		return null;
	}

	private CraftDefinition FindBodyExhumeCraft()
	{
		return GameBalance.me.GetData<CraftDefinition>("gr_exhume");
	}

	private static InventoryWidget.ItemFilterResult GravePartsFilter(Item item, ItemDefinition.ItemType type)
	{
		if (item == null || item.definition == null)
		{
			return InventoryWidget.ItemFilterResult.Hide;
		}
		if (item.definition.type != type)
		{
			return InventoryWidget.ItemFilterResult.Inactive;
		}
		return InventoryWidget.ItemFilterResult.Active;
	}

	public void OnFenceExtractPressed()
	{
		_ = btn_fence_extract.isEnabled;
	}

	public void OnBodyExtractPressed()
	{
		if (!btn_body_extract.isEnabled)
		{
			return;
		}
		CraftDefinition exhume_craft = FindBodyExhumeCraft();
		if (exhume_craft == null)
		{
			return;
		}
		if (MainGame.me.player.GetMultiInventory().IsEnoughItems(exhume_craft.needs))
		{
			GUIElements.me.dialog.OpenDialog("exhume_confirmation", GJL.L("OK"), delegate
			{
				_grave_wgo.components.craft.CraftAsPlayer(exhume_craft);
				Hide();
			}, GJL.L("Cancel"), null, null, GameKey.Select, GameKey.Back, "exhume_confirmation_bot", exhume_craft.needs[0]);
		}
		else
		{
			GUIElements.me.dialog.OpenDialog("exhume_confirmation", GJL.L("OK"), null, null, null, null, GameKey.Select, GameKey.Back, "", exhume_craft.needs[0]);
		}
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Deactivate();
		}
	}

	public override void OnAboveWindowClosed()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Activate();
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.RestoreFocus();
		}
	}

	private void OnGamepadOver(GamepadSelectableButton button)
	{
		if (BaseGUI.for_gamepad)
		{
			UIButton component = button.GetComponent<UIButton>();
			button_enabled_for_tip = component.isEnabled;
			base.button_tips.Print(GameKeyTip.Select(component.isEnabled), GameKeyTip.Close());
			Sounds.OnGUIHover();
		}
	}

	private void OnGamepadSelect(GamepadSelectableButton button)
	{
		if (!BaseGUI.for_gamepad || !button.GetComponent<UIButton>().isEnabled)
		{
			return;
		}
		foreach (EventDelegate item in button.GetComponent<UIEventTrigger>().onPress)
		{
			item.TryExecute();
		}
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void Hide(bool play_hide_sound = true)
	{
		MainGame.me.player.components.interaction.UpdateNearestHint();
		GJTimer.AddTimer(0.5f, MainGame.me.player.components.interaction.UpdateNearestHint);
		DisableSelectionFrame();
		base.Hide(play_hide_sound);
	}

	private void RedrawGraveInventory()
	{
		grave_inventory_table.DestroyChildren(new BaseItemCellGUI[1] { prefab_grave_item });
		prefab_grave_item.gameObject.SetActive(value: false);
		int num = 1;
		BaseItemCellGUI baseItemCellGUI = prefab_grave_item.Copy();
		baseItemCellGUI.transform.SetSiblingIndex(num++);
		BaseItemCellGUI baseItemCellGUI2 = prefab_grave_item.Copy();
		baseItemCellGUI2.transform.SetSiblingIndex(num++);
		float num2 = 0f;
		num2 += DrawGravePart(baseItemCellGUI, _cross, ItemDefinition.ItemType.GraveStone);
		num2 += DrawGravePart(baseItemCellGUI2, _fence, ItemDefinition.ItemType.GraveFence);
		grave_inventory_table.Reposition();
		int negative = 0;
		int positive_avaialble = 0;
		body_panel.skull_bar.filled = 0;
		body_panel.skull_bar.RedrawFilledSkulls();
		if (_body != null)
		{
			_body.GetBodySkulls(out negative, out var _, out positive_avaialble);
		}
		red_vertical_bar.gameObject.SetActive(negative > 0);
		red_vertical_bar.height = 7 + 13 * negative;
		tb_slots.DestroyChildren(new Transform[2] { prefab_slot_grey.transform, prefab_slot_red.transform });
		tb_skulls.DestroyChildren(new Transform[2] { prefab_w.transform, prefab_w_red.transform });
		_ = _grave_wgo.quality;
		for (int i = 0; i < positive_avaialble; i++)
		{
			prefab_slot_grey.Copy(tb_slots.transform);
		}
		for (int j = 0; j < negative; j++)
		{
			prefab_slot_red.Copy(tb_slots.transform);
		}
		List<GameObject> list = new List<GameObject>();
		for (int k = 0; k < Mathf.FloorToInt(num2); k++)
		{
			GameObject item = ((k < negative) ? prefab_w_red : prefab_w).Copy(tb_skulls.transform);
			if (k >= negative)
			{
				list.Add(item);
			}
		}
		foreach (GameObject item2 in list)
		{
			item2.transform.SetSiblingIndex(0);
		}
		tb_slots.Reposition();
		tb_skulls.Reposition();
		int num3 = Math.Max(Mathf.FloorToInt(num2), positive_avaialble + negative);
		bool flag = num3 > scroll_skulls_limit && base.gameObject.activeSelf;
		_scroll_view.transform.localPosition = Vector3.zero;
		_scroll_view.panel.UpdateAnchors();
		_scroll_view.enabled = flag;
		middle_bar_item.active = flag && middle_bar_item.gameObject.activeInHierarchy;
		_scroll_view.StopScrolling();
		_scroll_view.transform.DOKill();
		_scroll_view.RestrictWithinBounds(instant: false);
		_scroll_view.transform.localPosition = Vector3.zero;
		_scroll_view.ResetPosition();
		if (flag)
		{
			Vector3 localPosition = _scroll_view.transform.localPosition;
			localPosition.y = (num3 - scroll_skulls_limit) * -13 + 3;
			_scroll_view.transform.localPosition = localPosition;
		}
		else
		{
			_scroll_view.transform.localPosition = Vector3.zero;
		}
		_scroll_view.panel.UpdateAnchors();
		Update();
	}

	public override void Update()
	{
		base.Update();
		if (red_vertical_bar.gameObject.activeSelf)
		{
			Transform obj = red_vertical_bar.transform;
			Vector3 localPosition = obj.localPosition;
			localPosition.y = (float)Math.Round(-167f + _scroll_view.transform.localPosition.y, MidpointRounding.AwayFromZero);
			obj.localPosition = localPosition;
		}
		if (LazyInput.gamepad_active && frame.gameObject.activeSelf)
		{
			float num = LazyInput.GetDirection2().y * 0.1f;
			if (!num.EqualsTo(0f))
			{
				_scroll_view.Scroll(num);
			}
		}
	}

	private float DrawGravePart(BaseItemCellGUI o, Item item, ItemDefinition.ItemType type)
	{
		o.SetCallbacks((GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)delegate
		{
			OnGravePartPressed(null, item, type);
		});
		o.GetComponent<GamepadNavigationItem>().SetCallbacks(delegate
		{
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
			Sounds.OnGUIHover();
		}, null, delegate
		{
			OnGravePartPressed(null, item, type);
		});
		if (item == null)
		{
			o.container = o.x1;
			o.progress.gameObject.SetActive(value: false);
			switch (type)
			{
			case ItemDefinition.ItemType.GraveStone:
				o.x1.icon.sprite2D = EasySpritesCollection.GetSprite("i_additem");
				o.item_name.text = GJL.L("no_grave_cross");
				break;
			case ItemDefinition.ItemType.GraveFence:
				o.x1.icon.sprite2D = EasySpritesCollection.GetSprite("i_additem");
				o.item_name.text = GJL.L("no_grave_fence");
				break;
			default:
				o.x1.icon.sprite2D = null;
				break;
			}
			if (o.x1.icon.sprite2D != null)
			{
				o.interaction_enabled = true;
				o.SetInactiveState(set_inactive: false);
			}
			o.item_description.text = "";
			return 0f;
		}
		o.DrawItem(item);
		float itemQuality = item.GetItemQuality();
		o.item_description.text = "(wr)" + itemQuality;
		return itemQuality;
	}

	public bool CanCraft(CraftDefinition craft, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null)
	{
		if (craft is ObjectCraftDefinition && !(craft as ObjectCraftDefinition).enabled)
		{
			return false;
		}
		return MainGame.me.player.GetMultiInventory().IsEnoughItems(craft.needs);
	}

	public bool OnCraft(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null, WorldGameObject other_obj_override = null)
	{
		string id = craft.id;
		Debug.Log("Grave craft: " + craft.id + " ==> " + id);
		if (!_grave_wgo.components.craft.CraftAsPlayer(GameBalance.me.GetData<CraftDefinition>(id), null, null, craft.needs, ignore_crafts_list: true))
		{
			return false;
		}
		GUIElements.me.craft.Hide(play_hide_sound: false);
		Hide();
		return true;
	}

	public new void OnRightClick()
	{
		base.OnRightClick();
	}

	public void EnableSelectionFrame()
	{
		if (!(frame == null) && (_scroll_view.enabled || !base.gameObject.activeInHierarchy))
		{
			frame.gameObject.SetActive(value: true);
			OnSkullsOver();
			Sounds.OnGUIHover();
		}
	}

	public void DisableSelectionFrame()
	{
		if (!(frame == null) && _scroll_view.enabled)
		{
			OnSkullsOut();
			frame.gameObject.SetActive(value: false);
		}
	}

	public void OnSkullsOver()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Close(), GameKeyTip.RightStick(active: true, gamepad_only: true, translate: true, "move_tip"));
		}
	}

	public void OnSkullsOut()
	{
		_ = BaseGUI.for_gamepad;
	}
}
