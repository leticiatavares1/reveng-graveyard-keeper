using System;
using UnityEngine;

public class HPActionComponent : WorldGameObjectComponent
{
	private bool _hp_was_positive;

	public override void StartComponent()
	{
		base.StartComponent();
		InitComponent();
	}

	public override bool DoAction(WorldGameObject player_wgo, float delta_time, bool for_gratitude_points = false)
	{
		Item equippedTool = player_wgo.GetEquippedTool();
		if (equippedTool == null)
		{
			return false;
		}
		if (!base.wgo.obj_def.tool_actions.GetToolK(equippedTool.definition.type, out var k))
		{
			return false;
		}
		k *= equippedTool.definition.efficiency;
		CraftComponent craft = base.components.craft;
		if (craft.enabled && craft.is_crafting && !craft.current_craft.is_auto)
		{
			return false;
		}
		if (!CanSpendPlayerEnergy(player_wgo, delta_time, really_spend_energy: true))
		{
			return false;
		}
		base.wgo.OnWorkAction();
		if (equippedTool.definition.durability_decrease_on_use)
		{
			float num = equippedTool.definition.durability_decrease_on_use_speed * delta_time;
			Debug.Log("Dur dec = " + num);
			equippedTool.durability -= num;
			if (equippedTool.durability_state == Item.DurabilityState.Broken)
			{
				MainGame.me.OnEquippedToolBroken(equippedTool);
				MainGame.me.player.components.interaction.UpdateNearestHint();
				MainGame.me.player.components.tool.TryStop();
				GUIElements.me.dialog.OpenOK(GJL.L("tool_broken_txt", equippedTool.GetItemName()));
				Sounds.PlaySound("break");
				return false;
			}
		}
		base.wgo.hp -= k * delta_time;
		if (base.wgo.hp <= 0f)
		{
			return _hp_was_positive;
		}
		return false;
	}

	public bool CanSpendPlayerEnergy(WorldGameObject player_wgo, float delta_time, bool really_spend_energy = false)
	{
		Item equippedTool = player_wgo.GetEquippedTool();
		if (equippedTool == null)
		{
			return false;
		}
		GameRes gameRes = new GameRes(equippedTool.definition.params_on_use);
		if (gameRes.IsEmpty())
		{
			return true;
		}
		float num = gameRes.Get("energy");
		if (num >= 0f)
		{
			return true;
		}
		if (equippedTool.definition.type == ItemDefinition.ItemType.Hand)
		{
			num *= delta_time;
		}
		if (equippedTool.definition.tool_energy_k != null && equippedTool.definition.tool_energy_k.has_expression)
		{
			num *= equippedTool.definition.tool_energy_k.EvaluateFloat(base.wgo, player_wgo);
		}
		if (player_wgo.energy < 0f - num)
		{
			Debug.Log("player energy = " + player_wgo.energy + ", need = " + num);
			return false;
		}
		if (really_spend_energy)
		{
			if (!player_wgo.components.character.player.TrySpendEnergy(0f - num))
			{
				Debug.LogError("Impossable error. Energy check was OK, but TrySpendEnergy returned false");
				return false;
			}
			gameRes.Set("energy", 0f);
			if (!gameRes.IsEmpty())
			{
				player_wgo.AddToParams(gameRes);
				EffectBubblesManager.ShowStacked(player_wgo, gameRes);
			}
		}
		return true;
	}

