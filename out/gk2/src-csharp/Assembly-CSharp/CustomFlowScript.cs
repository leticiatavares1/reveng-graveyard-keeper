using System;
using FlowCanvas;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class CustomFlowScript : MonoBehaviour
{
	protected GlobalFlowScript.TerminateCallBack OnFinished;

	protected FlowScriptController flowScriptController;

	protected Blackboard blackboard;

	protected FlowGraph flowGraph;

	private static bool wasCustomExceptionHandlerRegistered;

	public string ScriptName { get; private set; }

	private void Awake()
	{
		if (!wasCustomExceptionHandlerRegistered)
		{
			ResourceManager.ExceptionHandler = CustomExceptionHandler;
			wasCustomExceptionHandlerRegistered = true;
		}
	}

	protected void Run(GameObject gameObject, FlowGraph graph, string scriptName, GlobalFlowScript.TerminateCallBack onFinished = null, FlowScriptLoadMode loadMode = FlowScriptLoadMode.DeserializeOnInit)
	{
		ScriptName = scriptName;
		OnFinished = onFinished;
		flowScriptController = gameObject.AddComponent<FlowScriptController>();
		blackboard = gameObject.AddComponent<Blackboard>();
		flowScriptController.graph = graph;
		flowScriptController.blackboard = blackboard;
		if (loadMode == FlowScriptLoadMode.DeserializeOnInit)
		{
			flowScriptController.Initialize();
		}
		if (gameObject.activeInHierarchy)
		{
			flowScriptController.StartBehaviour();
		}
	}

	public void Reattach(FlowGraph graph, string scriptName)
	{
		ScriptName = scriptName;
		flowScriptController.SwitchBehaviour(graph as FlowScript);
		UnityEngine.Object.Destroy(blackboard);
		blackboard = base.gameObject.AddComponent<Blackboard>();
		flowScriptController.blackboard = blackboard;
	}

	public virtual void Terminate(bool invokeOnFinished = true)
	{
		if (flowGraph != null)
		{
			FlowScriptAssetLoadPolicy.ReleaseGraph(flowGraph);
			flowGraph = null;
		}
		flowScriptController.StopBehaviour();
		OnFinished?.Invoke(invokeOnFinished);
		UnityEngine.Object.Destroy(flowScriptController);
		UnityEngine.Object.Destroy(blackboard);
		UnityEngine.Object.Destroy(this);
	}

	public void FireEvent(string eventName)
	{
		D.LogColor("FireEvent [" + eventName + "] at [" + base.name + "]", "yellow", this);
		flowScriptController.SendEvent(eventName);
	}

	public void PauseBehaviour()
	{
		flowScriptController.PauseBehaviour();
	}

	public void StartBehaviour()
	{
		flowScriptController.StartBehaviour();
	}

	protected FlowGraph GetGraph(string graphPath)
	{
		if (flowGraph != null)
		{
			FlowScriptAssetLoadPolicy.ReleaseGraph(flowGraph);
			flowGraph = null;
		}
		flowGraph = FlowScriptAssetLoadPolicy.Load(graphPath);
		return flowGraph;
	}

	private void CustomExceptionHandler(AsyncOperationHandle handle, Exception exception)
	{
		string text = string.Empty;
		if (exception is InvalidKeyException { Key: string key } && key.StartsWith("VoiceOvers/"))
		{
			text = key.Replace("VoiceOvers/", "");
		}
		if (string.IsNullOrEmpty(text))
		{
			Addressables.LogException(handle, exception);
		}
		else
		{
			Debug.LogWarning("Failed to load VoiceOver: " + text);
		}
	}
}
