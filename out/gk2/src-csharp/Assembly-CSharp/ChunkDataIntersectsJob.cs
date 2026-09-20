using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct ChunkDataIntersectsJob : IJobParallelFor
{
	public BurstableBounds sourceData;

	public NativeArray<BurstableBounds> objectsData;

	public NativeArray<bool> results;

	public void Execute(int index)
	{
		results[index] = sourceData.Intersects(objectsData[index]);
	}
}
