using System;
using System.Collections.Generic;

public class UICraftWidgetData : UIBaseCraftWidgetData
{
	public UICraftWidgetData(WgoData wgoData, CraftDef craftDef, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onPress, Action onOver, Action onOut)
		: base(wgoData, craftDef, onPress, onOver, onOut)
	{
	}

	public UICraftWidgetData(WgoData wgoData, AlchemyMixDef mixDef, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onPress, Action onOver, Action onOut)
		: base(wgoData, mixDef, onPress, onOver, onOut)
	{
	}
}
