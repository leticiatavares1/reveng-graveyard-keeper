using System.Collections.Generic;

public class CraftElementSermon : CraftElementT<SermonDef>
{
	public CraftElementSermon(string craftId, int count, List<NeedItemData> requirements, CraftParamsData paramsData)
		: base(craftId, count, requirements, paramsData)
	{
	}

	public CraftElementSermon(CraftDefBase definition)
		: base(definition)
	{
	}

	protected CraftElementSermon(CraftElementSermon other, int count = 1)
		: base((CraftElementT<SermonDef>)other, count)
	{
	}

	public override CraftElementBase Clone(int count = 1)
	{
		return new CraftElementSermon(this, count);
	}

	protected override CraftDefBase GetCraftDef()
	{
		return GameBalance.GetSermonDef(craftId);
	}
}
