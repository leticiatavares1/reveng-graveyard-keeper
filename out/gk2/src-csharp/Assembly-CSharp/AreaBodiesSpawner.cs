using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class AreaBodiesSpawner : MonoBehaviour
{
	private struct BodyFlowData
	{
		public float speedJitter;
	}

	public Collider col;

	public float baseY;

	[Range(0f, 1f)]
	public float yRange = 0.2f;

	public int bodiesCount = 50;

	public GameObject bodyPrefab;

	public GameObject bodiesParent;

	public List<Rigidbody> preSetBodies = new List<Rigidbody>();

	public SplineContainer flowSpline;

	[SerializeField]
	private List<GameObject> spawnedBodies = new List<GameObject>();

	[Header("River Flow Settings")]
	public bool flowActive;

	public float flowSpeed = 1.5f;

	[Min(0f)]
	public float accelerationGain = 3f;

	[Min(0f)]
	public float flowSplineMaxDistance = 10f;

	[Range(0f, 1f)]
	public float speedRandomness = 0.25f;

	[Header("Flow Mode")]
	public bool transformFlowActive;

	[Min(0f)]
	public float transformFlowMaxDelay = 1f;

	[Header("Rigidbodies")]
	[Min(0f)]
	public float linearDrag = 1f;

	[Min(0f)]
	public float angularDrag = 0.5f;

	[Header("Editor No-Overlap Spawn")]
	[Min(0f)]
	public float noOverlapPadding = 0.05f;

	[Min(1f)]
	public int noOverlapMaxAttemptsPerBody = 200;

	public List<GameObject> objToDisableOnFlowStart = new List<GameObject>();

	private List<Vector3> presetBodiesPositions = new List<Vector3>();

	private List<Vector3> bodiesPositionsOnStart = new List<Vector3>();

	private readonly Dictionary<Rigidbody, BodyFlowData> bodyFlowData = new Dictionary<Rigidbody, BodyFlowData>();

	private float transformFlowStartTime;

	public void SpawnBodies()
	{
		if (spawnedBodies.Count > 0)
		{
			ClearBodies();
		}
		if (col == null || bodyPrefab == null)
		{
			Debug.LogWarning("Collider or Body Prefab not set.", this);
			return;
		}
		Bounds bounds = col.bounds;
		for (int i = 0; i < bodiesCount; i++)
		{
			bool flag = false;
			int num = 100;
			Vector3 vector;
			do
			{
				vector = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), UnityEngine.Random.Range(bounds.min.y, bounds.max.y), UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
				flag = col.ClosestPoint(vector) == vector;
			}
			while (!flag && --num > 0);
			if (!flag)
			{
				vector = bounds.center;
				Debug.LogWarning("Could not find a point inside collider '" + col.name + "' after 100 attempts. Using bounds center as fallback.", this);
			}
			vector.y = baseY + UnityEngine.Random.Range(0f - yRange, 0f);
			GameObject gameObject = UnityEngine.Object.Instantiate(bodyPrefab, vector, Quaternion.identity);
			gameObject.transform.SetParent(bodiesParent.transform, worldPositionStays: true);
			gameObject.gameObject.SetActive(value: true);
			spawnedBodies.Add(gameObject);
			if (gameObject.TryGetComponent<Rigidbody>(out var component))
			{
				component = gameObject.AddComponent<Rigidbody>();
			}
			component.useGravity = false;
			for (int j = 0; j < preSetBodies.Count; j++)
			{
				Rigidbody rigidbody = preSetBodies[j];
				rigidbody.transform.position = presetBodiesPositions[j];
				spawnedBodies.Add(rigidbody.gameObject);
			}
		}
	}

	private bool IsPointInsideCollider(Vector3 point, Collider targetCollider)
	{
		bool flag = true;
		if (targetCollider is MeshCollider meshCollider)
		{
			flag = meshCollider.convex;
		}
		if (flag)
		{
			return targetCollider.ClosestPoint(point) == point;
		}
		bool queriesHitBackfaces = Physics.queriesHitBackfaces;
		Physics.queriesHitBackfaces = true;
		int num = 0;
		RaycastHit[] array = Physics.RaycastAll(new Ray(point, Vector3.up), float.PositiveInfinity);
		foreach (RaycastHit raycastHit in array)
		{
			if (raycastHit.collider == targetCollider)
			{
				num++;
			}
		}
		Physics.queriesHitBackfaces = queriesHitBackfaces;
		return num % 2 == 1;
	}

	public void SpawnBodies_NoOverlap()
	{
		if (spawnedBodies.Count > 0)
		{
			ClearBodies();
		}
		if (col == null || bodyPrefab == null)
		{
			Debug.LogWarning("Collider or Body Prefab not set.", this);
			return;
		}
		Vector3 prefabHalfExtentsWorld = GetPrefabHalfExtentsWorld();
		prefabHalfExtentsWorld += Vector3.one * Mathf.Max(0f, noOverlapPadding);
		Bounds bounds = col.bounds;
		List<Bounds> list = new List<Bounds>();
		for (int i = 0; i < preSetBodies.Count; i++)
		{
			Rigidbody rigidbody = preSetBodies[i];
			if ((bool)rigidbody)
			{
				if (i < presetBodiesPositions.Count)
				{
					rigidbody.transform.position = presetBodiesPositions[i];
				}
				Bounds item = ComputeWorldBoundsFromColliders(rigidbody.gameObject);
				item.Expand(noOverlapPadding * 2f);
				list.Add(item);
				if (!spawnedBodies.Contains(rigidbody.gameObject))
				{
					spawnedBodies.Add(rigidbody.gameObject);
				}
			}
		}
		float num = Mathf.Max(prefabHalfExtentsWorld.x * 2f, prefabHalfExtentsWorld.z * 2f);
		int num2 = Mathf.CeilToInt((bounds.max.x - bounds.min.x) / num);
		int num3 = Mathf.CeilToInt((bounds.max.z - bounds.min.z) / num);
		bool[,] array = new bool[num2, num3];
		foreach (Bounds item3 in list)
		{
			int b = Mathf.FloorToInt((item3.min.x - bounds.min.x) / num);
			int b2 = Mathf.CeilToInt((item3.max.x - bounds.min.x) / num);
			int b3 = Mathf.FloorToInt((item3.min.z - bounds.min.z) / num);
			int b4 = Mathf.CeilToInt((item3.max.z - bounds.min.z) / num);
			for (int j = Mathf.Max(0, b); j < Mathf.Min(num2, b2); j++)
			{
				for (int k = Mathf.Max(0, b3); k < Mathf.Min(num3, b4); k++)
				{
					array[j, k] = true;
				}
			}
		}
		List<Vector2Int> list2 = new List<Vector2Int>();
		int num4 = num2 * num3;
		for (int l = 0; l < num2; l++)
		{
			for (int m = 0; m < num3; m++)
			{
				if (array[l, m])
				{
					continue;
				}
				float num5 = bounds.min.x + (float)l * num;
				float x = bounds.min.x + (float)(l + 1) * num;
				float num6 = bounds.min.z + (float)m * num;
				float z = bounds.min.z + (float)(m + 1) * num;
				Vector3[] obj = new Vector3[5]
				{
					new Vector3(num5 + num * 0.5f, baseY, num6 + num * 0.5f),
					new Vector3(num5, baseY, num6),
					new Vector3(x, baseY, num6),
					new Vector3(num5, baseY, z),
					new Vector3(x, baseY, z)
				};
				bool flag = false;
				Vector3[] array2 = obj;
				foreach (Vector3 point in array2)
				{
					if (IsPointInsideCollider(point, col))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list2.Add(new Vector2Int(l, m));
				}
			}
		}
		int num7 = 0;
		for (int num8 = 0; num8 < num2; num8++)
		{
			for (int num9 = 0; num9 < num3; num9++)
			{
				if (array[num8, num9])
				{
					num7++;
				}
			}
		}
		Debug.Log($"Grid: {num2}x{num3} ({num4} total), occupied by presets: {num7}, cells intersecting collider: {list2.Count}");
		for (int num10 = 0; num10 < bodiesCount; num10++)
		{
			if (list2.Count <= 0)
			{
				break;
			}
			bool flag2 = false;
			for (int num11 = 0; num11 < Mathf.Min(noOverlapMaxAttemptsPerBody, list2.Count); num11++)
			{
				int index = UnityEngine.Random.Range(0, list2.Count);
				Vector2Int vector2Int = list2[index];
				float x2 = bounds.min.x + ((float)vector2Int.x + 0.5f) * num;
				float z2 = bounds.min.z + ((float)vector2Int.y + 0.5f) * num;
				Vector3 vector = new Vector3(x2, baseY + UnityEngine.Random.Range(0f - yRange, 0f), z2);
				if (!IsPointInsideCollider(vector, col))
				{
					list2.RemoveAt(index);
					continue;
				}
				Bounds bounds2 = new Bounds(vector, prefabHalfExtentsWorld * 2f);
				bounds2.Expand(noOverlapPadding * 2f);
				bool flag3 = false;
				for (int num12 = 0; num12 < list.Count; num12++)
				{
					if (list[num12].Intersects(bounds2))
					{
						flag3 = true;
						break;
					}
				}
				if (flag3)
				{
					list2.RemoveAt(index);
					continue;
				}
				GameObject gameObject = null;
				gameObject = UnityEngine.Object.Instantiate(bodyPrefab, vector, Quaternion.identity);
				gameObject.transform.SetParent(bodiesParent.transform, worldPositionStays: true);
				gameObject.gameObject.SetActive(value: true);
				spawnedBodies.Add(gameObject);
				if (!gameObject.GetComponent<Rigidbody>())
				{
					gameObject.AddComponent<Rigidbody>().useGravity = false;
				}
				Bounds item2 = ComputeWorldBoundsFromColliders(gameObject);
				item2.Expand(noOverlapPadding * 2f);
				list.Add(item2);
				int b5 = Mathf.FloorToInt((item2.min.x - bounds.min.x) / num);
				int b6 = Mathf.CeilToInt((item2.max.x - bounds.min.x) / num);
				int b7 = Mathf.FloorToInt((item2.min.z - bounds.min.z) / num);
				int b8 = Mathf.CeilToInt((item2.max.z - bounds.min.z) / num);
				for (int num13 = Mathf.Max(0, b5); num13 < Mathf.Min(num2, b6); num13++)
				{
					for (int num14 = Mathf.Max(0, b7); num14 < Mathf.Min(num3, b8); num14++)
					{
						array[num13, num14] = true;
					}
				}
				list2.RemoveAt(index);
				flag2 = true;
				break;
			}
			if (!flag2)
			{
				Debug.LogWarning($"Could not place body {num10 + 1}/{bodiesCount} without overlap after {noOverlapMaxAttemptsPerBody} attempts", this);
			}
		}
	}

	public void SpawnBodies_Poisson()
	{
		if (spawnedBodies.Count > 0)
		{
			ClearBodies();
		}
		if (col == null || bodyPrefab == null)
		{
			Debug.LogWarning("Collider or Body Prefab not set.", this);
			return;
		}
		Vector3 prefabHalfExtentsWorld = GetPrefabHalfExtentsWorld();
		prefabHalfExtentsWorld += Vector3.one * Mathf.Max(0f, noOverlapPadding);
		Bounds bounds = col.bounds;
		List<Bounds> list = new List<Bounds>();
		for (int i = 0; i < preSetBodies.Count; i++)
		{
			Rigidbody rigidbody = preSetBodies[i];
			if ((bool)rigidbody)
			{
				if (i < presetBodiesPositions.Count)
				{
					rigidbody.transform.position = presetBodiesPositions[i];
				}
				Bounds item = ComputeWorldBoundsFromColliders(rigidbody.gameObject);
				item.Expand(noOverlapPadding * 2f);
				list.Add(item);
				if (!spawnedBodies.Contains(rigidbody.gameObject))
				{
					spawnedBodies.Add(rigidbody.gameObject);
				}
			}
		}
		float minDistance = Mathf.Max(prefabHalfExtentsWorld.x * 2f, prefabHalfExtentsWorld.z * 2f);
		List<Vector2> list2 = GeneratePoissonSamples(bounds, minDistance, col, list, bodiesCount);
		Debug.Log($"Poisson sampling generated {list2.Count} valid positions");
		for (int j = 0; j < Mathf.Min(list2.Count, bodiesCount); j++)
		{
			Vector3 position = new Vector3(list2[j].x, baseY + UnityEngine.Random.Range(0f - yRange, 0f), list2[j].y);
			GameObject gameObject = null;
			gameObject = UnityEngine.Object.Instantiate(bodyPrefab, position, Quaternion.identity);
			gameObject.transform.SetParent(bodiesParent.transform, worldPositionStays: true);
			gameObject.gameObject.SetActive(value: true);
			if (gameObject.TryGetComponent<BodyTrailer>(out var component))
			{
				component.RollAndSetTexture();
			}
			spawnedBodies.Add(gameObject);
			if (!gameObject.GetComponent<Rigidbody>())
			{
				gameObject.AddComponent<Rigidbody>().useGravity = false;
			}
			Bounds item2 = ComputeWorldBoundsFromColliders(gameObject);
			item2.Expand(noOverlapPadding * 2f);
			list.Add(item2);
		}
		Debug.Log($"Spawn completed: {spawnedBodies.Count} bodies total ({preSetBodies.Count} preset + {spawnedBodies.Count - preSetBodies.Count} spawned)");
	}

	private List<Vector2> GeneratePoissonSamples(Bounds areaBounds, float minDistance, Collider collider, List<Bounds> obstacles, int maxSamples)
	{
		List<Vector2> list = new List<Vector2>();
		List<Vector2> list2 = new List<Vector2>();
		float num = minDistance / Mathf.Sqrt(2f);
		int num2 = Mathf.CeilToInt((areaBounds.max.x - areaBounds.min.x) / num);
		int num3 = Mathf.CeilToInt((areaBounds.max.z - areaBounds.min.z) / num);
		List<Vector2>[,] array = new List<Vector2>[num2, num3];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				array[i, j] = new List<Vector2>();
			}
		}
		foreach (Bounds obstacle in obstacles)
		{
			int b = Mathf.FloorToInt((obstacle.min.x - areaBounds.min.x) / num);
			int b2 = Mathf.CeilToInt((obstacle.max.x - areaBounds.min.x) / num);
			int b3 = Mathf.FloorToInt((obstacle.min.z - areaBounds.min.z) / num);
			int b4 = Mathf.CeilToInt((obstacle.max.z - areaBounds.min.z) / num);
			for (int k = Mathf.Max(0, b); k < Mathf.Min(num2, b2); k++)
			{
				for (int l = Mathf.Max(0, b3); l < Mathf.Min(num3, b4); l++)
				{
					array[k, l].Add(new Vector2(float.MinValue, float.MinValue));
				}
			}
		}
		int m = 0;
		Vector2? vector = null;
		for (; m < 30; m++)
		{
			if (vector.HasValue)
			{
				break;
			}
			float x = UnityEngine.Random.Range(areaBounds.min.x, areaBounds.max.x);
			float num4 = UnityEngine.Random.Range(areaBounds.min.z, areaBounds.max.z);
			Vector3 vector2 = new Vector3(x, baseY, num4);
			if (IsPointInsideCollider(vector2, collider) && !IsNearObstacle(vector2, obstacles, minDistance))
			{
				vector = new Vector2(x, num4);
			}
		}
		if (!vector.HasValue)
		{
			Debug.LogWarning("Could not find initial sample for Poisson disk sampling");
			return list;
		}
		list.Add(vector.Value);
		list2.Add(vector.Value);
		int num5 = Mathf.FloorToInt((vector.Value.x - areaBounds.min.x) / num);
		int num6 = Mathf.FloorToInt((vector.Value.y - areaBounds.min.z) / num);
		if (num5 >= 0 && num5 < num2 && num6 >= 0 && num6 < num3)
		{
			array[num5, num6].Add(vector.Value);
		}
		while (list2.Count > 0 && list.Count < maxSamples)
		{
			int index = UnityEngine.Random.Range(0, list2.Count);
			Vector2 vector3 = list2[index];
			bool flag = false;
			for (int n = 0; n < 30; n++)
			{
				float f = UnityEngine.Random.value * 2f * MathF.PI;
				float num7 = UnityEngine.Random.Range(minDistance, 2f * minDistance);
				float num8 = vector3.x + num7 * Mathf.Cos(f);
				float num9 = vector3.y + num7 * Mathf.Sin(f);
				if (num8 < areaBounds.min.x || num8 > areaBounds.max.x || num9 < areaBounds.min.z || num9 > areaBounds.max.z)
				{
					continue;
				}
				Vector3 vector4 = new Vector3(num8, baseY, num9);
				if (IsPointInsideCollider(vector4, collider) && !IsNearObstacle(vector4, obstacles, minDistance) && IsValidSample(new Vector2(num8, num9), array, areaBounds, num, minDistance))
				{
					list.Add(new Vector2(num8, num9));
					list2.Add(new Vector2(num8, num9));
					int num10 = Mathf.FloorToInt((num8 - areaBounds.min.x) / num);
					int num11 = Mathf.FloorToInt((num9 - areaBounds.min.z) / num);
					if (num10 >= 0 && num10 < num2 && num11 >= 0 && num11 < num3)
					{
						array[num10, num11].Add(new Vector2(num8, num9));
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list2.RemoveAt(index);
			}
		}
		return list;
	}

	private bool IsNearObstacle(Vector3 position, List<Bounds> obstacles, float minDistance)
	{
		foreach (Bounds obstacle in obstacles)
		{
			if (obstacle.Contains(position) || obstacle.SqrDistance(position) < minDistance * minDistance)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsValidSample(Vector2 candidate, List<Vector2>[,] grid, Bounds areaBounds, float cellSize, float minDistance)
	{
		int num = Mathf.FloorToInt((candidate.x - areaBounds.min.x) / cellSize);
		int num2 = Mathf.FloorToInt((candidate.y - areaBounds.min.z) / cellSize);
		int num3 = Mathf.Max(0, num - 2);
		int num4 = Mathf.Min(grid.GetLength(0), num + 3);
		int num5 = Mathf.Max(0, num2 - 2);
		int num6 = Mathf.Min(grid.GetLength(1), num2 + 3);
		for (int i = num3; i < num4; i++)
		{
			for (int j = num5; j < num6; j++)
			{
				foreach (Vector2 item in grid[i, j])
				{
					if (item.x != float.MinValue && (candidate - item).sqrMagnitude < minDistance * minDistance)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public void AddBody(GameObject body)
	{
		spawnedBodies.Add(body);
	}

	public void ResetBodiesPos(bool? isKinematic = null)
	{
		for (int i = 0; i < Mathf.Min(spawnedBodies.Count, bodiesPositionsOnStart.Count); i++)
		{
			spawnedBodies[i].transform.position = bodiesPositionsOnStart[i];
			if (isKinematic.HasValue && spawnedBodies[i].TryGetComponent<Rigidbody>(out var component))
			{
				component.isKinematic = isKinematic.Value;
			}
		}
	}

	public void StartFlow()
	{
		if (!flowActive)
		{
			flowActive = true;
			objToDisableOnFlowStart.ForEach(delegate(GameObject go)
			{
				go?.SetActive(value: false);
			});
			preSetBodies.ForEach(delegate(Rigidbody rb)
			{
				rb.isKinematic = false;
			});
			InitBodiesForFlow();
		}
	}

	public void StopFlow()
	{
		preSetBodies.ForEach(delegate(Rigidbody rb)
		{
			rb.isKinematic = true;
		});
		flowActive = false;
	}

	public void StartFlowTransform()
	{
		if (transformFlowActive)
		{
			return;
		}
		transformFlowActive = true;
		objToDisableOnFlowStart.ForEach(delegate(GameObject go)
		{
			go?.SetActive(value: false);
		});
		for (int i = 0; i < spawnedBodies.Count; i++)
		{
			GameObject gameObject = spawnedBodies[i];
			if ((bool)gameObject)
			{
				Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
				if (!rigidbody)
				{
					rigidbody = gameObject.AddComponent<Rigidbody>();
				}
				rigidbody.isKinematic = true;
				rigidbody.useGravity = false;
			}
		}
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		for (int j = 0; j < spawnedBodies.Count; j++)
		{
			Transform transform = (spawnedBodies[j] ? spawnedBodies[j].transform : null);
			if ((bool)transform)
			{
				float x = transform.position.x;
				if (x < num)
				{
					num = x;
				}
				if (x > num2)
				{
					num2 = x;
				}
			}
		}
		float num3 = Mathf.Max(0.0001f, num2 - num);
		for (int k = 0; k < spawnedBodies.Count; k++)
		{
			GameObject gameObject2 = spawnedBodies[k];
			if (!gameObject2)
			{
				continue;
			}
			Rigidbody component = gameObject2.GetComponent<Rigidbody>();
			if ((bool)component)
			{
				if (!bodyFlowData.TryGetValue(component, out var value))
				{
					value = CreateFlowDataFor(gameObject2);
				}
				float x2 = gameObject2.transform.position.x;
				float value2 = (num2 - x2) / num3;
				value.speedJitter = Mathf.Clamp01(value2) * Mathf.Max(0f, transformFlowMaxDelay);
				bodyFlowData[component] = value;
			}
		}
		transformFlowStartTime = Time.time;
	}

	public void StopFlowTransform()
	{
		transformFlowActive = false;
		for (int i = 0; i < spawnedBodies.Count; i++)
		{
			GameObject gameObject = spawnedBodies[i];
			if ((bool)gameObject)
			{
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if ((bool)component)
				{
					component.isKinematic = true;
				}
			}
		}
	}

	public void ClearBodies()
	{
		foreach (GameObject spawnedBody in spawnedBodies)
		{
			if (!spawnedBody)
			{
				continue;
			}
			Rigidbody component = spawnedBody.GetComponent<Rigidbody>();
			if (!preSetBodies.Contains(component))
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(spawnedBody);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(spawnedBody);
				}
			}
		}
		spawnedBodies.Clear();
	}

	public void RemoveBody(GameObject body)
	{
		spawnedBodies.Remove(body);
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(body);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(body);
		}
	}

	private void Start()
	{
		bodyPrefab?.SetActive(value: false);
		foreach (Rigidbody preSetBody in preSetBodies)
		{
			presetBodiesPositions.Add(preSetBody.transform.position);
		}
		foreach (GameObject spawnedBody in spawnedBodies)
		{
			bodiesPositionsOnStart.Add(spawnedBody.transform.position);
		}
	}

	private void FixedUpdate()
	{
		if (transformFlowActive || !flowActive || spawnedBodies.Count == 0)
		{
			return;
		}
		for (int i = 0; i < spawnedBodies.Count; i++)
		{
			GameObject gameObject = spawnedBodies[i];
			if (!gameObject)
			{
				continue;
			}
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			if (!component)
			{
				continue;
			}
			if (!bodyFlowData.TryGetValue(component, out var value))
			{
				value = CreateFlowDataFor(gameObject);
				bodyFlowData[component] = value;
				ConfigureRigidbody(component);
			}
			float num = 1f;
			Vector3 vector2;
			if (flowSpline != null && flowSpline.Spline != null && flowSpline.Spline.Count > 1)
			{
				Vector3 position = gameObject.transform.position;
				Vector3 vector = flowSpline.transform.InverseTransformPoint(position);
				SplineUtility.GetNearestPoint(flowSpline.Spline, vector, out var nearest, out var t);
				float3 @float = flowSpline.Spline.EvaluateTangent(t);
				vector2 = flowSpline.transform.TransformDirection(@float).normalized;
				Vector3 b = flowSpline.transform.TransformPoint(nearest);
				float num2 = Vector3.Distance(position, b);
				if (flowSplineMaxDistance > 0.001f)
				{
					num = 1f - Mathf.Clamp01(num2 / flowSplineMaxDistance);
					num *= num;
				}
			}
			else
			{
				vector2 = Vector3.right;
			}
			float num3 = 1f + value.speedJitter * speedRandomness;
			Vector3 vector3 = (vector2 * (flowSpeed * num3) - GetLinearVelocity(component)) * Mathf.Max(0f, accelerationGain);
			component.AddForce(vector3 * num, ForceMode.Acceleration);
		}
	}

	private void Update()
	{
		if (!transformFlowActive || spawnedBodies.Count == 0 || MainGame.IsGamePaused)
		{
			return;
		}
		Vector3 right = Vector3.right;
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < spawnedBodies.Count; i++)
		{
			GameObject gameObject = spawnedBodies[i];
			if (!gameObject)
			{
				continue;
			}
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			if ((bool)component)
			{
				if (!bodyFlowData.TryGetValue(component, out var value))
				{
					value = CreateFlowDataFor(gameObject);
					bodyFlowData[component] = value;
				}
				float num = Mathf.Max(0f, value.speedJitter);
				if (Time.time - transformFlowStartTime < num)
				{
					continue;
				}
			}
			gameObject.transform.position += right * flowSpeed * deltaTime;
		}
	}

	private void InitBodiesForFlow()
	{
		bodyFlowData.Clear();
		for (int i = 0; i < spawnedBodies.Count; i++)
		{
			GameObject gameObject = spawnedBodies[i];
			if ((bool)gameObject)
			{
				Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
				if (!rigidbody)
				{
					rigidbody = gameObject.AddComponent<Rigidbody>();
				}
				rigidbody.useGravity = false;
				rigidbody.isKinematic = false;
				ConfigureRigidbody(rigidbody);
				bodyFlowData[rigidbody] = CreateFlowDataFor(gameObject);
			}
		}
	}

	private void EnsureFlowData()
	{
		for (int i = 0; i < spawnedBodies.Count; i++)
		{
			GameObject gameObject = spawnedBodies[i];
			if ((bool)gameObject)
			{
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if ((bool)component && !bodyFlowData.ContainsKey(component))
				{
					bodyFlowData[component] = CreateFlowDataFor(gameObject);
				}
			}
		}
	}

	private void ConfigureRigidbody(Rigidbody rb)
	{
		rb.linearDamping = Mathf.Max(0f, linearDrag);
		rb.angularDamping = Mathf.Max(0f, angularDrag);
	}

	private BodyFlowData CreateFlowDataFor(GameObject go)
	{
		float num = HashTo01(go.GetInstanceID() * 123457 + 76543);
		BodyFlowData result = default(BodyFlowData);
		result.speedJitter = num * 2f - 1f;
		return result;
	}

	private static float HashTo01(int v)
	{
		int num = (v ^ (v >>> 17)) * -312814405;
		int num2 = (num ^ (num >>> 11)) * -1404298415;
		int num3 = (num2 ^ (num2 >>> 15)) * 830770091;
		return (float)(((uint)num3 ^ ((uint)num3 >> 14)) & 0xFFFFFFu) / 16777216f;
	}

	private Vector3 GetPrefabHalfExtentsWorld()
	{
		if (!bodyPrefab)
		{
			return Vector3.one * 0.5f;
		}
		return ComputeWorldBoundsFromColliders(bodyPrefab).extents;
	}

	private static Bounds ComputeWorldBoundsFromColliders(GameObject go)
	{
		Collider[] componentsInChildren = go.GetComponentsInChildren<Collider>(includeInactive: true);
		Bounds? bounds = null;
		foreach (Collider collider in componentsInChildren)
		{
			if ((bool)collider)
			{
				Bounds bounds2 = collider.bounds;
				if (!bounds.HasValue)
				{
					bounds = bounds2;
					continue;
				}
				Bounds value = bounds.Value;
				value.Encapsulate(bounds2.min);
				value.Encapsulate(bounds2.max);
				bounds = value;
			}
		}
		return bounds ?? new Bounds(go.transform.position, Vector3.zero);
	}

	private static Vector3 GetLinearVelocity(Rigidbody rb)
	{
		return rb.linearVelocity;
	}
}
