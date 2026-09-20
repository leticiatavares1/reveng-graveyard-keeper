using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TrailDefinition : ScriptableObject
{
	public List<TrailTypeDefinition> trails = new List<TrailTypeDefinition>();

	[NonSerialized]
	private List<Ground.GroudType> _types = new List<Ground.GroudType>();

	public TrailTypeDefinition GetByType(Ground.GroudType type)
	{
		if (_types.Count == 0)
		{
			foreach (TrailTypeDefinition trail in trails)
			{
				_types.Add(trail.type);
			}
		}
		int num = _types.IndexOf(type);
		if (num == -1)
		{
			return null;
		}
		return trails[num];
	}
}
