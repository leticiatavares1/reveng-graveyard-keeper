using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class LazySceneInfo
{
	public string sceneId;

	public SceneStatus status;

	public SceneInstance sceneInstance;

	public AsyncOperationHandle sceneHandle;

	public Action<SceneInstance> onLoadedCallback;
}
