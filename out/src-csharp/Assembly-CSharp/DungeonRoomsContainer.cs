using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

public class DungeonRoomsContainer : ScriptableObject
{
	public const string ASSET_NAME = "Dungeon/Rooms/DungeonRoomsContainer";

	public List<SerializedRoom> rooms_list;

	private Dictionary<DungeonRoomInterior.BiomType, Dictionary<string, Dictionary<DungeonRoomInterior.RoomSize, List<string>>>> _rooms_dictionary;

	private static DungeonRoomsContainer _me;

	public static DungeonRoomsContainer me
	{
		get
		{
			if (_me == null)
			{
				_me = Resources.Load<DungeonRoomsContainer>("Dungeon/Rooms/DungeonRoomsContainer");
			}
			return _me;
		}
	}

	public void FillRoomsDictionary()
	{
		_rooms_dictionary = new Dictionary<DungeonRoomInterior.BiomType, Dictionary<string, Dictionary<DungeonRoomInterior.RoomSize, List<string>>>>();
		foreach (SerializedRoom item in rooms_list)
		{
			Dictionary<string, Dictionary<DungeonRoomInterior.RoomSize, List<string>>> value = null;
			if (!_rooms_dictionary.TryGetValue(item.biom_type, out value))
			{
				value = new Dictionary<string, Dictionary<DungeonRoomInterior.RoomSize, List<string>>>();
				_rooms_dictionary.Add(item.biom_type, value);
			}
			Dictionary<DungeonRoomInterior.RoomSize, List<string>> value2 = null;
			if (!value.TryGetValue(item.room_type, out value2))
			{
				value2 = new Dictionary<DungeonRoomInterior.RoomSize, List<string>>();
				value.Add(item.room_type, value2);
			}
			List<string> value3 = null;
			if (!value2.TryGetValue(item.room_size, out value3))
			{
				value3 = new List<string>();
				value2.Add(item.room_size, value3);
			}
			if (value3.Contains(item.preset_name))
			{
				Debug.LogError("Found dublicate room name: " + item.preset_name);
			}
			else
			{
				value3.Add(item.preset_name);
			}
		}
		Debug.Log("Filled Rooms Dictionary.");
	}

	public static void FillRoomsDict()
	{
		DungeonRoomsContainer dungeonRoomsContainer = me;
		if (dungeonRoomsContainer == null)
		{
			Debug.LogError("Can not fill rooms dictionary: Container is null!!!");
		}
		else
		{
			dungeonRoomsContainer.FillRoomsDictionary();
		}
	}

	public bool NeedFillDict()
	{
		return _rooms_dictionary == null;
	}

	public static bool NeedFillDictionary()
	{
		DungeonRoomsContainer dungeonRoomsContainer = me;
		if (dungeonRoomsContainer == null)
		{
			Debug.LogError("Can not fill rooms dictionary: Container is null!!!");
			return true;
		}
		return dungeonRoomsContainer.NeedFillDict();
	}

	public List<SerializedRoom> GetRooms(DungeonRoomInterior.BiomType t_biom_type, string t_room_type, DungeonRoomInterior.RoomSize t_room_size)
	{
		if (t_biom_type == DungeonRoomInterior.BiomType.Unknown)
		{
			return null;
		}
		List<SerializedRoom> list = new List<SerializedRoom>();
		if (!_rooms_dictionary.TryGetValue(t_biom_type, out var value))
		{
			return null;
		}
		if (string.IsNullOrEmpty(t_room_type))
		{
			foreach (KeyValuePair<string, Dictionary<DungeonRoomInterior.RoomSize, List<string>>> item in value)
			{
				foreach (KeyValuePair<DungeonRoomInterior.RoomSize, List<string>> item2 in item.Value)
				{
					foreach (string item3 in item2.Value)
					{
						list.Add(new SerializedRoom
						{
							biom_type = t_biom_type,
							room_type = item.Key,
							room_size = item2.Key,
							preset_name = item3
						});
					}
				}
			}
			return list;
		}
		if (!value.TryGetValue(t_room_type, out var value2))
		{
			return null;
		}
		if (t_room_size == DungeonRoomInterior.RoomSize.Unknown)
		{
			foreach (KeyValuePair<DungeonRoomInterior.RoomSize, List<string>> item4 in value2)
			{
				foreach (string item5 in item4.Value)
				{
					list.Add(new SerializedRoom
					{
						biom_type = t_biom_type,
						room_type = t_room_type,
						room_size = item4.Key,
						preset_name = item5
					});
				}
			}
			return list;
		}
		if (!value2.TryGetValue(t_room_size, out var value3))
		{
			return null;
		}
		foreach (string item6 in value3)
		{
			list.Add(new SerializedRoom
			{
				biom_type = t_biom_type,
				room_type = t_room_type,
				room_size = t_room_size,
				preset_name = item6
			});
		}
		return list;
	}

	public SerializedRoom GetRandomRoomName(DungeonRoomInterior.BiomType t_biom_type, string t_room_type, DungeonRoomInterior.RoomSize t_room_size)
	{
		if (t_biom_type == DungeonRoomInterior.BiomType.Unknown)
		{
			return GetAbsolutelyRandomRoomName();
		}
		List<SerializedRoom> rooms = GetRooms(t_biom_type, t_room_type, t_room_size);
		if (rooms == null || rooms.Count == 0)
		{
			return null;
		}
		int index = Dungeon.RandomRange(0, rooms.Count);
		return rooms[index];
	}

	public SerializedRoom GetRandomRoomName(DungeonRoomInterior.BiomType t_biom_type, string t_room_type)
	{
		return GetRandomRoomName(t_biom_type, t_room_type, DungeonRoomInterior.RoomSize.Unknown);
	}

	public SerializedRoom GetRandomRoomName(DungeonRoomInterior.BiomType t_biom_type)
	{
		return GetRandomRoomName(t_biom_type, null, DungeonRoomInterior.RoomSize.Unknown);
	}

	public SerializedRoom GetAbsolutelyRandomRoomName()
	{
		int index = Dungeon.RandomRange(0, rooms_list.Count);
		return rooms_list[index];
	}
}
