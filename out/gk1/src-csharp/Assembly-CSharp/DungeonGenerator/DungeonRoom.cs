using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator;

public class DungeonRoom
{
	public DungeonRoomInterior.BiomType given_biom_type = DungeonRoomInterior.BiomType.Unknown;

	public string given_room_type = string.Empty;

	public DungeonRoomInterior.RoomSize given_room_size = DungeonRoomInterior.RoomSize.Unknown;

	public string given_room_interior_name = string.Empty;

	public int room_width;

	public int room_height;

	public DungeonRoomInterior room_interior;

	public List<DungeonRoomInterior> wrong_room_interiors = new List<DungeonRoomInterior>();

	public bool is_placed;

	public IntVector2 coords;

	public List<IntVector2> enters_coords = new List<IntVector2>();

	public DungeonRoom(DungeonRoomInterior room_interior)
	{
		if (room_interior == null)
		{
			Debug.LogError("Can not create DungeonRoom: Room interor is null!");
			return;
		}
		room_height = room_interior.room_height;
		room_width = room_interior.room_width;
		this.room_interior = room_interior;
	}

	public DungeonRoom Copy()
	{
		return new DungeonRoom(room_interior);
	}

	public bool IsCorrectRoom()
	{
		if (room_width < 3 || room_height < 3)
		{
			return false;
		}
		if (room_interior == null)
		{
			return false;
		}
		return true;
	}

	public bool TryChangeRoomInterior(DungeonWalker.Direction dir, int cor_diam, List<DungeonRoom> blacklist)
	{
		if (is_placed || (coords != null && coords != new IntVector2(0, 0)) || enters_coords.Count != 0)
		{
			return false;
		}
		List<DungeonRoomInterior> list = new List<DungeonRoomInterior>();
		if (blacklist != null && blacklist.Count > 0)
		{
			foreach (DungeonRoom item in blacklist)
			{
				if (item.room_interior != null)
				{
					list.Add(item.room_interior);
				}
			}
		}
		wrong_room_interiors.Add(room_interior);
		DungeonRoomsContainer me = DungeonRoomsContainer.me;
		if (me == null)
		{
			Debug.LogError("Rooms Container is null!");
			return false;
		}
		if (!string.IsNullOrEmpty(given_room_interior_name))
		{
			return false;
		}
		List<SerializedRoom> rooms = me.GetRooms(given_biom_type, given_room_type, given_room_size);
		if (rooms == null || rooms.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < 25; i++)
		{
			int index = Dungeon.RandomRange(0, rooms.Count);
			DungeonRoomInterior roomInterior = DungeonRoomInterior.GetRoomInterior(rooms[index]);
			if (roomInterior == null)
			{
				return false;
			}
			if (roomInterior.GetPossibleEnters(dir, cor_diam).Count != 0 && !list.Contains(roomInterior) && !wrong_room_interiors.Contains(roomInterior))
			{
				room_interior = roomInterior;
				room_height = room_interior.room_height;
				room_width = room_interior.room_width;
				return true;
			}
		}
		return false;
	}
}
