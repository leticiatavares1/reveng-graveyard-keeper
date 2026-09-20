using Pathfinding;
using UnityEngine;

public class GraphUpdateSceneUnit : MonoBehaviour
{
	private const float MIN_BOX_AXIS = 0.01f;

	private BoxCollider graphUpdateBoxCollider;

	private GraphUpdateScene graphUpdateScene;

	private GraphMask graphUpdateMask = GraphMask.everything;

	private bool graphUpdateApplied;

	private bool graphUpdateSetWalkability;

	private bool graphUpdateUpdatePhysics;

	[SerializeField]
	private int graphUpdatePenaltyDelta;

	public SGuid holder = SGuid.Empty;

	public void Assign(SGuid holder)
	{
		this.holder = holder;
	}

	public void Release()
	{
		if (graphUpdateApplied)
		{
			ApplyGraphUpdate(graphUpdateMask, !graphUpdateSetWalkability, graphUpdateUpdatePhysics, -graphUpdatePenaltyDelta);
			graphUpdateApplied = false;
		}
		holder = SGuid.Empty;
	}

	public void UpdateParameters(GraphMask graphMask, Vector3 center, WgoPartBakedData.GraphUpdateSceneBoxData graphUpdateSceneBoxData)
	{
		graphUpdateMask = graphMask;
		UpdateParameters(center, graphUpdateSceneBoxData);
	}

	public void UpdateParameters(Vector3 center, WgoPartBakedData.GraphUpdateSceneBoxData graphUpdateSceneBoxData)
	{
		if (graphUpdateSceneBoxData != null)
		{
			EnsureComponents();
			if (!(graphUpdateBoxCollider == null) && !((Object)(object)graphUpdateScene == null))
			{
				graphUpdateSetWalkability = graphUpdateSceneBoxData.setWalkability;
				graphUpdateUpdatePhysics = graphUpdateSceneBoxData.updatePhysics;
				graphUpdatePenaltyDelta = graphUpdateSceneBoxData.penaltyDelta;
				graphUpdateApplied = true;
				base.transform.position = center + graphUpdateSceneBoxData.localCenter;
				graphUpdateBoxCollider.center = Vector3.zero;
				graphUpdateBoxCollider.size = new Vector3(Mathf.Max(graphUpdateSceneBoxData.size.x, 0.01f), Mathf.Max(graphUpdateSceneBoxData.size.y, 0.01f), Mathf.Max(graphUpdateSceneBoxData.size.z, 0.01f));
				ApplyGraphUpdate(graphUpdateMask, graphUpdateSetWalkability, graphUpdateUpdatePhysics, graphUpdatePenaltyDelta);
			}
		}
	}

	private void EnsureComponents()
	{
		if (graphUpdateBoxCollider == null)
		{
			TryGetComponent<BoxCollider>(out graphUpdateBoxCollider);
			if (graphUpdateBoxCollider == null)
			{
				graphUpdateBoxCollider = base.gameObject.AddComponent<BoxCollider>();
			}
		}
		if ((Object)(object)graphUpdateScene == null)
		{
			TryGetComponent<GraphUpdateScene>(out graphUpdateScene);
			if ((Object)(object)graphUpdateScene == null)
			{
				graphUpdateScene = base.gameObject.AddComponent<GraphUpdateScene>();
			}
		}
	}

	private void ApplyGraphUpdate(GraphMask graphMask, bool setWalkability, bool updatePhysics, int penaltyDelta = 0)
	{
		if (!((Object)(object)graphUpdateScene == null) && !((Object)(object)AstarPath.active == null))
		{
			graphUpdateScene.applyOnStart = true;
			graphUpdateScene.applyOnScan = true;
			graphUpdateScene.modifyWalkability = true;
			graphUpdateScene.setWalkability = setWalkability;
			graphUpdateScene.updatePhysics = updatePhysics;
			graphUpdateScene.updateErosion = true;
			graphUpdateScene.resetPenaltyOnPhysics = true;
			graphUpdateScene.penaltyDelta = penaltyDelta;
			GraphUpdateObject graphUpdate = graphUpdateScene.GetGraphUpdate();
			if (graphUpdate != null)
			{
				graphUpdate.graphMask = graphMask;
				AstarPath.active.UpdateGraphs(graphUpdate);
			}
		}
	}
}
