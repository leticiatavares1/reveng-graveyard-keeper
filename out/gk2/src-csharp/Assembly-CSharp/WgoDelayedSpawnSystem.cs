using System.Collections.Generic;
using UnityEngine;

public class WgoDelayedSpawnSystem : ICustomUpdatable
{
	private float minSpawnDistance = -1f;

	private WgoDelayedSpawnSystemData data;

	private WgoDelayedSpawnSystemData Data => data ?? MainGame.Instance.GameSave.wgoDelayedSpawnSystemData;

	private float MinSpawnDistance
	{
		get
		{
			if (minSpawnDistance < 0f)
			{
				minSpawnDistance = GameBalance.Me.GetData<ConstDef>("spawn_distance").FloatValue;
			}
			return minSpawnDistance;
		}
	}

	public void CustomUpdate(float deltaTime)
	{
		if (Data == null)
		{
			return;
		}
		List<SpawnDelayedObject> spawnDelayedObjects = Data.spawnDelayedObjects;
		for (int num = spawnDelayedObjects.Count - 1; num >= 0; num--)
		{
			if (TrySpawn(spawnDelayedObjects[num]))
			{
				spawnDelayedObjects.RemoveAt(num);
			}
		}
	}

	public void Add(WgoData wgoData, CraftElementBase craftElement)
	{
		Data.spawnDelayedObjects.Add(new SpawnDelayedObject(craftElement as CraftElement, wgoData));
	}

	public bool CanSpawn(WgoData wgoData)
	{
		return Vector3.Distance(wgoData.Position, MainGame.PlayerData.position.Value) > MinSpawnDistance;
	}

	public bool Contains(WgoData wgoData)
	{
		return Data.Contains(wgoData);
	}

	private bool TrySpawn(SpawnDelayedObject spawnDelayedWgo)
	{
		WgoData resolvedWgoData = spawnDelayedWgo.ResolvedWgoData;
		if (resolvedWgoData == null)
		{
			return true;
		}
		if (!CanSpawn(resolvedWgoData))
		{
			return false;
		}
		CraftElement craftElement = spawnDelayedWgo.craftElement;
		if (craftElement == null)
		{
			return true;
		}
		craftElement.BindCraftable(resolvedWgoData);
		craftElement.Count = 1;
		resolvedWgoData.CraftComponent.AddCraftNoStart(craftElement);
		resolvedWgoData.OnCraftEnd(craftElement);
		return true;
	}
}
