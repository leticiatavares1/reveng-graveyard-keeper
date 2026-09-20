using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IndoorAreaData
{
	public string id;

	public List<IndoorAreaBoundData> bounds = new List<IndoorAreaBoundData>();

	public bool ContainsXZ(Vector3 worldPos)
	{
		if (bounds == null)
		{
			return false;
		}
		for (int i = 0; i < bounds.Count; i++)
		{
			IndoorAreaBoundData indoorAreaBoundData = bounds[i];
			if (indoorAreaBoundData != null && indoorAreaBoundData.ContainsXZ(worldPos))
			{
				return true;
			}
		}
		return false;
	}

	public float GetXZArea()
	{
		float num = 0f;
		if (bounds == null)
		{
			return num;
		}
		for (int i = 0; i < bounds.Count; i++)
		{
			IndoorAreaBoundData indoorAreaBoundData = bounds[i];
			if (indoorAreaBoundData != null)
			{
				num += indoorAreaBoundData.GetXZArea();
			}
		}
		return num;
	}
}
