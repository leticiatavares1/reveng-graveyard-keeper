using System;
using System.Collections.Generic;

public class UICraftSelectionWindowData : UIBaseCraftSelectionWindowData
{
	public bool IsGravePartRemove { get; set; }

	public UICraftSelectionWindowData(WgoData wgoData, CraftDef craftDef, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onAddToQueue, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onStartCraft)
		: base(wgoData, craftDef, onAddToQueue, onStartCraft)
	{
	}
}
