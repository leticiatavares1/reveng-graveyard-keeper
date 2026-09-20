using System;
using UnityEngine;

[Serializable]
public class SerializableCollision
{
	public int goHashCode;

	public int layer;

	public SerializableContactPoint[] contactPoints;

	public static SerializableCollision CreateFrom(Collision collision)
	{
		return new SerializableCollision
		{
			goHashCode = collision.gameObject.GetHashCode(),
			layer = collision.gameObject.layer,
			contactPoints = GetContactPoints(collision)
		};
	}

	public void UpdateContactPoints(Collision collision)
	{
		contactPoints = GetContactPoints(collision);
	}

	private static SerializableContactPoint[] GetContactPoints(Collision collision)
	{
		SerializableContactPoint[] array = new SerializableContactPoint[collision.contactCount];
		ContactPoint[] array2 = new ContactPoint[collision.contactCount];
		collision.GetContacts(array2);
		for (int i = 0; i < array2.Length; i++)
		{
			ContactPoint contactPoint = array2[i];
			array[i] = new SerializableContactPoint(contactPoint.point, contactPoint.normal);
		}
		return array;
	}
}
