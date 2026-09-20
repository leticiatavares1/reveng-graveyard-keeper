using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnWgoComponent : IComponent
{
	[SerializeField]
	private List<SpawnStage> spawnStages = new List<SpawnStage>();

	public List<SpawnStage> SpawnStages
	{
		get
		{
			return spawnStages;
		}
		set
		{
			spawnStages = value;
		}
	}

	public SpawnStage FindStageById(string id)
	{
		for (int i = 0; i < spawnStages.Count; i++)
		{
			SpawnStage spawnStage = spawnStages[i];
			for (int j = 0; j < spawnStage.variations.Count; j++)
			{
				if (spawnStages[i].variations[j].wgoPartId.StartsWith(id))
				{
					return spawnStage;
				}
			}
		}
		return null;
	}
}
