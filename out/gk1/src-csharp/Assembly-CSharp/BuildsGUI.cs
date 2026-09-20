using System.Collections.Generic;
using DG.Tweening;

public class BuildsGUI : BaseGameGUI
{
	private UIScrollView _scroll_view;

	private UIPanel _scroll_view_panel;

	private BuildItemGUI _item_prefab;

	private SimpleUITable _items_table;

	private List<ObjectCraftDefinition> _crafts;

	private List<BuildItemGUI> _build_items;

	private WorldGameObject _build_desk;

	public UniversalObjectInfoGUI object_info;

	public override void Init()
	{
		_items_table = GetComponentInChildren<SimpleUITable>(includeInactive: true);
		_item_prefab = GetComponentInChildren<BuildItemGUI>(includeInactive: true);
		_item_prefab.InitPrefab();
		_crafts = new List<ObjectCraftDefinition>();
		_build_items = new List<BuildItemGUI>();
		_scroll_view = GetComponentInChildren<UIScrollView>(includeInactive: true);
		_scroll_view_panel = _scroll_view.GetComponent<UIPanel>();
		base.Init();
	}

	public override void OpenFromGameGUI()
	{
		Open(MainGame.me.save.obj_crafts, _build_desk);
		TooltipBubbleGUI.ChangeAvaibility(available: true);
	}

	public override void CloseFromGameGUI()
	{
		base.CloseFromGameGUI();
		Clear();
		TooltipBubbleGUI.ChangeAvaibility(available: false);
	}

	public void Open(CraftsInventory crafts_inventory, WorldGameObject build_desk)
	{
		base.Open();
		_build_desk = build_desk;
		object_info.Draw(build_desk.GetUniversalObjectInfo());
		GUIElements.me.build_mode_gui.Hide();
		Clear();
		if (crafts_inventory != null)
		{
			foreach (ObjectCraftDefinition objectCrafts in crafts_inventory.GetObjectCraftsList())
			{
				if (MainGame.me.save.IsCraftVisible(objectCrafts))
				{
					_crafts.Add(objectCrafts);
				}
			}
		}
		MultiInventory multi_inventory = MainGame.me.build_mode_logics.multi_inventory;
		foreach (ObjectCraftDefinition craft in _crafts)
		{
			BuildItemGUI buildItemGUI = _item_prefab.Copy();
			bool can_craft = multi_inventory.IsEnoughItems(craft.needs);
			buildItemGUI.Init(craft, can_craft, BaseGUI.for_gamepad);
			_build_items.Add(buildItemGUI);
		}
		_items_table.Reposition();
		_scroll_view.RestrictWithinBounds(instant: false);
		if (!BaseGUI.for_gamepad)
		{
			return;
		}
		if (base.isActiveAndEnabled)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
		}
		CraftDefinition craftDefinition = null;
		foreach (BuildItemGUI build_item in _build_items)
		{
			if (build_item.definition == craftDefinition)
			{
				base.gamepad_controller.SetFocusedItem(build_item.gamepad_item, animate_auto_scroll: false);
				return;
			}
		}
		_scroll_view.RestrictWithinBounds(instant: false);
		base.gamepad_controller.FocusOnFirstActive();
		GJTimer.AddTimer(0.001f, delegate
		{
		});
		if (_crafts.Count == 0)
		{
			base.button_tips.PrintClose();
		}
	}

	private new void LateUpdate()
	{
		if (_scroll_view.RestrictWithinBounds(instant: false) && DOTween.IsTweening(_scroll_view.transform))
		{
			_scroll_view.transform.DOKill();
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		Clear();
		_scroll_view.transform.DOKill();
		_scroll_view.RestrictWithinBounds(instant: false);
		_scroll_view.ResetPosition();
		base.Hide(play_hide_sound);
	}

	private void Clear()
	{
		_crafts.Clear();
		foreach (BuildItemGUI build_item in _build_items)
		{
			build_item.DestroyGO();
		}
		_build_items.Clear();
	}

	public void UpdateTip(bool can_craft)
	{
		base.button_tips.Print(GameKeyTip.Select("build", can_craft), GameKeyTip.Close());
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		Hide();
		MainGame.me.build_mode_logics.SetCurrentBuildZone(string.Empty);
	}
}
