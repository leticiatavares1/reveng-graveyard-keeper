using FlowCanvas;
using UnityEngine;

public class WgoDataScript : CustomFlowScript
{
	public const string WGO_DATA_SCRIPTS_RELATIVE_PATH = "Assets/AddressableAssets/VisualScripts/WGODataScripts/";

	public WgoData wgoData;

	public bool Run(WgoData wgoData, GameObject gameObject, string scriptName)
	{
		this.wgoData = wgoData;
		FlowGraph graph = GetGraph("Assets/AddressableAssets/VisualScripts/WGODataScripts/" + scriptName);
		if (graph == null)
		{
			return false;
		}
		Run(gameObject, graph, scriptName);
		return true;
	}
}
