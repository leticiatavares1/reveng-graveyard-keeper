using System;
using System.Collections.Generic;

[Serializable]
public class WgoDelayedSpawnSystemData
{
	public List<SpawnDelayedObject> spawnDelayedObjects = new List<SpawnDelayedObject>();

	public bool Contains(WgoData wgoData)
	{
		if (wgoData == null)
		{
			return false;
		}
		SGuid uniqueId = wgoData.UniqueId;
		for (int i = 0; i < spawnDelayedObjects.Count; i++)
		{
			SpawnDelayedObject spawnDelayedObject = spawnDelayedObjects[i];
			if (spawnDelayedObject != null)
			{
				if (!SGuid.IsNullOrEmpty(spawnDelayedObject.wgoUniqueId) && spawnDelayedObject.wgoUniqueId == uniqueId)
				{
					return true;
				}
				if (spawnDelayedObject.wgoData != null && spawnDelayedObject.wgoData.UniqueId == uniqueId)
				{
					return true;
				}
			}
		}
		return false;
	}
}
