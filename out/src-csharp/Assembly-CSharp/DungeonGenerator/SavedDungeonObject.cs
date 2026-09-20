using System;
using UnityEngine;

namespace DungeonGenerator;

[Serializable]
public class SavedDungeonObject
{
	public enum SavedDungeonObjectType
	{
		Unknown = -1,
		Mob,
		WGO
	}

	[SerializeField]
	public string name = "";

	[SerializeField]
	public Vector2 local_position = new Vector2(-1f, -1f);

	[SerializeField]
	public SavedDungeonObjectType type = SavedDungeonObjectType.Unknown;

	[SerializeField]
	private bool _mob_is_alive = true;

	public bool mob_is_alive
	{
		get
		{
			if (type == SavedDungeonObjectType.Mob)
			{
				return _mob_is_alive;
			}
			Debug.LogError(me + "Only Mob can be alive!");
			return false;
		}
		set
		{
			if (type == SavedDungeonObjectType.Mob)
			{
				_mob_is_alive = value;
			}
			else
			{
				Debug.LogError(me + "Only Mob can be alive!");
			}
		}
	}

	public string me
	{
		get
		{
			Vector2 vector = local_position;
			return " [" + vector.ToString() + "] " + name;
		}
	}
}
