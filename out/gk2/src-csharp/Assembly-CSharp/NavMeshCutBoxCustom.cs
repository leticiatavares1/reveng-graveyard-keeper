using Pathfinding;
using UnityEngine;

[DefaultExecutionOrder(1)]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(NavmeshCut))]
[ExecuteAlways]
public class NavMeshCutBoxCustom : CustomNavMeshCutBakeSourceBase
{
	[SerializeField]
	private BoxCollider boxCollider;

	[SerializeField]
	private NavmeshCut navmeshCut;

	public bool dontUseCut;

	public NavmeshCut.RadiusExpansionMode radiusExpansionMode = NavmeshCut.RadiusExpansionMode.ExpandByAgentRadius;

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
			navmeshCut.center = boxCollider.center.XZ();
			navmeshCut.rectangleSize.x = boxCollider.size.x;
			navmeshCut.rectangleSize.y = boxCollider.size.z;
			navmeshCut.height = boxCollider.size.y;
			navmeshCut.radiusExpansionMode = radiusExpansionMode;
		}
	}
}
