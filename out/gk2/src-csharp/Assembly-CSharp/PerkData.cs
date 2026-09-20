using System;
using LazyBearTechnology;

[Serializable]
public class PerkData : ObjectLinkedToDefinition<PerkDef>
{
	public float currentDuration;

	public float tickTimer;

	public PerkData()
	{
	}

	public PerkData(string id)
		: base(id)
	{
	}
}
