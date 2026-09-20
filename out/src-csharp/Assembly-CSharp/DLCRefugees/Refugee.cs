using System;
using UnityEngine;

namespace DLCRefugees;

[Serializable]
public class Refugee
{
	[NonSerialized]
	private WorldGameObject _world_game_object;

	[SerializeField]
	private long _unique_id;

	[SerializeField]
	private string _home_gd_point_tag;

	public WorldGameObject world_game_object => _world_game_object;

	public long unique_id => _unique_id;

	public string home_gd_point_tag
	{
		get
		{
			return _home_gd_point_tag;
		}
		set
		{
			_home_gd_point_tag = value;
		}
	}

	public Refugee()
	{
	}

	public Refugee(WorldGameObject world_game_object)
	{
		_world_game_object = world_game_object;
		_unique_id = world_game_object.unique_id;
	}

	public void Init()
	{
		_world_game_object = WorldMap.GetWorldGameObjectByUniqueId(_unique_id);
	}
}
