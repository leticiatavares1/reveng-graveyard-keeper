using System;
using Unity.Mathematics;

[Serializable]
public struct BurstableBounds
{
	public float3 center;

	public float3 size;

	public float3 Min => center - Extents;

	public float3 Max => center + Extents;

	public float3 Extents => size * 0.5f;

	public bool Intersects(BurstableBounds bounds)
	{
		if ((double)Min.x <= (double)bounds.Max.x && Max.x >= bounds.Min.x && Min.y <= bounds.Max.y && Max.y >= bounds.Min.y && Min.z <= bounds.Max.z)
		{
			return Max.z >= bounds.Min.z;
		}
		return false;
	}

	public BurstableBounds(float3 center, float3 size)
	{
		this.center = center;
		this.size = size;
	}
}
