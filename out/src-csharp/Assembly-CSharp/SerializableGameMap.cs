using System;
using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

[Serializable]
public class SerializableGameMap
{
	[SerializeField]
	private List<SerializableWGO> _wgos = new List<SerializableWGO>();

	[SerializeField]
	private List<MobSpawner.SerializableSpawner> _spawners = new List<MobSpawner.SerializableSpawner>();

	[SerializeField]
	private List<SerializedTechPointDrop> _tech_drops = new List<SerializedTechPointDrop>();

	public List<SerializableWGO> wgos => wgos;

	public string ToJSON(bool at_game_start = false)
	{
		SaveSceneToMe(at_game_start);
		return JsonUtility.ToJson(this, prettyPrint: true);
	}

	public byte[] ToBinary(bool at_game_start = false)
	{
		SaveSceneToMe(at_game_start);
		byte[] array = SmartSerializer.Serialize(this);
		Debug.Log("SerializableGameMap length = " + array.Length);
		return array;
	}

	public void FromJSON(string json)
	{
		JsonUtility.FromJsonOverwrite(json, this);
	}

	public void FromBinary(byte[] data)
	{
		SmartSerializer.DeserializeInto(this, data);
	}

	public void SaveSceneToMe(bool at_game_start = false)
	{
		_wgos.Clear();
		WorldGameObject[] componentsInChildren = MainGame.me.world_root.gameObject.GetComponentsInChildren<WorldGameObject>(includeInactive: true);
		Debug.Log("SaveSceneToMe, count = " + componentsInChildren.Length);
		WorldGameObject[] array = componentsInChildren;
		foreach (WorldGameObject worldGameObject in array)
		{
			if (!worldGameObject.is_player && !(worldGameObject.name == "Player") && !(worldGameObject.obj_id == "0"))
			{
				if (at_game_start)
				{
					worldGameObject.SetParam("hp_inited", 0f);
				}
				_wgos.Add(SerializableWGO.FromWGO(worldGameObject));
			}
		}
		SimplifiedWGO[] componentsInChildren2 = MainGame.me.world_root.gameObject.GetComponentsInChildren<SimplifiedWGO>(includeInactive: true);
		foreach (SimplifiedWGO simplifiedWGO in componentsInChildren2)
		{
			_wgos.Add(simplifiedWGO.swgo);
		}
		_spawners.Clear();
		MobSpawner[] componentsInChildren3 = MainGame.me.world.GetComponentsInChildren<MobSpawner>(includeInactive: true);
		foreach (MobSpawner mobSpawner in componentsInChildren3)
		{
			_spawners.Add(mobSpawner.ToSerializable());
		}
		_tech_drops.Clear();
		foreach (TechPointDrop item in TechPointDrop.all)
		{
			SerializedTechPointDrop serializedTechPointDrop = new SerializedTechPointDrop();
			serializedTechPointDrop.FromTechPointDrop(item);
			_tech_drops.Add(serializedTechPointDrop);
		}
	}

	public void RestoreSceneToInitialState()
	{
		Debug.Log("RestoreSceneToInitialState");
		FromBinary(GameLoader.initial_map_bin);
		RestoreScene();
	}

	public void RestoreScene()
	{
		Debug.Log("RestoreScene, wgos count = " + _wgos.Count);
		ClearSceneMap();
		WorldMap.RescanGDPoints();
		foreach (SerializableWGO wgo in _wgos)
		{
			WorldGameObject.InstantiateWGOPrefab().RestoreFromSerializedObject(wgo);
		}
		MobSpawner[] componentsInChildren = MainGame.me.world.GetComponentsInChildren<MobSpawner>(includeInactive: true);
		foreach (MobSpawner mobSpawner in componentsInChildren)
		{
			IntVector2 v = new IntVector2(mobSpawner.transform.position);
			foreach (MobSpawner.SerializableSpawner spawner in _spawners)
			{
				if (spawner.coords.EqualsTo(v))
				{
					mobSpawner.FromSerializable(spawner);
					break;
				}
			}
		}
		ObjectDynamicShadow.InstantiateAllAdditionalShadows();
	}

	public void DeserializeTechPoints()
	{
		if (_tech_drops == null)
		{
			Debug.LogError("TechPoints list is null!");
			return;
		}
		Debug.Log($"Deserializing tech points, Count = {_tech_drops.Count}");
		foreach (SerializedTechPointDrop tech_drop in _tech_drops)
		{
			TechPointDrop.Spawn(GUIElements.me.tech_points_spawner.prefab, tech_drop.type).transform.position = tech_drop.pos;
		}
	}

	public void ClearSceneMap()
	{
		SaveSlotsMenuGUI.PrepareScene();
		WorldGameObject[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<WorldGameObject>(includeInactive: true);
		foreach (WorldGameObject worldGameObject in componentsInChildren)
		{
			if (!(worldGameObject.GetComponent<PlayerComponent>() != null))
			{
				if (worldGameObject.name.Contains("Player"))
				{
					Debug.LogError("Deleting player: " + worldGameObject.name);
				}
				UnityEngine.Object.Destroy(worldGameObject.gameObject);
			}
		}
		SimplifiedWGO[] componentsInChildren2 = MainGame.me.world_root.GetComponentsInChildren<SimplifiedWGO>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[i].gameObject);
		}
		ChunkManager.ClearChunksList();
		ChunkedGameObject[] componentsInChildren3 = MainGame.me.world_root.GetComponentsInChildren<ChunkedGameObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].ResetAtTheBeginning();
		}
	}
}
