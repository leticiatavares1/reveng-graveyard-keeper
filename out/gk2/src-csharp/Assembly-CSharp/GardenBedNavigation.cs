using System.Collections.Generic;
using UnityEngine;

public static class GardenBedNavigation
{
	public const string ApproachCustomTag = "garden_bed_approach";

	private const float NeighborSearchPadding = 2f;

	private const float WalkwayConnectMaxDistance = 1.5f;

	private const float FallbackOccupancyHalfWidth = 0.6f;

	private const float FallbackOccupancyHalfDepth = 0.75f;

	private static readonly HashSet<string> rebuiltThisSession = new HashSet<string>();

	public static bool IsGardenPlot(WgoData wgoData)
	{
		if (wgoData == null || wgoData.isTempObject || wgoData.Definition == null)
		{
			return false;
		}
		string wgoGroup = wgoData.Definition.wgoGroup;
		if (!(wgoGroup == "garden_bed"))
		{
			return wgoGroup == "vineyard_objects";
		}
		return true;
	}

	public static void TryRebuild(WgoData wgoData)
	{
		if (IsGardenPlot(wgoData))
		{
			RebuildApproachPoints(wgoData);
			MarkRebuilt(wgoData);
			RefreshAround(wgoData, null);
		}
	}

	public static void TryRebuildOnViewRespawn(WgoData wgoData)
	{
		if (IsGardenPlot(wgoData) && !WasRebuiltThisSession(wgoData))
		{
			TryRebuild(wgoData);
		}
	}

	public static void UnlinkApproachPoints(WgoData wgoData)
	{
		ClearRebuilt(wgoData);
		if (wgoData?.gdPointsData == null)
		{
			return;
		}
		foreach (GDPointData gdPointsDatum in wgoData.gdPointsData)
		{
			if (IsApproachPoint(gdPointsDatum))
			{
				UnlinkFromNeighbors(gdPointsDatum);
			}
		}
	}

	public static void RefreshAround(WgoData origin, WgoData ignore)
	{
		WorldZoneData worldZoneData = origin?.WorldZoneData;
		if (worldZoneData != null)
		{
			Rect searchRect = Expand(GetWorldOccupancyRect(origin), 2f);
			ApplyOccupancy(worldZoneData, searchRect, ignore);
			MainGame.Instance?.GraphHelper?.QueueRescanGDPointGraph();
		}
	}

	public static bool TryGetOpenApproach(WgoData plot, Vector3 fromPosition, out Vector3 position, out Direction direction)
	{
		position = default(Vector3);
		direction = Direction.Down;
		if (!IsGardenPlot(plot))
		{
			return false;
		}
		GDPointData gDPointData = null;
		float num = float.MaxValue;
		if (plot.gdPointsData != null)
		{
			foreach (GDPointData gdPointsDatum in plot.gdPointsData)
			{
				if (IsApproachPoint(gdPointsDatum) && gdPointsDatum.Enabled)
				{
					float sqrMagnitude = (gdPointsDatum.Position - fromPosition).sqrMagnitude;
					if (!(sqrMagnitude >= num))
					{
						num = sqrMagnitude;
						gDPointData = gdPointsDatum;
					}
				}
			}
		}
		if (gDPointData != null)
		{
			position = gDPointData.Position;
			direction = gDPointData.Direction;
			return true;
		}
		if (plot.MainWgoPartData == null)
		{
			return false;
		}
		DockPointData dockPointData = null;
		foreach (DockPointData dockPoint in plot.MainWgoPartData.GetDockPoints())
		{
			if (dockPoint.BakedData == null || dockPoint.BakedData.DontUseForWorkerPlacement)
			{
				continue;
			}
			Vector3 dockPointDataWorldPosition = plot.GetDockPointDataWorldPosition(dockPoint);
			if (!IsInsideAnyOtherPlot(dockPointDataWorldPosition, plot, null))
			{
				float sqrMagnitude2 = (dockPointDataWorldPosition - fromPosition).sqrMagnitude;
				if (!(sqrMagnitude2 >= num))
				{
					num = sqrMagnitude2;
					dockPointData = dockPoint;
					position = dockPointDataWorldPosition;
					direction = dockPoint.Direction;
				}
			}
		}
		return dockPointData != null;
	}

