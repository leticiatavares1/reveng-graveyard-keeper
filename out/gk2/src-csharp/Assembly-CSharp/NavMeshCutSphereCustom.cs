using Pathfinding;
using UnityEngine;

[DefaultExecutionOrder(1)]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(NavmeshCut))]
[ExecuteAlways]
public class NavMeshCutSphereCustom : CustomNavMeshCutBakeSourceBase
{
	[SerializeField]
	private SphereCollider sphereCollider;

	[SerializeField]
	private NavmeshCut navmeshCut;

	public bool dontUseCut;

	private new void Awake()
	{
		base.Awake();
		if ((bool)(Object)(object)navmeshCut)
		{
			((Behaviour)(object)navmeshCut).enabled = !dontUseCut;
		}
	}

	private void UpdateNavMeshCut()
	{
		if (!((Object)(object)navmeshCut == null))
		{
			navmeshCut.type = NavmeshCut.MeshType.Circle;
			navmeshCut.center = sphereCollider.center.XZ();
			navmeshCut.circleRadius = sphereCollider.radius;
			navmeshCut.height = sphereCollider.radius * 2f;
		}
	}
}
