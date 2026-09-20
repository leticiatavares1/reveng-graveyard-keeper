using System;
using UnityEngine;

[Serializable]
public struct WorldZoneElevationAreaBakedData
{
	public Rect xzRect;

	public float elevationY;

	public bool ContainsXZ(Vector2 xz)
	{
		return xzRect.Contains(xz);
	}
}