	private static void RebuildApproachPoints(WgoData wgoData)
	{
		UnlinkApproachPoints(wgoData);
		List<GDPointData> list = new List<GDPointData>();
		if (wgoData.gdPointsData != null)
		{
			foreach (GDPointData gdPointsDatum in wgoData.gdPointsData)
			{
				if (!IsApproachPoint(gdPointsDatum))
				{
					list.Add(gdPointsDatum);
				}
			}
		}
		if (wgoData.MainWgoPartData != null)
		{
			List<DockPointData> dockPoints = wgoData.MainWgoPartData.GetDockPoints();
			int num = -536870912 + wgoData.UniqueId.GetHashCode();
			for (int i = 0; i < dockPoints.Count; i++)
			{
				DockPointData dockPointData = dockPoints[i];
				if (dockPointData.BakedData != null && !dockPointData.BakedData.DontUseForWorkerPlacement)
				{
					Vector3 dockPointDataWorldPosition = wgoData.GetDockPointDataWorldPosition(dockPointData);
					GDPointData item = new GDPointData($"{wgoData.UniqueId.Id}_approach_{i}", "garden_bed_approach", num + i, dockPointDataWorldPosition, dockPointData.Direction, wgoData.WorldId, isWaypoint: true, enabled: true);
					list.Add(item);
				}
			}
		}
		wgoData.RewriteGdPointsData(list);
	}

	private static void ApplyOccupancy(WorldZoneData zone, Rect searchRect, WgoData ignore)
	{
		GdPointsData gdPointsData = MainGame.Instance?.GameSave?.worldData?.gdPointsData;
		if (gdPointsData == null)
		{
			return;
		}
		List<WgoData> list = CollectPlots(zone, searchRect, ignore);
		List<GDPointData> points = gdPointsData.Points;
		foreach (WgoData item in list)
		{
			if (item.gdPointsData == null)
			{
				continue;
			}
			foreach (GDPointData gdPointsDatum in item.gdPointsData)
			{
				if (IsApproachPoint(gdPointsDatum))
				{
					UnlinkFromNeighbors(gdPointsDatum);
					bool flag = !IsInsideAnyOtherPlot(gdPointsDatum.Position, item, ignore);
					gdPointsDatum.SetEnabledStateSilent(flag);
					if (flag)
					{
						ConnectToNearestWalkway(gdPointsDatum, points, list);
					}
				}
			}
		}
		foreach (GDPointData item2 in points)
		{
			if (!IsApproachPoint(item2) && item2.IsWaypoint)
			{
				Vector2 point = new Vector2(item2.Position.x, item2.Position.z);
				if (searchRect.Contains(point))
				{
					bool flag2 = IsInsideAnyPlot(item2.Position, ignore, list);
					item2.SetEnabledStateSilent(!flag2);
				}
			}
		}
	}

	private static void ConnectToNearestWalkway(GDPointData approach, List<GDPointData> allPoints, List<WgoData> plots)
	{
		GDPointData gDPointData = null;
		float num = 1.5f;
		foreach (GDPointData allPoint in allPoints)
		{
			if (allPoint != null && allPoint != approach && !IsApproachPoint(allPoint) && allPoint.IsWaypoint && allPoint.Enabled && !IsInsideAnyPlot(allPoint.Position, null, plots))
			{
				float num2 = Vector3.Distance(approach.Position, allPoint.Position);
				if (!(num2 >= num))
				{
					num = num2;
					gDPointData = allPoint;
				}
			}
		}
		if (gDPointData != null)
		{
			approach.AddNextNodeInstanceId(gDPointData.InstanceId);
			gDPointData.AddNextNodeInstanceId(approach.InstanceId);
		}
	}

	private static void UnlinkFromNeighbors(GDPointData point)
	{
		GdPointsData gdPointsData = MainGame.Instance?.GameSave?.worldData?.gdPointsData;
		if (gdPointsData == null || point.NextNodeInstanceIds == null || point.NextNodeInstanceIds.Count == 0)
		{
			return;
		}
		foreach (int item in new List<int>(point.NextNodeInstanceIds))
		{
			gdPointsData.GetGDPointDataByInstanceId(item)?.RemoveNextNodeInstanceId(point.InstanceId);
		}
		point.SetNextNodeInstanceIds(new List<int>());
	}

