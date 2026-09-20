using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class TechTreeGUIItem : MonoBehaviour
{
	private TechDefinition _tech;

	public UI2DSprite[] sprs;

	public UILabel txt_name;

	public UILabel txt_name_2;

	public UILabel txt_name_3;

	public UILabel txt_cost;

	private static TechTreeGUIUnlockItem _unlock_prefab;

	public UIGrid unlocks_table;

	public GameObject gamepad_frame;

	private List<TechTreeGUIUnlockItem> _unlocks;

	public GameObject go_alpha_locked;

	public GameObject go_purchased;

	public GameObject go_locked;

	public GameObject go_not_purchased;

	public GameObject go_question_mark;

	public Transform pos1;

	public Transform pos2;

	private TechTreeGUI _tech_tree;

	public Color c_price_normal;

	public Color c_price_not_enough;

	public Color c_price_disabled;

	private int _on_out_call_frame;

	public string tech_id => _tech.id;

	public TechDefinition tech => _tech;

	public void InitPrefab()
	{
		base.gameObject.SetActive(value: false);
		_unlock_prefab = GetComponentInChildren<TechTreeGUIUnlockItem>(includeInactive: true);
		_unlocks = new List<TechTreeGUIUnlockItem> { _unlock_prefab };
		_unlock_prefab.gameObject.SetActive(value: false);
		for (int i = 0; i < 2; i++)
		{
			TechTreeGUIUnlockItem techTreeGUIUnlockItem = _unlock_prefab.Copy();
			_unlocks.Add(techTreeGUIUnlockItem);
			techTreeGUIUnlockItem.gameObject.SetActive(value: false);
		}
		unlocks_table.repositionNow = true;
	}

	public void Draw(TechDefinition tech)
	{
		if (_unlocks == null)
		{
			_unlocks = GetComponentsInChildren<TechTreeGUIUnlockItem>(includeInactive: true).ToList();
		}
		_tech_tree = GUIElements.me.tech_tree;
		_tech = tech;
		txt_name.text = GJL.L(tech.id);
		if (txt_name_2 != null)
		{
			txt_name_2.text = txt_name.text;
		}
		if (txt_name_3 != null)
		{
			txt_name_3.text = txt_name.text;
		}
		go_question_mark.SetActive(value: false);
		List<TechUnlock> visibleUnlocksList = tech.GetVisibleUnlocksList();
		TechDefinition.TechState techState = GetTechState();
		if (techState == TechDefinition.TechState.Hidden)
		{
			visibleUnlocksList.Clear();
		}
		for (int i = 0; i < _unlocks.Count; i++)
		{
			_unlocks[i].Draw((i < visibleUnlocksList.Count) ? visibleUnlocksList[i] : null, !BaseGUI.for_gamepad);
		}
		if (BaseGUI.for_gamepad)
		{
			InitGamepadTooltip(visibleUnlocksList);
		}
		RedrawTechState(techState);
		if (techState != 0)
		{
			if (techState == TechDefinition.TechState.Hidden)
			{
				txt_name.text = "";
				txt_cost.text = "";
			}
			else
			{
				Color color = ((techState == TechDefinition.TechState.Unavailable) ? c_price_disabled : c_price_normal);
				if (MainGame.me.player.IsEnough(tech.price))
				{
					txt_cost.color = color;
					txt_cost.text = tech.price.ToPrintableString();
				}
				else
				{
					txt_cost.color = Color.white;
					txt_cost.text = tech.price.ToPrintableString(use_colors: true, color, c_price_not_enough);
				}
			}
		}
		else
		{
			txt_cost.text = "";
		}
		GetComponent<GamepadNavigationItem>().SetCallbacks(OnGamepadOver, OnGamepadOut, OnClickedTech);
		OnGamepadOut();
	}

	private void InitGamepadTooltip(List<TechUnlock> visible_unlocks)
	{
		Tooltip component = GetComponent<Tooltip>();
		if (component == null)
		{
			return;
		}
		for (int i = 0; i < visible_unlocks.Count; i++)
		{
			visible_unlocks[i].GetTooltip(component);
			if (i != visible_unlocks.Count - 1)
			{
				component.AddData(new BubbleWidgetSeparatorData());
			}
		}
	}

	public TechDefinition.TechState GetTechState()
	{
		return _tech.GetState();
	}

	private void OnGamepadOver()
	{
		_tech_tree.gamepad_controller.restore_last_in_group = true;
		gamepad_frame.Activate();
		if (!Sounds.WasAnySoundPlayedThisFrame())
		{
			Sounds.OnGUIHover(Sounds.ElementType.ItemCell);
		}
		if (MainGame.me.save.unlocked_techs.Contains(tech_id))
		{
			_tech_tree.button_tips.Print(GameKeyTip.Close());
		}
		else
		{
			_tech_tree.button_tips.Print(GameKeyTip.Select("buy", MainGame.me.save.CanBuyTech(tech_id)), GameKeyTip.Close());
		}
		TooltipsManager.Redraw();
	}

	private void OnGamepadOut()
	{
		gamepad_frame.Deactivate();
	}

	public void OnMouseOvered()
	{
		if (!BaseGUI.for_gamepad)
		{
			if (_on_out_call_frame != Time.frameCount && !Sounds.WasAnySoundPlayedThisFrame())
			{
				Sounds.OnGUIHover(Sounds.ElementType.ItemCell);
			}
			gamepad_frame.Activate();
			TooltipsManager.Redraw();
		}
	}

	public void OnMouseOuted()
	{
		if (!BaseGUI.for_gamepad)
		{
			gamepad_frame.Deactivate();
			_on_out_call_frame = Time.frameCount;
		}
	}

	public void OnClickedTech()
	{
		if (!BaseGUI.IsLastClickRightButton())
		{
			_tech_tree.OnClickTech(_tech);
		}
	}

	private void RedrawTechState(TechDefinition.TechState state)
	{
		go_purchased.SetActive(state == TechDefinition.TechState.Purchased);
		go_locked.SetActive(state == TechDefinition.TechState.Unavailable || state == TechDefinition.TechState.Hidden);
		go_not_purchased.SetActive(state == TechDefinition.TechState.AvailableForPurchase);
		go_question_mark.SetActive(state == TechDefinition.TechState.Hidden);
		unlocks_table.gameObject.SetActive(state != TechDefinition.TechState.Hidden);
		go_alpha_locked.SetActive(value: false);
	}
}
