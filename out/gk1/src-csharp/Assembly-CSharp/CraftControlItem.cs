using UnityEngine;

public class CraftControlItem : MonoBehaviour
{
	public static CraftControlItem current_overed;

	public UI2DSprite object_icon;

	public UI2DSprite zombie_icon;

	public UILabel object_label;

	public UILabel res_label;

	public UILabel zombie_eff;

	public BaseItemCellGUI item_cell;

	public BubbleWidgetProgress progress_bar;

	public GamepadNavigationItem _gamepad_navigation_item;

	public UIWidget selection_frame;

	public PanelAutoScroll auto_scroll;

	private GJCommons.VoidDelegate _on_btn_click;

	public Color c_normal = Color.white;

	public Color c_not_enough = Color.red;

	[HideInInspector]
	public WorldGameObject linked_wgo;

	private bool _was_overed;

	private bool interactable;

	private bool _overed_item;

	private bool _overed_additional;

	private bool has_totem;

	private void Update()
	{
		Redraw();
		if (BaseGUI.for_gamepad)
		{
			return;
		}
		bool flag = _overed_item || _overed_additional;
		if (flag == _was_overed)
		{
			return;
		}
		if (flag)
		{
			if (current_overed != this)
			{
				Sounds.OnGUIHover();
			}
			if (current_overed != null && current_overed != this)
			{
				current_overed.SetOveredState(ovr: false);
			}
			current_overed = this;
			SetMouseOveredGraphics(overed: true);
		}
		else
		{
			if (current_overed == this)
			{
				current_overed = null;
			}
			SetMouseOveredGraphics(overed: false);
		}
		_was_overed = flag;
	}

	public void Draw(WorldGameObject wgo, bool has_totem)
	{
		_overed_item = false;
		_overed_additional = false;
		_was_overed = false;
		this.has_totem = has_totem;
		base.gameObject.SetActive(value: true);
		linked_wgo = wgo;
		if (BaseGUI.for_gamepad && _gamepad_navigation_item != null)
		{
			_gamepad_navigation_item.SetCallbacks(OnOver, OnOut, OnItemAction);
		}
		UniversalObjectInfo universalObjectInfo = linked_wgo.GetUniversalObjectInfo();
		Sprite sprite = EasySpritesCollection.GetSprite(universalObjectInfo.icon);
		object_icon.sprite2D = sprite;
		object_label.text = GJL.L(universalObjectInfo.header);
		res_label.text = string.Empty;
		_overed_additional = (_overed_item = (_was_overed = false));
		progress_bar.Draw(new BubbleWidgetProgressData(() => linked_wgo.components.craft.wgo.progress));
		Update();
	}

	public void Redraw()
	{
		if (linked_wgo == null)
		{
			zombie_icon.gameObject.SetActive(value: false);
			item_cell.gameObject.SetActive(value: false);
			progress_bar.gameObject.SetActive(value: false);
			interactable = false;
			object_label.color = c_not_enough;
			return;
		}
		progress_bar.UpdateWidget();
		bool flag = linked_wgo.has_linked_worker && !linked_wgo.linked_worker.IsInvisibleWorker();
		zombie_icon.gameObject.SetActive(flag);
		zombie_eff.gameObject.SetActive(flag);
		if (linked_wgo.has_linked_worker)
		{
			zombie_eff.text = linked_wgo.linked_worker.worker.GetWorkerEfficiencyTextOnlyPercent();
		}
		bool is_crafting = linked_wgo.components.craft.is_crafting;
		bool flag2 = linked_wgo.components.craft.current_craft != null && linked_wgo.components.craft.current_craft.output != null && linked_wgo.components.craft.current_craft.output.Count > 0;
		bool flag3 = linked_wgo.components.craft.current_craft != null;
		item_cell.gameObject.SetActive(is_crafting && flag2);
		progress_bar.gameObject.SetActive(is_crafting);
		if (is_crafting && flag3 && linked_wgo.components.craft.current_craft.output != null && linked_wgo.components.craft.current_craft.output.Count > 0)
		{
			if (flag2)
			{
				Item i = linked_wgo.components.craft.current_craft.output[0];
				if (linked_wgo.components.craft.current_craft.IsBodyPartInsertionCraft() && linked_wgo.components.craft.current_item != null)
				{
					i = linked_wgo.components.craft.current_item;
				}
				item_cell.DrawItem(i);
			}
			if (flag)
			{
				item_cell.DrawCapIcon(linked_wgo.components.craft.worker_is_paused);
			}
			item_cell.DrawGratitudeIcon(linked_wgo.is_current_craft_gratitude);
		}
		if (!is_crafting && linked_wgo.components.craft.HasGratitudeCraftInQueue())
		{
			item_cell.DrawGratitudeIcon(vis: true, enough: false);
		}
		interactable = has_totem && (linked_wgo.obj_def.can_insert_zombie || linked_wgo.obj_def.tool_actions.no_actions || !linked_wgo.components.craft.is_crafting);
		if (linked_wgo.obj_def.interaction_type != ObjectDefinition.InteractionType.Craft && linked_wgo.obj_def.GetValidInteraction(linked_wgo) == null)
		{
			interactable = false;
		}
		object_label.color = (interactable ? c_normal : c_not_enough);
	}

	public void OnMouseOvered()
	{
		if (!BaseGUI.for_gamepad)
		{
			_overed_item = true;
			selection_frame.SetActive(active: true);
		}
	}

	public void OnMouseOuted()
	{
		if (!BaseGUI.for_gamepad)
		{
			_overed_item = false;
			selection_frame.SetActive(active: false);
		}
	}

	public void OnMouseOveredAdditionalButtons()
	{
		if (!BaseGUI.for_gamepad)
		{
			_overed_additional = true;
		}
	}

	public void OnMouseOutedAdditionalButtons()
	{
		if (!BaseGUI.for_gamepad)
		{
			_overed_additional = false;
		}
	}

	private void SetOveredState(bool ovr)
	{
		_was_overed = ovr;
		SetMouseOveredGraphics(ovr);
	}

	protected void SetMouseOveredGraphics(bool overed)
	{
		selection_frame.SetActive(overed);
		Sounds.OnGUIHover();
	}

	public void OnOver()
	{
		if (BaseGUI.for_gamepad)
		{
			current_overed = this;
			GUIElements.me.global_craft_control_gui.GetButtonTips().Print(GameKeyTip.Select(), GameKeyTip.Close());
			selection_frame.SetActive(active: true);
			Sounds.OnGUIHover();
		}
	}

	public void OnOut()
	{
		selection_frame.SetActive(active: false);
		if (BaseGUI.for_gamepad && !(this != current_overed))
		{
			GUIElements.me.global_craft_control_gui.GetButtonTips().Clear();
		}
	}

	public void OnItemAction()
	{
		if (BaseGUI.IsLastClickRightButton())
		{
			GUIElements.me.global_craft_control_gui.OnRightClick();
		}
		else
		{
			if (!interactable)
			{
				return;
			}
			if (_on_btn_click != null)
			{
				if (BaseGUI.for_gamepad)
				{
					LazyInput.ClearKeyDown(GameKey.Select);
				}
				_on_btn_click();
			}
			else
			{
				GlobalCraftControlGUI.current_instance.last_selected = this;
				GlobalCraftControlGUI.current_instance.Hide(play_hide_sound: false);
				GUIElements.me.OpenCraftGUI(linked_wgo);
				current_overed = null;
			}
		}
	}

	private void OnDisable()
	{
		OnOut();
	}
}
