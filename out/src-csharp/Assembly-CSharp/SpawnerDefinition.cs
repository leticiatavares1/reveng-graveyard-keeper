using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnerDefinition : BalanceBaseObject
{
	[Serializable]
	public class MobDefinition
	{
		public string mob_name;

		public int weight;

		public int mobs_count;

		public string craft_name;
	}

	public string spawner_id;

	public int dungeon_level;

	public List<MobDefinition> mobs;

	public MobDefinition GetMobToSpawn()
	{
		if (mobs == null || mobs.Count == 0)
		{
			return null;
		}
		int num = 0;
		foreach (MobDefinition mob in mobs)
		{
			num += mob.weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		MobDefinition mobDefinition = null;
		foreach (MobDefinition mob2 in mobs)
		{
			num3 += (float)mob2.weight;
			if (num2 <= num3)
			{
				mobDefinition = mob2;
				break;
			}
		}
		if (mobDefinition == null)
		{
			Debug.LogError("OMG WHAT THE FUCK???!!!");
		}
		return mobDefinition;
	}
}
