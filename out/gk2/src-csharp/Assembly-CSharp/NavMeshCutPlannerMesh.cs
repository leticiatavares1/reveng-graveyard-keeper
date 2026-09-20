using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[DefaultExecutionOrder(1)]
[RequireComponent(typeof(NavmeshCut))]
[ExecuteAlways]
public class NavMeshCutPlannerMesh : CustomNavMeshCutBakeSourceBase
{
	[SerializeField]
	private NavmeshCut navmeshCut;

	[SerializeField]
	private WgoPart wgoPart;

	public bool dontUseCut;

	private readonly Dictionary<int, Mesh> cachedMeshesByHash = new Dictionary<int, Mesh>();

	private WgoPartData subscribedData;

	private new void Awake()
	{
		base.Awake();
		TryResolveReferences();
		SyncNavMeshCut();
	}

	private void OnEnable()
	{
		TryResolveReferences();
		TrySubscribe();
		SyncNavMeshCut();
	}

	private void OnDisable()
	{
		TryUnsubscribe();
	}

	private void Update()
	{
		if (subscribedData == null)
		{
			TryResolveReferences();
			TrySubscribe();
		}
	}

	private void TryResolveReferences()
	{
		if ((Object)(object)navmeshCut == null)
		{
			TryGetComponent<NavmeshCut>(out navmeshCut);
		}
		if (wgoPart == null)
		{
			wgoPart = GetComponentInParent<WgoPart>(includeInactive: true);
		}
	}

	private void TrySubscribe()
	{
		WgoPartData wgoPartData = subscribedData ?? wgoPart?.WgoPartData;
		if (wgoPartData != null && subscribedData != wgoPartData)
		{
			TryUnsubscribe();
			subscribedData = wgoPartData;
			subscribedData.OnStateChange += HandleWgoPartStateChanged;
		}
	}

	private void TryUnsubscribe()
	{
		if (subscribedData != null)
		{
			subscribedData.OnStateChange -= HandleWgoPartStateChanged;
			subscribedData = null;
		}
	}

	private void HandleWgoPartStateChanged(string variationId, int rotationIndex)
	{
		SyncNavMeshCut(WgoPartData.GetStateHash(variationId, rotationIndex));
	}

	private void SyncNavMeshCut(int stateHash = int.MinValue)
	{
		if ((Object)(object)navmeshCut == null)
		{
			return;
		}
		navmeshCut.type = NavmeshCut.MeshType.CustomMesh;
		navmeshCut.center = Vector3.zero;
		navmeshCut.meshScale = 1f;
		((Behaviour)(object)navmeshCut).enabled = !dontUseCut;
		if (!((Behaviour)(object)navmeshCut).enabled)
		{
			navmeshCut.mesh = null;
			return;
		}
		WgoPartData wgoPartData = subscribedData ?? wgoPart?.WgoPartData;
		if (wgoPartData?.BakedData == null)
		{
			navmeshCut.mesh = null;
			return;
		}
		int num = ((stateHash == int.MinValue) ? wgoPartData.GetStateHash() : stateHash);
		if (!wgoPartData.BakedData.TryGetPlannerMeshData(num, out var data))
		{
			navmeshCut.mesh = null;
			return;
		}
		if (!cachedMeshesByHash.TryGetValue(num, out var value) || value == null)
		{
			if (!WgoPartBakedData.TryCreateMesh(data, $"PlannerMesh_{wgoPart?.Id ?? base.gameObject.name}_{num}", out value))
			{
				navmeshCut.mesh = null;
				return;
			}
			cachedMeshesByHash[num] = value;
		}
		navmeshCut.mesh = value;
	}

	public override void OnCustomNavMeshCutSpawn(WgoData wgoData, WgoPartData wgoPartData)
	{
		base.OnCustomNavMeshCutSpawn(wgoData, wgoPartData);
		subscribedData = wgoPartData;
		TryResolveReferences();
		TrySubscribe();
		SyncNavMeshCut();
	}
}
