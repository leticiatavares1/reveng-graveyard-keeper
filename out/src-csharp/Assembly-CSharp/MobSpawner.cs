using System;
using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

[Serializable]
public class MobSpawner : WorldSimpleObject
{
	[Serializable]
	public struct SerializableSpawner
	{
		public long unique_id;

		public string custom_tag;

		public string spawner_id;

		public float chance_to_spawn;

		public List<long> mobs;

		public IntVector2 coords;
	}

	public string spawner_id = string.Empty;

	public float chance_to_spawn = 1f;

	public List<WorldGameObject> spawned_mobs = new List<WorldGameObject>();

	public string custom_tag = "";

	private bool _delayed_mobs_list_deserialize;

	private List<long> _delayed_list;

	public void ActivateSpawner(int dungeon_level, List<SavedDungeonObject> saved_objs)
	{
		bool flag = false;
		if (saved_objs != null && saved_objs.Count > 0)
		{
			Vector2 vector = base.transform.localPosition;
			foreach (SavedDungeonObject saved_obj in saved_objs)
			{
				if (saved_obj.type != 0)
				{
					continue;
				}
				Vector2 vector2 = saved_obj.local_position - vector;
				if ((double)Mathf.Abs(vector2.x) < 0.1 && (double)Mathf.Abs(vector2.y) < 0.1)
				{
					if (saved_obj.mob_is_alive)
					{
						flag = true;
					}
					Debug.Log("#dgen# Mob Spawner " + base.name + "[" + spawner_id + "] is " + (flag ? "" : "NOT ") + "activated because of saved_objs");
					break;
				}
			}
		}
		else
		{
			float value = UnityEngine.Random.value;
			if (value <= chance_to_spawn)
			{
				Debug.Log("[" + value + "<=" + chance_to_spawn + "]");
				flag = true;
			}
			else
			{
				Debug.Log("[" + value + ">" + chance_to_spawn + "]");
			}
		}
		if (flag)
		{
			SpawnerDefinition spawnerDefinition = null;
			int num = 0;
			while (dungeon_level - num >= 1)
			{
				spawnerDefinition = GameBalance.me.GetDataOrNull<SpawnerDefinition>(spawner_id + "_" + (dungeon_level - num));
				num++;
				if (spawnerDefinition != null)
				{
					break;
				}
			}
			if (spawnerDefinition == null)
			{
				Debug.LogError("Can not find data for: [" + spawner_id + "_" + dungeon_level + "]");
			}
			else
			{
				SpawnerDefinition.MobDefinition mobToSpawn = spawnerDefinition.GetMobToSpawn();
				if (mobToSpawn == null)
				{
					Debug.Log("Mob to spawn is null!");
					return;
				}
				Debug.Log("Spawning mob: " + mobToSpawn.mob_name);
				SpawnMob(mobToSpawn);
			}
		}
		else
		{
			Debug.Log("NOT Spawning mob");
		}
	}

	public void ActivateSpawner()
	{
		SpawnerDefinition dataOrNull = GameBalance.me.GetDataOrNull<SpawnerDefinition>(spawner_id);
		if (dataOrNull == null)
		{
			Debug.LogError("Can not find data for: [" + spawner_id + "]");
			return;
		}
		SpawnerDefinition.MobDefinition mobToSpawn = dataOrNull.GetMobToSpawn();
		if (mobToSpawn == null)
		{
			Debug.Log("Mob to spawn is null!");
			return;
		}
		Debug.Log("Spawning mob: " + mobToSpawn.mob_name);
		SpawnMob(mobToSpawn);
	}

	private void SpawnMob(SpawnerDefinition.MobDefinition mob_to_spawn)
	{
		if (mob_to_spawn.mobs_count == 0)
		{
			return;
		}
		int num = 0;
		do
		{
			Vector2 vector = (Vector2)base.transform.localPosition + UnityEngine.Random.insideUnitCircle * 0.01f;
			WorldGameObject wgo = WorldMap.SpawnWGO(base.transform.parent, mob_to_spawn.mob_name);
			wgo.transform.localPosition = vector;
			BaseCharacterComponent character = wgo.components.character;
			character.spawner_coords = base.transform.localPosition;
			character.spawner = this;
			WorldMap.OnUsedSpawner(this);
			spawned_mobs.Add(wgo);
			if (!string.IsNullOrEmpty(mob_to_spawn.craft_name))
			{
				GJTimer.AddTimer(0.01f, delegate
				{
					wgo.TryStartCraft(mob_to_spawn.craft_name);
				});
			}
			num++;
		}
		while (num < mob_to_spawn.mobs_count);
	}

	public SerializableSpawner ToSerializable()
	{
		if (unique_id == -1)
		{
			unique_id = UniqueID.GetUniqueID();
		}
		List<long> list = new List<long>();
		foreach (WorldGameObject spawned_mob in spawned_mobs)
		{
			if (spawned_mob.unique_id == -1)
			{
				Debug.LogError("Mob doesn't have a unique ID (has to be created before spawner serialize)", spawned_mob);
			}
			else
			{
				list.Add(spawned_mob.unique_id);
			}
		}
		SerializableSpawner result = default(SerializableSpawner);
		result.chance_to_spawn = chance_to_spawn;
		result.unique_id = unique_id;
		result.custom_tag = custom_tag;
		result.spawner_id = spawner_id;
		result.mobs = list;
		result.coords = new IntVector2(base.transform.position);
		return result;
	}

	public void FromSerializable(SerializableSpawner data)
	{
		chance_to_spawn = data.chance_to_spawn;
		unique_id = data.unique_id;
		custom_tag = data.custom_tag;
		spawner_id = data.spawner_id;
		spawned_mobs.Clear();
		_delayed_mobs_list_deserialize = true;
		_delayed_list = data.mobs;
	}

	public void DelayedDeserialize()
	{
		if (!_delayed_mobs_list_deserialize)
		{
			return;
		}
		spawned_mobs.Clear();
		foreach (long item in _delayed_list)
		{
			WorldGameObject worldGameObjectByUniqueId = WorldMap.GetWorldGameObjectByUniqueId(item);
			if (worldGameObjectByUniqueId != null)
			{
				spawned_mobs.Add(worldGameObjectByUniqueId);
			}
		}
		_delayed_mobs_list_deserialize = false;
		_delayed_list = null;
	}
}
