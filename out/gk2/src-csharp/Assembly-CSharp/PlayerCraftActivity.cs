using System;
using UnityEngine;

public class PlayerCraftActivity : PlayerActivity
{
	private CraftComponent craftComponent;

	public CraftComponent CraftComponent => craftComponent;

	public static event Action<string> OnNotEnoughResOccurred;

	public PlayerCraftActivity(PlayerData playerData, WgoData wgoData)
	{
		base.playerData = playerData;
		base.wgoData = wgoData;
		craftComponent = wgoData.CraftComponent;
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
		if (craftComponent.CurrentCraftElement == null)
		{
			return 0;
		}
		int num = MainGame.PlayerController.GetMasteryLevelForTalentBranch(wgoData.Definition.talent, craftComponent.CurrentCraftElement.Def);
		int num2 = craftComponent.CurrentCraftElement.Def.talentLock;
		if (!craftComponent.CurrentCraftElement.Def.isStarCraft && !craftComponent.CurrentCraftElement.Def.isAutopsyCraft)
		{
			if (num < num2)
			{
				return 0;
			}
			if (num2 == 0)
			{
				num2 = 1;
				if (num == 0)
				{
					num = 1;
				}
			}
			return Math.Clamp(num / num2, 0, ConstDef.Get("max_cells_per_one_hit").IntValue);
		}
		float num3 = 100f / (float)num2;
		if (num < num2)
		{
			float num4 = num3 * (float)num;
			if (!((float)UnityEngine.Random.Range(1, 100) <= num4))
			{
				return 0;
			}
			return 1;
		}
		return Math.Clamp(num / num2, 0, ConstDef.Get("max_cells_per_one_hit").IntValue);
	}

	public override bool CanStartActivity()
	{
		if (!craftComponent.IsAutoCraftable)
		{
			if (craftComponent.CurrentCraftElement == null)
			{
				return craftComponent.IsQueueDelayed;
			}
			return true;
		}
		return false;
	}

	public override void OnStartActivity()
	{
		if (craftComponent.Status == CraftComponentStatus.ReadyToStartCraft || craftComponent.IsQueueDelayed)
		{
			craftComponent.TryContinueFromQueue();
		}
	}

	public override bool IsEnoughMastery()
	{
		if (wgoData.CraftComponent.CurrentCraftElement.Def.isStarCraft)
		{
			return true;
		}
		if (wgoData.CraftComponent.CurrentCraftElement.Def.isAutopsyCraft)
		{
			return true;
		}
		if (wgoData.CraftComponent.CurrentCraftElement.Def is SurveyDef)
		{
			return true;
		}
		if (wgoData.CraftComponent.CurrentCraftElement is CraftElementMix)
		{
			return true;
		}
		return wgoData.Worker.GetMasteryLevelForTalentBranch(wgoData.Definition.talent, wgoData.CraftComponent.CurrentCraftElement?.Def) >= ((wgoData.CraftComponent.CurrentCraftElement != null) ? wgoData.CraftComponent.CurrentCraftElement.Def.talentLock : wgoData.Definition.MasteryLock);
	}

	public override bool CanUseTool(Item tool)
	{
		if (craftComponent.CurrentCraftElement == null || craftComponent.IsQueueDelayed)
		{
			if (!craftComponent.IsQueueDelayed)
			{
				return false;
			}
			if (craftComponent.CraftElementsQueue.FindIndex((CraftElementBase x) => x.CraftStatus == CraftStatus.OK) == -1)
			{
				return false;
			}
		}
		else if (craftComponent.IsFinishDelayed)
		{
			return false;
		}
		return true;
	}

	public override void UseTool(Item tool, int deltaTick)
	{
		int actionDamage = GetActionDamage(tool);
		craftComponent.UpdateManual(actionDamage);
		wgoData.NotifyApplyTool(isFirstHit: false);
	}

	public override bool IsEnoughEnergy(Item tool, float energyPerTick)
	{
		if (!craftComponent.IsStarted && craftComponent.CraftElementsQueue.Find((CraftElementBase x) => x.CraftStatus == CraftStatus.OK) != null)
		{
			return true;
		}
		bool num = PlayerEnergyGameResSystem.GetSystem().IsEnoughValue(energyPerTick);
		if (!num)
		{
			Action<string> onNotEnoughResOccurred = PlayerCraftActivity.OnNotEnoughResOccurred;
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
		if (craftComponent.IsQueueDelayed || craftComponent.CurrentCraftElement?.Def == null)
		{
			return 0f;
		}
		return craftComponent.CurrentCraftElement.Def.energyPerTick.EvaluateFloat() + wgoData.Worker.GetPerksEnergyBonusValue(craftComponent.CurrentCraftElement.Def) - tool.Definition.GetGameResOnUse("energy");
	}

	public override bool CanChangeInsanity(Item tool, float insanityPerTick)
	{
		if (!craftComponent.IsStarted && craftComponent.CraftElementsQueue.Find((CraftElementBase x) => x.CraftStatus == CraftStatus.OK) != null)
		{
			return true;
		}
		bool num = PlayerInsanityGameResSystem.GetSystem().CanChangeInsanity(insanityPerTick);
		if (!num)
		{
			Action<string> onNotEnoughResOccurred = PlayerCraftActivity.OnNotEnoughResOccurred;
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
		if (craftComponent.IsQueueDelayed || craftComponent.CurrentCraftElement?.Def == null)
		{
			return 0f;
		}
		return craftComponent.CurrentCraftElement.Def.insanityPerTick.EvaluateFloat() + wgoData.Worker.GetPerksInsanityBonusValue(craftComponent.CurrentCraftElement.Def) - tool.Definition.GetGameResOnUse("insanity");
	}

	public ItemType GetRequiredToolType()
	{
		return wgoData.Definition.toolAction.actionableTool;
	}
}
