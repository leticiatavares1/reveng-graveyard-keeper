using Pathfinding;
using UnityEngine;
using UnityEngine.ProBuilder;

[DefaultExecutionOrder(1)]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(NavmeshCut))]
public class NavMeshCutMeshFilter : CustomNavMeshCutBakeSourceBase
{
	public enum NavMeshCutMeshFilterMode
	{
		MeshFilter,
		ProBuilderMesh
	}

	[SerializeField]
	private NavMeshCutMeshFilterMode mode;

	[SerializeField]
	private MeshFilter meshFilter;

	[SerializeField]
	private ProBuilderMesh proBuilderMesh;

	[SerializeField]
	private NavmeshCut navmeshCut;

	private void Start()
	{
		if (!(Object)(object)navmeshCut)
		{
			TryGetComponent<NavmeshCut>(out navmeshCut);
		}
		if (!(Object)(object)navmeshCut)
		{
			Debug.LogError("NavMeshCutMeshFilter is missing a required component: " + base.name);
			return;
		}
		switch (mode)
		{
		case NavMeshCutMeshFilterMode.MeshFilter:
			if (!meshFilter)
			{
				Debug.LogError("NavMeshCutMeshFilter is missing a required component: " + base.name);
				((Behaviour)(object)navmeshCut).enabled = false;
				return;
			}
			navmeshCut.mesh = meshFilter.mesh;
			break;
		case NavMeshCutMeshFilterMode.ProBuilderMesh:
			if (!proBuilderMesh)
			{
				Debug.LogError("NavMeshCutMeshFilter is missing a required component: " + base.name);
				((Behaviour)(object)navmeshCut).enabled = false;
				return;
			}
			navmeshCut.mesh = proBuilderMesh.mesh;
			break;
		}
		navmeshCut.type = NavmeshCut.MeshType.CustomMesh;
		((Behaviour)(object)navmeshCut).enabled = true;
		navmeshCut.ForceUpdate();
	}
}
