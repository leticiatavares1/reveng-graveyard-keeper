using Unity.Collections;

public class ChunkVisibilityJobBuffer
{
	public NativeArray<BurstableBounds> data;

	public NativeArray<byte> results;

	private int capacity;

	public void EnsureCapacity(int required)
	{
		if (capacity < required)
		{
			Dispose();
			capacity = required;
			data = new NativeArray<BurstableBounds>(capacity, Allocator.Persistent);
			results = new NativeArray<byte>(capacity, Allocator.Persistent);
		}
	}

	public void Dispose()
	{
		if (data.IsCreated)
		{
			data.Dispose();
		}
		if (results.IsCreated)
		{
			results.Dispose();
		}
		capacity = 0;
	}
}
