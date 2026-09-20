using System;
using System.Collections.Generic;
using LinqTools;
using Unity.Collections;
using UnityEngine;

public static class SpecialPhysicsCastUtils
{
	private const int POSSIBLE_MASK = 16843008;

	private static readonly int DropGroundMask = 6144;

	private const float DropGroundRayUp = 8f;

	private const float DropGroundRayDistance = 16f;

	private const float DropGroundSnapEpsilon = 0.02f;

	public static void GetLinecastVisibility(Vector3 origin, IReadOnlyList<Vector3> destinations, int obstacleLayerMask, List<bool> visibilityResults, int maxHitsPerCommand = 30)
	{
		if (destinations == null)
		{
			throw new ArgumentNullException("destinations");
		}
		if (visibilityResults == null)
		{
			throw new ArgumentNullException("visibilityResults");
		}
		if (maxHitsPerCommand <= 0)
		{
			throw new ArgumentOutOfRangeException("maxHitsPerCommand", "maxHitsPerCommand must be greater than zero.");
		}
		visibilityResults.Clear();
		int count = destinations.Count;
		if (count == 0)
		{
			return;
		}
		NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
		NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(count * maxHitsPerCommand, Allocator.TempJob);
		NativeArray<byte> nativeArray = new NativeArray<byte>(count, Allocator.TempJob);
		QueryParameters queryParameters = new QueryParameters(obstacleLayerMask, hitMultipleFaces: true, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = destinations[i] - origin;
			float magnitude = vector.magnitude;
			if (magnitude <= Mathf.Epsilon)
			{
				commands[i] = new RaycastCommand(Physics.defaultPhysicsScene, origin, Vector3.forward, queryParameters, 0f);
				nativeArray[i] = 1;
			}
			else
			{
				Vector3 direction = vector / magnitude;
				commands[i] = new RaycastCommand(Physics.defaultPhysicsScene, origin, direction, queryParameters, magnitude);
			}
		}
		int minCommandsPerJob = Mathf.Clamp(count / 4, 1, count);
		RaycastCommand.ScheduleBatch(commands, results, minCommandsPerJob, maxHitsPerCommand).Complete();
		for (int j = 0; j < count; j++)
		{
			if (nativeArray[j] == 1)
			{
				visibilityResults.Add(item: true);
				continue;
			}
			bool flag = false;
			int num = j * maxHitsPerCommand;
			for (int k = 0; k < maxHitsPerCommand; k++)
			{
				if ((bool)results[num + k].collider)
				{
					flag = true;
					break;
				}
			}
			visibilityResults.Add(!flag);
		}
		results.Dispose();
		commands.Dispose();
		nativeArray.Dispose();
	}

	public static bool GetSpawnPosForBigDropRadial(Vector3 searchPoint, float searchRadius, Vector3 size, out Vector3 foundDropPos)
	{
		foundDropPos = default(Vector3);
		float num = Mathf.Atan2(size.x / 2f, searchRadius);
		int num2 = Mathf.CeilToInt(MathF.PI * 2f / num);
		float num3 = MathF.PI / 4f;
		float num4 = UnityEngine.Random.Range(0f - num3, num3);
		int num5 = 10;
		int length = num2 * num5;
		NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(length, Allocator.TempJob);
		NativeArray<BoxcastCommand> commands = new NativeArray<BoxcastCommand>(num2, Allocator.TempJob);
		QueryParameters queryParameters = new QueryParameters(7424, hitMultipleFaces: false, QueryTriggerInteraction.Ignore, hitBackfaces: true);
		Vector3 halfExtents = size / 2f;
		Vector3 down = Vector3.down;
		float num6 = 10f;
		for (int i = 0; i < num2; i++)
		{
			Vector3 vector = searchPoint + Vector3.Scale(new Vector3(searchRadius, 0f, searchRadius), new Vector3(Mathf.Cos(num * (float)i + num4), 0f, Mathf.Sin(num * (float)i + num4))) + Vector3.up * num6 / 2f;
			commands[i] = new BoxcastCommand(vector, halfExtents, Quaternion.identity, down, queryParameters, 5f);
			Debug.DrawLine(vector - Vector3.up * num6 / 2f, vector + Vector3.up, Color.cyan, 2f);
		}
		BoxcastCommand.ScheduleBatch(commands, results, 10, num5).Complete();
		int j = 0;
		bool result = false;
		Vector3 vector2 = default(Vector3);
		for (; j < num2; j++)
		{
			bool flag = false;
			bool flag2 = false;
			for (int k = 0; k < num5; k++)
			{
				RaycastHit raycastHit = results[j * num5 + k];
				if (raycastHit.collider == null)
				{
					continue;
				}
				int layer = raycastHit.collider.gameObject.layer;
				if (layer == 10 || layer == 8)
				{
					flag2 = true;
					break;
				}
				if (!flag)
				{
					layer = raycastHit.collider.gameObject.layer;
					if (layer == 11 || layer == 12)
					{
						flag = true;
						vector2 = raycastHit.point;
					}
				}
			}
			if (flag && !flag2)
			{
				result = true;
				foundDropPos = vector2;
				break;
			}
		}
		results.Dispose();
		commands.Dispose();
		return result;
	}

