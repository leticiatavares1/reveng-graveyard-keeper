using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UVs
{
	public List<Vector4> uvs = new List<Vector4>();

	public UVs(int capacity)
	{
		uvs = new List<Vector4>(capacity);
	}
}
