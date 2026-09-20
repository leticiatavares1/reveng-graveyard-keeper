using System;
using UnityEngine;

[Serializable]
public class TexturesData
{
	[NonSerialized]
	public Texture2D texture;

	[NonSerialized]
	public Texture2D normalMap;

	[NonSerialized]
	public Texture2D lut;
}