	public void DecHP(float value)
	{
		if (!base.wgo.is_player)
		{
			value *= base.wgo.obj_def.damage_factor;
		}
		if (value > 0f)
		{
			if (base.wgo.is_player)
			{
				Item equippedItem = base.wgo.GetEquippedItem(ItemDefinition.EquipmentType.HeadArmor);
				if (equippedItem != null)
				{
					value -= equippedItem.definition.armor;
				}
				Item equippedItem2 = base.wgo.GetEquippedItem(ItemDefinition.EquipmentType.BodyArmor);
				if (equippedItem2 != null)
				{
					value -= equippedItem2.definition.armor;
				}
				value -= base.wgo.GetParam("add_armor");
			}
			else
			{
				value -= base.wgo.obj_def.armor;
			}
		}
		if (value < 0f)
		{
			value = 0f;
		}
		if (base.wgo.is_player && base.wgo.IsPlayerInvulnerable())
		{
			value = 0f;
		}
		base.wgo.hp -= value;
		if (value > 0f)
		{
			EffectBubblesManager.ShowStackedHP(base.wgo, 0f - value);
		}
		if (base.wgo.obj_def.IsCharacter() && base.wgo.hp > 0f)
		{
			EnemiesHPBarsManager.me.AddIfNeeded(base.wgo);
		}
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		if (base.wgo.hp > 0f)
		{
			_hp_was_positive = true;
		}
		else if (_hp_was_positive)
		{
			base.wgo.hp = 0f;
			base.wgo.DoPreZeroHPActivity();
			_hp_was_positive = false;
		}
	}

	public bool HasHPInDefinition()
	{
		try
		{
			return base.wgo.obj_def.hp.EvaluateFloat(base.wgo) > 0f;
		}
		catch (NullReferenceException)
		{
			Debug.LogError("Probably obj_def is null for object id = " + base.wgo.obj_id, base.wgo);
			return false;
		}
	}

	protected float GetMaxHP()
	{
		return base.wgo.obj_def.hp.EvaluateFloat(base.wgo);
	}

	public float GetHPProgress()
	{
		float num = base.wgo.hp;
		float maxHP = GetMaxHP();
		if (num < 0f)
		{
			num = 0f;
		}
		if (!maxHP.EqualsTo(num))
		{
			return (maxHP - num) / maxHP;
		}
		return -1f;
	}

	public override void InitComponent()
	{
		if (base.wgo == null || base.wgo.obj_def == null)
		{
			return;
		}
		if (!base.wgo.is_player && base.wgo.GetParamInt("hp_inited") == 0)
		{
			base.wgo.hp = base.wgo.obj_def.hp.EvaluateFloat(base.wgo);
			for (int i = 0; i < 10; i++)
			{
				if (!(Mathf.Abs(base.wgo.obj_def.Damage(i)) < 0.01f))
				{
					base.wgo.SetParam("damage" + ((i == 0) ? "" : ("_" + i)), base.wgo.obj_def.Damage(i));
				}
			}
			if (Application.isPlaying)
			{
				base.wgo.SetParam("hp_inited", 1f);
			}
		}
		_hp_was_positive = base.wgo.hp > 0f;
	}

	protected override int GetExecutionOrder()
	{
		return 11;
	}

	public override void RefreshComponentBubbleData(bool show_interaction_buttons)
	{
		if (base.components.craft.enabled && ((base.components.craft.is_crafting && (base.components.craft.current_craft == null || !base.components.craft.current_craft.hidden)) || !base.components.craft.IsCraftQueueEmpty()))
		{
			return;
		}
		BubbleWidgetProgressData wdata = null;
		if (HasHPInDefinition() && show_interaction_buttons)
		{
			wdata = new BubbleWidgetProgressData(base.components.hp.GetHPProgress);
		}
		base.wgo.SetBubbleWidgetData(wdata, BubbleWidgetData.WidgetID.HPProgress);
		if (!show_interaction_buttons || base.wgo.obj_def.tool_actions.no_actions || !base.wgo.CanProcessWork())
		{
			return;
		}
		string text = "";
		ItemDefinition.ItemType itemType = base.wgo.obj_def.tool_actions.action_tools[0];
		if (itemType != ItemDefinition.ItemType.Sword)
		{
			Item equippedTool = MainGame.me.player.GetEquippedTool(itemType);
			text = "(" + itemType.ToString().ToLower() + ")";
			if (MainGame.me.save.IsWorkAvailible(base.wgo.obj_def))
			{
				if (equippedTool == null)
				{
					text = "(not_equipped)" + text;
				}
			}
			else
			{
				text = "(tech_locked)";
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = GameKeyTip.Get(GameKey.Work, text);
			MainGame.me.player.components.character.wgo_hilighted_for_work = base.wgo;
			base.wgo.SetBubbleWidgetData(text, BubbleWidgetData.WidgetID.Work);
		}
	}
}
