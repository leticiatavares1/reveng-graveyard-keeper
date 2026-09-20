using Pathfinding;
using UnityEngine;

public class GraphCutUnit : MonoBehaviour
{
	private const float Y_SIZE = 0.1f;

	private Mesh customCutMesh;

	public NavmeshCut navmeshCut;

	public SGuid holder = SGuid.Empty;

	public void Assign(SGuid holder)
	{
		this.holder = holder;
	}

	public void Release()
	{
		holder = SGuid.Empty;
		ClearCustomMesh();
		if ((Object)(object)navmeshCut != null)
		{
			((Behaviour)(object)navmeshCut).enabled = false;
		}
	}

	public void UpdateParameters(GraphMask graphMask, Vector3 center, Vector2 size)
	{
		navmeshCut.graphMask = graphMask;
		UpdateParameters(center, size);
	}

	public void UpdateParameters(Vector3 center, Vector2 size)
	{
		navmeshCut.type = NavmeshCut.MeshType.Box;
		navmeshCut.mesh = null;
		((Component)(object)navmeshCut).transform.position = center;
		navmeshCut.rectangleSize = size;
		navmeshCut.height = 0.1f;
		((Behaviour)(object)navmeshCut).enabled = true;
		navmeshCut.ForceUpdate();
	}

	public void UpdateParameters(GraphMask graphMask, Vector3 center, float radius)
	{
		navmeshCut.graphMask = graphMask;
		UpdateParameters(center, radius);
	}

	public void UpdateParameters(Vector3 center, float radius)
	{
		navmeshCut.type = NavmeshCut.MeshType.Sphere;
		navmeshCut.mesh = null;
		((Component)(object)navmeshCut).transform.position = center;
		navmeshCut.circleRadius = radius;
		navmeshCut.height = 0.1f;
		((Behaviour)(object)navmeshCut).enabled = true;
		navmeshCut.ForceUpdate();
	}

	public void UpdateParameters(GraphMask graphMask, Vector3 center, WgoPartBakedData.PlannerMeshData plannerMeshData)
	{
		navmeshCut.graphMask = graphMask;
		UpdateParameters(center, plannerMeshData);
	}

	public void UpdateParameters(Vector3 center, WgoPartBakedData.PlannerMeshData plannerMeshData)
	{
		if (WgoPartBakedData.TryCreateMesh(plannerMeshData, $"PlannerCut_{holder}_{plannerMeshData?.hash}", out var mesh))
		{
			ClearCustomMesh();
			customCutMesh = mesh;
			navmeshCut.type = NavmeshCut.MeshType.CustomMesh;
			((Component)(object)navmeshCut).transform.position = center;
			navmeshCut.center = Vector3.zero;
			navmeshCut.meshScale = 1f;
			navmeshCut.height = 0.1f;
			navmeshCut.mesh = customCutMesh;
			((Behaviour)(object)navmeshCut).enabled = true;
			navmeshCut.ForceUpdate();
		}
	}

	private void ClearCustomMesh()
	{
		if ((Object)(object)navmeshCut != null)
		{
			navmeshCut.mesh = null;
		}
		if (!(customCutMesh == null))
		{
			Object.Destroy(customCutMesh);
			customCutMesh = null;
		}
	}
}
