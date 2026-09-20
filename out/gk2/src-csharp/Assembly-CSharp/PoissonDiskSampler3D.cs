using System;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiskSampler3D
{
	private struct GridPos
	{
		public int x;

		public int y;

		public int z;

		public GridPos(Vector3 sample, float cellSize)
		{
			x = (int)(sample.x / cellSize);
			y = (int)(sample.y / cellSize);
			z = (int)(sample.z / cellSize);
		}
	}

	private const int k = 30;

	private readonly Vector3 cube;

	private readonly float radius2;

	private readonly float cellSize;

	private Vector3[,,] grid;

	private List<Vector3> activeSamples = new List<Vector3>();

	public PoissonDiskSampler3D(float width, float height, float depth, float radius)
	{
		cube = new Vector3(width, height, depth);
		radius2 = radius * radius;
		cellSize = radius / Mathf.Sqrt(3f);
		grid = new Vector3[Mathf.CeilToInt(width / cellSize), Mathf.CeilToInt(height / cellSize), Mathf.CeilToInt(depth / cellSize)];
		Debug.Log(grid.GetLength(0));
		Debug.Log(grid.GetLength(1));
		Debug.Log(grid.GetLength(2));
	}

	public List<Vector3> Samples()
	{
		List<Vector3> list = new List<Vector3>();
		list.Add(AddSample(new Vector3(UnityEngine.Random.value * cube.x, UnityEngine.Random.value * cube.y, UnityEngine.Random.value * cube.z)));
		while (activeSamples.Count > 0)
		{
			int index = (int)UnityEngine.Random.value * activeSamples.Count;
			Vector3 point = activeSamples[index];
			bool flag = false;
			for (int i = 0; i < 30; i++)
			{
				Vector3 vector = GenerateRandomPointAround(point, UnityEngine.Random.value * 3f * radius2 + radius2);
				if (IsContains(vector, cube) && IsFarEnough(vector))
				{
					flag = true;
					list.Add(AddSample(vector));
					break;
				}
			}
			if (!flag)
			{
				activeSamples[index] = activeSamples[activeSamples.Count - 1];
				activeSamples.RemoveAt(activeSamples.Count - 1);
			}
		}
		return list;
	}

	private bool IsContains(Vector3 v, Vector3 area)
	{
		if (v.x >= 0f && v.x < area.x && v.y >= 0f && v.y < area.y && v.z >= 0f && v.z < area.z)
		{
			return true;
		}
		return false;
	}

	private Vector3 GenerateRandomPointAround(Vector3 point, float minDist)
	{
		float value = UnityEngine.Random.value;
		float value2 = UnityEngine.Random.value;
		float value3 = UnityEngine.Random.value;
		float num = minDist * (value + 1f);
		float f = MathF.PI * 2f * value2;
		float f2 = MathF.PI * 2f * value3;
		float x = point.x + num * Mathf.Cos(f) * Mathf.Sin(f2);
		float y = point.y + num * Mathf.Sin(f) * Mathf.Sin(f2);
		float z = point.z + num * Mathf.Cos(f2);
		return new Vector3(x, y, z);
	}

	private bool IsFarEnough(Vector3 sample)
	{
		GridPos gridPos = new GridPos(sample, cellSize);
		int num = Mathf.Max(gridPos.x - 2, 0);
		int num2 = Mathf.Max(gridPos.y - 2, 0);
		int num3 = Mathf.Max(gridPos.z - 2, 0);
		int num4 = Mathf.Min(gridPos.x + 2, grid.GetLength(0) - 1);
		int num5 = Mathf.Min(gridPos.y + 2, grid.GetLength(1) - 1);
		int num6 = Mathf.Min(gridPos.z + 2, grid.GetLength(2) - 1);
		for (int i = num3; i <= num6; i++)
		{
			for (int j = num2; j <= num5; j++)
			{
				for (int k = num; k <= num4; k++)
				{
					Vector3 vector = grid[k, j, i];
					if (vector != Vector3.zero)
					{
						Vector3 vector2 = vector - sample;
						if (vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z < radius2)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	private Vector3 AddSample(Vector3 sample)
	{
		activeSamples.Add(sample);
		GridPos gridPos = new GridPos(sample, cellSize);
		grid[gridPos.x, gridPos.y, gridPos.z] = sample;
		return sample;
	}
}
