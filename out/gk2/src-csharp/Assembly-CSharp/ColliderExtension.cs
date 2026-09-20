using UnityEngine;

public static class ColliderExtension
{
	public static bool IsLossyScaleNegative(this Collider col)
	{
		if (!(col.transform.lossyScale.x < 0f) && !(col.transform.lossyScale.y < 0f))
		{
			return col.transform.lossyScale.z < 0f;
		}
		return true;
	}

	public static void FixBoxColliderLossyScale(this BoxCollider boxCol)
	{
		Transform transform = boxCol.transform;
		Vector3 localScale = transform.localScale;
		Vector3 center = boxCol.center;
		Vector3 size = boxCol.size;
		if (transform.lossyScale.x < 0f)
		{
			localScale.x *= -1f;
			center.x *= -1f;
			size.x *= -1f;
		}
		if (transform.lossyScale.y < 0f)
		{
			localScale.y *= -1f;
			center.y *= -1f;
			size.y *= -1f;
		}
		if (transform.lossyScale.z < 0f)
		{
			localScale.z *= -1f;
			center.z *= -1f;
			size.z *= -1f;
		}
		transform.localScale = localScale;
		boxCol.size = size;
		boxCol.center = center;
	}
}
