using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class CraftDefBase : BalanceBaseObject
{
	[AutoParse("crafts_in")]
	public List<string> craftsIn = new List<string>();

	[AutoParse("need_extension")]
	public string extensionNeedId;

	[AutoParse("is_auto")]
	public bool isAuto;

	[AutoParse("is_hidden")]
	public bool isHidden;

	[AutoParse("need_item")]
	public List<NeedItemData> needItems = new List<NeedItemData>();

	[AutoParse("needs_from_wgo")]
	public List<NeedItemData> needItemsFromWgo = new List<NeedItemData>();

	public bool haveStarCraftOutput;

	public bool isStarCraft;

	public bool isFuelCraft;

	public bool isAutopsyCraft;

	public bool isPocketExtractCraft;

	[SerializeField]
	private string fuelItemDefId;

	[AutoParse("duration")]
	public LazyExpression duration = new LazyExpression();

	[AutoParse("energy")]
	public LazyExpression energyPerTick = new LazyExpression();

	[AutoParse("insanity")]
	public LazyExpression insanityPerTick = new LazyExpression();

	[AutoParse("insanity_lock")]
	public LazyExpression insanityLock = new LazyExpression();

	[AutoParse("difficulty")]
	public int talentLock = 1;

	[AutoParse("is_auto_finish")]
	public bool isAutoFinish;

	[AutoParse("linked_perks")]
	public List<string> linkedPerks = new List<string>();

	[AutoParse("out_item")]
	public OutputItems outputItems = new OutputItems();

	[AutoParse("set_out_param_on_start_to_wgo")]
	public GameRes setWgoParamsOnStart = new GameRes();

	[AutoParse("set_out_param_on_finish_to_wgo")]
	public GameRes setWgoParamsOnFinish = new GameRes();

	[AutoParse("add_out_param_on_start_to_wgo")]
	public GameRes addWgoParamsOnStart = new GameRes();

	[AutoParse("add_out_param_on_finish_to_wgo")]
	public GameRes addWgoParamsOnFinish = new GameRes();

	[AutoParse("add_items_to_wgo_on_start")]
	public OutputItems addItemsToWgoOnStart = new OutputItems();

	[AutoParse("add_items_to_wgo_on_finish")]
	public OutputItems addItemsToWgoOnFinish = new OutputItems();

	public bool autoFinishAutoCraft;

	public ItemDef FuelItemDef => GameBalance.Me.GetData<ItemDef>(fuelItemDefId);

	public virtual AutopsyTypeCraft AutopsyType => AutopsyTypeCraft.None;

	public virtual OutputPreview GetOutputPreview(WgoData wgoData = null)
	{
		return outputItems.GetOutputPreview(id, wgoData);
	}

	public void SetIsFuelRelated()
	{
		isFuelCraft = ContainsFuelItem(addItemsToWgoOnFinish.chanceOutputItems, out var fuelItemDef);
		if (!isFuelCraft)
		{
			isFuelCraft = ContainsFuelItem(addItemsToWgoOnFinish.groupChanceOutputItems, out fuelItemDef);
		}
		if (!isFuelCraft)
		{
			return;
		}
		fuelItemDefId = fuelItemDef.id;
		foreach (string item in craftsIn)
		{
			WGODef data = GameBalance.Me.GetData<WGODef>(item);
			if (data != null)
			{
				data.isFuelContainer = true;
				continue;
			}
			Debug.LogError("CraftDef [" + id + "]. Can not find craft's in wgo [" + item + "].");
		}
	}

	protected bool ContainsFuelItem(List<ChanceOutputItem> chanceOutputItems, out ItemDef fuelItemDef)
	{
		fuelItemDef = null;
		foreach (ChanceOutputItem chanceOutputItem in chanceOutputItems)
		{
			ItemDef data = GameBalance.Me.GetData<ItemDef>(chanceOutputItem.id);
			if (data.isFuel)
			{
				fuelItemDef = data;
				return true;
			}
		}
		return false;
	}

	protected bool ContainsFuelItem(List<GroupChanceOutputItem> groupChanceOutputItems, out ItemDef fuelItemDef)
	{
		fuelItemDef = null;
		for (int i = 0; i < groupChanceOutputItems.Count; i++)
		{
			if (ContainsFuelItem(groupChanceOutputItems[i].chanceItems, out fuelItemDef))
			{
				return true;
			}
		}
		return false;
	}
}
