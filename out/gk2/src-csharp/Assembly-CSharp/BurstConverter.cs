using UnityEngine;

public static class BurstConverter
{
	public static BurstableBounds ConvertBoundsToBurstable(Bounds bounds)
	{
		return new BurstableBounds(bounds.center, bounds.size);
	}

	public static BurstablePlane[] ConvertToBurstablePlanes(Plane[] planes)
	{
		BurstablePlane[] result = new BurstablePlane[planes.Length];
		ConvertToBurstablePlanes(planes, result);
		return result;
	}

	public static void ConvertToBurstablePlanes(Plane[] planes, BurstablePlane[] result)
	{
		for (int i = 0; i < planes.Length; i++)
		{
			result[i] = new BurstablePlane
			{
				normal = planes[i].normal,
				distance = planes[i].distance
			};
		}
	}
}
