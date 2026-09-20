using System;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[Serializable]
public class ProCamera2DParallaxLayer
{
	public Camera ParallaxCamera;

	[Range(0f, 5f)]
	public float Speed = 1f;

	public LayerMask LayerMask;

	[NonSerialized]
	[HideInInspector]
	public Transform CameraTransform;
}
