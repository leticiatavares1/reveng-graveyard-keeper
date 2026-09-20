using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class ZombieTalentData : ObjectLinkedToDefinition<TalentDef>
{
	public int curTalentValue;

	public List<string> studiedLevelUps = new List<string>();

	public ZombieTalentData(string talentId)
		: base(talentId)
	{
		curTalentValue = 0;
	}
}