	public static bool GetSpawnPosForBigDropRectangular(Vector3 searchPoint, Vector3 size, float xStepSize, int xStepsCount, float zStepSize, int zStepsCount, out Vector3 foundDropPos, IReadOnlyList<Vector3> occupiedPositions = null)
	{
		foundDropPos = default(Vector3);
		int num = xStepsCount * zStepsCount;
		int num2 = 10;
		int length = num * num2;
		float num3 = Mathf.Max(size.x, size.z);
		float occupyRadiusSq = num3 * num3;
		NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(length, Allocator.TempJob);
		NativeArray<BoxcastCommand> commands = new NativeArray<BoxcastCommand>(num, Allocator.TempJob);
		QueryParameters queryParameters = new QueryParameters(72960, hitMultipleFaces: false, QueryTriggerInteraction.Ignore, hitBackfaces: true);
		Vector3 halfExtents = size / 2f;
		Vector3 down = Vector3.down;
		float num4 = 10f;
		float centerX = (float)(xStepsCount - 1) / 2f;
		float centerZ = (float)(zStepsCount - 1) / 2f;
		(int, int)[] array = new(int, int)[num];
		int num5 = 0;
		for (int i = 0; i < xStepsCount; i++)
		{
			for (int j = 0; j < zStepsCount; j++)
			{
				array[num5++] = (i, j);
			}
		}
		Array.Sort(array, delegate((int ix, int iz) a, (int ix, int iz) b)
		{
			float num6 = ((float)a.ix - centerX) * ((float)a.ix - centerX) + ((float)a.iz - centerZ) * ((float)a.iz - centerZ);
			float value = ((float)b.ix - centerX) * ((float)b.ix - centerX) + ((float)b.iz - centerZ) * ((float)b.iz - centerZ);
			return num6.CompareTo(value);
		});
		for (int k = 0; k < num; k++)
		{
			(int, int) tuple = array[k];
			int item = tuple.Item1;
			int item2 = tuple.Item2;
			float x = ((float)item - centerX) * xStepSize;
			float z = ((float)item2 - centerZ) * zStepSize;
			Vector3 center = searchPoint + new Vector3(x, num4 / 2f, z);
			commands[k] = new BoxcastCommand(center, halfExtents, Quaternion.identity, down, queryParameters, 5f);
		}
		BoxcastCommand.ScheduleBatch(commands, results, 10, num2).Complete();
		int l = 0;
		bool result = false;
		Vector3 vector = default(Vector3);
		for (; l < num; l++)
		{
			bool flag = false;
			bool flag2 = false;
			for (int m = 0; m < num2; m++)
			{
				RaycastHit raycastHit = results[l * num2 + m];
				if (raycastHit.collider == null)
				{
					continue;
				}
				int layer = raycastHit.collider.gameObject.layer;
				if (layer == 10 || layer == 8 || layer == 16)
				{
					Debug.DrawLine(raycastHit.point, raycastHit.point + Vector3.up * 0.5f, Color.red, 2f);
					flag2 = true;
					break;
				}
				if (!flag)
				{
					layer = raycastHit.collider.gameObject.layer;
					if (layer == 11 || layer == 12)
					{
						Debug.DrawLine(raycastHit.point, raycastHit.point + Vector3.up * 0.5f, Color.green, 2f);
						flag = true;
						vector = raycastHit.point;
					}
				}
			}
			if (flag && !flag2 && IsReachableDropElevation(searchPoint.y, vector.y) && !IsOccupiedByDropPosition(vector, occupiedPositions, occupyRadiusSq))
			{
				result = true;
				foundDropPos = vector;
				break;
			}
		}
		results.Dispose();
		commands.Dispose();
		return result;
	}

