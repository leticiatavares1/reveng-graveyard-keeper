using System;
using UnityEngine;

[Serializable]
public class SerializedTechPointDrop
{
	public Vector2 pos;

	public TechPointsSpawner.Type type;

	public void FromTechPointDrop(TechPointDrop drop)
	{
		if (drop == null)
		{
			Debug.LogError("TechPointDrop is null!");
			return;
		}
		pos = drop.transform.position;
		int num = TechDefinition.TECH_POINTS.IndexOf(drop.type);
		if (num < 0 || num > TechDefinition.TECH_POINTS.Count - 1)
		{
			Debug.LogError("Wrong DropTechPoint type: " + drop.type);
		}
		else
		{
			type = (TechPointsSpawner.Type)num;
		}
	}
}
