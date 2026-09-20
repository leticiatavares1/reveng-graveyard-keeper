using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class SkinPresetPart
{
	public int id;

	public Color color = Color.white;

	public float hue;

	public float saturation = 1f;

	public float velocity = 1f;

	public Texture2D palette;
}
