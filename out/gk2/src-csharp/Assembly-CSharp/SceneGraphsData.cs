using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneGraphsData", menuName = "ScriptableObjects/Create scene graphs data object", order = 1)]
public class SceneGraphsData : ScriptableObject
{
	[Serializable]
	private class SceneGraphData
	{
		public string sceneName;

		public string worldId;

		public List<GraphData> recastGraphs = new List<GraphData>();
	}

	[Serializable]
	private class GraphData
	{
		public string name;

		public int index;
	}

	[SerializeField]
	private List<SceneGraphData> graphsIdsData = new List<SceneGraphData>();

	private int gdPointGraphIndex = -1;

	private int gdPointGraphMask = -1;

	public int GDPointGraphIndex
	{
		get
		{
			if (gdPointGraphIndex == -1)
			{
				gdPointGraphIndex = (int)AstarPath.active.data.FindGraphOfType(typeof(GDPointGraph)).graphIndex;
			}
			return gdPointGraphIndex;
		}
	}

	public int GDPointGraphMask
	{
		get
		{
			if (gdPointGraphMask == -1)
			{
				gdPointGraphMask = (int)Mathf.Pow(2f, GDPointGraphIndex);
			}
			return gdPointGraphMask;
		}
	}

	public IReadOnlyList<string> ConfiguredSceneNames
	{
		get
		{
			List<string> list = new List<string>(graphsIdsData.Count);
			foreach (SceneGraphData graphsIdsDatum in graphsIdsData)
			{
				if (!string.IsNullOrEmpty(graphsIdsDatum.sceneName))
				{
					list.Add(graphsIdsDatum.sceneName);
				}
			}
			return list;
		}
	}

	public Dictionary<int, List<string>> GetBakeGroupsByGraphIndex()
	{
		Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>();
		foreach (SceneGraphData graphsIdsDatum in graphsIdsData)
		{
			if (string.IsNullOrEmpty(graphsIdsDatum.sceneName))
			{
				continue;
			}
			foreach (GraphData recastGraph in graphsIdsDatum.recastGraphs)
			{
				if (!dictionary.TryGetValue(recastGraph.index, out var value))
				{
					value = new List<string>();
					dictionary[recastGraph.index] = value;
				}
				if (!value.Contains(graphsIdsDatum.sceneName))
				{
					value.Add(graphsIdsDatum.sceneName);
				}
			}
		}
		return dictionary;
	}

	public List<int> GetRecastGraphIndexesBySceneName(string sceneName)
	{
		SceneGraphData sceneGraphData = graphsIdsData.Find((SceneGraphData x) => x.sceneName == sceneName);
		if (sceneGraphData == null)
		{
			Debug.LogError("[RecastGraphIndex]Cant get graph data. Wrong sceneName [" + sceneName + "]");
			return new List<int>();
		}
		return sceneGraphData.recastGraphs.ConvertAll((GraphData x) => x.index);
	}

	public List<int> GetRecastGraphIndexByWorldId(string worldId)
	{
		SceneGraphData sceneGraphData = graphsIdsData.Find((SceneGraphData x) => x.worldId == worldId);
		if (sceneGraphData == null)
		{
			Debug.LogError("[RecastGraphIndex]Cant get graph data. Wrong worldId [" + worldId + "]");
			return new List<int>();
		}
		return sceneGraphData.recastGraphs.ConvertAll((GraphData x) => x.index);
	}

	public int GetRecastGraphMaskByWorldId(string worldId)
	{
		SceneGraphData sceneGraphData = graphsIdsData.Find((SceneGraphData x) => x.worldId == worldId);
		if (sceneGraphData == null)
		{
			Debug.LogError("[RecastGraphMask]Cant get graph data. Wrong worldId [" + worldId + "]");
			return 0;
		}
		int num = 0;
		foreach (GraphData recastGraph in sceneGraphData.recastGraphs)
		{
			num |= 1 << recastGraph.index;
		}
		return num;
	}
}
