public class BodyCraftGUI : ResourceBasedCraftGUI
{
	private Item _body;

	public BodyPanelGUI body_panel;

	public UniversalObjectInfoGUI obj_info;

	private bool _can_start_craft;

	public override void Init()
	{
		base.Init();
		body_panel.skull_bar.on_enable_skulls_frame += OnSkullsOver;
		body_panel.skull_bar.on_disable_skulls_frame += OnSkullsOut;
	}

	public void Open(WorldGameObject craftery_wgo)
	{
		Open(craftery_wgo, CraftDefinition.CraftType.ResourcesBasedCraft);
		_body = craftery_wgo.GetBodyFromInventory();
		obj_info.Draw(craftery_wgo.GetUniversalObjectInfo());
		if (_body != null)
		{
			_body.inventory_size = 99;
		}
		body_panel.Draw(_body);
		if (body_panel?.btn_remove_body != null)
		{
			body_panel.btn_remove_body.GetComponent<GamepadSelectableButton>().SetCallbacks(DropBody, delegate
			{
				if (BaseGUI.for_gamepad)
				{
					base.button_tips.Print(GameKeyTip.Select(_body != null), GameKeyTip.Close());
				}
			});
		}
		Redraw();
	}

	public void DropBody()
	{
		if (_body != null)
		{
			craftery_wgo.GiveItemToPlayersHands(_body);
			Hide(play_hide_sound: false);
		}
	}

	public new void Redraw()
	{
		label_resourse_hint.text = GJL.L("pick_embalming");
		_can_start_craft = false;
		if (_selected_item != null)
		{
			label_resourse_hint.text = GJL.L("embalming_effect") + "\n" + _selected_item.GetItemBodyModificators();
			if (_body != null)
			{
				CheckEmbalmAvailability();
			}
		}
		base.Redraw();
	}

	protected override bool CanCraft()
	{
		if (_can_start_craft)
		{
			return base.CanCraft();
		}
		return false;
	}

	private void CheckEmbalmAvailability()
	{
		_can_start_craft = false;
		if (_body.HasItemInInventory(_selected_item.id))
		{
			UILabel uILabel = label_resourse_hint;
			uILabel.text = uILabel.text + "\n\n" + GJL.L("embalm_already_applied");
			return;
		}
		_body.GetBodySkulls(out var negative, out var positive, out var _);
		if (_selected_item.GetRedSkullsValue() < 0 && -_selected_item.GetRedSkullsValue() > negative)
		{
			UILabel uILabel2 = label_resourse_hint;
			uILabel2.text = uILabel2.text + "\n\n" + GJL.L("embalm_not_enough", "(rskull)");
		}
		else if (_selected_item.GetWhiteSkullsValue() < 0 && -_selected_item.GetWhiteSkullsValue() > positive)
		{
			UILabel uILabel3 = label_resourse_hint;
			uILabel3.text = uILabel3.text + "\n\n" + GJL.L("embalm_not_enough", "(skull)");
		}
		else
		{
			_can_start_craft = true;
		}
	}

	protected override void OnResourcePickerClosed(Item item)
	{
		base.OnResourcePickerClosed(item);
		Redraw();
	}

	public override void Hide(bool play_hide_sound = true)
	{
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
		base.Hide(play_hide_sound);
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
