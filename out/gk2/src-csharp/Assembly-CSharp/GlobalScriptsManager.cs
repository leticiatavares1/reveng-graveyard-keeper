using System;
using System.Collections.Generic;
using FlowCanvas;
using UnityEngine;

public class GlobalScriptsManager : MonoBehaviour
{
	private const string GLOBAL_FLOW_SCRIPT_OBJECT_PREFIX = "[g_fs]";

	private static GlobalScriptsManager cachedInstance;

	private readonly List<GlobalFlowScript> runningScripts = new List<GlobalFlowScript>();

	public static GlobalScriptsManager Instance
	{
		get
		{
			if (cachedInstance == null)
			{
				cachedInstance = UnityEngine.Object.FindObjectOfType<GlobalScriptsManager>();
				if (cachedInstance == null)
				{
					Debug.LogError($"Cannot find instance {typeof(GlobalScriptsManager)} On Scene.");
				}
			}
			return cachedInstance;
		}
	}

	public static GlobalFlowScript RunFlowScript(string scriptName, Action onFinished, FlowScriptLoadMode loadMode = FlowScriptLoadMode.DeserializeOnInit)
	{
		D.LogColor($"Running global script [{scriptName}] ({loadMode})", "yellow");
		GameObject scriptObject = new GameObject("[g_fs] " + scriptName);
		scriptObject.transform.SetParent(Instance.transform);
		GlobalFlowScript globalScript = scriptObject.AddComponent<GlobalFlowScript>();
		Instance.runningScripts.Add(globalScript);
		if (!globalScript.Run(scriptObject, scriptName, delegate(bool invokeOnFinished)
		{
			if (invokeOnFinished)
			{
				onFinished?.Invoke();
			}
			Instance.runningScripts.Remove(globalScript);
			UnityEngine.Object.Destroy(scriptObject);
		}, loadMode))
		{
			D.LogColor("Running global script [" + scriptName + "] was failed. Terminating.", "red");
			Instance.runningScripts.Remove(globalScript);
			UnityEngine.Object.Destroy(scriptObject);
			return null;
		}
		return globalScript;
	}

	public static void RunFlowScript(FlowScript flowScript, Action onFinished, FlowScriptLoadMode loadMode = FlowScriptLoadMode.DeserializeOnInit)
	{
		D.LogColor($"Running flow script [{flowScript.name}] ({loadMode})", "yellow");
		GameObject scriptObject = new GameObject("[g_fs] " + flowScript.name);
		scriptObject.transform.SetParent(Instance.transform);
		GlobalFlowScript globalScript = scriptObject.AddComponent<GlobalFlowScript>();
		Instance.runningScripts.Add(globalScript);
		if (!globalScript.Run(scriptObject, flowScript, delegate(bool invokeOnFinished)
		{
			if (invokeOnFinished)
			{
				onFinished?.Invoke();
			}
			Instance.runningScripts.Remove(globalScript);
			UnityEngine.Object.Destroy(scriptObject);
		}, loadMode))
		{
			D.LogColor("Running global script [" + flowScript.name + "] was failed. Terminating.", "red");
			Instance.runningScripts.Remove(globalScript);
			UnityEngine.Object.Destroy(scriptObject);
		}
	}

	public static void FireEvent(string scriptName, string eventName, Action onFinished = null)
	{
		GlobalFlowScript globalFlowScript = RunFlowScript(scriptName, onFinished);
		if (globalFlowScript != null)
		{
			globalFlowScript.FireEvent(eventName);
		}
	}

	public static bool HasFlowScript(string scriptName)
	{
		return Instance.runningScripts.Find((GlobalFlowScript s) => s.ScriptName == scriptName) != null;
	}

	public static void TerminateAllRunningScripts()
	{
		for (int num = Instance.runningScripts.Count - 1; num >= 0; num--)
		{
			Instance.runningScripts[0].Terminate();
		}
	}
}
