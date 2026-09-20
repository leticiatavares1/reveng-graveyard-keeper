using System;
using UnityEngine;

[Serializable]
public struct WorldZoneNavigationHoleBakedData
{
	public Vector3 center;

	public Vector3 size;

	public Quaternion rotation;

	public static WorldZoneNavigationHoleBakedData FromBoxCollider(BoxCollider boxCollider)
	{
		Vector3 lossyScale = boxCollider.transform.lossyScale;
		Vector3 vector = Vector3.Scale(boxCollider.size, lossyScale);
		WorldZoneNavigationHoleBakedData result = default(WorldZoneNavigationHoleBakedData);
		result.center = boxCollider.transform.TransformPoint(boxCollider.center);
		result.size = new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		result.rotation = boxCollider.transform.rotation;
		return result;
	}

	public WorldZoneNavigationHoleBakedData WithOffset(Vector3 offset)
	{
		WorldZoneNavigationHoleBakedData result = default(WorldZoneNavigationHoleBakedData);
		result.center = center + offset;
		result.size = size;
		result.rotation = rotation;
		return result;
	}
}
