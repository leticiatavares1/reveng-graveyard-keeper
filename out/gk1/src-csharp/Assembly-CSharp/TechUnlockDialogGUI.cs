using System.Collections.Generic;
using UnityEngine;

public class TechUnlockDialogGUI : BaseGUI
{
	public UILabel label_header;

	public UILabel label_cost;

	public Color c_price_normal;

	public Color c_price_not_enough;

	public Color c_price_disabled;

	public UIWidget[] widgets_to_update;

	private List<TechTreeGUIUnlockItem> _unlocks = new List<TechTreeGUIUnlockItem>();

	private DialogButtonsGUI _dialog_buttons;

	public GameObject subheader_container;

	public UILabel subheader;

	public UITableOrGrid table;

	public UIWidget background;

	private bool _opened_as_infobox;

	private bool _needs_resize;

	public override void Init()
	{
		_dialog_buttons = GetComponentInChildren<DialogButtonsGUI>(includeInactive: true);
		TechTreeGUIUnlockItem componentInChildren = GetComponentInChildren<TechTreeGUIUnlockItem>(includeInactive: true);
		_unlocks.Add(componentInChildren);
		for (int i = 1; i < 3; i++)
		{
			TechTreeGUIUnlockItem techTreeGUIUnlockItem = componentInChildren.Copy();
			techTreeGUIUnlockItem.Deactivate();
			_unlocks.Add(techTreeGUIUnlockItem);
		}
		base.Init();
	}

	public void Open(TechDefinition tech, GJCommons.VoidDelegate on_hide, bool forced_unlock = false, bool reveal_tech = false, bool show_tech_tree_after = false, bool pseudotech = false)
	{
		Debug.Log("TechUnlockDialogGUI, tech = " + ((tech == null) ? "NULL" : tech.id) + ", forced_unlock = " + forced_unlock + ", reveal_tech = " + reveal_tech + ", show_tech_tree_after = " + show_tech_tree_after);
		base.Open();
		_opened_as_infobox = false;
		TooltipsManager.Redraw();
		subheader_container.SetActive(forced_unlock);
		label_header.text = GJL.L(tech.id);
		subheader.text = GJL.L("youve_unlocked_tech");
		TechDefinition.TechState state = tech.GetState();
		bool flag = MainGame.me.player.IsEnough(tech.price);
		bool flag2 = state == TechDefinition.TechState.Unavailable;
		List<TechUnlock> visibleUnlocksList = tech.GetVisibleUnlocksList();
		for (int i = 0; i < _unlocks.Count; i++)
		{
			_unlocks[i].Draw((i < visibleUnlocksList.Count) ? visibleUnlocksList[i] : null, init_tooltip: false);
		}
		Color color = (flag2 ? c_price_disabled : c_price_normal);
		if (flag)
		{
			label_cost.color = color;
			label_cost.text = tech.price.ToPrintableString();
		}
		else
		{
			label_cost.color = Color.white;
			label_cost.text = tech.price.ToPrintableString(use_colors: true, color, c_price_not_enough);
		}
		if (forced_unlock)
		{
			Sounds.PlaySound("unlock");
			label_cost.text = "";
			_dialog_buttons.Set("OK", delegate
			{
				if (!pseudotech)
				{
					if (reveal_tech)
					{
						MainGame.me.save.RevealHiddenTech(tech.id);
					}
					else
					{
						MainGame.me.save.UnlockTech(tech.id);
					}
				}
				Hide();
				if (show_tech_tree_after)
				{
					GUIElements.me.tech_tree.OpenTech(tech.id);
				}
			});
		}
		else
		{
			if (reveal_tech)
			{
				Debug.LogError("Reveal tech dialog should be forced, not optional, tech id = " + tech.id);
			}
			_dialog_buttons.Set("unlock", delegate
			{
				Sounds.PlaySound("unlock");
				MainGame.me.save.BuyTech(tech.id);
				Hide();
			}, "cancel", delegate
			{
				Hide();
			});
			_dialog_buttons.SetEnabled(flag && !flag2);
		}
		SetOnHide(on_hide);
		UpdatePixelPerfect();
		SimpleUITable[] componentsInChildren = GetComponentsInChildren<SimpleUITable>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].Reposition();
		}
		UITable[] componentsInChildren2 = GetComponentsInChildren<UITable>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].Reposition();
		}
		table.Reposition();
		UpdateAllAnchors();
		_needs_resize = true;
	}

	public void OpenAsItemsList(string sub_header_translated, List<Item> items, GJCommons.VoidDelegate on_hide)
	{
		base.Open();
		TooltipsManager.Redraw();
		label_cost.text = "";
		_opened_as_infobox = true;
		SetOnHide(on_hide);
		label_header.text = GJL.L("information");
		subheader.text = sub_header_translated;
		_dialog_buttons.Set("OK", delegate
		{
			Hide();
		});
		for (int i = 0; i < _unlocks.Count; i++)
		{
			if (i >= items.Count)
			{
				_unlocks[i].Draw(null);
				continue;
			}
			ItemDefinition definition = items[i].definition;
			if (definition == null)
			{
				Debug.LogWarning("Item definition is null for id = " + items[i].id);
				_unlocks[i].Draw(null);
				continue;
			}
			_unlocks[i].Draw(new TechUnlock.TechUnlockData
			{
				sprite = EasySpritesCollection.GetSprite(definition.GetIcon()),
				name = definition.GetItemName(),
				quality_icon = definition.TryGetQualitySprite(),
				description = definition.GetItemDescription()
			});
		}
		table.Reposition();
		UpdatePixelPerfect();
		UpdateAllAnchors();
		if (background != null)
		{
			background.UpdateAnchors();
			UpdateAllAnchors();
		}
		_needs_resize = true;
	}

	private new void LateUpdate()
	{
		if (_needs_resize)
		{
			_needs_resize = false;
			UpdateWidgets();
		}
	}

	protected override bool OnPressedBack()
	{
		if (_opened_as_infobox)
		{
			OnClosePressed();
			return true;
		}
		return base.OnPressedBack();
	}

	private void UpdateWidgets()
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget obj in componentsInChildren)
		{
			obj.Update();
			obj.UpdateAnchors();
		}
	}
}
