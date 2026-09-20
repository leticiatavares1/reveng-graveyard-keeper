using System;
using System.Collections.Generic;
using UnityEngine;

public class ResurrectionGUI : BaseGUI, CraftGUIInterface, CraftInterface
{
	private UniversalObjectInfoGUI _universal_info;

	private Item _body;

	public BodyPanelGUI body_panel;

	public CraftItemGUI craft_item;

	private CraftDefinition _craft;

	private WorldGameObject _wgo;

	private bool is_skulls_frame_active;

	public UIButton resurrect_btn;

	public UILabel cant_resurrect_txt;

	public override void Init()
	{
		_universal_info = GetComponentInChildren<UniversalObjectInfoGUI>(includeInactive: true);
		craft_item.craft_gui_interface = this;
		base.Init();
		body_panel.skull_bar.on_enable_skulls_frame += OnSkullsOver;
		body_panel.skull_bar.on_disable_skulls_frame += OnSkullsOut;
	}

	public void Open(WorldGameObject craft_obj)
	{
		base.Open();
		_body = craft_obj.GetBodyFromInventory();
		_universal_info.Draw(craft_obj.GetUniversalObjectInfo());
		_universal_info.text_descr.text = GJL.L("zombie_crafting_table_d");
		_wgo = craft_obj;
		body_panel.Draw(_body);
		craft_item.ingredients_table_universal.gameObject.SetActive(_body != null);
		if (_body == null)
		{
			resurrect_btn.gameObject.SetActive(value: true);
			cant_resurrect_txt.SetActive(active: false);
		}
		else
		{
			bool flag = _body.durability >= 0.9f;
			resurrect_btn.gameObject.SetActive(flag);
			cant_resurrect_txt.gameObject.SetActive(!flag);
			_craft = GameBalance.me.GetData<CraftDefinition>("zombie_craft");
			craft_item.Draw(_craft);
		}
		bool flag2 = CanCraftZombie();
		resurrect_btn.isEnabled = flag2;
		resurrect_btn.SetState((!flag2) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, immediate: true);
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
			resurrect_btn.GetComponent<GamepadNavigationItem>().SetCallbacks(null, null, OnPressedResurrect);
			body_panel.btn_remove_body.GetComponent<GamepadNavigationItem>().SetCallbacks(null, null, DropBody);
		}
		UpdateAllAnchors();
	}

	public void DropBody()
	{
		for (int i = 0; i < _wgo.data.inventory.Count; i++)
		{
			Item item = _wgo.data.inventory[i];
			if (item.definition.type == ItemDefinition.ItemType.Body)
			{
				_wgo.GiveItemToPlayersHands(item);
				Hide(play_hide_sound: false);
				break;
			}
		}
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void Hide(bool play_hide_sound = true)
	{
		base.Hide(play_hide_sound);
	}

	public void OnPressedResurrect()
	{
		Debug.Log("Resurrect!");
		if (CanCraft(_craft))
		{
			_wgo.components.craft.CraftAsPlayer(_craft);
			GS.SetPlayerEnable(player_enabled: false, affect_cinematic: false);
			_wgo.components.animator.SetTrigger("rise_fx");
			Hide();
		}
	}

	private bool CanCraftZombie()
	{
		if (_body != null && MainGame.me.player.GetMultiInventoryForInteraction().IsEnoughItems(_craft.needs))
		{
			return _body.durability >= 0.9f;
		}
		return false;
	}

	public bool CanCraft(CraftDefinition craft_not_used, List<string> multiquality_ids_not_used = null, int amount_not_used = 1, List<Item> override_needs = null)
	{
		return CanCraftZombie();
	}

	public bool OnCraft(CraftDefinition craft_not_used, Item try_use_particular_item_no_used = null, List<string> multiquality_ids_not_used = null, int amount_not_used = 1, List<Item> override_needs_not_used = null, WorldGameObject other_obj_override = null)
	{
		return CanCraftZombie();
	}

	public new void OnRightClick()
	{
		base.OnRightClick();
	}

	public ButtonTipsStr GetButtonTips()
	{
		return new ButtonTipsStr();
	}

	public GamepadNavigationController GetGamepadController()
	{
		return base.gamepad_controller;
	}

	public List<CraftItemGUI> GetItemsList()
	{
		throw new NotImplementedException();
	}

	public WorldGameObject GetCrafteryWGO()
	{
		return _wgo;
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
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
		}
	}
}
