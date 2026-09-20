using System;
using UnityEngine;

[Serializable]
public class SerializableContactPoint
{
	public Vector3 point;

	public Vector3 normal;

	public SerializableContactPoint(Vector3 point, Vector3 normal)
	{
		this.point = point;
		this.normal = normal;
	}
}
