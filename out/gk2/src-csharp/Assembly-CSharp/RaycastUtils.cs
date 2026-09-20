using UnityEngine;

public static class RaycastUtils
{
	private const int HIT_LAYER_MASK = 6144;

	public static Vector3 TrySnapToTheGround(Vector3 posToSnapOn, float hitPointCast, float hitMaxDistance)
	{
		if (Physics.Raycast(new Ray(posToSnapOn + Vector3.up * hitPointCast, Vector3.down), out var hitInfo, hitMaxDistance, 6144))
		{
			posToSnapOn = new Vector3(posToSnapOn.x, hitInfo.point.y, posToSnapOn.z);
		}
		return posToSnapOn;
	}
}
