using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameSceneConfig : ScriptableObject
{
	public const string GAME_SCENE_CONFIG_PATH = "Assets/AddressableAssets/SceneContents/_GameSceneConfigs";

	public const string DEV_GAME_SCENE_CONFIG_PATH = "Assets/AddressableAssets/SceneContentsDev/_GameSceneConfigs";

	private const string SCENE_CONTENT_DATA_REF_PATH = "Assets/AddressableAssets/SceneContents/WgoContents";

	private const string DEV_SCENE_CONTENT_DATA_REF_PATH = "Assets/AddressableAssets/SceneContentsDev/WgoContents";

	private const string SCENE_DATA_CONTENT_DATA_REF_PATH = "Assets/AddressableAssets/SceneContents/WgoContentsData";

	private const string DEV_SCENE_DATA_CONTENT_DATA_REF_PATH = "Assets/AddressableAssets/SceneContentsDev/WgoContentsData";

	private const string SCENE_WAYPOINT_CONTENT_REF_PATH = "Assets/AddressableAssets/SceneContents/WaypointContents";

	private const string DEV_SCENE_WAYPOINT_CONTENT_REF_PATH = "Assets/AddressableAssets/SceneContentsDev/WaypointContents";

	public const string FIGHTING_LEVEL_CONTENT_REF_PATH = "Assets/AddressableAssets/Fighting/Levels/Contents";

	private const string CONFIG_ADDRESSABLES_LABEL = "GameSceneConfigs";

	public Vector3 sceneGlobalPosition;

	public List<AssetReference> contentDataRefs;

	public List<bool> autoLoadFlags;

	public List<string> assetNames = new List<string>();

	public List<string> assetGuids = new List<string>();

	[SerializeField]
	private GDPointData[] gdPointsData;

	[SerializeField]
	private List<IndoorAreaData> indoorAreas = new List<IndoorAreaData>();

	public bool disableUnloadOnTeleport;

	private Dictionary<AssetReference, GameObject> contentDataRefInstances = new Dictionary<AssetReference, GameObject>();

	public GDPointData[] GdPointsData => gdPointsData;

	public IReadOnlyList<IndoorAreaData> IndoorAreas => indoorAreas ?? (indoorAreas = new List<IndoorAreaData>());

	public static string GetPathToGameSceneConfig(bool isDev)
	{
		if (!isDev)
		{
			return "Assets/AddressableAssets/SceneContents/_GameSceneConfigs";
		}
		return "Assets/AddressableAssets/SceneContentsDev/_GameSceneConfigs";
	}

	public static string GetPathToSceneData(bool isDev)
	{
		if (!isDev)
		{
			return "Assets/AddressableAssets/SceneContents/WgoContentsData";
		}
		return "Assets/AddressableAssets/SceneContentsDev/WgoContentsData";
	}

	public static string GetPathToSceneContent(bool isDev)
	{
		if (!isDev)
		{
			return "Assets/AddressableAssets/SceneContents/WgoContents";
		}
		return "Assets/AddressableAssets/SceneContentsDev/WgoContents";
	}

	public static string GetPathToSceneWaypoint(bool isDev)
	{
		if (!isDev)
		{
			return "Assets/AddressableAssets/SceneContents/WaypointContents";
		}
		return "Assets/AddressableAssets/SceneContentsDev/WaypointContents";
	}

	public static List<GameSceneConfig> LoadAllConfigs()
	{
		List<GameSceneConfig> configs = new List<GameSceneConfig>();
		Debug.Log($"#shutdown# GameSceneConfig.LoadAllConfigs: WaitForCompletion begin (shutdownRequested:[{GameShutdown.IsRequested}])");
		if (GameShutdown.IsQuitting)
		{
			return configs;
		}
		Addressables.LoadAssetsAsync("GameSceneConfigs", delegate(ScriptableObject obj)
		{
			if (obj is GameSceneConfig item)
			{
				configs.Add(item);
			}
		}).WaitForCompletion();
		Debug.Log("#shutdown# GameSceneConfig.LoadAllConfigs: WaitForCompletion done");
		Debug.Log($"Loaded {configs.Count} GameSceneConfigs");
		return configs;
	}

	public bool TryLoadSceneDataContent(out List<SceneWgoContentData> sceneWgoContentDatas)
	{
		if (contentDataRefs == null || contentDataRefs.Count == 0)
		{
			sceneWgoContentDatas = null;
			return false;
		}
		contentDataRefInstances = new Dictionary<AssetReference, GameObject>();
		sceneWgoContentDatas = new List<SceneWgoContentData>();
		for (int i = 0; i < contentDataRefs.Count; i++)
		{
			AssetReference contentDataRef = contentDataRefs[i];
			if ((autoLoadFlags == null || autoLoadFlags.Count <= i || autoLoadFlags[i]) && TryLoadSceneWgoContentData(contentDataRef, out var sceneWgoContentData))
			{
				sceneWgoContentDatas.Add(sceneWgoContentData);
			}
		}
		return sceneWgoContentDatas.Count > 0;
	}

	public bool TryLoadSceneDataContentByName(string contentName, out SceneWgoContentData sceneWgoContentData)
	{
		sceneWgoContentData = null;
		AssetReference contentDataRef = GetContentDataRef(contentName);
		if (contentDataRef == null)
		{
			return false;
		}
		return TryLoadSceneWgoContentData(contentDataRef, out sceneWgoContentData);
	}

	public bool TryUnloadSceneDataContent(string contentName)
	{
		AssetReference contentDataRef = GetContentDataRef(contentName);
		if (contentDataRef != null && contentDataRefInstances.TryGetValue(contentDataRef, out var value))
		{
			contentDataRef.ReleaseInstance(value);
			contentDataRefInstances.Remove(contentDataRef);
			return true;
		}
		return false;
	}

	public bool IsSceneContentDataLoaded(string contentName)
	{
		AssetReference contentDataRef = GetContentDataRef(contentName);
		return IsSceneContentDataLoaded(contentDataRef);
	}

	public void UnloadSceneDataContents()
	{
		if (contentDataRefInstances == null)
		{
			return;
		}
		foreach (KeyValuePair<AssetReference, GameObject> contentDataRefInstance in contentDataRefInstances)
		{
			contentDataRefInstance.Key.ReleaseInstance(contentDataRefInstance.Value);
		}
		contentDataRefInstances.Clear();
	}

	public void UnloadSceneDataContentByObject(GameObject contentObject)
	{
		if (!(contentObject == null))
		{
			AssetReference key = contentDataRefInstances.FirstOrDefault((KeyValuePair<AssetReference, GameObject> x) => x.Value == contentObject).Key;
			if (key != null)
			{
				key.ReleaseInstance(contentObject);
				contentDataRefInstances.Remove(key);
			}
		}
	}

	public AssetReference GetContentDataRef(string contentName)
	{
		contentName += "Data";
		int num = assetNames.FindIndex((string c) => c == contentName);
		if (num <= -1)
		{
			return null;
		}
		return contentDataRefs[num];
	}

	public AssetReference GetContentDataRefByGuid(string assetGuid)
	{
		if (string.IsNullOrEmpty(assetGuid) || contentDataRefs == null)
		{
			return null;
		}
		for (int i = 0; i < contentDataRefs.Count; i++)
		{
			AssetReference assetReference = contentDataRefs[i];
			if (assetReference != null && assetReference.AssetGUID == assetGuid)
			{
				return assetReference;
			}
		}
		if (assetGuids != null)
		{
			int num = assetGuids.FindIndex((string guid) => guid == assetGuid);
			if (num > -1 && num < contentDataRefs.Count)
			{
				return contentDataRefs[num];
			}
		}
		return null;
	}

	public bool TryLoadSceneDataContentByRef(AssetReference contentDataRef, out SceneWgoContentData sceneWgoContentData)
	{
		return TryLoadSceneWgoContentData(contentDataRef, out sceneWgoContentData);
	}

	public bool IsSceneContentDataLoaded(AssetReference contentDataRef)
	{
		GameObject contentDataInstance;
		return TryGetLoadedContentInstance(contentDataRef, out contentDataInstance);
	}

	public bool TryUnloadSceneDataContent(AssetReference contentDataRef)
	{
		if (!TryGetLoadedContentInstance(contentDataRef, out var contentDataInstance))
		{
			return false;
		}
		AssetReference loadedContentDictionaryKey = GetLoadedContentDictionaryKey(contentDataRef);
		if (loadedContentDictionaryKey == null)
		{
			return false;
		}
		loadedContentDictionaryKey.ReleaseInstance(contentDataInstance);
		contentDataRefInstances.Remove(loadedContentDictionaryKey);
		return true;
	}

	public bool TryGetLoadedContent(string contentName, out GameObject contentDataInstance)
	{
		contentDataInstance = null;
		AssetReference contentDataRef = GetContentDataRef(contentName);
		if (contentDataRef == null)
		{
			return false;
		}
		return contentDataRefInstances.TryGetValue(contentDataRef, out contentDataInstance);
	}

	private bool TryGetLoadedContentInstance(AssetReference contentDataRef, out GameObject contentDataInstance)
	{
		contentDataInstance = null;
		if (contentDataRef == null || contentDataRefInstances == null)
		{
			return false;
		}
		if (contentDataRefInstances.TryGetValue(contentDataRef, out contentDataInstance))
		{
			return contentDataInstance != null;
		}
		AssetReference loadedContentDictionaryKey = GetLoadedContentDictionaryKey(contentDataRef);
		if (loadedContentDictionaryKey == null)
		{
			return false;
		}
		if (contentDataRefInstances.TryGetValue(loadedContentDictionaryKey, out contentDataInstance))
		{
			return contentDataInstance != null;
		}
		return false;
	}

	private AssetReference GetLoadedContentDictionaryKey(AssetReference contentDataRef)
	{
		if (contentDataRef == null || contentDataRefInstances == null)
		{
			return null;
		}
		if (contentDataRefInstances.ContainsKey(contentDataRef))
		{
			return contentDataRef;
		}
		string assetGUID = contentDataRef.AssetGUID;
		if (string.IsNullOrEmpty(assetGUID))
		{
			return null;
		}
		foreach (KeyValuePair<AssetReference, GameObject> contentDataRefInstance in contentDataRefInstances)
		{
			if (contentDataRefInstance.Key != null && contentDataRefInstance.Key.AssetGUID == assetGUID)
			{
				return contentDataRefInstance.Key;
			}
		}
		return GetContentDataRefByGuid(assetGUID);
	}

	private bool TryLoadSceneWgoContentData(AssetReference contentDataRef, out SceneWgoContentData sceneWgoContentData)
	{
		sceneWgoContentData = null;
		if (contentDataRefInstances == null)
		{
			contentDataRefInstances = new Dictionary<AssetReference, GameObject>();
		}
		if (contentDataRefInstances.TryGetValue(contentDataRef, out var value))
		{
			sceneWgoContentData = value.GetComponent<SceneWgoContentData>();
			return true;
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = contentDataRef.InstantiateAsync();
		asyncOperationHandle.WaitForCompletion();
		contentDataRefInstances.Add(contentDataRef, asyncOperationHandle.Result);
		asyncOperationHandle.Result.transform.position = Vector3.zero;
		asyncOperationHandle.Result.SetActive(value: true);
		if (asyncOperationHandle.Result.TryGetComponent<SceneWgoContentData>(out sceneWgoContentData))
		{
			return true;
		}
		return false;
	}
}
