using System;
using FlowCanvas;
using UnityEngine;

public static class GameScriptUtility
{
	public static void RunGlobalScript(string globalScriptName, Action callback = null)
	{
		GlobalScriptsManager.RunFlowScript(globalScriptName, callback);
	}

	public static void RunGlobalScript(FlowScript flowScript, Action callback = null)
	{
		GlobalScriptsManager.RunFlowScript(flowScript, callback);
	}

	public static void FireEvent(GameObject gameObject, string eventName)
	{
		Internal_FireEvent(gameObject, eventName);
	}

	private static void Internal_FireEvent(GameObject gameObject, string eventName)
	{
		CustomFlowScript component = gameObject.GetComponent<CustomFlowScript>();
		if (component != null)
		{
			component.FireEvent(eventName);
		}
		else
		{
			Debug.LogError($"Not found flow script component in game object: {gameObject}, event name: {eventName}");
		}
	}

	public static void TerminateScript(GameObject gameObject)
	{
		if (gameObject != null)
		{
			CustomFlowScript component = gameObject.GetComponent<CustomFlowScript>();
			if (component != null)
			{
				component.Terminate();
			}
		}
	}

	public static void PauseScript(GameObject gameObject)
	{
		if (gameObject != null)
		{
			gameObject.GetComponent<CustomFlowScript>().PauseBehaviour();
		}
	}

	public static void StartScriptOnObject(GameObject gameObject)
	{
		if (gameObject != null)
		{
			gameObject.GetComponent<CustomFlowScript>().StartBehaviour();
		}
	}
}