	public static bool IsReachableDropElevation(float anchorY, float foundGroundY)
	{
		return Mathf.Abs(anchorY - foundGroundY) <= 1.5f;
	}

	private static bool IsOccupiedByDropPosition(Vector3 candidate, IReadOnlyList<Vector3> occupiedPositions, float occupyRadiusSq)
	{
		if (occupiedPositions == null || occupiedPositions.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < occupiedPositions.Count; i++)
		{
			Vector3 vector = occupiedPositions[i];
			float num = candidate.x - vector.x;
			float num2 = candidate.z - vector.z;
			if (num * num + num2 * num2 <= occupyRadiusSq)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetPlayerDropPosition(Vector3 playerPos, Vector2 playerDir, out Vector3 foundDropPos)
	{
		foundDropPos = default(Vector3);
		float[] obj = new float[13]
		{
			0.75f, 0.85f, 0.95f, 1.05f, 1.15f, 1.25f, 1.35f, 1.45f, 1.55f, 1.65f,
			1.75f, 1.85f, 1.95f
		};
		float num = 15f;
		Vector3 normalized = new Vector3(playerDir.x, 0f, playerDir.y).normalized;
		float[] array = obj;
		foreach (float num2 in array)
		{
			int num3 = Mathf.CeilToInt(360f / num);
			int num4 = 20;
			int length = num3 * num4;
			NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(length, Allocator.TempJob);
			NativeArray<BoxcastCommand> commands = new NativeArray<BoxcastCommand>(num3, Allocator.TempJob);
			QueryParameters queryParameters = new QueryParameters(7424, hitMultipleFaces: true, QueryTriggerInteraction.Ignore, hitBackfaces: true);
			Vector3 down = Vector3.down;
			Vector3 halfExtents = LazyConsts.BIG_DROP_COLLIDER_SIZE / 2f;
			for (int j = 0; j < num3; j++)
			{
				float y = ((j == 0) ? 0f : ((j % 2 != 1) ? ((float)(j / 2) * num) : ((float)(-((j + 1) / 2)) * num)));
				Vector3 vector = Quaternion.Euler(0f, y, 0f) * normalized;
				Vector3 vector2 = playerPos + vector.normalized * num2 + Vector3.up * 2f;
				commands[j] = new BoxcastCommand(vector2, halfExtents, Quaternion.identity, down, queryParameters, 5f);
				Debug.DrawLine(color: new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.2f), start: vector2, end: vector2 + down * 2f, duration: 5f);
			}
			BoxcastCommand.ScheduleBatch(commands, results, 10, num4).Complete();
			for (int k = 0; k < num3; k++)
			{
				bool flag = false;
				bool flag2 = false;
				Vector3 vector3 = Vector3.zero;
				for (int l = 0; l < num4; l++)
				{
					RaycastHit raycastHit = results[k * num4 + l];
					if (raycastHit.collider == null)
					{
						continue;
					}
					if (raycastHit.collider.TryGetComponent<WaterComponentTag>(out var _))
					{
						flag2 = true;
						break;
					}
					switch (raycastHit.collider.gameObject.layer)
					{
					case 11:
					case 12:
						if (!IsReachableDropElevation(playerPos.y, raycastHit.point.y))
						{
							continue;
						}
						flag = true;
						vector3 = new Vector3(commands[k].center.x, raycastHit.point.y, commands[k].center.z);
						break;
					case 8:
						flag2 = true;
						break;
					default:
						continue;
					}
					break;
				}
				if (!flag || flag2 || IsOverlappingSomething(vector3, halfExtents, Quaternion.identity))
				{
					continue;
				}
				Vector3 vector4 = Vector3.up * 0.6f;
				Vector3 vector5 = playerPos + vector4;
				Vector3 vector6 = vector3 + vector4;
				Vector3 normalized2 = (vector6 - vector5).normalized;
				float maxDistance = Vector3.Distance(vector5, vector6);
				if (Physics.Raycast(vector5, normalized2, out var _, maxDistance, 3328, QueryTriggerInteraction.Ignore))
				{
					Debug.DrawLine(vector5, vector6, Color.red, 5f);
					continue;
				}
				foundDropPos = vector3;
				if (TrySnapDropPosToTopmostGround(foundDropPos, out var snapped))
				{
					if (!IsReachableDropElevation(playerPos.y, snapped.y))
					{
						continue;
					}
					foundDropPos = snapped;
				}
				Debug.DrawLine(foundDropPos, foundDropPos + Vector3.up * 2f, Color.green, 5f);
				Debug.DrawLine(vector5, vector6, Color.green, 5f);
				results.Dispose();
				commands.Dispose();
				return true;
			}
			results.Dispose();
			commands.Dispose();
		}
		foundDropPos = playerPos + Vector3.up * 2f;
		if (TrySnapDropPosToTopmostGround(playerPos, out var snapped2))
		{
			foundDropPos = snapped2;
		}
		return false;
	}

	public static bool TrySnapDropPosToTopmostGround(Vector3 worldPos, out Vector3 snapped)
	{
		snapped = worldPos;
		if (!TryGetTopmostGroundHit(worldPos, out var bestHit))
		{
			return false;
		}
		Vector3 vector = bestHit.point + bestHit.normal * 0.02f;
		snapped = new Vector3(worldPos.x, vector.y, worldPos.z);
		return true;
	}

	public static bool TryGetTopmostGroundHit(Vector3 worldPos, out RaycastHit bestHit)
	{
		bestHit = default(RaycastHit);
		RaycastHit[] array = Physics.RaycastAll(new Vector3(worldPos.x, worldPos.y + 8f, worldPos.z), Vector3.down, 16f, DropGroundMask, QueryTriggerInteraction.Ignore);
		if (array == null || array.Length == 0)
		{
			return false;
		}
		float num = float.MaxValue;
		bool result = false;
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (!(raycastHit.collider == null) && !(raycastHit.collider.GetComponentInParent<DropView>() != null) && !(raycastHit.distance >= num))
			{
				num = raycastHit.distance;
				bestHit = raycastHit;
				result = true;
			}
		}
		return result;
	}

