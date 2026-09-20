using System;
using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

public class DungeonRoomInterior : ScriptableObject
{
	public enum BiomType
	{
		Unknown = -1,
		Simple,
		Mine,
		Cave,
		Stony
	}

	public enum RoomSize
	{
		Unknown = -1,
		Tiny,
		Small,
		Medium,
		Big,
		Huge
	}

	public const string DOORS_CONTAINS_THIS_WORDS = "dungeon_exit";

	public const int TINY_ROOM = 8;

	public const int SMALL_ROOM = 11;

	public const int MEDIUM_ROOM = 14;

	public const int BIG_ROOM = 17;

	public const int HUGE_ROOM = 20;

	public string preset_name = "RoomInterior";

	public int room_height = 5;

	public int room_width = 5;

	public List<SerializedWGO> interior_objects = new List<SerializedWGO>();

	[SerializeField]
	public List<IntVectors> possible_enters_3 = new List<IntVectors>();

	[SerializeField]
	public List<IntVectors> possible_enters_4 = new List<IntVectors>();

	[SerializeField]
	public List<IntVectors> possible_enters_5 = new List<IntVectors>();

	[SerializeField]
	public List<IntVectors> possible_enters_6 = new List<IntVectors>();

	[SerializeField]
	public List<IntVectors> possible_enters_7 = new List<IntVectors>();

	public void RecalcHeightAndWidth()
	{
		int num = 0;
		int num2 = 0;
		foreach (SerializedWGO interior_object in interior_objects)
		{
			if (interior_object.coordinates.y + 1f > (float)num)
			{
				num = (int)Math.Ceiling(interior_object.coordinates.y) + 1;
			}
			if (interior_object.coordinates.x + 1f > (float)num2)
			{
				num2 = (int)Math.Ceiling(interior_object.coordinates.x) + 1;
			}
		}
		room_width = num2;
		room_height = num;
	}

	public static DungeonRoomInterior GetRoomInterior(SerializedRoom t_serialized_room)
	{
		DungeonRoomInterior dungeonRoomInterior = Resources.Load<DungeonRoomInterior>(t_serialized_room.GetFilename());
		if (dungeonRoomInterior == null)
		{
			Debug.LogError("Failed to load room: {biom: " + t_serialized_room.biom_type.ToString() + ", type: " + t_serialized_room.room_type + ", size: " + t_serialized_room.room_size.ToString() + ", name: " + t_serialized_room.preset_name + "}");
		}
		return dungeonRoomInterior;
	}

	public List<IntVector2> GetPossibleEnters(DungeonWalker.Direction t_direction, int corridor_diameter)
	{
		if (corridor_diameter < 3 || corridor_diameter > 7)
		{
			return null;
		}
		List<IntVectors> list = null;
		switch (corridor_diameter)
		{
		case 3:
			list = possible_enters_3;
			break;
		case 4:
			list = possible_enters_4;
			break;
		case 5:
			list = possible_enters_5;
			break;
		case 6:
			list = possible_enters_6;
			break;
		case 7:
			list = possible_enters_7;
			break;
		default:
			Debug.LogError("WHAT THE FUCK???!!!!");
			break;
		}
		if (list == null)
		{
			Debug.LogError("Possible enters is null!");
			return null;
		}
		return list[(int)t_direction].list;
	}

	public void SetPossibleEnters(List<List<IntVector2>> possible_enters_list_of_lists, int t_corridor_width)
	{
		if (t_corridor_width < 3 || t_corridor_width > 7)
		{
			return;
		}
		List<IntVectors> list = possible_enters_3;
		switch (t_corridor_width)
		{
		case 3:
			possible_enters_3 = new List<IntVectors>();
			list = possible_enters_3;
			break;
		case 4:
			possible_enters_4 = new List<IntVectors>();
			list = possible_enters_4;
			break;
		case 5:
			possible_enters_5 = new List<IntVectors>();
			list = possible_enters_5;
			break;
		case 6:
			possible_enters_6 = new List<IntVectors>();
			list = possible_enters_6;
			break;
		case 7:
			possible_enters_7 = new List<IntVectors>();
			list = possible_enters_7;
			break;
		default:
			Debug.LogError("WHAT THE FUCK???!!!!");
			break;
		}
		if (possible_enters_list_of_lists != null)
		{
			for (int i = 0; i < 4; i++)
			{
				list.Add(new IntVectors(possible_enters_list_of_lists[i]));
			}
		}
	}

