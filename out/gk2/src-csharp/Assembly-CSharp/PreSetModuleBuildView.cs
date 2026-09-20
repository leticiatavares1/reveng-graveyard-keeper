using UnityEngine;

public class PreSetModuleBuildView : MonoBehaviour
{
	public Transform planeTransform;

	private Vector3 size;

	public Bounds Bounds => new Bounds(base.transform.position, size);

	public void SetPositionAndScaleAs(BuildArea buildArea)
	{
		Collider collider = buildArea.Collider;
		if (!(collider == null))
		{
			base.transform.position = collider.bounds.center;
			size = collider.bounds.size;
			if (planeTransform != null)
			{
				planeTransform.localScale = collider.bounds.size * 0.1f;
			}
		}
	}

	public bool IsFullyInsideIn(Bounds otherBounds)
	{
		Bounds bounds = Bounds;
		Vector3 vector = new Vector3(bounds.min.x, otherBounds.center.y, bounds.min.z);
		Vector3 vector2 = new Vector3(bounds.max.x, otherBounds.center.y, bounds.max.z);
		float num = 0.001f;
		bool num2 = vector.x >= otherBounds.min.x - num && vector.x <= otherBounds.max.x + num && vector.z >= otherBounds.min.z - num && vector.z <= otherBounds.max.z + num;
		bool flag = vector2.x >= otherBounds.min.x - num && vector2.x <= otherBounds.max.x + num && vector2.z >= otherBounds.min.z - num && vector2.z <= otherBounds.max.z + num;
		return num2 && flag;
	}
}
