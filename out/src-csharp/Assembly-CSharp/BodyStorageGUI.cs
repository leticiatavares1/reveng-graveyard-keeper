public class BodyStorageGUI : BaseGUI
{
	public BodyPanelGUI[] body_panels;

	private UniversalObjectInfoGUI _universal_info;

	private Item[] _bodies = new Item[2];

	private WorldGameObject _wgo;

	public override void Init()
	{
		_universal_info = GetComponentInChildren<UniversalObjectInfoGUI>(includeInactive: true);
		base.Init();
		body_panels[0].skull_bar.on_enable_skulls_frame += OnSkullsOver;
		body_panels[1].skull_bar.on_enable_skulls_frame += OnSkullsOver;
		body_panels[0].skull_bar.on_disable_skulls_frame += OnSkullsOut;
		body_panels[1].skull_bar.on_disable_skulls_frame += OnSkullsOut;
	}

	public void Open(WorldGameObject craft_obj)
	{
		base.Open();
		_wgo = craft_obj;
		int can_insert_items_limit = _wgo.obj_def.can_insert_items_limit;
		body_panels[1].gameObject.SetActive(can_insert_items_limit > 1);
		GetComponentInChildren<UITableOrGrid>(includeInactive: true).Reposition();
		_bodies[0] = craft_obj.GetBodyFromInventory();
		_bodies[1] = craft_obj.GetBodyFromInventory(first: false);
		if (_bodies[1] == _bodies[0])
		{
			_bodies[1] = null;
		}
		for (int j = 0; j < can_insert_items_limit; j++)
		{
			bool has_body = body_panels[j] != null;
			int i = j;
			body_panels[j].Draw(_bodies[j]);
			body_panels[j].button_item.SetCallbacks(delegate
			{
				base.button_tips.Print(GameKeyTip.Select(has_body), GameKeyTip.Close());
				Sounds.OnGUIHover();
			}, delegate
			{
			}, delegate
			{
				DropBody(i);
			});
		}
		_universal_info.Draw(craft_obj.GetUniversalObjectInfo());
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Close());
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			base.gamepad_controller.FocusOnFirstActive();
		}
	}

	public void DropBody_1()
	{
		DropBody(0);
	}

	public void DropBody_2()
	{
		DropBody(1);
	}

	private void DropBody(int n)
	{
		if (_bodies[n] != null)
		{
			_wgo.GiveItemToPlayersHands(_bodies[n]);
			Hide(play_hide_sound: false);
		}
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
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
