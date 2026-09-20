using System;
using UnityEngine;

namespace DungeonGenerator;

[CreateAssetMenu(fileName = "Tileset", menuName = "Tileset", order = 2)]
public class Tileset : ScriptableObject
{
	public enum WallType
	{
		Unknown,
		Up,
		Down,
		Left,
		Right,
		CornerOutUpLeft,
		CornerOutUpRight,
		CornerOutDownLeft,
		CornerOutDownRight,
		CornerInUpLeft,
		CornerInUpRight,
		CornerInDownLeft,
		CornerInDownRight,
		Floor
	}

	public const string DEFAULT_TILESET_NAME = "DungeonTileset";

	public const string TILESET_PATH = "Dungeon/Tilesets/";

	public WSOList wall_up;

	public WSOList wall_down;

	public WSOList wall_left;

	public bool wall_right_mirror;

	public WSOList wall_right;

	public WSOList wall_corner_out_up_left;

	public bool wall_corner_out_up_right_mirror;

	public WSOList wall_corner_out_up_right;

	public WSOList wall_corner_out_down_left;

	public bool wall_corner_out_down_right_mirror;

	public WSOList wall_corner_out_down_right;

	public WSOList wall_corner_in_up_left;

	public bool wall_corner_in_up_right_mirror;

	public WSOList wall_corner_in_up_right;

	public WSOList wall_corner_in_down_left;

	public bool wall_corner_in_down_right_mirror;

	public WSOList wall_corner_in_down_right;

	public WSOList floor;

	public WorldSimpleObject GetTilePrefab(WallType t_wall_type, out bool need_mirror)
	{
		WSOList wSOList = null;
		need_mirror = false;
		switch (t_wall_type)
		{
		case WallType.Unknown:
			return null;
		case WallType.Up:
			wSOList = wall_up;
			break;
		case WallType.Down:
			wSOList = wall_down;
			break;
		case WallType.Left:
			wSOList = wall_left;
			break;
		case WallType.Right:
			wSOList = (wall_right_mirror ? wall_left : wall_right);
			need_mirror = wall_right_mirror;
			break;
		case WallType.CornerOutUpLeft:
			wSOList = wall_corner_out_up_left;
			break;
		case WallType.CornerOutUpRight:
			wSOList = (wall_corner_out_up_right_mirror ? wall_corner_out_up_left : wall_corner_out_up_right);
			need_mirror = wall_corner_out_up_right_mirror;
			break;
		case WallType.CornerOutDownLeft:
			wSOList = wall_corner_out_down_left;
			break;
		case WallType.CornerOutDownRight:
			wSOList = (wall_corner_out_down_right_mirror ? wall_corner_out_down_left : wall_corner_out_down_right);
			need_mirror = wall_corner_out_down_right_mirror;
			break;
		case WallType.CornerInUpLeft:
			wSOList = wall_corner_in_up_left;
			break;
		case WallType.CornerInUpRight:
			wSOList = (wall_corner_in_up_right_mirror ? wall_corner_in_up_left : wall_corner_in_up_right);
			need_mirror = wall_corner_in_up_right_mirror;
			break;
		case WallType.CornerInDownLeft:
			wSOList = wall_corner_in_down_left;
			break;
		case WallType.CornerInDownRight:
			wSOList = (wall_corner_in_down_right_mirror ? wall_corner_in_down_left : wall_corner_in_down_right);
			need_mirror = wall_corner_in_down_right_mirror;
			break;
		case WallType.Floor:
			wSOList = floor;
			break;
		default:
			throw new ArgumentOutOfRangeException("t_wall_type", t_wall_type, null);
		}
		if (wSOList == null)
		{
			return null;
		}
		return wSOList.GetRandomWSO();
	}
}
