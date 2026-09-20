using System;
using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

[Serializable]
public class SavedDungeonsList
{
	[SerializeField]
	private List<SavedDungeon> _saved_dungeons = new List<SavedDungeon>();

	[SerializeField]
	private int _global_seed = -1;

	public int GetDungeonSeed(int dungeon_level)
	{
		int result = -1;
		if (dungeon_level < 1)
		{
			return result;
		}
		if (_global_seed == -1)
		{
			result = UnityEngine.Random.Range(0, 10000);
		}
		else
		{
			System.Random random = new System.Random(_global_seed);
			for (int i = 0; i < dungeon_level; i++)
			{
				result = random.Next(0, 100000);
			}
		}
		return result;
	}

	public SavedDungeon GetSavedDungeon(int dungeon_level)
	{
		if (_saved_dungeons.Count < dungeon_level)
		{
			for (int i = _saved_dungeons.Count; i < dungeon_level; i++)
			{
				_saved_dungeons.Add(new SavedDungeon());
			}
		}
		return _saved_dungeons[dungeon_level - 1];
	}

	public void SetGlobalSeed(int new_global_seed)
	{
		if (new_global_seed >= 0)
		{
			Debug.Log("Dungeon Generation: Changed global seed [" + _global_seed + " => " + new_global_seed + "]");
			_global_seed = new_global_seed;
		}
	}
}
