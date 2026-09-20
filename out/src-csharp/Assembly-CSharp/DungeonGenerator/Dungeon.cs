using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator;

public class Dungeon
{
	public enum CellType
	{
		Nothing,
		Corridor,
		Room
	}

	public const int MIN_DUNGE_HEIGHT = 16;

	public const int MIN_DUNGE_WIDTH = 16;

	public int dungeon_width;

	public int dungeon_height;

	public int room_borders;

	public IntVector2 enter_to_dunge;

	public DungeonWalker main_walker;

	public List<DungeonWalker> sub_walkers;

	private static int[] _dunge_matrix = new int[65536];

	private static int[] _clean_matrix = new int[65536];

	private static System.Random _rnd_float = null;

	private static int _rnd_float_calls_count = 0;

	public bool is_correst
	{
		get
		{
			if (!main_walker.is_correct)
			{
				return false;
			}
			foreach (DungeonWalker sub_walker in sub_walkers)
			{
				if (!sub_walker.is_finished || !sub_walker.is_correct)
				{
					return false;
				}
			}
			return true;
		}
	}

	public List<DungeonRoom> placed_rooms
	{
		get
		{
			List<DungeonRoom> list = new List<DungeonRoom>();
			list.AddRange(main_walker.placed_rooms);
			foreach (DungeonWalker sub_walker in sub_walkers)
			{
				list.AddRange(sub_walker.placed_rooms);
			}
			return list;
		}
	}

	public static void Init()
	{
		for (int i = 0; i < _clean_matrix.Length; i++)
		{
			_clean_matrix[i] = 0;
		}
	}

	public Dungeon(int t_width = 256, int t_height = 256, IntVector2 t_enter = null, int room_borders = 2)
	{
		dungeon_width = t_width;
		dungeon_height = t_height;
		this.room_borders = ((room_borders > 0) ? room_borders : 2);
		enter_to_dunge = t_enter ?? new IntVector2(dungeon_width / 2, dungeon_height / 2);
		sub_walkers = new List<DungeonWalker>();
	}

	public bool TryGenerateDungeon()
	{
		FillMatrix();
		int num = 0;
		while (!main_walker.is_finished)
		{
			main_walker.CalculateTick();
			num++;
			_ = 1024;
			if (num > 10000)
			{
				MainGame.me.dungeon_root.statistics.finish_main_walker_by_iterator++;
				return false;
			}
		}
		if (!main_walker.is_correct)
		{
			MainGame.me.dungeon_root.statistics.main_walker_is_not_correct++;
			return false;
		}
		foreach (DungeonWalker sub_walker in sub_walkers)
		{
			if (sub_walker.cur_position.x != -1 && sub_walker.cur_position.y != -1)
			{
				int num2 = 0;
				while (!sub_walker.is_finished)
				{
					sub_walker.CalculateTick();
					num2++;
					if (num2 > 2000)
					{
						MainGame.me.dungeon_root.statistics.finish_sub_walker_by_iterator++;
						return false;
					}
				}
			}
			if (!sub_walker.is_correct)
			{
				MainGame.me.dungeon_root.statistics.sub_walker_is_not_correct++;
				return false;
			}
		}
		return true;
	}

	private void FillMatrix()
	{
		Array.Copy(_clean_matrix, _dunge_matrix, _dunge_matrix.Length);
	}

	public bool TrySetCellType(int x, int y, CellType t_cell_type)
	{
		if (x < 0 || x >= dungeon_width || y < 0 || y >= dungeon_height)
		{
			MainGame.me.dungeon_root.statistics.touched_borders_1++;
			return false;
		}
		_dunge_matrix[x + (y << 8)] = (int)t_cell_type;
		return true;
	}

	public CellType GetCellType(int x, int y)
	{
		return (CellType)_dunge_matrix[x + (y << 8)];
	}

	public bool TryGetCellType(int x, int y, out CellType cell_type)
	{
		cell_type = CellType.Nothing;
		if (x < 0 || x >= dungeon_width || y < 0 || y >= dungeon_height)
		{
			MainGame.me.dungeon_root.statistics.touched_borders_2++;
			return false;
		}
		cell_type = (CellType)_dunge_matrix[x + (y << 8)];
		return true;
	}

	public bool IsEmptyCell(int x, int y)
	{
		if (x < 0 || x >= dungeon_width || y < 0 || y >= dungeon_height)
		{
			MainGame.me.dungeon_root.statistics.touched_borders_3++;
			return false;
		}
		return _dunge_matrix[x + (y << 8)] == 0;
	}

	public int[,] GetMatrix()
	{
		int[,] array = new int[256, 256];
		for (int i = 0; i < 256; i++)
		{
			int num = i << 8;
			for (int j = 0; j < 256; j++)
			{
				array[j, i] = _dunge_matrix[num];
				num++;
			}
		}
		return array;
	}

	public void MakePostproduction()
	{
		for (int i = 0; i < dungeon_width - 1; i++)
		{
			for (int j = 0; j < dungeon_height - 1; j++)
			{
				if (_dunge_matrix[i + (j << 8)] == 1 && (_dunge_matrix[i + (j + 1 << 8)] == 0 || _dunge_matrix[i + 1 + (j << 8)] == 0 || _dunge_matrix[i + 1 + (j + 1 << 8)] == 0))
				{
					_dunge_matrix[i + (j << 8)] = 0;
				}
			}
		}
	}

	public bool IsCorrectDungeon()
	{
		if (dungeon_width < 16)
		{
			return false;
		}
		if (dungeon_height < 16)
		{
			return false;
		}
		return true;
	}

	public static float RandomRange(float min, float max)
	{
		_rnd_float_calls_count++;
		return (float)_rnd_float.NextDouble() * (max - min) + min;
	}

	public static int RandomRange(int min, int max)
	{
		_rnd_float_calls_count++;
		int num = Mathf.FloorToInt((float)_rnd_float.NextDouble() * (float)(max - min) + (float)min);
		if (num == max)
		{
			num--;
		}
		return num;
	}

	public static void SetRandomSeed(int seed, int random_calls_count)
	{
		_rnd_float = new System.Random(seed);
		for (int i = 0; i < random_calls_count; i++)
		{
			_rnd_float.NextDouble();
		}
		_rnd_float_calls_count = random_calls_count;
	}

	public static int GetRandomCallsCount()
	{
		return _rnd_float_calls_count;
	}

	public static bool RandomIsInitialized()
	{
		return _rnd_float != null;
	}
}
