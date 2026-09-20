using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseCraftGUI : BaseGUI
{
	protected static Dictionary<long, string> last_crafts = new Dictionary<long, string>();

	[NonSerialized]
	protected List<CraftDefinition> crafts = new List<CraftDefinition>();

	[NonSerialized]
	protected WorldGameObject craftery_wgo;

	[NonSerialized]
	protected CraftComponent craft_component;

	protected CraftsInventory crafts_inventory;

	public UniversalObjectInfoGUI object_info;

	public WorldGameObject additional_inventory
	{
		get
		{
			if (!base.is_shown)
			{
				return null;
			}
			return craftery_wgo;
		}
	}

	public MultiInventory multi_inventory
	{
		get
		{
			if (GlobalCraftControlGUI.is_global_control_active && craftery_wgo != null)
			{
				WorldZone myWorldZone = craftery_wgo.GetMyWorldZone();
				if (myWorldZone != null && !myWorldZone.IsPlayerInZone())
				{
					return craftery_wgo.GetMultiInventory();
				}
			}
			return MainGame.me.player.GetMultiInventoryForInteraction();
		}
	}

	protected void CommonOpen(WorldGameObject o, CraftDefinition.CraftType allowed_type)
	{
		Debug.Log("Open craft: " + o.obj_id);
		craftery_wgo = o;
		if (object_info != null)
		{
			object_info.Draw(o.GetUniversalObjectInfo());
		}
		base.Open();
		craft_component = o.components.craft;
		if (craft_component == null)
		{
			Debug.LogError("WorkbenchComponent not found in object " + o, o);
			return;
		}
		crafts.Clear();
		List<CraftDefinition> list;
		if (crafts_inventory == null)
		{
			list = craft_component.crafts;
		}
		else
		{
			list = new List<CraftDefinition>();
			if (crafts_inventory.is_building)
			{
				foreach (ObjectCraftDefinition objectCrafts in crafts_inventory.GetObjectCraftsList())
				{
					if (MainGame.me.save.IsCraftVisible(objectCrafts))
					{
						list.Add(objectCrafts);
					}
				}
			}
			else
			{
				list.AddRange(crafts_inventory.GetCraftsList());
			}
		}
		foreach (CraftDefinition item in list)
		{
			if (MainGame.me.save.IsCraftVisible(item) && item.craft_type == allowed_type && (allowed_type != CraftDefinition.CraftType.AlchemyDecompose || MainGame.me.save.IsSurveyComplete(CraftDefinition.CraftSubType.Alchemy, item.needs[0].id)))
			{
				crafts.Add(item);
			}
		}
	}

	public void OverrideSettings(WorldGameObject wgo)
	{
		craftery_wgo = wgo;
		craft_component = wgo.components.craft;
	}

	public override void Open()
	{
		throw new Exception("Can't call Open() for a craft gui without any parameter.");
	}

	public bool CanCraft(CraftDefinition craft, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null)
	{
		if (craft == null)
		{
			return false;
		}
		if (craft.can_craft_always)
		{
			return true;
		}
		if (craft is ObjectCraftDefinition && !(craft as ObjectCraftDefinition).enabled)
		{
			return false;
		}
		if (GlobalCraftControlGUI.is_global_control_active && craft.gratitude_points_craft_cost != null && !craftery_wgo.components.craft.CanSpendPlayerGratitudePoints(craft.gratitude_points_craft_cost.EvaluateFloat(MainGame.me.player)))
		{
			return false;
		}
		if (!multi_inventory.IsEnoughItems(override_needs ?? craft.needs, MultiInventory.DestinationType.AllFromFirst, multiquality_ids, amount))
		{
			return false;
		}
		if (!craftery_wgo.data.IsEnoughItems(craft.needs_from_wgo, amount))
		{
			return false;
		}
		if (craft.item_needs.Count > 0 && (craftery_wgo.data.inventory.Count < 0 || !craftery_wgo.data.inventory[0].IsEnoughItems(craft.item_needs)))
		{
			return false;
		}
		return craft.condition.EvaluateBoolean(craftery_wgo, MainGame.me.player);
	}

	public virtual bool OnCraft(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null, WorldGameObject other_obj_override = null)
	{
		Debug.Log("OnCraft " + craft.id, this);
		if (!CanCraft(craft, multiquality_ids, 1, override_needs))
		{
			Debug.LogError("Can't start a craft: " + craft.id);
			return false;
		}
		if (IsBuildMode())
		{
			if (craft.id == "_remove_")
			{
				Hide();
				BuildGrid.ShowBuildGrid(show: false);
				MainGame.me.EnterBuildMode(removing_mode: true);
				return true;
			}
			if (MainGame.me.build_mode_logics.CanBuild(craft))
			{
				MainGame.me.build_mode_logics.CraftBuilding(craft);
				RememberCraft(craft.id);
				return true;
			}
			return false;
		}
		if (!craft_component.CraftAsPlayer(craft, try_use_particular_item, multiquality_ids, override_needs, ignore_crafts_list: false, amount, other_obj_override))
		{
			Debug.LogError("Craft not started");
			return false;
		}
		if (craft.dont_close_window_on_craft)
		{
			Redraw();
		}
		else
		{
			if (GlobalCraftControlGUI.is_global_control_active)
			{
				GUIElements.me.global_craft_control_gui.Open();
			}
			Hide();
		}
		if (GUIElements.me.rat_cell_gui.is_shown)
		{
			GUIElements.me.rat_cell_gui.Hide(play_hide_sound: false);
		}
		return true;
	}

	public virtual void Redraw()
	{
	}

	private void RememberCraft(string craft_id)
	{
		if (!(craftery_wgo == null))
		{
			if (last_crafts.ContainsKey(craftery_wgo.unique_id))
			{
				last_crafts[craftery_wgo.unique_id] = craft_id;
			}
			else
			{
				last_crafts.Add(craftery_wgo.unique_id, craft_id);
			}
		}
	}

	protected bool IsBuildMode()
	{
		if (crafts_inventory != null)
		{
			return crafts_inventory.is_building;
		}
		return false;
	}

	public override void OnClosePressed()
	{
		if (craftery_wgo != null && last_crafts.ContainsKey(craftery_wgo.unique_id))
		{
			last_crafts.Remove(craftery_wgo.unique_id);
		}
		base.OnClosePressed();
		if (IsBuildMode())
		{
			MainGame.me.build_mode_logics.SetCurrentBuildZone(string.Empty);
			MainGame.me.player.components.interaction.RedrawCurrentInteractiveHint();
		}
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
	}

	public ButtonTipsStr GetButtonTips()
	{
		return base.button_tips;
	}

	public GamepadNavigationController GetGamepadController()
	{
		return base.gamepad_controller;
	}

	public WorldGameObject GetCrafteryWGO()
	{
		return craftery_wgo;
	}
}
