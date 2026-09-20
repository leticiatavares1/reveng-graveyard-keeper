using System;
using UnityEngine;

[Serializable]
public class KickSettings
{
	public Vector3 kickForce = new Vector3(5f, 0f, 12f);

	public float distanceToKick = 1f;

	public float kickDelay = 0.5f;
}
