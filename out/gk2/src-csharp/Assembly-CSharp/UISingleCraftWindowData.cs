using System;
using System.Collections.Generic;

public class UISingleCraftWindowData : UIBaseCraftSelectionWindowData
{
	protected override int MinCraftsCount
	{
		get
		{
			if (!base.CraftComponent.IsStarted)
			{
				return 0;
			}
			return 1;
		}
	}

	public UISingleCraftWindowData(WgoData wgoData, CraftDef craftDef, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onAddToQueue, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onStartCraft)
		: base(wgoData, craftDef, onAddToQueue, onStartCraft)
	{
	}
}
