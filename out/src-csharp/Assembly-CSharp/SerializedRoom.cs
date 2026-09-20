using System;
using System.Collections.Generic;
using System.Text;
using DungeonGenerator;
using UnityEngine;

[Serializable]
public class SerializedRoom
{
	[SerializeField]
	public string room_type = "";

	[SerializeField]
	public DungeonRoomInterior.RoomSize room_size = DungeonRoomInterior.RoomSize.Unknown;

	[SerializeField]
	public DungeonRoomInterior.BiomType biom_type = DungeonRoomInterior.BiomType.Unknown;

	[SerializeField]
	public string preset_name = "";

	private static Dictionary<string, Dictionary<int, Dictionary<int, Dictionary<string, string>>>> _names_hash = new Dictionary<string, Dictionary<int, Dictionary<int, Dictionary<string, string>>>>();

	private static StringBuilder path_sb = new StringBuilder();

	public DungeonRoom GetRoom(List<DungeonRoom> blacklist)
	{
		DungeonRoom dungeonRoom = null;
		DungeonRoomsContainer me = DungeonRoomsContainer.me;
		if (me == null)
		{
			Debug.LogError("Rooms Container is null!");
			return null;
		}
		dungeonRoom = ((biom_type == DungeonRoomInterior.BiomType.Unknown) ? new DungeonRoom(DungeonRoomInterior.GetRoomInterior(me.GetAbsolutelyRandomRoomName())) : (string.IsNullOrEmpty(room_type) ? new DungeonRoom(DungeonRoomInterior.GetRoomInterior(me.GetRandomRoomName(biom_type))) : ((room_size == DungeonRoomInterior.RoomSize.Unknown) ? new DungeonRoom(DungeonRoomInterior.GetRoomInterior(me.GetRandomRoomName(biom_type, room_type))) : ((!string.IsNullOrEmpty(preset_name)) ? new DungeonRoom(DungeonRoomInterior.GetRoomInterior(this)) : new DungeonRoom(DungeonRoomInterior.GetRoomInterior(me.GetRandomRoomName(biom_type, room_type, room_size)))))));
		if (dungeonRoom != null)
		{
			dungeonRoom.given_biom_type = biom_type;
			dungeonRoom.given_room_type = room_type;
			dungeonRoom.given_room_size = room_size;
			dungeonRoom.given_room_interior_name = preset_name;
		}
		return dungeonRoom;
	}

	public string GetFilename()
	{
		if (!_names_hash.TryGetValue(room_type, out var value))
		{
			value = new Dictionary<int, Dictionary<int, Dictionary<string, string>>>();
			_names_hash.Add(room_type, value);
		}
		if (!value.TryGetValue((int)room_size, out var value2))
		{
			value2 = new Dictionary<int, Dictionary<string, string>>();
			value.Add((int)room_size, value2);
		}
		if (!value2.TryGetValue((int)biom_type, out var value3))
		{
			value3 = new Dictionary<string, string>();
			value2.Add((int)biom_type, value3);
		}
		if (!value3.TryGetValue(preset_name, out var value4))
		{
			path_sb.Length = 0;
			path_sb.Append("Dungeon/Rooms/");
			path_sb.Append(biom_type);
			path_sb.Append("/");
			path_sb.Append(room_type);
			path_sb.Append("/");
			path_sb.Append(room_size);
			path_sb.Append("/");
			path_sb.Append(preset_name);
			value4 = path_sb.ToString();
			value3.Add(preset_name, value4);
		}
		return value4;
	}
}
