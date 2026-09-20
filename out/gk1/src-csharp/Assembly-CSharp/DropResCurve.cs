using System;
using UnityEngine;

[Serializable]
public class DropResCurve
{
	public AnimationCurve curve;

	[Range(0f, 1f)]
	public float duration_factor;
}
