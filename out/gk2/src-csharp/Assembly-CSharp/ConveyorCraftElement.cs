using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConveyorCraftElement : CraftElement
{
	[SerializeField]
	private int outputCount;

	[SerializeField]
	private int currentOutputIndex;

	public override int TotalProgressTicks => Mathf.CeilToInt((float)totalProgressTicks / (float)outputCount);

	public override bool IsStarted
	{
		get
		{
			if (!isStarted)
			{
				return currentOutputIndex > 0;
			}
			return true;
		}
	}

	public override bool CaBeFinished => currentOutputIndex == outputCount - 1;

	public ConveyorCraftElement(string craftId, int count, CraftParamsData craftParamsData)
		: base(craftId, count, craftParamsData)
	{
	}

	public ConveyorCraftElement(string craftId, int count, List<NeedItemData> requirements, CraftParamsData paramsData)
		: base(craftId, count, requirements, paramsData)
	{
	}

	public ConveyorCraftElement(CraftDefBase definition)
		: base(definition)
	{
	}

	public ConveyorCraftElement(CraftDefBase definition, CraftParamsData craftParamsData)
		: base(definition, craftParamsData)
	{
	}

	protected ConveyorCraftElement(CraftElement other, int count = 1)
		: base(other, count)
	{
	}

	public override CraftElementBase Clone(int count = 1)
	{
		return new ConveyorCraftElement(this, count);
	}

	public override void Start(ICraftable craftable)
	{
		base.Start(craftable);
		outputCount = 0;
		foreach (ItemCount preOutputItem in preOutputItems)
		{
			outputCount += preOutputItem.count;
		}
	}

	public override void Update(int deltaTicks)
	{
		currentProgressTicks += deltaTicks;
		succeededProgressTicks += deltaTicks;
		craftable.OnSuccessfulTicksChange(succeededProgressTicks - deltaTicks, succeededProgressTicks, this);
		NotifyProgressChanged();
	}

	public override CraftStatus CanFinishCraft(ICraftable craftableObject)
	{
		if (!craftableObject.CraftableObjectCraftInventory.CanAddItemsToInventory(preOutputItems))
		{
			return CraftStatus.NotEnoughSpaceInWgo;
		}
		return CraftStatus.OK;
	}

	public override void UpdateCountOnFinish()
	{
		currentOutputIndex++;
		if (currentOutputIndex >= outputCount)
		{
			base.Count--;
			currentOutputIndex = 0;
		}
	}

	public override List<Item> MakeOutput()
	{
		List<Item> list = OutputItems.MakeOutput(preOutputItems);
		List<Item> list2 = new List<Item>();
		int num = 0;
		foreach (Item item in list)
		{
			num += item.Count;
			if (num > currentOutputIndex)
			{
				list2.Add(item.Split(1));
				break;
			}
		}
		return list2;
	}

	public override CraftStatus CanStartCraft(ICraftable craftableObject, MultiInventory multiInventory = null)
	{
		if (currentOutputIndex > 0)
		{
			return CraftStatus.OK;
		}
		if (multiInventory == null)
		{
			multiInventory = craftableObject.GetCraftableMultiInventory();
		}
		if (requirements.Count > 0 && !multiInventory.HasItemsById(requirements, craftableObject as WgoData))
		{
			return CraftStatus.NotEnoughResources;
		}
		if (base.Def.needItemsFromWgo.Count > 0 && !craftableObject.CraftableObjectInventory.Data.HasItemsWithIds(base.Def.needItemsFromWgo))
		{
			return CraftStatus.NotEnoughResources;
		}
		if (base.Def.addItemsToWgoOnStart.HasOutputItems && !craftableObject.CraftableObjectInventory.CanAddItemsToInventory(preToWgoOnStartItems))
		{
			return CraftStatus.NotEnoughSpaceInWgo;
		}
		return CraftStatus.OK;
	}

	public override void Finish()
	{
		base.Finish();
		currentProgressTicks = 0;
		succeededProgressTicks = 0;
		failedProgressTicks = 0;
	}
}
