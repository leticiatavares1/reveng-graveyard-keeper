using System;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyRandom
{
	private int seed;

	private System.Random random;

	public int Seed => seed;

	public LazyRandom(int? seed = null)
	{
		this.seed = seed ?? UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		random = new System.Random(this.seed);
	}

	public int Range(int minValue, int maxValue)
	{
		int num = Mathf.RoundToInt((float)random.NextDouble() * (float)(maxValue - minValue) + (float)minValue);
		if (num == maxValue)
		{
			num--;
		}
		return num;
	}

	public float Range(float minValue, float maxValue)
	{
		return (float)random.NextDouble() * (maxValue - minValue) + minValue;
	}
}
