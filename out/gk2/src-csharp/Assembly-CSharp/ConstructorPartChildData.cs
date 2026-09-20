using System;
using UnityEngine;

[Serializable]
public class ConstructorPartChildData
{
	public bool canNotBeBaked;

	public string pathToObject;

	public Vector3 localPosition;

	public Vector3 localScale = Vector3.one;

	public Quaternion rotation;

	public BurstableChunkBoundsPair chunkBounds;

	[NonSerialized]
	public ConstructorPartChildObject view;

	public Texture2D lut;
}
