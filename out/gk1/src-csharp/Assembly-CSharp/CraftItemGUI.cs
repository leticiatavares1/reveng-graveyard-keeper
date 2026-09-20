using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class CraftItemGUI : MonoBehaviour
{
	public static CraftItemGUI current_overed;

	public BaseItemCellGUI item_gui;

	[SerializeField]
	[HideInInspector]
	private CraftGUI _craft_gui;

	[NonSerialized]
	public CraftGUIInterface craft_gui_interface;

	private List<CraftDefinition> _possible_crafts = new List<CraftDefinition>();

	private int _current_craft_index;

	private BaseItemCellGUI[] _ingredients;

	private CraftIngredientButtonsPair[] _ingredient_buttons;

	private CraftItemPerkGUI[] _perk_icons;

	public UITable ingredients_table;

	public UITableOrGrid ingredients_table_universal;

	public UIWidget selection_frame;

	public UILabel label_name;

	public UILabel label_descr;

	public Color c_normal = Color.white;

	public Color c_not_enough = Color.red;

	public UIButton button;

	public Tooltip tooltip;

	private Item _body_item;

	private GJCommons.VoidDelegate _on_btn_click;

	private GamepadNavigationItem _gamepad_navigation_item;

	public int no_multi_quality_height = 45;

	public int multi_quality_height = 55;

	public int multi_quality_height_full = 100;

	public GameObject multi_quality_go;

	public GameObject full_detailed_go;

	public bool full_detailed_view;

	public UIWidget hgt_widget;

	public int widget_hgt_normal;

	public int widget_hgt_detailed;

	public UILabel no_influenced_perks_txt;

	public UILabel txt_spend;

	private List<string> _multiquality_ids = new List<string>();

	public UIButton multiquality_craft_btn;

	public UILabel value_items;

	public UILabel value_perks;

	public UILabel value_items_and_perks_sum;

	public UILabel value_difficulty;

	public UILabel value_result;

	public UILabel[] quality_probabilities;

	public UI2DSprite quality_bar;

	public Transform quality_bar_edge_container;

	private bool _just_outed;

	public Color color_probability_0;

	public Color color_probability_100;

	public Color color_probability_other;

	public PanelAutoScroll auto_scroll;

	public UIWidget inner_selection_frame;

	private UIWidget _current_inner_selectable_item;

	private BaseItemCellGUI _current_inner_ingredient;

	private bool _inited;

	public CraftDefinition craft_definition;

	public GameObject amount_buttons;

	public UIButton btn_amount_plus;

	public UIButton btn_amount_minus;

	private bool _overed_item;

	private bool _overed_additional;

	private bool _was_overed;

	public bool auto_height = true;

	public static CraftItemGUI last_item_pressed;

	[NonSerialized]
	private int _amount = 1;

	public CraftDefinition current_craft
	{
		get
		{
			if (_current_craft_index >= _possible_crafts.Count)
			{
				return null;
			}
			return _possible_crafts[_current_craft_index];
		}
	}

	public GamepadNavigationItem gamepad_navigation_item
	{
		get
		{
			if (_gamepad_navigation_item == null)
			{
				_gamepad_navigation_item = GetComponentInChildren<GamepadNavigationItem>(includeInactive: true);
			}
			return _gamepad_navigation_item;
		}
	}

	private GameObject GetIngredientsTableGameObject()
	{
		if (!(ingredients_table != null))
		{
			return ingredients_table_universal?.gameObject;
		}
		return ingredients_table.gameObject;
	}

	public void Init()
	{
		if (_inited)
		{
			return;
		}
		_inited = true;
		_craft_gui = GUIElements.me.craft;
		craft_gui_interface = GUIElements.me.craft;
		selection_frame.SetActive(active: false);
		base.gameObject.SetActive(value: false);
		if (button != null)
		{
			button.gameObject.SetActive(value: false);
		}
		if (label_descr != null)
		{
			label_descr.text = "";
		}
		UIEventTrigger[] componentsInChildren = GetComponentsInChildren<UIEventTrigger>(includeInactive: true);
		foreach (UIEventTrigger uIEventTrigger in componentsInChildren)
		{
			uIEventTrigger.onHoverOver.Add(new EventDelegate(OnMouseOvered));
			uIEventTrigger.onHoverOut.Add(new EventDelegate(OnMouseOuted));
			if (uIEventTrigger.GetComponent<UIButton>() != null)
			{
				uIEventTrigger.onHoverOver.Add(new EventDelegate(_craft_gui.SoundOnMouseOverCloseButton));
			}
		}
		UIScrollView componentInParent = GetComponentInParent<UIScrollView>();
		Collider2D[] componentsInChildren2 = GetComponentsInChildren<Collider2D>();
		foreach (Collider2D collider2D in componentsInChildren2)
		{
			if (!(collider2D.GetComponent<UIDragScrollView>() != null))
			{
				collider2D.gameObject.AddComponent<UIDragScrollView>().scrollView = componentInParent;
			}
		}
	}

	public void DrawBody(Item body, GJCommons.VoidDelegate on_click = null)
	{
		_body_item = body;
		label_name.text = "";
		_on_btn_click = on_click;
		item_gui.x1.container.SetActive(value: false);
		item_gui.x2.container.SetActive(value: true);
		label_descr.gameObject.SetActive(value: true);
		GraveGUI.FillBodyDescription(body, item_gui.x2.icon, label_name, label_descr);
		GetIngredientsTableGameObject().SetActive(value: false);
		if (button != null)
		{
			button.gameObject.SetActive(value: true);
		}
		if (BaseGUI.for_gamepad && gamepad_navigation_item != null)
		{
			gamepad_navigation_item.SetCallbacks(OnOver, OnOut, OnItemAction);
		}
	}

	public void Draw(CraftDefinition craft_definition)
	{
		this.craft_definition = craft_definition;
		_possible_crafts.Clear();
		_possible_crafts.Add(craft_definition);
		_multiquality_ids.Clear();
		foreach (Item need in craft_definition.needs)
		{
			_multiquality_ids.Add((need.is_multiquality && current_craft.IsMultiqualityOutput()) ? need.multiquality_items.FirstOrDefault() : null);
		}
		_current_craft_index = 0;
		_body_item = null;
		_on_btn_click = null;
		_ingredient_buttons = GetComponentsInChildren<CraftIngredientButtonsPair>(includeInactive: true);
		_perk_icons = GetComponentsInChildren<CraftItemPerkGUI>(includeInactive: true);
		_ingredients = GetIngredientsTableGameObject().GetComponentsInChildren<BaseItemCellGUI>(includeInactive: true);
		for (int j = 0; j < _ingredients.Length; j++)
		{
			int ingredient_index = j;
			_ingredients[j].GetComponentInChildren<UIEventTrigger>().onPress.Add(new EventDelegate(delegate
			{
				OnIngredientPressed(ingredient_index);
			}));
		}
		item_gui.SetCallbacks(OnOver, OnOut, OnItemAction);
		if (BaseGUI.for_gamepad)
		{
			if (gamepad_navigation_item != null)
			{
				gamepad_navigation_item.SetCallbacks(OnOver, OnOut, OnItemAction);
			}
			GamepadNavigationItem[] componentsInChildren = GetComponentsInChildren<GamepadNavigationItem>();
			foreach (GamepadNavigationItem gamepadNavigationItem in componentsInChildren)
			{
				if (!(gamepad_navigation_item == gamepadNavigationItem))
				{
					GamepadNavigationItem i = gamepadNavigationItem;
					gamepadNavigationItem.SetCallbacks(delegate
					{
						OnChildElementOver(i);
					}, null, delegate
					{
						OnChildElementSelect(i);
					});
					gamepadNavigationItem.sub_group = craft_definition.id.GetHashCode();
				}
			}
			if (multiquality_craft_btn != null)
			{
				multiquality_craft_btn.GetComponent<GamepadNavigationItem>().sub_group = craft_definition.id.GetHashCode();
			}
		}
		_overed_additional = (_overed_item = (_was_overed = false));
		Redraw();
		SetMouseOveredGraphics(overed: false);
	}

	public void Redraw()
	{
		bool flag = current_craft.IsMultiqualityOutput();
		if (!current_craft.CanCraftMultiple())
		{
			_amount = 1;
		}
		GetIngredientsTableGameObject().SetActive(value: true);
		if (button != null)
		{
			button.gameObject.SetActive(value: false);
		}
		item_gui.quality_icon.enabled = true;
		item_gui.quality_icon.gameObject.SetActive(value: false);
		item_gui.DrawItem((current_craft.output.Count > 0) ? current_craft.GetFirstRealOutput() : Item.empty);
		if (!string.IsNullOrEmpty(current_craft.icon))
		{
			item_gui.DrawIcon(current_craft.icon, draw_back: true, current_craft.hide_quality_icon);
		}
		if (item_gui.x1.icon != null)
		{
			item_gui.x1.icon.MakePixelPerfect();
		}
		item_gui.InitInputBehaviour();
		if (item_gui?.x1?.counter != null)
		{
			item_gui.x1.counter.text = GUIElements.me.craft?.GetCrafteryWGO()?.GetCraftAmountCounter(craft_definition, _amount);
		}
		string text = GJL.L(current_craft.GetNameNonLocalized());
		string text2 = current_craft.GetDescription();
		if (MainGame.me.build_mode_logics.cur_build_zone != null && MainGame.me.build_mode_logics.cur_build_zone.definition != null)
		{
			text2 = text2.Replace("(*)", MainGame.me.build_mode_logics.cur_build_zone.definition.quality_icon);
		}
		label_name.text = text;
		label_descr.gameObject.SetActive(!string.IsNullOrEmpty(text2));
		label_descr.text = text2;
		tooltip.available = !string.IsNullOrEmpty(text2);
		UpdateLabelsVisibility();
		BaseItemCellGUI.DrawIngredients(multi_inventory: (!GlobalCraftControlGUI.is_global_control_active) ? MainGame.me.player.GetMultiInventoryForInteraction() : GUIElements.me.craft.multi_inventory, ingredients: _ingredients, items: current_craft.needs, multiquality_ids: _multiquality_ids, amount: _amount);
		if (flag)
		{
			item_gui.quality_icon.enabled = false;
			full_detailed_go.SetActive(full_detailed_view);
			multi_quality_go.SetActive(!full_detailed_view);
			if (auto_height)
			{
				GetComponent<UIWidget>().height = (full_detailed_view ? multi_quality_height_full : multi_quality_height);
			}
			hgt_widget.height = (full_detailed_view ? widget_hgt_detailed : widget_hgt_normal);
			if (full_detailed_view)
			{
				bool flag2 = CanCraft();
				multiquality_craft_btn.SetState((!flag2) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, immediate: true);
				multiquality_craft_btn.GetComponent<Collider2D>().enabled = flag2;
				label_name.text = "";
				RedrawIngredientsButtons();
				RedrawPerksAndValues();
				UpdateMultiQualityHint();
				UpdateInnerSelection();
			}
		}
		else
		{
			if (full_detailed_go != null)
			{
				full_detailed_go.SetActive(value: false);
			}
			if (multi_quality_go != null)
			{
				multi_quality_go.SetActive(value: false);
			}
			if (auto_height)
			{
				GetComponent<UIWidget>().height = no_multi_quality_height;
			}
			if (hgt_widget != null)
			{
				hgt_widget.height = 2;
			}
			if (craft_definition.output.Count > 0)
			{
				craft_definition.output[0].definition?.TryDrawQualityOrDisableGameObject(item_gui.quality_icon);
				if (!string.IsNullOrEmpty(craft_definition.icon) && craft_definition.hide_quality_icon)
				{
					item_gui.quality_icon.sprite2D = null;
				}
			}
		}
		if (full_detailed_go != null && GetComponent<BoxCollider2D>() != null)
		{
			GetComponent<BoxCollider2D>().enabled = !full_detailed_go.activeSelf;
		}
		if (full_detailed_view && !BaseGUI.for_gamepad)
		{
			selection_frame?.gameObject?.SetActive(value: false);
		}
		selection_frame?.UpdateAnchors();
		label_name.color = (CanCraft() ? c_normal : c_not_enough);
		if (ingredients_table != null)
		{
			ingredients_table.repositionNow = true;
		}
		if (ingredients_table_universal != null)
		{
			ingredients_table_universal.Reposition();
		}
		if (txt_spend != null)
		{
			txt_spend.text = current_craft.GetSpendTxt(craft_gui_interface.GetCrafteryWGO(), _amount);
		}
		if (label_descr.gameObject.activeSelf && auto_height)
		{
			UIWidget component = label_descr.gameObject.transform.parent.GetComponent<UIWidget>();
			component.UpdateAnchors();
			label_descr.ProcessText();
			int num = label_descr.height - 20;
			if (flag)
			{
				if (!full_detailed_view && num > 0)
				{
					GetComponent<UIWidget>().height = multi_quality_height + num;
					hgt_widget.height = 2 + num / 2;
					component.bottomAnchor.absolute = -num;
				}
			}
			else if (num > 0)
			{
				GetComponent<UIWidget>().height = no_multi_quality_height + num;
				hgt_widget.height = 2 + num / 2;
				component.bottomAnchor.absolute = -num;
			}
		}
		if (ingredients_table_universal != null)
		{
			ingredients_table_universal.Reposition();
		}
		else
		{
			GetComponentInChildren<SimpleUITable>().Reposition();
		}
		BoxCollider2D component2 = GetComponent<BoxCollider2D>();
		if (component2 != null)
		{
			UIWidget component3 = GetComponent<UIWidget>();
			component2.size = new Vector2(component2.size.x, component3.height);
			component2.offset = new Vector2(component2.offset.x, (float)(-component3.height) / 2f);
		}
		GetComponent<UIWidget>().BroadcastMessage("UpdateAnchors");
		if (_current_inner_ingredient != null)
		{
			TooltipsManager.Redraw();
		}
		RedrawAmountButtons();
	}

	private void SetOveredState(bool ovr)
	{
		_was_overed = ovr;
		SetMouseOveredGraphics(ovr);
	}

	private void Update()
	{
		if (!BaseGUI.for_gamepad)
		{
			bool flag = _overed_item || _overed_additional;
			if (flag != _was_overed)
			{
				if (flag)
				{
					Sounds.OnGUIHover();
					if (current_overed != null)
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
		}
		if (BaseGUI.for_gamepad && !(current_overed != this) && (!(_craft_gui != null) || _craft_gui.is_shown_and_top))
		{
			UpdateInnerSelection();
		}
	}

	public bool ProcessIngredientStep(int step)
	{
		if (step == 0 || !full_detailed_view || _current_inner_ingredient == null)
		{
			return false;
		}
		for (int i = 0; i < _ingredients.Length; i++)
		{
			if (!(_ingredients[i] != _current_inner_ingredient) && IngredientHasOption(i, step))
			{
				OnChangeIngredient(i, step);
				return true;
			}
		}
		return false;
	}

	public void RedrawPerksAndValues()
	{
		List<string> neededPerks = current_craft.GetNeededPerks();
		List<string> linked_buffs = current_craft.linked_buffs;
		int num = 0;
		foreach (string item in neededPerks)
		{
			if (num == _perk_icons.Length)
			{
				break;
			}
			_perk_icons[num++].DrawPerk(item);
		}
		foreach (string item2 in linked_buffs)
		{
			if (num == _perk_icons.Length)
			{
				break;
			}
			if (BuffsLogics.FindBuffByID(item2) != null)
			{
				_perk_icons[num++].DrawBuff(item2);
			}
		}
		for (int i = num; i < _perk_icons.Length; i++)
		{
			_perk_icons[i].DrawPerk("");
		}
		no_influenced_perks_txt.gameObject.SetActive(num == 0);
		CraftDefinition.MultiqualityCraftResult multiqualityResult = current_craft.GetMultiqualityResult(_multiquality_ids);
		value_items.text = "(s1)" + multiqualityResult.value_items.ToString("0.0");
		value_perks.text = "(s1)" + multiqualityResult.value_perks.ToString("0.0");
		value_items_and_perks_sum.text = FloatValueToStr(multiqualityResult.value_items_and_perks_sum);
		value_difficulty.text = FloatValueToStr(0f - multiqualityResult.value_difficulty);
		value_result.text = multiqualityResult.value_result.ToString("0.0");
		float num2 = 0f;
		bool flag = false;
		for (int num3 = 2; num3 >= 0; num3--)
		{
			num2 += multiqualityResult.quality_probabilities[num3];
			int num4 = Mathf.RoundToInt(multiqualityResult.quality_probabilities[num3] * 100f);
			quality_probabilities[num3].text = ((num4 != 100 || !flag) ? (num4 + "%") : "");
			switch (num4)
			{
			case 0:
				quality_probabilities[num3].color = color_probability_0;
				break;
			case 100:
				quality_probabilities[num3].color = color_probability_100;
				flag = true;
				break;
			default:
				quality_probabilities[num3].color = color_probability_other;
				break;
			}
		}
		quality_bar.fillAmount = num2 / 3f;
		quality_bar_edge_container.localScale = Vector3.right * quality_bar.fillAmount;
	}

	private string FloatValueToStr(float value)
	{
		string text = "(s1)";
		if (value > 0f)
		{
			text += "+";
		}
		else if (value < 0f)
		{
			text += "-";
		}
		return text + Mathf.Abs(value).ToString("0.0");
	}

	public void AddOneMoreCraftDefinition(CraftDefinition craft_definition)
	{
		_possible_crafts.Add(craft_definition);
	}

	private void UpdateLabelsVisibility()
	{
		label_descr.SetActive(!string.IsNullOrEmpty(label_descr.text) && !full_detailed_view);
	}

	private void UpdateInnerSelection()
	{
		if (full_detailed_view)
		{
			inner_selection_frame.SetActive(current_overed == this && _current_inner_selectable_item != null);
			if (!(_current_inner_selectable_item == null))
			{
				inner_selection_frame.transform.position = _current_inner_selectable_item.transform.position;
				inner_selection_frame.width = _current_inner_selectable_item.width;
				inner_selection_frame.height = _current_inner_selectable_item.height;
			}
		}
	}

	public void OnMouseOvered()
	{
		if (!BaseGUI.for_gamepad)
		{
			_overed_item = true;
		}
	}

	public void OnMouseOuted()
	{
		if (!BaseGUI.for_gamepad)
		{
			_overed_item = false;
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

	public void OnOver()
	{
		if (!BaseGUI.for_gamepad)
		{
			return;
		}
		current_overed = this;
		if (_body_item == null)
		{
			if (current_craft.IsMultiqualityOutput())
			{
				UpdateMultiQualityHint();
				UpdateInnerSelection();
			}
			else
			{
				craft_gui_interface.GetButtonTips().Print(GameKeyTip.Select("craft", CanCraft()), GameKeyTip.Option2("information"), GameKeyTip.Close());
			}
		}
		else
		{
			craft_gui_interface.GetButtonTips().Print(GameKeyTip.Select(), GameKeyTip.Close());
		}
		selection_frame.SetActive(active: true);
		RedrawAmountButtons();
		Sounds.OnGUIHover();
		if (craft_gui_interface.GetItemsList().Count > 0 && (this == craft_gui_interface.GetItemsList()[0] || this == craft_gui_interface.GetItemsList().Last()) && auto_scroll != null)
		{
			auto_scroll.Perform();
		}
	}

	private void RedrawAmountButtons()
	{
		if (!(amount_buttons == null))
		{
			bool has_amount_buttons = _craft_gui.has_amount_buttons;
			has_amount_buttons &= current_craft != null && current_craft.CanCraftMultiple();
			amount_buttons.SetActive(has_amount_buttons);
		}
	}

	public void OnAboveWindowClosed()
	{
		if (current_craft.IsMultiqualityOutput() && full_detailed_view)
		{
			if (_current_inner_selectable_item == null)
			{
				craft_gui_interface.GetGamepadController().Enable(GamepadNavigationController.OpenMethod.GetAllAndFocus);
				return;
			}
			craft_gui_interface.GetGamepadController().Enable(GamepadNavigationController.OpenMethod.GetAll);
			craft_gui_interface.GetGamepadController().SetFocusedItem(_current_inner_selectable_item.GetComponent<GamepadNavigationItem>());
		}
	}

	private void OnChildElementOver(GamepadNavigationItem item)
	{
		_current_inner_selectable_item = item.GetComponent<UIWidget>();
		_current_inner_ingredient = _current_inner_selectable_item.GetComponent<BaseItemCellGUI>();
		UpdateInnerSelection();
		UpdateMultiQualityHint();
	}

	private void OnChildElementSelect(GamepadNavigationItem item)
	{
		UIButton componentInChildren = item.GetComponentInChildren<UIButton>();
		if (componentInChildren != null)
		{
			foreach (EventDelegate item2 in componentInChildren.onClick)
			{
				item2.Execute();
			}
		}
		UIEventTrigger componentInChildren2 = item.GetComponentInChildren<UIEventTrigger>();
		if (!(componentInChildren2 != null))
		{
			return;
		}
		foreach (EventDelegate item3 in componentInChildren2.onPress)
		{
			item3.Execute();
		}
	}

	private void UpdateMultiQualityHint()
	{
		List<GameKeyTip> list = new List<GameKeyTip>();
		if (full_detailed_view)
		{
			if (_current_inner_selectable_item == multiquality_craft_btn.GetComponent<UIWidget>())
			{
				list.Add(GameKeyTip.Select("craft", CanCraft()));
			}
			if (_current_inner_ingredient != null)
			{
				for (int i = 0; i < _ingredients.Length; i++)
				{
					if (!(_ingredients[i] != _current_inner_ingredient) && IsSwitchableIngredient(i))
					{
						list.Add(GameKeyTip.Select("select"));
					}
				}
			}
		}
		else
		{
			list.Add(GameKeyTip.Select("expand"));
		}
		if (full_detailed_view)
		{
			list.Add(GameKeyTip.Back());
		}
		else
		{
			list.Add(GameKeyTip.Close());
		}
		craft_gui_interface.GetButtonTips().Print(list);
	}

	public void OnOut()
	{
		selection_frame.SetActive(active: false);
		inner_selection_frame.Deactivate();
		if (BaseGUI.for_gamepad && !(this != current_overed))
		{
			craft_gui_interface.GetButtonTips().Clear();
			RedrawAmountButtons();
		}
	}

	public void OnItemAction()
	{
		if (BaseGUI.IsLastClickRightButton())
		{
			craft_gui_interface.OnRightClick();
		}
		else if (_on_btn_click != null)
		{
			if (BaseGUI.for_gamepad)
			{
				LazyInput.ClearKeyDown(GameKey.Select);
			}
			_on_btn_click();
		}
		else
		{
			if (UtilityStuff.ProcessCraftItemAction(this, current_craft))
			{
				return;
			}
			if (current_craft.IsMultiqualityOutput())
			{
				if (full_detailed_view)
				{
					if (BaseGUI.for_gamepad)
					{
						OnCraftPressed();
					}
					else
					{
						_craft_gui.CollapseItem();
					}
				}
				else
				{
					selection_frame.Deactivate();
					OnOpenDetailsButtonPressed();
					_craft_gui.ExpandItem(this);
				}
			}
			else
			{
				OnCraftPressed();
			}
		}
	}

	private void OnCraftPressed()
	{
		if (CraftGUI.in_redraw_mode)
		{
			return;
		}
		Sounds.OnGUIClick();
		last_item_pressed = this;
		bool flag = CanCraft();
		int num = 0;
		int num2 = _amount;
		bool flag2 = true;
		flag2 = _craft_gui.GetCrafteryWGO().obj_def.can_insert_zombie;
		if (_craft_gui.custom_craft_interface != null)
		{
			flag2 = false;
		}
		Debug.Log($"OnCraftPressed, custom_ui={_craft_gui.custom_craft_interface}, can_craft={flag}, n={_amount}, _multiquality_ids={_multiquality_ids?.JoinToString()}");
		CraftComponent craftComponent = _craft_gui.GetCrafteryWGO()?.components?.craft;
		if (craftComponent != null)
		{
			bool flag3 = !current_craft.CanEnqueue();
			if (craftComponent.is_crafting && craftComponent.current_craft != null && !craftComponent.current_craft.CanEnqueue())
			{
				flag3 = true;
			}
			if (_multiquality_ids != null && _multiquality_ids.Count > 0 && (craftComponent.is_crafting || !craftComponent.IsCraftQueueEmpty()))
			{
				using List<string>.Enumerator enumerator = _multiquality_ids.GetEnumerator();
				while (enumerator.MoveNext() && string.IsNullOrEmpty(enumerator.Current))
				{
				}
			}
			if (!craftComponent.IsCraftQueueEmpty() && craftComponent.craft_queue.Count > 0 && !craftComponent.craft_queue[0].craft.CanEnqueue())
			{
				flag3 = true;
			}
			if (craftComponent.IsCraftQueueEmpty() && !craftComponent.is_crafting)
			{
				flag3 = false;
			}
			if (flag3)
			{
				CraftGUI.NeedToCancelCraftsDialog(craftComponent, delegate
				{
					GUIElements.me.dialog.Hide();
					OnCraftPressed();
				});
				return;
			}
		}
		if (flag2)
		{
			num = num2;
			num2 = 0;
			if (CanCraft(1) && craftComponent != null && craftComponent.IsCraftQueueEmpty() && !craftComponent.is_crafting)
			{
				num2 = 1;
				num--;
			}
		}
		else if (!flag)
		{
			return;
		}
		bool flag4 = true;
		if (num2 > 0)
		{
			flag4 = ((_craft_gui.custom_craft_interface == null) ? craft_gui_interface.OnCraft(current_craft, null, _multiquality_ids, num2) : _craft_gui.custom_craft_interface.OnCraft(current_craft, null, _multiquality_ids, num2));
		}
		if (num > 0)
		{
			if (!current_craft.CanEnqueue())
			{
				GUIElements.me.dialog.OpenOK("not_enough_resources", CraftGUI.RestoreFocusAfterDialogWindow);
				LazyInput.ClearKeyDown(GameKey.Select);
				return;
			}
			craftComponent?.EnqueueCraft(current_craft, _multiquality_ids, num, can_use_player_inventory: true);
			flag4 = true;
		}
		if (_craft_gui.custom_craft_interface == null)
		{
			_craft_gui.queue?.Redraw();
		}
		if (flag4 && BaseGUI.for_gamepad)
		{
			LazyInput.ClearKeyDown(GameKey.Select);
		}
		MainGame.me.player.components.interaction.RedrawCurrentInteractiveHint();
	}

	private bool CanCraft(int? amount = null)
	{
		if (craft_gui_interface == null)
		{
			craft_gui_interface = GUIElements.me.craft;
		}
		if (_craft_gui?.custom_craft_interface != null)
		{
			return _craft_gui.custom_craft_interface.CanCraft(current_craft, _multiquality_ids, amount ?? _amount);
		}
		return craft_gui_interface.CanCraft(current_craft, _multiquality_ids, amount ?? _amount);
	}

	protected void SetMouseOveredGraphics(bool overed)
	{
		if (!full_detailed_view)
		{
			selection_frame.SetActive(overed);
		}
	}

	public void OnOpenDetailsButtonPressed()
	{
		if (BaseGUI.IsLastClickRightButton())
		{
			return;
		}
		if (full_detailed_view)
		{
			_craft_gui.CollapseItem(this);
			return;
		}
		_craft_gui.ExpandItem(this);
		if (auto_scroll != null)
		{
			GJTimer.AddTimer(0.03f, delegate
			{
				auto_scroll.Perform();
			});
		}
	}

	private void OnIngredientPressed(int ingredient_index)
	{
		if (current_craft.IsMultiqualityOutput() && full_detailed_view)
		{
			WorldGameObject obj = MainGame.me.player;
			if (GlobalCraftControlGUI.is_global_control_active && _craft_gui.GetCrafteryWGO() != null)
			{
				obj = _craft_gui.GetCrafteryWGO();
			}
			if (BaseGUI.for_gamepad)
			{
				craft_gui_interface.GetButtonTips().Deactivate();
			}
			GUIElements.me.resource_picker.Open(obj, (Item item, InventoryWidget widget) => IngredientFiler(ingredient_index, item), delegate(Item item)
			{
				OnIngredientChanged(ingredient_index, item);
			});
		}
	}

	private void OnIngredientChanged(int ingredient_index, Item item)
	{
		if (item == null || item.IsEmpty() || ingredient_index >= current_craft.needs.Count)
		{
			return;
		}
		Item item2 = current_craft.needs[ingredient_index];
		if (item.id == item2.id || item.id == _multiquality_ids[ingredient_index])
		{
			return;
		}
		if (item2.is_multiquality && item2.multiquality_items.Contains(item.id))
		{
			_multiquality_ids[ingredient_index] = item.id;
			Redraw();
			return;
		}
		foreach (CraftDefinition possible_craft in _possible_crafts)
		{
			item2 = possible_craft.needs[ingredient_index];
			if (item2.id != item.id && (!item2.is_multiquality || !item2.multiquality_items.Contains(item.id)))
			{
				continue;
			}
			_current_craft_index = _possible_crafts.IndexOf(possible_craft);
			_multiquality_ids.Clear();
			foreach (Item need in current_craft.needs)
			{
				_multiquality_ids.Add(need.multiquality_items.FirstOrDefault());
			}
			Redraw();
			break;
		}
	}

	public InventoryWidget.ItemFilterResult IngredientFiler(int ingredient_index, Item item)
	{
		if (item == null || item.IsEmpty() || ingredient_index >= current_craft.needs.Count)
		{
			return InventoryWidget.ItemFilterResult.Inactive;
		}
		foreach (CraftDefinition possible_craft in _possible_crafts)
		{
			Item item2 = possible_craft.needs[ingredient_index];
			if (item2.id == item.id)
			{
				return InventoryWidget.ItemFilterResult.Active;
			}
			if (item2.is_multiquality && item2.multiquality_items.Contains(item.id))
			{
				return InventoryWidget.ItemFilterResult.Active;
			}
		}
		return InventoryWidget.ItemFilterResult.Inactive;
	}

	private void RedrawIngredientsButtons()
	{
		for (int i = 0; i < _ingredient_buttons.Length; i++)
		{
			int ingredient_index = i;
			bool flag = IsSwitchableIngredient(i);
			_ingredient_buttons[i].Init(flag, delegate(int step)
			{
				OnChangeIngredient(ingredient_index, step);
			});
			if (flag)
			{
				_ingredient_buttons[i].SetEnabled(IngredientHasOption(i, 1), IngredientHasOption(i, -1));
			}
		}
	}

	private void OnChangeIngredient(int ingredient_index, int step)
	{
		if (ingredient_index >= current_craft.needs.Count)
		{
			return;
		}
		Item item = current_craft.needs[ingredient_index];
		if (item.is_multiquality)
		{
			List<string> multiquality_items = item.multiquality_items;
			string item2 = _multiquality_ids[ingredient_index];
			int num = multiquality_items.IndexOf(item2) + step;
			if (num >= 0 && num < multiquality_items.Count)
			{
				_multiquality_ids[ingredient_index] = multiquality_items[num];
				Redraw();
				return;
			}
		}
		int num2 = _current_craft_index + step;
		if (num2 < 0 && num2 >= _possible_crafts.Count)
		{
			return;
		}
		_current_craft_index = num2;
		_multiquality_ids.Clear();
		int num3 = 0;
		foreach (Item need in current_craft.needs)
		{
			string item3 = need.multiquality_items.FirstOrDefault();
			if (num3 == ingredient_index && step < 0 && need.multiquality_items != null)
			{
				item3 = need.multiquality_items.Last();
			}
			_multiquality_ids.Add(item3);
			num3++;
		}
		Redraw();
	}

	private bool IngredientHasOption(int ingredient_index, int step)
	{
		if (ingredient_index >= current_craft.needs.Count)
		{
			return false;
		}
		Item item = current_craft.needs[ingredient_index];
		if (_possible_crafts.Count == 1 && !item.is_multiquality)
		{
			return false;
		}
		if (item.is_multiquality)
		{
			List<string> multiquality_items = item.multiquality_items;
			string text = _multiquality_ids[ingredient_index];
			if (step > 0 && text != multiquality_items.LastElement())
			{
				return true;
			}
			if (step < 0 && text != multiquality_items[0])
			{
				return true;
			}
		}
		int num = _current_craft_index + step;
		if (num < 0 || num >= _possible_crafts.Count)
		{
			return false;
		}
		return item.id != _possible_crafts[num].needs[ingredient_index].id;
	}

	private bool IsSwitchableIngredient(int ingredient_index)
	{
		if (!IngredientHasOption(ingredient_index, -1))
		{
			return IngredientHasOption(ingredient_index, 1);
		}
		return true;
	}

	public void OnAmountPlus()
	{
		if (current_craft.CanCraftMultiple())
		{
			_amount++;
			Redraw();
			OnOver();
		}
	}

	public void OnAmountMinus()
	{
		if (current_craft.CanCraftMultiple() && _amount != 1)
		{
			_amount--;
			Redraw();
			OnOver();
		}
	}
}
