using System.Collections.Generic;

namespace DungeonGenerator;

public class DungeonPattern
{
	public bool[] step_pattern;

	public bool[] hit_pattern;

	public int xmax;

	public int ymax;

	private static List<DungeonPattern> _patterns = null;

	private const int MAX_THICKNESS = 7;

	public const int STEP_PATTERN_SIZE_BITS = 8;

	private const int STEP_PATTERN_SIZE = 256;

	public static readonly int[][,] STEP_PATTERNS = new int[5][,]
	{
		new int[1, 1],
		new int[1, 1] { { 1 } },
		new int[3, 3]
		{
			{ 1, 1, 1 },
			{ 1, 1, 1 },
			{ 1, 1, 1 }
		},
		new int[5, 5]
		{
			{ 1, 1, 1, 1, 1 },
			{ 1, 1, 1, 1, 1 },
			{ 1, 1, 1, 1, 1 },
			{ 1, 1, 1, 1, 1 },
			{ 1, 1, 1, 1, 1 }
		},
		new int[7, 7]
		{
			{ 0, 0, 1, 1, 1, 0, 0 },
			{ 0, 1, 1, 1, 1, 1, 0 },
			{ 1, 1, 1, 1, 1, 1, 1 },
			{ 1, 1, 1, 1, 1, 1, 1 },
			{ 1, 1, 1, 1, 1, 1, 1 },
			{ 0, 1, 1, 1, 1, 1, 0 },
			{ 0, 0, 1, 1, 1, 0, 0 }
		}
	};

	public static void InitPatternsCache()
	{
		_patterns = new List<DungeonPattern>();
		for (int i = 0; i <= 7; i++)
		{
			_patterns.Add(new DungeonPattern(i));
		}
	}

	public static DungeonPattern GetPattern(int thickness)
	{
		return _patterns[thickness];
	}

	private DungeonPattern(int thickness)
	{
		step_pattern = new bool[65536];
		hit_pattern = new bool[65536];
		for (int i = 0; i < step_pattern.Length; i++)
		{
			step_pattern[i] = (hit_pattern[i] = false);
		}
		int[,] array;
		if (thickness < STEP_PATTERNS.Length)
		{
			array = STEP_PATTERNS[thickness];
		}
		else
		{
			array = new int[2 * thickness - 1, 2 * thickness - 1];
			for (int j = 0; j < 2 * thickness - 1; j++)
			{
				for (int k = 0; k < 2 * thickness - 1; k++)
				{
					array[j, k] = 1;
				}
			}
		}
		xmax = array.GetUpperBound(0);
		ymax = array.GetUpperBound(1);
		for (int l = 0; l <= ymax; l++)
		{
			for (int m = 0; m <= xmax; m++)
			{
				step_pattern[(l << 8) + m] = array[m, l] == 1;
			}
		}
		GenerateHitPattern(thickness);
	}

	private void GenerateHitPattern(int thickness)
	{
		if (thickness == 1)
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					hit_pattern[i + (j << 8)] = true;
				}
			}
			return;
		}
		for (int k = 0; k < 2 * thickness - 1; k++)
		{
			for (int l = 0; l < 2 * thickness - 1; l++)
			{
				hit_pattern[k + 1 + (l + 1 << 8)] = step_pattern[k + (l << 8)];
			}
		}
		for (int m = 0; m < thickness + 1; m++)
		{
			for (int n = 0; n < thickness + 1; n++)
			{
				if (hit_pattern[n + 1 + (m << 8)] || hit_pattern[n + (m + 1 << 8)] || hit_pattern[n + 1 + (m + 1 << 8)])
				{
					hit_pattern[n + (m << 8)] = true;
					hit_pattern[2 * thickness - n + (m << 8)] = true;
					hit_pattern[n + (2 * thickness - m << 8)] = true;
					hit_pattern[2 * thickness - n + (2 * thickness - m << 8)] = true;
				}
				else
				{
					hit_pattern[n + (m << 8)] = false;
				}
			}
		}
	}
}
