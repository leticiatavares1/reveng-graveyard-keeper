using System;
using System.Collections.Generic;

[Serializable]
public abstract class CraftElementT<T> : CraftElementBase where T : CraftDefBase
{
	public T Definition => GameBalance.GetCraftDef<T>(craftId);

	public CraftElementT(string craftId, int count, List<NeedItemData> requirements, CraftParamsData paramsData)
		: base(craftId, count, requirements, paramsData)
	{
		base.craftId = craftId;
		base.count = count;
		base.requirements = requirements;
		base.paramsData = paramsData;
	}

	public CraftElementT(CraftDefBase definition)
	{
		craftId = definition.id;
		count = 1;
		requirements = definition.needItems;
		paramsData = null;
	}

	public CraftElementT(CraftDefBase definition, CraftParamsData paramsData)
	{
		craftId = definition.id;
		count = 1;
		requirements = definition.needItems;
		base.paramsData = paramsData;
	}

	protected CraftElementT(CraftElementT<T> other, int count = 1)
		: base(other, count)
	{
	}
}
