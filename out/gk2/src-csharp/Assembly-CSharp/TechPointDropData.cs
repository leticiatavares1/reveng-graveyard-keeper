using System;
using UnityEngine;

[Serializable]
public class TechPointDropData
{
	public Vector3 pos;

	public TechPointsSpawner.Type type;

	public string worldId;

	public TechPointDropData()
	{
	}

	public TechPointDropData(Vector3 pos, TechPointsSpawner.Type type, string worldId)
	{
		this.pos = pos;
		this.type = type;
		this.worldId = worldId;
	}
}