	private static List<WgoData> CollectPlots(WorldZoneData zone, Rect searchRect, WgoData ignore)
	{
		List<WgoData> list = new List<WgoData>();
		foreach (SGuid wgoData2 in zone.wgoDataList)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(wgoData2);
			if (wgoData != null && wgoData != ignore && !wgoData.isRemovingFromData && IsGardenPlot(wgoData))
			{
				Rect worldOccupancyRect = GetWorldOccupancyRect(wgoData);
				if (searchRect.Overlaps(worldOccupancyRect, allowInverse: true))
				{
					list.Add(wgoData);
				}
			}
		}
		return list;
	}

	private static bool IsInsideAnyOtherPlot(Vector3 worldPos, WgoData owner, WgoData ignore)
	{
		WorldZoneData worldZoneData = owner?.WorldZoneData;
		if (worldZoneData == null)
		{
			return false;
		}
		foreach (SGuid wgoData2 in worldZoneData.wgoDataList)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(wgoData2);
			if (wgoData != null && wgoData != owner && wgoData != ignore && !wgoData.isRemovingFromData && IsGardenPlot(wgoData) && ContainsXZ(GetWorldOccupancyRect(wgoData), worldPos))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsInsideAnyPlot(Vector3 worldPos, WgoData ignore, List<WgoData> plotsOverride = null)
	{
		if (plotsOverride != null)
		{
			foreach (WgoData item in plotsOverride)
			{
				if (item != ignore && ContainsXZ(GetWorldOccupancyRect(item), worldPos))
				{
					return true;
				}
			}
			return false;
		}
		WorldZoneData worldZoneData = ignore?.WorldZoneData;
		if (worldZoneData == null)
		{
			return false;
		}
		foreach (SGuid wgoData2 in worldZoneData.wgoDataList)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(wgoData2);
			if (wgoData != null && wgoData != ignore && !wgoData.isRemovingFromData && IsGardenPlot(wgoData) && ContainsXZ(GetWorldOccupancyRect(wgoData), worldPos))
			{
				return true;
			}
		}
		return false;
	}

	public static Rect GetWorldOccupancyRect(WgoData wgoData)
	{
		if (wgoData?.MainWgoPartData != null)
		{
			Rect collisionBoundsRect = wgoData.MainWgoPartData.GetCollisionBoundsRect(wgoData.Position);
			if (collisionBoundsRect.width > 0.01f && collisionBoundsRect.height > 0.01f)
			{
				return collisionBoundsRect;
			}
		}
		if (wgoData != null && wgoData.HasSerializedBounds)
		{
			Bounds bounds = wgoData.SerializedBounds.GetBounds();
			Vector3 vector = wgoData.Position + bounds.center;
			return new Rect(vector.x - bounds.extents.x, vector.z - bounds.extents.z, bounds.size.x, bounds.size.z);
		}
		Vector3 vector2 = wgoData?.Position ?? Vector3.zero;
		return new Rect(vector2.x - 0.6f, vector2.z - 0.75f, 1.2f, 1.5f);
	}

	private static void MarkRebuilt(WgoData wgoData)
	{
		string text = wgoData?.UniqueId?.Id;
		if (!string.IsNullOrEmpty(text))
		{
			rebuiltThisSession.Add(text);
		}
	}

	private static void ClearRebuilt(WgoData wgoData)
	{
		string text = wgoData?.UniqueId?.Id;
		if (!string.IsNullOrEmpty(text))
		{
			rebuiltThisSession.Remove(text);
		}
	}

	private static bool WasRebuiltThisSession(WgoData wgoData)
	{
		string text = wgoData?.UniqueId?.Id;
		if (!string.IsNullOrEmpty(text))
		{
			return rebuiltThisSession.Contains(text);
		}
		return false;
	}

	private static bool IsApproachPoint(GDPointData point)
	{
		if (point != null)
		{
			return point.CustomTag == "garden_bed_approach";
		}
		return false;
	}

	private static bool ContainsXZ(Rect rect, Vector3 worldPos)
	{
		return rect.Contains(new Vector2(worldPos.x, worldPos.z));
	}

	private static Rect Expand(Rect rect, float padding)
	{
		return new Rect(rect.xMin - padding, rect.yMin - padding, rect.width + padding * 2f, rect.height + padding * 2f);
	}
}
