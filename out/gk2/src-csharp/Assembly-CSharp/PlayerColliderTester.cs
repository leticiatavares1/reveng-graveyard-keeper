using UnityEngine;

public class PlayerColliderTester : MonoBehaviour
{
	private const float MAX_PENETRATION_DISTANCE = 0.05f;

	private static Collider[] neighbours;

	private static Collider thisCollider;

	private void OnEnable()
	{
		neighbours = new Collider[10];
		thisCollider = GetComponent<Collider>();
		if (!thisCollider)
		{
			Debug.LogWarning("PlayerColliderTester requires a Collider component.", this);
		}
	}

	public static bool IsPositionReachable(Vector3 position)
	{
		if (thisCollider == null)
		{
			return false;
		}
		position = new Vector3(position.x, thisCollider.transform.position.y, position.z);
		int num = Physics.OverlapSphereNonAlloc(position, 0.5f, neighbours, 256);
		for (int i = 0; i < num; i++)
		{
			Collider collider = neighbours[i];
			if ((bool)collider && !(collider == thisCollider) && Physics.ComputePenetration(thisCollider, position, thisCollider.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var _, out var distance) && distance > 0.05f)
			{
				return false;
			}
		}
		return true;
	}
}
