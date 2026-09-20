using System;
using System.Collections.Generic;

[Serializable]
public class InspirationLevelData
{
	public string id;

	public string talentId;

	public List<InspirationDef> levels = new List<InspirationDef>();

	public List<string> inspirationLocks = new List<string>();

	public List<string> techLocks = new List<string>();

	public List<string> questLocks = new List<string>();

	public InspirationDef GetDataForLevel(int level)
	{
		int num = level - 1;
		if (num < levels.Count)
		{
			return levels[num];
		}
		return null;
	}
}
