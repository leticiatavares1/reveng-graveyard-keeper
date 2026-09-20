using System;
using System.Collections.Generic;

[Serializable]
public class ModelsBySubgroup
{
	public string subgroupName;

	public List<string> modelsNames = new List<string>();

	public List<string> availableGroups = new List<string>();

	public ModelsBySubgroup(string subgroupName, List<string> modelsNames)
	{
		this.subgroupName = subgroupName;
		this.modelsNames = modelsNames;
	}

	public void InitAvailableGroups(ref List<string> allAvailableGroups)
	{
		for (int i = 0; i < modelsNames.Count; i++)
		{
			string[] array = modelsNames[i].Split('-');
			if (!availableGroups.Contains(array[1]))
			{
				availableGroups.Add(array[1]);
			}
			allAvailableGroups.Remove(array[1]);
		}
	}
}
