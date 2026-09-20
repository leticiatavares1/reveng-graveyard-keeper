using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator;

[Serializable]
public class SavedDungeon
{
	[SerializeField]
	public bool is_empty = true;

	[SerializeField]
	public string dungeon_preset_name = "";

	[SerializeField]
	public int seed = -1;

	[SerializeField]
	public int random_calls_count;

	[SerializeField]
	public List<SavedDungeonObject> objects = new List<SavedDungeonObject>();

	[SerializeField]
	public bool is_completed;
}
