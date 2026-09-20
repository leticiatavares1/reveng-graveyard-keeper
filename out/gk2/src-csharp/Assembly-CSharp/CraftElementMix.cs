using System;

[Serializable]
public class CraftElementMix : CraftElementT<AlchemyMixDef>
{
	public CraftElementMix(CraftDefBase definition)
		: base(definition)
	{
	}

	public CraftElementMix(CraftDefBase definition, CraftParamsData craftParamsData)
		: base(definition, craftParamsData)
	{
	}

	protected CraftElementMix(CraftElementMix other, int count = 1)
		: base((CraftElementT<AlchemyMixDef>)other, count)
	{
	}

	public override CraftElementBase Clone(int count = 1)
	{
		return new CraftElementMix(this, count);
	}

	public override void RemoveCraftRequirements(ICraftable craftable)
	{
	}
}
