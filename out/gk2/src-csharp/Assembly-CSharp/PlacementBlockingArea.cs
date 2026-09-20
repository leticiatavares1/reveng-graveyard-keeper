using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Collider))]
public class PlacementBlockingArea : MonoBehaviour
{
	[SerializeField]
	private Collider collider;

	public Collider Collider => collider;

	public static bool TryGet(Collider col, out PlacementBlockingArea blockingArea)
	{
		blockingArea = null;
		if (col != null)
		{
			return col.TryGetComponent<PlacementBlockingArea>(out blockingArea);
		}
		return false;
	}

	public static bool IsBlockingFor(Collider col, BuildingDef placingDef, Wgo selfTarget = null)
	{
		if (!TryGet(col, out var _) || placingDef == null)
		{
			return false;
		}
		Wgo componentInParent = col.GetComponentInParent<Wgo>();
		if (componentInParent == null || componentInParent.Data?.Definition == null)
		{
			return false;
		}
		if (componentInParent == selfTarget || componentInParent.Data.isTempObject)
		{
			return false;
		}
		return placingDef.IsAlwaysBlockedByWgoGroup(componentInParent.Data.Definition.wgoGroup);
	}
}
