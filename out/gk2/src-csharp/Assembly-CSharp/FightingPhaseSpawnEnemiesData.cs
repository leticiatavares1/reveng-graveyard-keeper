using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class FightingPhaseSpawnEnemiesData : FightingPhaseData
{
	public int spitSpawnSize = -1;

	public bool mixEnemiesForSpawn;

	public List<EnemyData> enemies = new List<EnemyData>();

	[NonSerialized]
	private Dictionary<string, int> spawnedEnemies = new Dictionary<string, int>();

	[NonSerialized]
	private int totalSpawnedThisPhase;

	[NonSerialized]
	private int totalBurstSpawnsTriggered;

	public int TotalEnemiesToSpawn => enemies.Sum((EnemyData enemy) => enemy.count);

	public override void UpdatePhase(float progress, FightingLevelPresetProcessor processor, FightingLevelPreset.FightingLineData line, out int sentToSpawnThisTime)
	{
		sentToSpawnThisTime = 0;
		int num = 0;
		foreach (EnemyData enemy in enemies)
		{
			num += enemy.count;
		}
		if (num == 0)
		{
			return;
		}
		if (spitSpawnSize == -1)
		{
			if (totalSpawnedThisPhase != 0)
			{
				return;
			}
			if (mixEnemiesForSpawn)
			{
				List<string> list = new List<string>();
				foreach (EnemyData enemy2 in enemies)
				{
					int value;
					int num2 = enemy2.count - (spawnedEnemies.TryGetValue(enemy2.id, out value) ? value : 0);
					for (int i = 0; i < num2; i++)
					{
						list.Add(enemy2.id);
					}
				}
				for (int num3 = list.Count - 1; num3 > 0; num3--)
				{
					int num4 = UnityEngine.Random.Range(0, num3 + 1);
					List<string> list2 = list;
					int index = num3;
					List<string> list3 = list;
					int index2 = num4;
					string text = list[num4];
					string text2 = list[num3];
					string text4 = (list2[index] = text);
					text4 = (list3[index2] = text2);
				}
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (string item in list)
				{
					if (!dictionary.ContainsKey(item))
					{
						dictionary[item] = 0;
					}
					dictionary[item]++;
				}
				{
					foreach (KeyValuePair<string, int> item2 in dictionary)
					{
						if (item2.Value > 0)
						{
							processor.SpawnEnemies(item2.Key, item2.Value, line);
							sentToSpawnThisTime += item2.Value;
							spawnedEnemies[item2.Key] = (spawnedEnemies.TryGetValue(item2.Key, out var value2) ? value2 : 0) + item2.Value;
							totalSpawnedThisPhase += item2.Value;
						}
					}
					return;
				}
			}
			{
				foreach (EnemyData enemy3 in enemies)
				{
					int value3;
					int num5 = enemy3.count - (spawnedEnemies.TryGetValue(enemy3.id, out value3) ? value3 : 0);
					if (num5 > 0)
					{
						processor.SpawnEnemies(enemy3.id, num5, line);
						sentToSpawnThisTime += num5;
						spawnedEnemies[enemy3.id] = (spawnedEnemies.TryGetValue(enemy3.id, out var value4) ? value4 : 0) + num5;
						totalSpawnedThisPhase += num5;
					}
				}
				return;
			}
		}
		int num6 = Mathf.Max(1, spitSpawnSize);
		int num7 = Mathf.CeilToInt((float)num / (float)num6);
		int a = (Mathf.FloorToInt(Mathf.Clamp01(progress / Mathf.Max(1f, duration)) * (float)((num7 > 1) ? (num7 - 1) : 0)) + 1) * num6;
		a = Mathf.Min(a, num);
		int num8 = Mathf.Max(0, a - totalSpawnedThisPhase);
		if (num8 <= 0)
		{
			return;
		}
		int num9 = num8;
		if (mixEnemiesForSpawn)
		{
			while (num9 > 0)
			{
				List<string> list4 = new List<string>();
				foreach (EnemyData enemy4 in enemies)
				{
					if ((spawnedEnemies.TryGetValue(enemy4.id, out var value5) ? value5 : 0) < enemy4.count)
					{
						list4.Add(enemy4.id);
					}
				}
				if (list4.Count == 0)
				{
					break;
				}
				string text6 = list4[UnityEngine.Random.Range(0, list4.Count)];
				processor.SpawnEnemies(text6, 1, line);
				sentToSpawnThisTime++;
				spawnedEnemies[text6] = (spawnedEnemies.TryGetValue(text6, out var value6) ? value6 : 0) + 1;
				totalSpawnedThisPhase++;
				num9--;
			}
		}
		else
		{
			foreach (EnemyData enemy5 in enemies)
			{
				if (num9 <= 0)
				{
					break;
				}
				int value7;
				int num10 = (spawnedEnemies.TryGetValue(enemy5.id, out value7) ? value7 : 0);
				int num11 = enemy5.count - num10;
				if (num11 > 0)
				{
					int num12 = Mathf.Min(num11, num9);
					processor.SpawnEnemies(enemy5.id, num12, line);
					sentToSpawnThisTime += num12;
					spawnedEnemies[enemy5.id] = num10 + num12;
					totalSpawnedThisPhase += num12;
					num9 -= num12;
				}
			}
		}
		totalBurstSpawnsTriggered = totalSpawnedThisPhase / num6;
	}

	public void FlushRemaining(FightingLevelPresetProcessor processor, FightingLevelPreset.FightingLineData line, out int sentToSpawnThisTime)
	{
		sentToSpawnThisTime = 0;
		foreach (EnemyData enemy in enemies)
		{
			int value;
			int num = (spawnedEnemies.TryGetValue(enemy.id, out value) ? value : 0);
			int num2 = enemy.count - num;
			if (num2 > 0)
			{
				processor.SpawnEnemies(enemy.id, num2, line);
				sentToSpawnThisTime += num2;
				spawnedEnemies[enemy.id] = num + num2;
				totalSpawnedThisPhase += num2;
			}
		}
	}

	public void Reset()
	{
		spawnedEnemies.Clear();
		totalSpawnedThisPhase = 0;
		totalBurstSpawnsTriggered = 0;
	}
}
