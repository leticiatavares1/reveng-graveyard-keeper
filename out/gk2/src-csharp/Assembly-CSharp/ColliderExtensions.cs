using UnityEngine;

public static class ColliderExtensions
{
	public static Vector3 GetContactPosition(this Collider collider, Vector3 otherPos, float lerpT = 0.5f)
	{
		Vector3 a = collider.ClosestPoint(otherPos);
		float num = Mathf.Max(a.y, otherPos.y);
		return Vector3.Lerp(a, collider.transform.position, lerpT).XZ() + Vector3.up * num;
	}
}
