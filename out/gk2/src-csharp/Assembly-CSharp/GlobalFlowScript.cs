using FlowCanvas;
using UnityEngine;

public class GlobalFlowScript : CustomFlowScript
{
	public delegate void TerminateCallBack(bool callOnFinished);

	public const string GLOBAL_SCRIPT_RELATIVE_PATH = "Assets/AddressableAssets/VisualScripts/GlobalScripts/";

	public bool Run(GameObject gameObject, string scriptName, TerminateCallBack onFinished = null)
	{
		return Run(gameObject, scriptName, onFinished, FlowScriptLoadMode.DeserializeOnInit);
	}

	public bool Run(GameObject gameObject, string scriptName, TerminateCallBack onFinished, FlowScriptLoadMode loadMode)
	{
		flowGraph = GetGraph("Assets/AddressableAssets/VisualScripts/GlobalScripts/" + scriptName);
		if (flowGraph == null)
		{
			return false;
		}
		Run(gameObject, flowGraph, scriptName, onFinished, loadMode);
		return true;
	}

	public bool Run(GameObject gameObject, FlowGraph graph, TerminateCallBack onFinished = null)
	{
		return Run(gameObject, graph, onFinished, FlowScriptLoadMode.DeserializeOnInit);
	}

	public bool Run(GameObject gameObject, FlowGraph graph, TerminateCallBack onFinished, FlowScriptLoadMode loadMode)
	{
		Run(gameObject, graph, graph.name, onFinished, loadMode);
		return true;
	}

	public override void Terminate(bool invokeOnFinished = true)
	{
		base.Terminate(invokeOnFinished);
		Object.Destroy(base.gameObject);
	}
}
