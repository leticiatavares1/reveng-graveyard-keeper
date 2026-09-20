using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnConfiguration", menuName = "GK2/SpawnConfiguration")]
public class SpawnConfiguration : ScriptableObject
{
	[SerializeField]
	private List<SpawnStage> spawnStages = new List<SpawnStage>();

	public List<SpawnStage> SpawnStages => spawnStages;

	public int FindStageById(string id)
	{
		for (int i = 0; i < spawnStages.Count; i++)
		{
			SpawnStage spawnStage = spawnStages[i];
			for (int j = 0; j < spawnStage.variations.Count; j++)
			{
				if (spawnStages[i].variations[j].wgoPartId.StartsWith(id))
				{
					return i;
				}
			}
		}
		return -1;
	}
}
