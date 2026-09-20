using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnZone : MonoBehaviour
{
	public string id;

	public bool useYFromGO = true;

	public List<BoxCollider> colliders = new List<BoxCollider>();

	private bool initialActiveState = true;

	[Header("Penalty")]
	public bool setWalkPenaltyTag;

	public List<PathfindingPenalty> pathfindingPenalties = new List<PathfindingPenalty>();

	[SerializeField]
	[Space]
	private bool hasCustomEnemyDataSet;

	public List<EnemyData> customEnemyData = new List<EnemyData>();

	public static EnemySpawnZone Create()
	{
		return new GameObject().AddComponent<EnemySpawnZone>();
	}

	public Vector3 GetRandomPosFromZone(out List<PathfindingPenalty> penalties)
	{
		penalties = (setWalkPenaltyTag ? pathfindingPenalties : null);
		if (colliders.Count == 0)
		{
			return base.transform.position;
		}
		Collider random = colliders.GetRandom();
		return GetInsidePoint(random);
	}

	public void ResetActiveState()
	{
		base.gameObject.SetActive(initialActiveState);
	}

	private Vector3 GetInsidePoint(Collider collider, int maxAttempts = 32)
	{
		if (collider is BoxCollider boxCollider)
		{
			Vector3 vector = new Vector3(Random.Range((0f - boxCollider.size.x) * 0.5f, boxCollider.size.x * 0.5f), Random.Range((0f - boxCollider.size.y) * 0.5f, boxCollider.size.y * 0.5f), Random.Range((0f - boxCollider.size.z) * 0.5f, boxCollider.size.z * 0.5f));
			Vector3 result = boxCollider.transform.TransformPoint(boxCollider.center + vector);
			if (useYFromGO)
			{
				result.y = base.transform.position.y;
			}
			return result;
		}
		if (collider is SphereCollider sphereCollider)
		{
			Vector3 vector2 = sphereCollider.transform.TransformPoint(sphereCollider.center);
			Vector3 lossyScale = sphereCollider.transform.lossyScale;
			float num = sphereCollider.radius * Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
			Vector3 result2 = vector2 + Random.insideUnitSphere * num;
			if (useYFromGO)
			{
				result2.y = base.transform.position.y;
			}
			return result2;
		}
		Bounds bounds = collider.bounds;
		for (int i = 0; i < maxAttempts; i++)
		{
			Vector3 vector3 = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
			if ((collider.ClosestPoint(vector3) - vector3).magnitude.EqualsTo(0f))
			{
				if (useYFromGO)
				{
					vector3.y = base.transform.position.y;
				}
				return vector3;
			}
		}
		Vector3 center = collider.bounds.center;
		if (useYFromGO)
		{
			center.y = base.transform.position.y;
		}
		return center;
	}

	private void Awake()
	{
		initialActiveState = base.gameObject.activeSelf;
	}
}
