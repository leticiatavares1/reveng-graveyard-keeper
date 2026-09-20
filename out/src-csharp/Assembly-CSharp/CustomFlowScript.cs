using FlowCanvas;
using NodeCanvas.Framework;
using UnityEngine;

public class CustomFlowScript : CustomScript
{
	public delegate void OnFinishedDelegate(string script_name);

	private FlowScriptController _fsc;

	private Blackboard _bb;

	private OnFinishedDelegate _on_finished;

	public UniversalStorage storage = new UniversalStorage();

	public static CustomFlowScript Create(GameObject parent_go, string script_name, bool is_global = false, OnFinishedDelegate on_finished = null)
	{
		FlowGraph graph = GetGraph(script_name);
		if (graph == null)
		{
			Debug.LogError("Error loading graph: " + script_name);
			return null;
		}
		return Create(parent_go, graph, is_global, on_finished, script_name);
	}

	public static CustomFlowScript Create(GameObject parent_go, FlowGraph g, bool is_global = false, OnFinishedDelegate on_finished = null, string custom_script_name = null)
	{
		string text = (string.IsNullOrEmpty(custom_script_name) ? g.name : custom_script_name);
		Debug.Log("<color=yellow>Run FlowScript:</color> " + text + ", parent_go: " + ((parent_go != null) ? parent_go.name : "null") + ", is_global = " + is_global, parent_go);
		Stats.DesignEvent("FlowScript:" + text);
		GameObject gameObject = new GameObject("[FS] " + text);
		if (parent_go != null)
		{
			gameObject.transform.SetParent(parent_go.transform, worldPositionStays: false);
		}
		CustomFlowScript customFlowScript = gameObject.AddComponent<CustomFlowScript>();
		customFlowScript.is_global = is_global;
		customFlowScript.script_name = text;
		customFlowScript._fsc = gameObject.AddComponent<FlowScriptController>();
		customFlowScript._fsc.disableAction = GraphOwner.DisableAction.DoNothing;
		customFlowScript._bb = gameObject.AddComponent<Blackboard>();
		customFlowScript._on_finished = on_finished;
		IBlackboard blackboard = (customFlowScript._fsc.blackboard = customFlowScript._bb);
		g.blackboard = blackboard;
		customFlowScript._fsc.graph = g;
		customFlowScript.started = true;
		return customFlowScript;
	}

	public static FlowGraph GetGraph(string name)
	{
		FlowScript resourceAs = SmartResourceHelper.GetResourceAs<FlowScript>("FlowCanvas/" + name);
		if (resourceAs == null)
		{
			Debug.LogError("No flow script: " + name);
			return null;
		}
		return resourceAs;
	}

	public void FireEvent(string event_id)
	{
		Debug.Log("Fire event:[" + event_id + "] on global script:[" + script_name + "]");
		_fsc.SendEvent(event_id);
	}

	public void FireEvent(string event_id, string param)
	{
		Debug.Log("Fire event:[" + event_id + "] on global script:[" + script_name + "] param:[" + param + "]");
		_fsc.SendEvent(event_id, param);
	}

	public void StartBehaviour()
	{
		_fsc.StartBehaviour();
	}

	public override void TerminateMe()
	{
		_fsc.PauseBehaviour();
		_fsc.graph = null;
		base.TerminateMe();
		if (_on_finished != null)
		{
			_on_finished(script_name);
		}
	}
}
