using System;
using UnityEngine;

public class PlayerHPActivity : PlayerActivity
{
	private HPComponent hpComponent;

	public static event Action<string> OnNotEnoughResOccurred;

	public PlayerHPActivity(PlayerData playerData, WgoData wgoData)
	{
		base.playerData = playerData;
		base.wgoData = wgoData;
		hpComponent = wgoData.HpComponent;
	}

	public override bool IsEnoughDurability(Item tool)
	{
		if (tool.TryGetProperty<DurabilitySerializedItemProperty>(out var property))
		{
			return property.Durability > tool.Definition.durDecreaseOnUse;
		}
		return true;
	}

	public override int GetActionDamage(Item tool)
	{
		bool flag = wgoData.GetGameResInt("seed_mastery_lock") > 0;
		int num = MainGame.PlayerController.GetMasteryLevelForTalentBranch(wgoData.Definition.talent);
		int num2 = (flag ? wgoData.GetGameResInt("seed_mastery_lock") : wgoData.Definition.MasteryLock);
		if (num <= num2 && flag)
		{
			return 1;
		}
		if (wgoData.Definition.noMasteryLock)
		{
			return 1;
		}
		if (num < num2)
		{
			return 0;
		}
		Debug.Log("HPActivity: Rolling HP Damage");
		if (num2 == 0)
		{
			num2 = 1;
			if (num == 0)
			{
				num = 1;
			}
		}
		return wgoData.Definition.playerHpActivityMod * Math.Clamp(num / num2, -ConstDef.Get("max_cells_per_one_hit").IntValue, ConstDef.Get("max_cells_per_one_hit").IntValue);
	}

	public override bool CanStartActivity()
	{
		int playerHpActivityMod = wgoData.Definition.playerHpActivityMod;
		if (hpComponent.Hp <= 0 || playerHpActivityMod <= 0)
		{
			if (hpComponent.Hp < hpComponent.MaxHpValue)
			{
				return playerHpActivityMod < 0;
			}
			return false;
		}
		return true;
	}

	public override void OnStartActivity()
	{
	}

	public override bool IsEnoughMastery()
	{
		if (wgoData.GetGameResInt("seed_mastery_lock") > 0)
		{
			return true;
		}
		return wgoData.Worker.GetMasteryLevelForTalentBranch(wgoData.Definition.talent) >= wgoData.Definition.MasteryLock;
	}

	public override bool CanUseTool(Item tool)
	{
		if ((hpComponent.isDeathDelayed || !CanStartActivity()) && !wgoData.Definition.reviveOnDie)
		{
			return false;
		}
		return true;
	}

	public override void UseTool(Item tool, int deltaTick)
	{
		bool hasFullHp = hpComponent.HasFullHp;
		int actionDamage = GetActionDamage(tool);
		hpComponent.ApplyDamage(actionDamage);
		wgoData.NotifyApplyTool(hasFullHp);
	}

	public override bool IsEnoughEnergy(Item tool, float energyPerTick)
	{
		bool num = PlayerEnergyGameResSystem.GetSystem().IsEnoughValue(energyPerTick);
		if (!num)
		{
			Action<string> onNotEnoughResOccurred = PlayerHPActivity.OnNotEnoughResOccurred;
			if (onNotEnoughResOccurred == null)
			{
				return num;
			}
			onNotEnoughResOccurred("energy");
		}
		return num;
	}

	public override float GetEnergyCostPerTick(Item tool)
	{
		return wgoData.Definition.energyPerTick.EvaluateFloat() - tool.Definition.GetGameResOnUse("energy");
	}

	public override bool CanChangeInsanity(Item tool, float insanityPerTick)
	{
		bool num = PlayerInsanityGameResSystem.GetSystem().CanChangeInsanity(insanityPerTick);
		if (!num)
		{
			Action<string> onNotEnoughResOccurred = PlayerHPActivity.OnNotEnoughResOccurred;
			if (onNotEnoughResOccurred == null)
			{
				return num;
			}
			onNotEnoughResOccurred("insanity");
		}
		return num;
	}

	public override void ConsumeEnergy(Item tool, float energyPerTick)
	{
		float value = 0f - energyPerTick;
		PlayerEnergyGameResSystem.GetSystem().Add(value);
	}

	public override void ChangeInsanity(Item tool, float insanityPerTick)
	{
		PlayerInsanityGameResSystem.GetSystem().Add(insanityPerTick);
	}

	public override float GetInsanityCostPerTick(Item tool)
	{
		return wgoData.Definition.insanityPerTick.EvaluateFloat() - tool.Definition.GetGameResOnUse("insanity");
	}

	public ItemType GetRequiredToolTypes()
	{
		return wgoData.Definition.toolAction.actionableTool;
	}
}
