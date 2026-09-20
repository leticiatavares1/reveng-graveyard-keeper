using System;
using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

[Serializable]
public class SerializedWGO
{
	public enum WorldObjectType
	{
		Unknown,
		WGO,
		WSO,
		MobSpawner
	}

	public string id = "";

	public Vector2 coordinates = Vector2.zero;

	public int divider = 1;

	public float floor_line;

	public Vector3 local_scale = Vector3.zero;

	public WorldObjectType obj_type;

	public List<string> s_params = new List<string>();

	public List<float> f_params = new List<float>();

	public int variation;

	public int variation_2;

	public DungeonObjectChance dungeon_object_chance = new DungeonObjectChance();

	public Item data;
}
