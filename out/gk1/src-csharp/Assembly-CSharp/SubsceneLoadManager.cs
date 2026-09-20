using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SubsceneLoadManager
{
	private class SubsceneLoader
	{
		private Action _on_scene_loaded;

		private List<GDPoint> _scene_gd_points;

		private List<WorldGameObject> _scene_wgos;

		private SubsceneLoadDependencies _subscene_load_dependencies;

		public string Name { get; }

		public List<GDPoint> Scene_gd_points => _scene_gd_points;

		public List<WorldGameObject> Scene_wgos => _scene_wgos;

		public SubsceneLoadDependencies Subscene_Load_Dependencies => _subscene_load_dependencies;

		public SubsceneLoader(string scene_name, Action on_scene_loaded)
		{
			Name = scene_name;
			_on_scene_loaded = on_scene_loaded;
		}

		public void OnSceneLoaded(Scene s, LoadSceneMode mode)
		{
			if (s.name == Name)
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
				_subscene_load_dependencies = FindSubsceneLoadDependenciesComponent(s);
				ScanWGOsOnLoadedScene(_subscene_load_dependencies.subworld, out _scene_wgos);
				RescanGDPointsForLoadedScene(_subscene_load_dependencies.subworld, out _scene_gd_points);
				WorldMap.ImportGDPointsOnLoadedScene(_scene_gd_points);
				RecalculateAStarForScene(_subscene_load_dependencies?.a_star_rescan_collider);
				UpdateGraph(WorldMap.gd_points);
				GJTimer.AddTimer(0.02f, delegate
				{
					_on_scene_loaded();
				});
			}
		}
	}

	private static List<SubsceneLoader> _subscene_loaders = new List<SubsceneLoader>();

	public static void Load(string scene_name, Action on_scene_loaded)
	{
		SubsceneLoader subsceneLoader = new SubsceneLoader(scene_name, on_scene_loaded);
		SceneManager.sceneLoaded += subsceneLoader.OnSceneLoaded;
		SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
		_subscene_loaders.Add(subsceneLoader);
	}

	private static void Unload()
	{
		if (_subscene_loaders.Count > 0)
		{
			SubsceneLoader subsceneLoader = _subscene_loaders.LastElement();
			WorldMap.ExportWGOsList(subsceneLoader.Scene_wgos);
			WorldMap.ExportGDPointsOnUnloadedScene(subsceneLoader.Scene_gd_points);
			RecalculateAStarForScene(subsceneLoader.Subscene_Load_Dependencies?.a_star_rescan_collider);
			UpdateGraph(WorldMap.gd_points);
			SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(subsceneLoader.Name));
			_subscene_loaders.RemoveAt(_subscene_loaders.Count - 1);
		}
	}

	public static void UnloadLastScene()
	{
		Unload();
		Resources.UnloadUnusedAssets();
	}

	public static void UnloadAllScenes()
	{
		int count = _subscene_loaders.Count;
		for (int i = 0; i < count; i++)
		{
			Unload();
		}
		Resources.UnloadUnusedAssets();
	}

	private static void RescanGDPointsForLoadedScene(GameObject subworld, out List<GDPoint> gd_points)
	{
		gd_points = null;
		if (!(subworld != null))
		{
			return;
		}
		gd_points = subworld.GetComponentsInChildren<GDPoint>(includeInactive: true).ToList();
		foreach (GDPoint gd_point in gd_points)
		{
			gd_point.ResetPos();
		}
		Debug.Log("RescanGDPoints, count = " + gd_points.Count);
	}

	public static void GetGDPoints(List<GDPoint> gd_points_list)
	{
		foreach (SubsceneLoader subscene_loader in _subscene_loaders)
		{
			foreach (GDPoint scene_gd_point in subscene_loader.Scene_gd_points)
			{
				gd_points_list.Add(scene_gd_point);
			}
		}
	}

	public static void CameraFlyToLastScene(GJCommons.VoidDelegate on_camera_moved)
	{
		if (_subscene_loaders.Count > 0)
		{
			SubsceneLoader subsceneLoader = _subscene_loaders.Last();
			GameObject gameObject = subsceneLoader.Subscene_Load_Dependencies?.camera_pos;
			if (gameObject != null)
			{
				CameraTools.CameraFlyTo(gameObject.transform, on_camera_moved, 0f);
				return;
			}
			Debug.Log("Subscene's camera pos didn't set. Choosing SubsceneLoadDependencies position");
			CameraTools.CameraFlyTo(subsceneLoader.Subscene_Load_Dependencies.transform, on_camera_moved, 0f);
		}
		else
		{
			Debug.LogError("There is no loaded scenes");
			on_camera_moved();
		}
	}

	private static void RecalculateAStarForScene(BoxCollider2D zone)
	{
		if (zone != null)
		{
			AStarTools.UpdateAstarBounds(zone.bounds);
		}
		else
		{
			Debug.LogError("Rescan zone is null");
		}
	}

	private static SubsceneLoadDependencies FindSubsceneLoadDependenciesComponent(Scene s)
	{
		GameObject[] rootGameObjects = s.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			SubsceneLoadDependencies componentInChildren = rootGameObjects[i].GetComponentInChildren<SubsceneLoadDependencies>();
			if (componentInChildren != null)
			{
				return componentInChildren;
			}
		}
		Debug.Log("SubsceneLoadDependencies component has not found in scene" + s.name);
		return null;
	}

	public static void UpdateGraph(List<GDPoint> gd_points_input)
	{
		((GDPointGraph)AstarPath.active.astarData.FindGraphOfType(typeof(GDPointGraph))).UpdateGraph(gd_points_input);
		Debug.Log("SubsceneLoadManager:UpdateGraph, updated points: " + gd_points_input.Count);
	}

	private static void ActivateWGOsOnLoadedScene(List<WorldGameObject> wgo_list)
	{
		foreach (WorldGameObject item in wgo_list)
		{
			WorldMap.ActivateGameObject(item.gameObject);
		}
		Debug.Log("SubsceneLoadManager:ActivateWGOsOnLoadedScene, WGOs activated: " + wgo_list.Count);
	}

	private static void ScanWGOsOnLoadedScene(GameObject subworld, out List<WorldGameObject> wgo_list)
	{
		wgo_list = null;
		if (subworld != null)
		{
			List<WorldGameObject> list = subworld.GetComponentsInChildren<WorldGameObject>(includeInactive: true).ToList();
			ActivateWGOsOnLoadedScene(list);
			wgo_list = list;
		}
		else
		{
			Debug.LogError("SubsceneLoadManager:ScanWGOsOnLoadedScene, subworld is null");
		}
	}
}
