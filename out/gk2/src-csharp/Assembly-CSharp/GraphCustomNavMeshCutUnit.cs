using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GraphCustomNavMeshCutUnit : MonoBehaviour
{
	public struct SpawnEntry
	{
		public WgoPartData wgoPartData;

		public WgoPartBakedData.CustomNavMeshCutPrefabEntryBakedData bakedEntry;
	}

	private class SpawnedInstance
	{
		public GameObject instance;

		public AsyncOperationHandle<GameObject> handle;
	}

	private readonly List<SpawnedInstance> spawnedInstances = new List<SpawnedInstance>();

	public SGuid holder = SGuid.Empty;

	public void Assign(SGuid holder)
	{
		this.holder = holder;
	}

	public void Release()
	{
		CleanupSpawnedInstances();
		holder = SGuid.Empty;
	}

	public void UpdateParameters(Vector3 center, Vector3 scale, WgoData wgoData)
	{
		base.transform.position = center;
		base.transform.localScale = scale;
		RebuildInstances(wgoData);
	}

	public void UpdateTransform(Vector3 center, Vector3 scale)
	{
		base.transform.position = center;
		base.transform.localScale = scale;
	}

	public static bool HasBakedCustomNavMeshCuts(WgoData wgoData)
	{
		if (wgoData == null)
		{
			return false;
		}
		if (PartHasCustomNavMeshCuts(wgoData.MainWgoPartData))
		{
			return true;
		}
		foreach (WgoPartData additionalWgoPartsDatum in wgoData.AdditionalWgoPartsData)
		{
			if (PartHasCustomNavMeshCuts(additionalWgoPartsDatum))
			{
				return true;
			}
		}
		return false;
	}

	private static bool PartHasCustomNavMeshCuts(WgoPartData partData)
	{
		if (partData == null || string.IsNullOrEmpty(partData.BakedData?.id))
		{
			return false;
		}
		int stateHash = partData.GetStateHash();
		if (!partData.BakedData.TryGetCustomNavMeshCutPrefabs(stateHash, out var entries) || entries == null)
		{
			return false;
		}
		for (int i = 0; i < entries.Count; i++)
		{
			WgoPartBakedData.CustomNavMeshCutPrefabEntryBakedData customNavMeshCutPrefabEntryBakedData = entries[i];
			if (customNavMeshCutPrefabEntryBakedData?.prefabRef != null && customNavMeshCutPrefabEntryBakedData.prefabRef.RuntimeKeyIsValid())
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryCollectSpawnEntries(WgoData wgoData, out List<SpawnEntry> entries)
	{
		entries = new List<SpawnEntry>();
		if (wgoData == null)
		{
			return false;
		}
		CollectPartEntries(wgoData.MainWgoPartData, entries);
		foreach (WgoPartData additionalWgoPartsDatum in wgoData.AdditionalWgoPartsData)
		{
			CollectPartEntries(additionalWgoPartsDatum, entries);
		}
		return entries.Count > 0;
	}

	private static void CollectPartEntries(WgoPartData partData, List<SpawnEntry> entries)
	{
		if (partData == null || string.IsNullOrEmpty(partData.BakedData?.id))
		{
			return;
		}
		int stateHash = partData.GetStateHash();
		if (!partData.BakedData.TryGetCustomNavMeshCutPrefabs(stateHash, out var entries2) || entries2 == null)
		{
			return;
		}
		for (int i = 0; i < entries2.Count; i++)
		{
			WgoPartBakedData.CustomNavMeshCutPrefabEntryBakedData customNavMeshCutPrefabEntryBakedData = entries2[i];
			if (customNavMeshCutPrefabEntryBakedData?.prefabRef != null && customNavMeshCutPrefabEntryBakedData.prefabRef.RuntimeKeyIsValid())
			{
				entries.Add(new SpawnEntry
				{
					wgoPartData = partData,
					bakedEntry = customNavMeshCutPrefabEntryBakedData
				});
			}
		}
	}

	private void RebuildInstances(WgoData wgoData)
	{
		CleanupSpawnedInstances();
		if (wgoData == null)
		{
			return;
		}
		if (!TryCollectSpawnEntries(wgoData, out var entries))
		{
			Debug.LogWarning($"[GraphCustomNavMeshCutUnit] No spawn entries for WGO [{wgoData.id}] holder [{holder}], " + $"part [{wgoData.MainWgoPartData?.id}] hash [{wgoData.MainWgoPartData?.GetStateHash()}]");
			return;
		}
		for (int i = 0; i < entries.Count; i++)
		{
			SpawnEntry spawnEntry = entries[i];
			string assetGUID = spawnEntry.bakedEntry.prefabRef.AssetGUID;
			AsyncOperationHandle<GameObject> handle = spawnEntry.bakedEntry.prefabRef.InstantiateAsync(base.transform);
			GameObject gameObject = handle.WaitForCompletion();
			if (handle.Status != AsyncOperationStatus.Succeeded || gameObject == null)
			{
				Debug.LogWarning("[GraphCustomNavMeshCutUnit] Failed to spawn cut prefab [" + assetGUID + "] for WGO [" + wgoData.id + "] " + $"holder [{holder}] status [{handle.Status}] error [{handle.OperationException?.Message}]");
				if (handle.IsValid())
				{
					Addressables.ReleaseInstance(handle);
				}
				continue;
			}
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(value: true);
			}
			Transform obj = gameObject.transform;
			obj.localPosition = spawnEntry.bakedEntry.localPosition;
			obj.localRotation = spawnEntry.bakedEntry.localRotation;
			obj.localScale = spawnEntry.bakedEntry.localScale;
			NotifySpawned(gameObject, wgoData, spawnEntry.wgoPartData);
			spawnedInstances.Add(new SpawnedInstance
			{
				instance = gameObject,
				handle = handle
			});
			Debug.Log("[GraphCustomNavMeshCutUnit] Spawned [" + gameObject.name + "] prefab [" + assetGUID + "] for WGO [" + wgoData.id + "] " + $"holder [{holder}] localPos [{spawnEntry.bakedEntry.localPosition}]");
		}
	}

	private static void NotifySpawned(GameObject instance, WgoData wgoData, WgoPartData wgoPartData)
	{
		ICustomNavMeshCut[] componentsInChildren = instance.GetComponentsInChildren<ICustomNavMeshCut>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnCustomNavMeshCutSpawn(wgoData, wgoPartData);
		}
	}

	private void CleanupSpawnedInstances()
	{
		for (int num = spawnedInstances.Count - 1; num >= 0; num--)
		{
			SpawnedInstance spawnedInstance = spawnedInstances[num];
			if (spawnedInstance.handle.IsValid())
			{
				Addressables.ReleaseInstance(spawnedInstance.handle);
			}
			else if (spawnedInstance.instance != null)
			{
				Object.Destroy(spawnedInstance.instance);
			}
		}
		spawnedInstances.Clear();
	}
}
