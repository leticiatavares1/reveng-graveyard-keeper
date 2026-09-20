using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftElement : CraftElementT<CraftDef>
{
	[SerializeField]
	private Item durabilityUse;

	public CraftElement(string craftId, int count, CraftParamsData craftParamsData)
		: this(craftId, count, new List<NeedItemData>(), craftParamsData)
	{
	}

	public CraftElement(string craftId, int count, List<NeedItemData> requirements, CraftParamsData paramsData)
		: base(craftId, count, requirements, paramsData)
	{
	}

	public CraftElement(CraftDefBase definition)
		: base(definition)
	{
	}

	public CraftElement(CraftDefBase definition, CraftParamsData craftParamsData)
		: base(definition, craftParamsData)
	{
	}

	protected CraftElement(CraftElement other, int count = 1)
		: base((CraftElementT<CraftDef>)other, count)
	{
		durabilityUse = other.durabilityUse;
	}

	public override CraftElementBase Clone(int count = 1)
	{
		return new CraftElement(this, count);
	}

	public override CraftStatus CanStartCraft(ICraftable craftableObject, MultiInventory multiInventory = null)
	{
		CraftDef definition = base.Definition;
		Inventory inventory = craftableObject.CraftableAttachedWorker?.WorkerInventory;
		if (definition.hasDurabilityUseItem && inventory != null && !inventory.Data.HasItemWithEnoughDurability(definition.durabilityUseItem.Id, definition.needItemsDurabilityUse))
		{
			return CraftStatus.DoesntHaveItemWithEnoughDurability;
		}
		return base.CanStartCraft(craftableObject, multiInventory);
	}

	public override void Finish()
	{
		base.Finish();
		if (durabilityUse != null && durabilityUse.TryGetProperty<DurabilitySerializedItemProperty>(out var property))
		{
			property.Durability -= base.Definition.needItemsDurabilityUse;
		}
	}

	public override void RemoveCraftRequirements(ICraftable craftable)
	{
		base.RemoveCraftRequirements(craftable);
		Inventory inventory = craftable.CraftableAttachedWorker?.WorkerInventory;
		CraftDef definition = base.Definition;
		if (definition.hasDurabilityUseItem && inventory != null)
		{
			durabilityUse = inventory.Data.GetAndRemoveItemWithEnoughDurability(definition.durabilityUseItem.Id, definition.needItemsDurabilityUse);
		}
	}

	public override int GetCurrentQuality()
	{
		if (!base.Def.isStarCraft)
		{
			return -1;
		}
		if (base.Definition.goldLevel == 0 && base.Definition.silverLevel == 0 && base.Definition.bronzeLevel == 0)
		{
			return -1;
		}
		int result = 0;
		if (succeededProgressTicks >= base.Definition.goldLevel)
		{
			result = 3;
		}
		else if (succeededProgressTicks >= base.Definition.silverLevel)
		{
			result = 2;
		}
		else if (succeededProgressTicks >= base.Definition.bronzeLevel)
		{
			result = 1;
		}
		return result;
	}

	public override CraftStatus CheckWorkerDependentValues(IWorker worker, float deltaTime = 1f, bool skipEnergyCheck = false, bool skipInsanityCheck = false)
	{
		return worker?.CheckWorkerDependentValues(this, deltaTime, skipEnergyCheck, skipInsanityCheck) ?? MainGame.PlayerController.CheckWorkerDependentValues(this, deltaTime, skipEnergyCheck, skipInsanityCheck);
	}
}
