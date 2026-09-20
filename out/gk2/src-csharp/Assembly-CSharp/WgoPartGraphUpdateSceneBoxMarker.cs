using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class WgoPartGraphUpdateSceneBoxMarker : MonoBehaviour
{
	[SerializeField]
	private BoxCollider sourceBoxCollider;

	[SerializeField]
	private bool setWalkability = true;

	[SerializeField]
	private bool updatePhysics;

	[SerializeField]
	private int penaltyDelta;

	public BoxCollider SourceBoxCollider
	{
		get
		{
			if (sourceBoxCollider == null)
			{
				TryGetComponent<BoxCollider>(out sourceBoxCollider);
			}
			return sourceBoxCollider;
		}
	}

	public WgoPartBakedData.GraphUpdateSceneBoxData ToBakedData(Transform rootTransform, bool mirror)
	{
		BoxCollider boxCollider = SourceBoxCollider;
		if (boxCollider == null)
		{
			return null;
		}
		Bounds bounds = boxCollider.bounds;
		Vector3 localCenter = rootTransform.InverseTransformPoint(bounds.center);
		if (mirror)
		{
			localCenter.x = 0f - localCenter.x;
		}
		WgoPartBakedData.GraphUpdateSceneBoxData graphUpdateSceneBoxData = new WgoPartBakedData.GraphUpdateSceneBoxData();
		graphUpdateSceneBoxData.localCenter = localCenter;
		graphUpdateSceneBoxData.size = bounds.size;
		graphUpdateSceneBoxData.setWalkability = setWalkability;
		graphUpdateSceneBoxData.updatePhysics = updatePhysics;
		graphUpdateSceneBoxData.SetPenaltyDelta(penaltyDelta);
		return graphUpdateSceneBoxData;
	}

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}
}