	public bool HaveAnyPossibleEnter()
	{
		foreach (IntVectors item in possible_enters_3)
		{
			if (item != null && item.list.Count != 0)
			{
				return true;
			}
		}
		foreach (IntVectors item2 in possible_enters_4)
		{
			if (item2 != null && item2.list.Count != 0)
			{
				return true;
			}
		}
		foreach (IntVectors item3 in possible_enters_5)
		{
			if (item3 != null && item3.list.Count != 0)
			{
				return true;
			}
		}
		foreach (IntVectors item4 in possible_enters_6)
		{
			if (item4 != null && item4.list.Count != 0)
			{
				return true;
			}
		}
		foreach (IntVectors item5 in possible_enters_7)
		{
			if (item5 != null && item5.list.Count != 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveAnyPossibleEnter(int t_corridor_width)
	{
		List<IntVectors> list = possible_enters_3;
		switch (t_corridor_width)
		{
		case 3:
			list = possible_enters_3;
			break;
		case 4:
			list = possible_enters_4;
			break;
		case 5:
			list = possible_enters_5;
			break;
		case 6:
			list = possible_enters_6;
			break;
		case 7:
			list = possible_enters_7;
			break;
		default:
			Debug.LogError("WHAT THE FUCK???!!!!");
			break;
		}
		foreach (IntVectors item in list)
		{
			if (item != null && item.list.Count != 0)
			{
				return true;
			}
		}
		return false;
	}

	public void DrawRoom(Transform parent, Vector2 offset, List<IntVector2> enters_coords, int enter_thickness, Tileset tileset, out List<MobSpawner> spawners, List<SavedDungeonObject> saved_objs, bool is_first_room = true)
	{
		string[] obj = new string[10] { "#dgen# Drawing room ", base.name, "[", preset_name, "];\n offset = ", null, null, null, null, null };
		Vector2 vector = offset;
		obj[5] = vector.ToString();
		obj[6] = ";\n";
		obj[7] = ((enters_coords != null) ? (" enter_coords.Count = " + enters_coords.Count) : " enter_coords is NULL!");
		obj[8] = ";\n";
		obj[9] = ((saved_objs == null) ? " saved_objs is NULL!" : (" saved_objs.Count = " + saved_objs.Count));
		Debug.Log(string.Concat(obj));
		if (tileset == null && enters_coords != null && enters_coords.Count > 0)
		{
			tileset = Resources.Load<Tileset>("Dungeon/Tilesets/DungeonTileset");
			Debug.LogError("Tileset is null!");
		}
		bool flag = true;
		if (saved_objs == null)
		{
			flag = false;
		}
		else if (saved_objs.Count == 0)
		{
			flag = false;
		}
		spawners = new List<MobSpawner>();
		foreach (SerializedWGO interior_object in interior_objects)
		{
			bool flag2 = false;
			if (interior_object.dungeon_object_chance != null && interior_object.dungeon_object_chance.chance < 1f)
			{
				if (Application.isPlaying)
				{
					if ((Dungeon.RandomIsInitialized() ? Dungeon.RandomRange(0f, 1f) : UnityEngine.Random.Range(0f, 1f)) > interior_object.dungeon_object_chance.chance)
					{
						continue;
					}
				}
				else
				{
					flag2 = true;
				}
			}
			switch (interior_object.obj_type)
			{
			case SerializedWGO.WorldObjectType.Unknown:
				Debug.LogError("WorldObjectType of " + interior_object.id + " is Unknown!");
				break;
			case SerializedWGO.WorldObjectType.WGO:
			{
				string text = "";
				if (interior_object.id.Contains("dungeon_exit"))
				{
					text = interior_object.id;
					if (is_first_room)
					{
						if (text[text.Length - 1] == '2')
						{
							text = text.Remove(text.Length - 1);
						}
						if (Application.isPlaying)
						{
							MainGame.me.dungeon_root.enter_to_dunge.transform.localPosition = interior_object.coordinates + offset;
						}
					}
					else if (text[text.Length - 1] != '2')
					{
						text += "2";
					}
				}
				else if (flag)
				{
					bool flag3 = true;
					foreach (SavedDungeonObject saved_obj in saved_objs)
					{
						if (saved_obj.type == SavedDungeonObject.SavedDungeonObjectType.WGO && !(saved_obj.name != interior_object.id))
						{
							Vector2 vector2 = interior_object.coordinates + offset - saved_obj.local_position;
							if (!(Mathf.Abs(vector2.x) > 0.1f) && !(Mathf.Abs(vector2.y) > 0.1f))
							{
								flag3 = false;
								break;
							}
						}
					}
					if (flag3)
					{
						break;
					}
				}
				WorldGameObject worldGameObject = WorldMap.SpawnWGO(parent, string.IsNullOrEmpty(text) ? interior_object.id : text);
				worldGameObject.transform.localPosition = interior_object.coordinates + offset;
				worldGameObject.transform.localScale = interior_object.local_scale;
				RoundAndSortComponent round_and_sort = worldGameObject.round_and_sort;
				round_and_sort.grid_divider = interior_object.divider;
				round_and_sort.floor_line = interior_object.floor_line;
				worldGameObject.variation = interior_object.variation;
				worldGameObject.variation_2 = interior_object.variation_2;
				for (int i = 0; i < interior_object.s_params.Count; i++)
				{
					worldGameObject.AddToInventory(interior_object.s_params[i], Mathf.RoundToInt(interior_object.f_params[i]));
				}
				if (flag2)
				{
					worldGameObject.gameObject.AddComponent<DungeonObjectChanceInspector>().dungeon_object_chance = ((interior_object.dungeon_object_chance == null) ? new DungeonObjectChance() : interior_object.dungeon_object_chance);
				}
				worldGameObject.Redraw();
				break;
			}
			case SerializedWGO.WorldObjectType.MobSpawner:
			{
				MobSpawner mobSpawner = Resources.Load<MobSpawner>("objects/WorldSimpleObjects/" + interior_object.id);
				if (mobSpawner == null)
				{
					Debug.LogError("Can not Load MobSpawner Prefab: " + interior_object.id);
					break;
				}
				MobSpawner mobSpawner2 = null;
				if (Application.isPlaying)
				{
					mobSpawner2 = UnityEngine.Object.Instantiate(mobSpawner, parent, worldPositionStays: false);
				}
				if (mobSpawner2 == null)
				{
					Debug.LogError("Can not Instantiate new MobSpawner!");
					break;
				}
				RoundAndSortComponent component2 = mobSpawner2.gameObject.GetComponent<RoundAndSortComponent>();
				component2.grid_divider = interior_object.divider;
				component2.floor_line = interior_object.floor_line;
				mobSpawner2.transform.localPosition = interior_object.coordinates + offset;
				if (interior_object.f_params != null && interior_object.f_params.Count > 0)
				{
					mobSpawner2.chance_to_spawn = interior_object.f_params[0];
				}
				if (interior_object.s_params != null && interior_object.s_params.Count > 0)
				{
					mobSpawner2.spawner_id = interior_object.s_params[0];
				}
				spawners.Add(mobSpawner2);
				break;
			}
			case SerializedWGO.WorldObjectType.WSO:
			{
				WorldSimpleObject worldSimpleObject = Resources.Load<WorldSimpleObject>("objects/WorldSimpleObjects/" + interior_object.id);
				if (worldSimpleObject == null)
				{
					Debug.LogError("Can not Load WSO Prefab: " + interior_object.id);
					break;
				}
				WorldSimpleObject worldSimpleObject2 = worldSimpleObject;
				bool need_mirror = false;
				IntVector2 intVector = new IntVector2(Mathf.FloorToInt(interior_object.coordinates.x), Mathf.FloorToInt(interior_object.coordinates.y));
				if (worldSimpleObject.wso_type == WorldSimpleObject.WSOType.WallStraight || worldSimpleObject.wso_type == WorldSimpleObject.WSOType.WallCorner)
				{
					if (intVector.x == 0)
					{
						foreach (IntVector2 enters_coord in enters_coords)
						{
							if (enters_coord.x == 0 && intVector.y >= enters_coord.y && intVector.y <= enters_coord.y + enter_thickness - 1)
							{
								worldSimpleObject = ((intVector.y != enters_coord.y) ? ((intVector.y != enters_coord.y + enter_thickness - 1) ? null : tileset.GetTilePrefab(Tileset.WallType.CornerInDownRight, out need_mirror)) : tileset.GetTilePrefab(Tileset.WallType.CornerInUpRight, out need_mirror));
								break;
							}
						}
					}
					else if (intVector.x == room_width - 1)
					{
						foreach (IntVector2 enters_coord2 in enters_coords)
						{
							if (enters_coord2.x == room_width - 1 && intVector.y >= enters_coord2.y && intVector.y <= enters_coord2.y + enter_thickness - 1)
							{
								worldSimpleObject = ((intVector.y != enters_coord2.y) ? ((intVector.y != enters_coord2.y + enter_thickness - 1) ? null : tileset.GetTilePrefab(Tileset.WallType.CornerInDownLeft, out need_mirror)) : tileset.GetTilePrefab(Tileset.WallType.CornerInUpLeft, out need_mirror));
								break;
							}
						}
					}
					else if (intVector.y == 0)
					{
						foreach (IntVector2 enters_coord3 in enters_coords)
						{
							if (enters_coord3.y == 0 && intVector.x >= enters_coord3.x && intVector.x <= enters_coord3.x + enter_thickness - 1)
							{
								worldSimpleObject = ((intVector.x != enters_coord3.x) ? ((intVector.x != enters_coord3.x + enter_thickness - 1) ? null : tileset.GetTilePrefab(Tileset.WallType.CornerInUpLeft, out need_mirror)) : tileset.GetTilePrefab(Tileset.WallType.CornerInUpRight, out need_mirror));
								break;
							}
						}
					}
					else if (intVector.y == room_height - 1)
					{
						foreach (IntVector2 enters_coord4 in enters_coords)
						{
							if (enters_coord4.y == room_height - 1 && intVector.x >= enters_coord4.x && intVector.x <= enters_coord4.x + enter_thickness - 1)
							{
								worldSimpleObject = ((intVector.x != enters_coord4.x) ? ((intVector.x != enters_coord4.x + enter_thickness - 1) ? null : tileset.GetTilePrefab(Tileset.WallType.CornerInDownLeft, out need_mirror)) : tileset.GetTilePrefab(Tileset.WallType.CornerInDownRight, out need_mirror));
								break;
							}
						}
					}
				}
				if (worldSimpleObject == null)
				{
					break;
				}
				WorldSimpleObject worldSimpleObject3 = null;
				if (Application.isPlaying)
				{
					worldSimpleObject3 = UnityEngine.Object.Instantiate(worldSimpleObject, parent, worldPositionStays: false);
				}
				if (worldSimpleObject3 == null)
				{
					Debug.LogError("Can not Instantiate new WSO!");
					break;
				}
				RoundAndSortComponent component = worldSimpleObject3.gameObject.GetComponent<RoundAndSortComponent>();
				component.grid_divider = interior_object.divider;
				component.floor_line = interior_object.floor_line;
				worldSimpleObject3.transform.localPosition = interior_object.coordinates + offset;
				if (need_mirror)
				{
					Vector3 localScale = worldSimpleObject3.transform.localScale;
					localScale.x *= -1f;
					worldSimpleObject3.transform.localScale = localScale;
				}
				else if (worldSimpleObject2 == worldSimpleObject)
				{
					worldSimpleObject3.transform.localScale = interior_object.local_scale;
				}
				if (flag2)
				{
					worldSimpleObject3.gameObject.AddComponent<DungeonObjectChanceInspector>().dungeon_object_chance = ((interior_object.dungeon_object_chance == null) ? new DungeonObjectChance() : interior_object.dungeon_object_chance);
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public static RoomSize GetRoomSize(int room_width, int room_height)
	{
		if (room_height < 8 && room_width < 8)
		{
			return RoomSize.Tiny;
		}
		if (room_height < 11 && room_width < 11)
		{
			return RoomSize.Small;
		}
		if (room_height < 14 && room_width < 14)
		{
			return RoomSize.Medium;
		}
		if (room_height < 17 && room_width < 17)
		{
			return RoomSize.Big;
		}
		if (room_height < 20)
		{
			_ = 20;
			return RoomSize.Huge;
		}
		return RoomSize.Huge;
	}
}
