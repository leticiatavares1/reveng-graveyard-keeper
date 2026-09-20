using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public class GameSceneManager : LazySingleton<GameSceneManager>
{
	private List<string> loadedGameSceneIds = new List<string>();

	private List<GameScene> loadedGameScenes = new List<GameScene>();

	public List<GameScene> LoadedGameScenes => loadedGameScenes;

	public List<string> LoadedGameSceneIds => loadedGameSceneIds;

	protected override void Awake()
	{
		base.Awake();
		loadedGameSceneIds.Clear();
		loadedGameScenes.Clear();
	}

	public void LoadScene(string sceneId, Action onSceneLoaded = null)
	{
		if (sceneId == "MainScene")
		{
			return;
		}
		if (LazySceneManager.IsSceneLoadingOrLoaded(sceneId))
		{
			Debug.LogWarning("[GameSceneManager]: trying to load already loading or loaded GameScene [" + sceneId + "]");
			InvokeWhenGameSceneReady(sceneId, onSceneLoaded);
			return;
		}
		LazySceneManager.LoadScene(sceneId, delegate(SceneInstance sceneInstance)
		{
			HandleGameSceneLoadCompleted(sceneInstance, onSceneLoaded);
		});
		loadedGameSceneIds.Add(sceneId);
		Debug.Log("LoadScene: [" + sceneId + "]");
	}

	public async UniTask LoadSceneAsync(string sceneId, IProgress<float> progress = null)
	{
		if (sceneId == "MainScene")
		{
			return;
		}
		while (LazySceneManager.IsSceneUnloading(sceneId))
		{
			GameShutdown.ThrowIfRequested();
			await UniTask.Yield();
		}
		if (LazySceneManager.IsSceneLoadingOrLoaded(sceneId))
		{
			Debug.LogWarning("[GameSceneManager]: trying to load already loading or loaded GameScene [" + sceneId + "]");
			return;
		}
		loadedGameSceneIds.Add(sceneId);
		Debug.Log("LoadSceneAsync: [" + sceneId + "]");
		SceneInstance sceneInstance = await LazySceneManager.LoadSceneAsync(sceneId, progress);
		bool isRequested = GameShutdown.IsRequested;
		if (isRequested || !sceneInstance.Scene.IsValid())
		{
			UnloadScene(sceneId);
			if (isRequested)
			{
				throw new OperationCanceledException("[GameSceneManager] GameScene [" + sceneId + "] load was cancelled by shutdown");
			}
			throw new Exception("[GameSceneManager] GameScene [" + sceneId + "] loaded with an invalid SceneInstance");
		}
		GameObject[] rootGameObjects = sceneInstance.Scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			if (rootGameObjects[i].TryGetComponent<GameScene>(out var gameScene))
			{
				loadedGameScenes.Add(gameScene);
				if (!(await gameScene.WaitForStartCompleted()))
				{
					Debug.LogWarning("[GameSceneManager] GameScene start was not completed for scene [" + gameScene.Id + "]");
				}
				break;
			}
			gameScene = null;
		}
	}

	public void UnloadScene(string sceneId, Action onSceneUnloaded = null)
	{
		if (!(sceneId == "MainScene"))
		{
			Debug.Log("UnloadScene: [" + sceneId + "]");
			LazySceneManager.UnloadScene(sceneId, HandleGameSceneUnloadStarted, onSceneUnloaded);
			loadedGameSceneIds.Remove(sceneId);
		}
	}

	public void UnloadAllScenes()
	{
		for (int num = loadedGameSceneIds.Count - 1; num >= 0; num--)
		{
			UnloadScene(loadedGameSceneIds[num]);
		}
	}

	public bool DisabledUnloadOnTeleport(string sceneId)
	{
		if ((bool)LazySingleton<GameSceneManager>.Instance)
		{
			return LazySingleton<GameSceneManager>.Instance.LoadedGameScenes.FirstOrDefault((GameScene x) => x.Id == sceneId)?.DisableUnloadOnTeleport ?? false;
		}
		return false;
	}

	private async void InvokeWhenGameSceneReady(string sceneId, Action onSceneLoadedCallback)
	{
		_ = 1;
		try
		{
			SceneInstance sceneInstance = await LazySceneManager.AwaitSceneLoadedAsync(sceneId);
			if (!loadedGameSceneIds.Contains(sceneId))
			{
				loadedGameSceneIds.Add(sceneId);
			}
			if (await RegisterGameSceneWhenReady(sceneId, sceneInstance) == null)
			{
				Debug.LogError("[GameSceneManager] GameScene component not found for scene [" + sceneId + "]");
			}
			else
			{
				onSceneLoadedCallback?.Invoke();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[GameSceneManager] Failed to wait for scene [" + sceneId + "]: " + ex.Message);
		}
	}

	private async void HandleGameSceneLoadCompleted(SceneInstance sceneInstance, Action onSceneLoadedCallback)
	{
		GameScene gameScene = null;
		GameObject[] rootGameObjects = sceneInstance.Scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			if (rootGameObjects[i].TryGetComponent<GameScene>(out gameScene))
			{
				loadedGameScenes.Add(gameScene);
				break;
			}
		}
		bool flag = gameScene != null;
		if (flag)
		{
			flag = !(await gameScene.WaitForStartCompleted());
		}
		if (flag)
		{
			Debug.LogWarning("[GameSceneManager] GameScene start was not completed for scene [" + gameScene.Id + "]");
		}
		onSceneLoadedCallback?.Invoke();
	}

	private async UniTask<GameScene> RegisterGameSceneWhenReady(string sceneId, SceneInstance sceneInstance)
	{
		GameScene gameScene = loadedGameScenes.FirstOrDefault((GameScene x) => x.Id == sceneId);
		if (gameScene == null)
		{
			GameObject[] rootGameObjects = sceneInstance.Scene.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].TryGetComponent<GameScene>(out gameScene))
				{
					if (!loadedGameScenes.Contains(gameScene))
					{
						loadedGameScenes.Add(gameScene);
					}
					break;
				}
			}
		}
		bool flag = gameScene != null;
		if (flag)
		{
			flag = !(await gameScene.WaitForStartCompleted());
		}
		if (flag)
		{
			Debug.LogWarning("[GameSceneManager] GameScene start was not completed for scene [" + gameScene.Id + "]");
		}
		return gameScene;
	}

	private void HandleGameSceneUnloadStarted(SceneInstance sceneInstance)
	{
		GameObject[] rootGameObjects = sceneInstance.Scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			if (rootGameObjects[i].TryGetComponent<GameScene>(out var component))
			{
				loadedGameScenes.Remove(component);
				break;
			}
		}
	}
}
