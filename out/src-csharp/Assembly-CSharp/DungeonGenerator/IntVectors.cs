using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator;

[Serializable]
public class IntVectors
{
	[SerializeField]
	public List<IntVector2> list = new List<IntVector2>();

	public IntVectors()
	{
		list = new List<IntVector2>();
	}

	public IntVectors(List<IntVector2> t_list)
	{
		list = t_list;
	}
}
