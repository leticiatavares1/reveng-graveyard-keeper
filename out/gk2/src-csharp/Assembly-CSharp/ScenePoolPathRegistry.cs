using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class ScenePoolPathRegistry : LazySingleton<ScenePoolPathRegistry>
{
	[SerializeField]
	private ScenePoolTrimPolicy trimPolicy;

	private readonly Dictionary<string, HashSet<string>> sceneIdToPaths = new Dictionary<string, HashSet<string>>();

	private readonly Dictionary<string, int> pathRefCount = new Dictionary<string, int>();

	public ScenePoolTrimPolicy TrimPolicy
	{
		get
		{
			return trimPolicy;
		}
		set
		{
			trimPolicy = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void RegisterScenePaths(string sceneId, IEnumerable<string> paths)
	{
		if (string.IsNullOrEmpty(sceneId) || paths == null)
		{
			return;
		}
		if (!sceneIdToPaths.TryGetValue(sceneId, out var value))
		{
			value = new HashSet<string>();
			sceneIdToPaths[sceneId] = value;
		}
		foreach (string path in paths)
		{
			if (!string.IsNullOrEmpty(path) && value.Add(path))
			{
				pathRefCount.TryGetValue(path, out var value2);
				pathRefCount[path] = value2 + 1;
			}
		}
	}

	public void UnregisterScenePaths(string sceneId)
	{
		if (string.IsNullOrEmpty(sceneId) || !sceneIdToPaths.TryGetValue(sceneId, out var value))
		{
			return;
		}
		sceneIdToPaths.Remove(sceneId);
		List<string> list = new List<string>();
		foreach (string item in value)
		{
			if (pathRefCount.TryGetValue(item, out var value2))
			{
				value2--;
				if (value2 <= 0)
				{
					pathRefCount.Remove(item);
					list.Add(item);
				}
				else
				{
					pathRefCount[item] = value2;
				}
			}
		}
		if (list.Count != 0)
		{
			TrimOrphanedPaths(sceneId, list);
		}
	}

	private void TrimOrphanedPaths(string sceneId, List<string> paths)
	{
		int num = BakedChunkableObjectPool.TrimPaths(paths, trimPolicy);
		int num2 = ConstructorPartPool.TrimPaths(paths, trimPolicy);
		int num3 = WgoPartPool.TrimPaths(paths, trimPolicy);
		int num4 = num + num2 + num3;
		if (num4 > 0)
		{
			Debug.Log($"[ScenePoolPathRegistry] scene=[{sceneId}] policy={trimPolicy} trimmed baked={num} constructor={num2} wgoPart={num3} total={num4}");
		}
	}

	private void LogRegistryState()
	{
		Debug.Log(string.Format("[{0}] policy={1}, scenes={2}, trackedPaths={3}", "ScenePoolPathRegistry", trimPolicy, sceneIdToPaths.Count, pathRefCount.Count));
	}
}
