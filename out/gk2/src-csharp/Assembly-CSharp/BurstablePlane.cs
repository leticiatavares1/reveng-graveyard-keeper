using Unity.Mathematics;

public struct BurstablePlane
{
	public float3 normal;

	public float distance;

	public bool GetSide(float3 point)
	{
		return math.dot(normal, point) + distance > 0f;
	}
}
