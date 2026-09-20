using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkVisibilityJob : IJobParallelFor
{
	private const byte OUT_OF_RANGE_STATE = 0;

	private const byte PREWARM_STATE = 1;

	private const byte VISIBLE_STATE = 2;

	public NativeArray<BurstableBounds> chunkableDataArray;

	public NativeArray<byte> visibilityStateResults;

	public float expandFactor;

	public float prewarmPlanePadding;

	public BurstablePlane plane0;

	public BurstablePlane plane1;

	public BurstablePlane plane2;

	public BurstablePlane plane3;

	public BurstablePlane plane4;

	public BurstablePlane plane5;

	public void Execute(int index)
	{
		float3 min = chunkableDataArray[index].Min;
		float3 max = chunkableDataArray[index].Max;
		ExpandBounds(ref min, ref max);
		if (IsInsidePlane(plane0, min, max, 0f) && IsInsidePlane(plane1, min, max, 0f) && IsInsidePlane(plane2, min, max, 0f) && IsInsidePlane(plane3, min, max, 0f) && IsInsidePlane(plane4, min, max, 0f) && IsInsidePlane(plane5, min, max, 0f))
		{
			visibilityStateResults[index] = 2;
			return;
		}
		bool flag = IsInsidePlane(plane0, min, max, prewarmPlanePadding) && IsInsidePlane(plane1, min, max, prewarmPlanePadding) && IsInsidePlane(plane2, min, max, prewarmPlanePadding) && IsInsidePlane(plane3, min, max, prewarmPlanePadding) && IsInsidePlane(plane4, min, max, prewarmPlanePadding) && IsInsidePlane(plane5, min, max, prewarmPlanePadding);
		visibilityStateResults[index] = (byte)(flag ? 1 : 0);
	}

	private void ExpandBounds(ref float3 min, ref float3 max)
	{
		if (!(expandFactor <= 1f))
		{
			float num = max.x - min.x;
			float num2 = max.z - min.z;
			float num3 = num * (expandFactor - 1f) * 0.5f;
			float num4 = num2 * (expandFactor - 1f) * 0.5f;
			min.x -= num3;
			max.x += num3;
			min.z -= num4;
			max.z += num4;
		}
	}

	private bool IsInsidePlane(BurstablePlane plane, float3 min, float3 max, float padding)
	{
		float3 y = math.select(min, max, plane.normal > 0f);
		return math.dot(plane.normal, y) + plane.distance > 0f - padding;
	}
}
