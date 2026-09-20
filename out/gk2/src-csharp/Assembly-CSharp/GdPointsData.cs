using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GdPointsData
{
	private class Cache
	{
		public Dictionary<string, List<GDPointData>> allPointsByIdCache = new Dictionary<string, List<GDPointData>>();

		public Dictionary<string, List<GDPointData>> allPointsByCustomTagCache = new Dictionary<string, List<GDPointData>>();

		public Dictionary<int, GDPointData> allPointsByInstanceIdCache = new Dictionary<int, GDPointData>();

		public void AddToCache(GDPointData gdPointData)
		{
			if (allPointsByIdCache.TryGetValue(gdPointData.Id, out var value))
			{
				value.Add(gdPointData);
			}
			else
			{
				allPointsByIdCache.Add(gdPointData.Id, new List<GDPointData> { gdPointData });
			}
			if (!string.IsNullOrEmpty(gdPointData.CustomTag))
			{
				if (allPointsByCustomTagCache.TryGetValue(gdPointData.CustomTag, out value))
				{
					value.Add(gdPointData);
				}
				else
				{
					allPointsByCustomTagCache.Add(gdPointData.CustomTag, new List<GDPointData> { gdPointData });
				}
			}
			allPointsByInstanceIdCache.TryAdd(gdPointData.InstanceId, gdPointData);
		}

		public void RemoveFromCache(GDPointData gdPointData)
		{
			if (allPointsByIdCache.TryGetValue(gdPointData.Id, out var value))
			{
				value.Remove(gdPointData);
				if (value.Count == 0)
				{
					allPointsByIdCache.Remove(gdPointData.Id);
				}
			}
			if (!string.IsNullOrEmpty(gdPointData.CustomTag) && allPointsByCustomTagCache.TryGetValue(gdPointData.CustomTag, out value))
			{
				value.Remove(gdPointData);
				if (value.Count == 0)
				{
					allPointsByCustomTagCache.Remove(gdPointData.Id);
				}
			}
			allPointsByInstanceIdCache.Remove(gdPointData.InstanceId);
		}

		public void Clear()
		{
			allPointsByIdCache.Clear();
			allPointsByCustomTagCache.Clear();
			allPointsByInstanceIdCache.Clear();
		}
	}

	[SerializeField]
	private List<GDPointData> scenePoints = new List<GDPointData>();

	private List<GDPointData> waypoints;

	private Cache cache;

	public List<GDPointData> Points => waypoints.Concat(scenePoints).ToList();

	public void PrepareForGame(List<GameSceneConfig> configs)
	{
		cache = new Cache();
		waypoints = new List<GDPointData>();
		foreach (GameSceneConfig config in configs)
		{
			InitDataFromConfig(config);
		}
		foreach (GDPointData scenePoint in scenePoints)
		{
			cache.AddToCache(scenePoint);
		}
		UpdateLinkGraphData();
	}

	public void InitScenePointsFromGameScene(GameScene gameScene, GDPoint[] points)
	{
		List<GDPointData> list = new List<GDPointData>();
		foreach (GDPoint gDPoint in points)
		{
			GDPointData gDPointDataByView = GetGDPointDataByView(gDPoint);
			if (gDPointDataByView != null)
			{
				gDPoint.Init(gDPointDataByView);
				gDPoint.gameObject.SetActive(gDPointDataByView.Enabled);
				continue;
			}
			gDPointDataByView = new GDPointData(gDPoint, gameScene.Id, gameScene.GameSceneConfig.sceneGlobalPosition, isWaypoint: false);
			list.Add(gDPointDataByView);
			gDPoint.Init(gDPointDataByView);
			gDPoint.gameObject.SetActive(gDPointDataByView.Enabled);
		}
		AddScenePoints(list);
		UpdateLinkGraphData();
	}

	public void AddScenePoint(GDPointData scenePoint)
	{
		scenePoints.Add(scenePoint);
		cache.AddToCache(scenePoint);
		UpdateLinkGraphData();
	}

	public void AddScenePoints(List<GDPointData> scenePoints)
	{
		this.scenePoints.AddRange(scenePoints);
		foreach (GDPointData scenePoint in scenePoints)
		{
			cache.AddToCache(scenePoint);
		}
		UpdateLinkGraphData();
	}

	public void RemoveScenePoints(List<GDPointData> scenePoints)
	{
		foreach (GDPointData scenePoint in scenePoints)
		{
			this.scenePoints.Remove(scenePoint);
			cache?.RemoveFromCache(scenePoint);
		}
		if (waypoints != null)
		{
			UpdateLinkGraphData();
		}
	}

	public GDPointData GetGDPointDataById(string gdPointId)
	{
		if (cache.allPointsByIdCache.TryGetValue(gdPointId, out var value))
		{
			return value[0];
		}
		Debug.LogWarning("Can't find gd point with Id [" + gdPointId + "]");
		return null;
	}

	public List<GDPointData> GetGDPointsDataById(string gdPointId)
	{
		if (cache.allPointsByIdCache.TryGetValue(gdPointId, out var value))
		{
			return value;
		}
		return new List<GDPointData>();
	}

	public GDPointData GetGDPointDataByCustomTag(string customTag)
	{
		if (cache.allPointsByCustomTagCache.TryGetValue(customTag, out var value))
		{
			return value[0];
		}
		Debug.LogWarning("Can't find gd point with customTag [" + customTag + "]");
		return null;
	}

	public List<GDPointData> GetGDPointsDataByCustomTag(string customTag)
	{
		if (cache.allPointsByCustomTagCache.TryGetValue(customTag, out var value))
		{
			return value;
		}
		return new List<GDPointData>();
	}

	public GDPointData GetGDPointDataByView(GDPoint gdPoint)
	{
		if (cache.allPointsByIdCache.TryGetValue(gdPoint.Id, out var value))
		{
			foreach (GDPointData item in value)
			{
				if ((gdPoint.transform.position - item.Position).sqrMagnitude < 0.001f)
				{
					return item;
				}
			}
			return value[0];
		}
		return null;
	}

	public GDPointData GetGDPointDataByInstanceId(int instanceId)
	{
		return cache.allPointsByInstanceIdCache.GetValueOrDefault(instanceId);
	}

	private void UpdateLinkGraphData()
	{
		foreach (GDPointData point in Points)
		{
			point.LinkNextGdPointsData();
		}
	}

	private void InitDataFromConfig(GameSceneConfig gameSceneConfig)
	{
		GDPointData[] gdPointsData = gameSceneConfig.GdPointsData;
		for (int i = 0; i < gdPointsData.Length; i++)
		{
			GDPointData gDPointData = new GDPointData(gdPointsData[i]);
			waypoints.Add(gDPointData);
			cache.AddToCache(gDPointData);
		}
	}
}
