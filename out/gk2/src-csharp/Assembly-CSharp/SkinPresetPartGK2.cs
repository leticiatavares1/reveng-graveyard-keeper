using System;
using UnityEngine;

[Serializable]
public class SkinPresetPartGK2
{
	[PreviewFormatInteger("0000")]
	public int id;

	public Color color = Color.white;

	public float hue;

	public float saturation = 1f;

	public float velocity = 1f;

	public Texture2D palette;

	public ColorReplaceType colorReplaceType;
}