	public static bool GetLandPositionByCapsule(Vector3 centerPos, Vector2 preferredDir, float lookDistance, float capsuleRadius, float capsuleHeight, out Vector3 foundDropPos, float angleStep = 20f, int radialSteps = 4)
	{
		foundDropPos = default(Vector3);
		lookDistance = Mathf.Max(0.1f, lookDistance);
		capsuleRadius = Mathf.Max(0.05f, capsuleRadius);
		capsuleHeight = Mathf.Max(capsuleRadius * 2f + 0.05f, capsuleHeight);
		angleStep = Mathf.Clamp(angleStep, 5f, 90f);
		radialSteps = Mathf.Clamp(radialSteps, 1, 12);
		Vector3 vector = new Vector3(preferredDir.x, 0f, preferredDir.y);
		if (vector.sqrMagnitude < 0.0001f)
		{
			vector = Vector3.forward;
		}
		vector.Normalize();
		int num = Mathf.CeilToInt(360f / angleStep);
		float num2 = 2f;
		float maxDistance = 6f;
		int layerMask = 7424;
		int layerMask2 = 1281;
		for (int i = 0; i < radialSteps; i++)
		{
			float num3 = ((float)i + 1f) / (float)radialSteps;
			float num4 = lookDistance * num3;
			for (int j = 0; j < num; j++)
			{
				float y = ((j == 0) ? 0f : ((j % 2 != 1) ? ((float)(j / 2) * angleStep) : ((float)(-((j + 1) / 2)) * angleStep)));
				Vector3 vector2 = Quaternion.Euler(0f, y, 0f) * vector;
				Vector3 vector3 = centerPos + vector2 * num4;
				if (!Physics.Raycast(vector3 + Vector3.up * num2, Vector3.down, out var hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore) || (hitInfo.collider != null && hitInfo.collider.gameObject.TryGetComponent<WaterComponentTag>(out var _)))
				{
					continue;
				}
				int num5 = ((hitInfo.collider != null) ? hitInfo.collider.gameObject.layer : (-1));
				if (num5 != 11 && num5 != 12)
				{
					continue;
				}
				Vector3 vector4 = new Vector3(vector3.x, hitInfo.point.y, vector3.z);
				float num6 = Mathf.Max(0.01f, capsuleHeight * 0.5f - capsuleRadius);
				Vector3 vector5 = vector4 + Vector3.up * capsuleRadius;
				Vector3 end = vector5 + Vector3.up * (num6 * 2f);
				if (!Physics.CheckCapsule(vector5, end, capsuleRadius, 16843008, QueryTriggerInteraction.Ignore))
				{
					Vector3 vector6 = centerPos + Vector3.up * 0.5f;
					Vector3 vector7 = vector4 + Vector3.up * 0.5f - vector6;
					float magnitude = vector7.magnitude;
					if (!(magnitude > 0.001f) || !Physics.Raycast(vector6, vector7 / magnitude, magnitude, layerMask2, QueryTriggerInteraction.Ignore))
					{
						foundDropPos = vector4;
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool IsOverlappingSomething(Vector3 pos, Vector3 halfExtents, Quaternion orientation)
	{
		return Physics.OverlapBox(pos, halfExtents, orientation, 16843008).Length != 0;
	}

	public static void GetSweepTriangleOverlap(BoxCollider boxCollider, Vector3 currentPosition, Quaternion currentRotation, Vector3 previousPosition, Quaternion previousRotation, Vector3 currentEndPoint, Vector3 previousEndPoint, int layerMask, HashSet<Collider> results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide)
	{
		if (boxCollider == null)
		{
			throw new ArgumentNullException("boxCollider");
		}
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Clear();
		Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale);
		Collider[] array = new Collider[32];
		int num = Physics.OverlapBoxNonAlloc(currentPosition, halfExtents, array, currentRotation, layerMask, queryTriggerInteraction);
		for (int i = 0; i < num; i++)
		{
			if (array[i] != boxCollider)
			{
				results.Add(array[i]);
			}
		}
		int num2 = Physics.OverlapBoxNonAlloc(previousPosition, halfExtents, array, previousRotation, layerMask, queryTriggerInteraction);
		for (int j = 0; j < num2; j++)
		{
			if (array[j] != boxCollider)
			{
				results.Add(array[j]);
			}
		}
		Vector3 vector = currentEndPoint - previousEndPoint;
		float magnitude = vector.magnitude;
		if (!(magnitude > 0.001f))
		{
			return;
		}
		Vector3 center = (previousEndPoint + currentEndPoint) * 0.5f;
		Quaternion orientation = Quaternion.LookRotation(vector.normalized);
		Vector3 halfExtents2 = new Vector3(halfExtents.x, halfExtents.y, magnitude * 0.5f);
		int num3 = Physics.OverlapBoxNonAlloc(center, halfExtents2, array, orientation, layerMask, queryTriggerInteraction);
		for (int k = 0; k < num3; k++)
		{
			if (array[k] != boxCollider)
			{
				results.Add(array[k]);
			}
		}
	}

	public static void GetSweepTriangleOverlap(BoxCollider boxCollider, Transform endPointTransform, Vector3 previousBoxPosition, Quaternion previousBoxRotation, Vector3 previousEndPoint, int layerMask, HashSet<Collider> results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide)
	{
		Vector3 currentPosition = boxCollider.transform.TransformPoint(boxCollider.center);
		Quaternion rotation = boxCollider.transform.rotation;
		Vector3 position = endPointTransform.position;
		GetSweepTriangleOverlap(boxCollider, currentPosition, rotation, previousBoxPosition, previousBoxRotation, position, previousEndPoint, layerMask, results, queryTriggerInteraction);
	}

	public static bool GetLineSweepHits(Vector3 previousPosition, Vector3 currentPosition, float radius, int layerMask, List<RaycastHit> results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Clear();
		Vector3 vector = currentPosition - previousPosition;
		float magnitude = vector.magnitude;
		if (magnitude < 0.001f)
		{
			return false;
		}
		Vector3 direction = vector / magnitude;
		RaycastHit[] array = ((!(radius > 0.001f)) ? Physics.RaycastAll(previousPosition, direction, magnitude, layerMask, queryTriggerInteraction) : Physics.SphereCastAll(previousPosition, radius, direction, magnitude, layerMask, queryTriggerInteraction));
		if (array.Length == 0)
		{
			return false;
		}
		Array.Sort(array, (RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
		results.AddRange(array);
		return true;
	}

	public static bool GetBoxLineSweepHits(BoxCollider boxCollider, Vector3 previousPosition, Vector3 currentPosition, Quaternion orientation, int layerMask, List<RaycastHit> results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide)
	{
		if (boxCollider == null)
		{
			throw new ArgumentNullException("boxCollider");
		}
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Clear();
		Vector3 vector = currentPosition - previousPosition;
		float magnitude = vector.magnitude;
		if (magnitude < 0.001f)
		{
			return false;
		}
		Vector3 direction = vector / magnitude;
		Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale);
		RaycastHit[] array = Physics.BoxCastAll(previousPosition, halfExtents, direction, orientation, magnitude, layerMask, queryTriggerInteraction);
		if (array.Length == 0)
		{
			return false;
		}
		Array.Sort(array, (RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
		results.AddRange(array);
		return true;
	}

	public static bool GetLineSweepFirstHit(Vector3 previousPosition, Vector3 currentPosition, float radius, int layerMask, out RaycastHit hit, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide)
	{
		hit = default(RaycastHit);
		Vector3 vector = currentPosition - previousPosition;
		float magnitude = vector.magnitude;
		if (magnitude < 0.001f)
		{
			return false;
		}
		Vector3 direction = vector / magnitude;
		if (radius > 0.001f)
		{
			return Physics.SphereCast(previousPosition, radius, direction, out hit, magnitude, layerMask, queryTriggerInteraction);
		}
		return Physics.Raycast(previousPosition, direction, out hit, magnitude, layerMask, queryTriggerInteraction);
	}

	public static bool TryGetWgosIntersectedByBuffCollider(Wgo wgo, bool isParentWorkbench, out HashSet<Wgo> intersectedWgos)
	{
		intersectedWgos = new HashSet<Wgo>();
		List<Collider> list = new List<Collider>();
		List<WGODef> parentWorkbenchDefs = new List<WGODef>();
		if (!isParentWorkbench)
		{
			GameBalance.Me.TryGetParentWorkbenchDefsForExtension(wgo.Id, out parentWorkbenchDefs);
			list = (from col in wgo.GetComponentsInChildren<Collider>()
				where col.gameObject.layer == 28
				select col).ToList();
		}
		else
		{
			WGODef workbenchExtensionLogicDef = GameBalance.Me.GetWorkbenchExtensionLogicDef(wgo.Data.id);
			parentWorkbenchDefs = ((workbenchExtensionLogicDef != null) ? (from id in workbenchExtensionLogicDef.attachedWorkbenchExtensionIds
				select GameBalance.Me.GetData<WGODef>(id) into def
				where def != null
				select def).ToList() : new List<WGODef>());
			list = (from col in wgo.GetComponentsInChildren<Collider>()
				where col.gameObject.layer == 19
				select col).ToList();
		}
		if (list.Count == 0)
		{
			return false;
		}
		int mask = ((!isParentWorkbench) ? 524288 : 268435456);
		Collider[] array = new Collider[20];
		foreach (Collider item in list)
		{
			int num = Physics.OverlapBoxNonAlloc(item.bounds.center, item.bounds.extents + Vector3.up * 0.5f - VisualConsts.XYZ_STEP, array, Quaternion.identity, mask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < num; i++)
			{
				Collider collider = array[i];
				if ((bool)collider && !(collider == item))
				{
					Wgo componentInParent = collider.gameObject.GetComponentInParent<Wgo>();
					if ((bool)componentInParent && componentInParent.Data != null && componentInParent.Data.Definition != null && parentWorkbenchDefs.Contains(componentInParent.Data.Definition))
					{
						intersectedWgos.Add(componentInParent);
						Debug.DrawLine(item.bounds.center, collider.bounds.center, Color.green, 5f);
					}
				}
			}
		}
		return intersectedWgos.Count > 0;
	}
}
