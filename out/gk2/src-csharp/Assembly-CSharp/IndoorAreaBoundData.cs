using System;
using UnityEngine;

[Serializable]
public class IndoorAreaBoundData
{
	public Vector3 center;

	public Vector3 size;

	public IndoorAreaBoundData(BoxCollider boxCollider)
	{
		Bounds bounds = boxCollider.bounds;
		center = bounds.center;
		size = bounds.size;
	}

	public bool ContainsXZ(Vector3 worldPos)
	{
		Vector3 vector = size * 0.5f;
		if (Mathf.Abs(worldPos.x - center.x) <= vector.x)
		{
			return Mathf.Abs(worldPos.z - center.z) <= vector.z;
		}
		return false;
	}

	public float GetXZArea()
	{
		return Mathf.Abs(size.x * size.z);
	}
}
