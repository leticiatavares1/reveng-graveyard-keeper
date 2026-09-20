using System;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameLoader
{
	private static string _cur_loading_scene = "";

	private static SceneDescription _sd_world = null;

	private static SceneDescription _sd_main = null;

	private static PlayerComponent _player_prefab = null;

	public static Vector3 player_prefab_pos = Vector3.zero;

	public static string initial_map_json = "";

	public static byte[] initial_map_bin;

	private static List<AsyncOperation> _additional_loaders = new List<AsyncOperation>();

	public static bool camera_initialized { get; private set; }

	public static void InitGameFromGUIScene()
	{
		string scene_name = GetWorldSceneName();
		Debug.Log("Init game from GUI scene. Loading world scene: " + scene_name);
		LoadingGUI.ShowWithProgressBar();
		GJTimer.AddTimer(0.01f, delegate
		{
			CommonLoadPart();
			LoadingGUI.LinkAsyncProcess(StartLoadingScene(scene_name));
		});
	}

	public static void InitGameFromWorldScene()
	{
		Debug.Log("Init game from World scene");
		CommonLoadPart();
		FindPlayerPrefabOnTheScene(default(Scene));
		StartLoadingScene("scene_main");
	}

	private static void CommonLoadPart()
	{
		if (true)
		{
			AsyncOperation item = SceneManager.LoadSceneAsync("title_screen", LoadSceneMode.Additive);
			_additional_loaders.Add(item);
			SceneManager.LoadSceneAsync("intro", LoadSceneMode.Additive);
			_additional_loaders.Add(item);
		}
	}

	private static bool IsAdditionalLoadersFinished()
	{
		foreach (AsyncOperation additional_loader in _additional_loaders)
		{
			if (!additional_loader.isDone)
			{
				return false;
			}
		}
		return true;
	}

	private static void FindPlayerPrefabOnTheScene(Scene s)
	{
		_player_prefab = UnityEngine.Object.FindObjectOfType<PlayerComponent>();
		if (_player_prefab == null)
		{
			Debug.Log("FindPlayerPrefabOnTheScene " + s.name);
			GameObject[] rootGameObjects = s.GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				Debug.Log("Checking obj " + gameObject.name, gameObject);
				_player_prefab = gameObject.GetComponent<PlayerComponent>();
				if (_player_prefab != null)
				{
					break;
				}
				_player_prefab = gameObject.GetComponentInChildren<PlayerComponent>(includeInactive: true);
				if (_player_prefab != null)
				{
					break;
				}
			}
		}
		if (_player_prefab == null)
		{
			Debug.LogError("Player prefab not found on the scene");
			return;
		}
		player_prefab_pos = _player_prefab.transform.position;
		UnityEngine.Object.Destroy(_player_prefab.gameObject);
	}

	private static string GetWorldSceneName()
	{
		return "scene_graveyard";
	}

	private static AsyncOperation StartLoadingScene(string scene_name)
	{
		Debug.Log("StartLoadingScene " + scene_name);
		_cur_loading_scene = scene_name;
		SceneManager.sceneLoaded += OnSceneLoaded;
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		return SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
	}

	private static void OnSceneLoaded(Scene s, LoadSceneMode mode)
	{
		Debug.Log("OnSceneLoaded: " + s.name);
		if (s.name == "title_screen")
		{
			OnTitleScreenSceneLoaded();
		}
		if (s.name == "intro")
		{
			OnIntroSceneLoaded(s);
		}
		if (s.name != _cur_loading_scene)
		{
			Debug.Log("Skipping wrong scene");
			return;
		}
		World.InitWorldOnApplicationStart();
		SceneDescription[] array = UnityEngine.Object.FindObjectsOfType<SceneDescription>();
		foreach (SceneDescription sceneDescription in array)
		{
			switch (sceneDescription.scene_type)
			{
			case SceneDescription.SceneType.WorldMap:
				_sd_world = sceneDescription;
				FindPlayerPrefabOnTheScene(sceneDescription.gameObject.scene);
				break;
			case SceneDescription.SceneType.MainScene:
				_sd_main = sceneDescription;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		if (_sd_main == null)
		{
			Debug.LogError("Scene description for the Main (GUI) scene not found!");
			return;
		}
		if (_sd_world == null)
		{
			Debug.LogError("Scene description for the World scene not found!");
			return;
		}
		MainGame.me.world.FindAndRemovePlayerPrefab();
		_sd_main.main_camera.InitGUIScene(_sd_world);
		initial_map_bin = MainGame.me.save.map.ToBinary(at_game_start: true);
		ProCamera2D.SetInstance(_sd_main.main_camera.GetComponentInChildren<ProCamera2D>());
		EnableNetworkDisabledComponents();
		GJTimer.AddTimer(0f, delegate
		{
			GDPoint.StoreGDPointsState();
			GJTimer.AddTimer(0.02f, OnBothMainScenesLoaded);
		});
		Debug.Log("OnSceneLoaded: Done");
	}

	private static void OnBothMainScenesLoaded()
	{
		if (!IsAdditionalLoadersFinished())
		{
			GJTimer.AddTimer(0.05f, OnBothMainScenesLoaded);
			return;
		}
		camera_initialized = true;
		if (true)
		{
			LoadingGUI.Hide();
		}
		if (Preloader.is_shown)
		{
			Preloader.Hide();
		}
	}

	private static void OnTitleScreenSceneLoaded()
	{
		Debug.Log("OnTitleScreenSceneLoaded");
	}

	private static void OnIntroSceneLoaded(Scene s)
	{
		Debug.Log("OnIntroSceneLoaded");
	}

	private static void EnableNetworkDisabledComponents()
	{
	}

	private static MainGame GetMainGameFromTheScene(Scene s)
	{
		GameObject[] rootGameObjects = s.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			if (gameObject.scene.name != s.name)
			{
				Debug.Log("Skipping go = " + gameObject.name + ", go.scene = " + gameObject.scene.name + ", looking for = " + s.name);
				continue;
			}
			Debug.Log("Checking go = " + gameObject.name);
			MainGame component = gameObject.GetComponent<MainGame>();
			if (component != null)
			{
				return component;
			}
			component = gameObject.GetComponentInChildren<MainGame>();
			if (component != null)
			{
				return component;
			}
		}
		Debug.LogError("MainGame component not found in a loaded scene");
		return null;
	}

	public static PlayerComponent GetPlayerPrefab()
	{
		return _player_prefab;
	}
}
