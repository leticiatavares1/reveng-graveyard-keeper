using System.Collections.Generic;
using UnityEngine;

public class WgoDataScriptsManager : MonoBehaviour
{
	private static WgoDataScriptsManager cachedInstance;

	private readonly Dictionary<SGuid, WgoDataScript> runningScripts = new Dictionary<SGuid, WgoDataScript>();

	public static WgoDataScriptsManager Instance
	{
		get
		{
			if (cachedInstance == null)
			{
				cachedInstance = Object.FindObjectOfType<WgoDataScriptsManager>();
				if (cachedInstance == null)
				{
					Debug.LogError($"Cannot find instance {typeof(WgoDataScriptsManager)} On Scene.");
				}
			}
			return cachedInstance;
		}
	}

	public static void CreateScript(WgoData wgoData, string scriptName)
	{
		D.LogColor("Running script: [" + scriptName + "] on WgoData: [" + wgoData.id + "].", "yellow");
		if (!Instance.runningScripts.ContainsKey(wgoData.UniqueId))
		{
			GameObject gameObject = new GameObject(wgoData.id ?? "");
			gameObject.transform.SetParent(Instance.transform);
			WgoDataScript wgoDataScript = gameObject.AddComponent<WgoDataScript>();
			if (!wgoDataScript.Run(wgoData, gameObject, scriptName))
			{
				D.LogColor("Running script: [" + scriptName + "] on WgoData: [" + wgoData.id + "] was failed. Terminating.", "red");
				Object.Destroy(gameObject);
			}
			else
			{
				Instance.runningScripts.TryAdd(wgoData.UniqueId, wgoDataScript);
			}
		}
	}

	public static void FireEvent(WgoData wgoData, string eventName)
	{
		if (!Instance.runningScripts.TryGetValue(wgoData.UniqueId, out var value))
		{
			Debug.LogError("[WgoDataScriptsManager]: cannot find active script for wgo data [" + wgoData.id + "].");
		}
		else
		{
			value.FireEvent(eventName);
		}
	}

	public static void DestroyScript(WgoData wgoData)
	{
		if (!Instance.runningScripts.TryGetValue(wgoData.UniqueId, out var value))
		{
			Debug.LogError("[WgoDataScriptsManager]: script for wgo data [" + wgoData.id + "] has already been destroyed.");
			return;
		}
		Instance.runningScripts.Remove(wgoData.UniqueId);
		value.Terminate(invokeOnFinished: false);
		Object.Destroy(value.gameObject);
	}

	public static void DestroyAll()
	{
		foreach (WgoDataScript value in Instance.runningScripts.Values)
		{
			value.Terminate(invokeOnFinished: false);
			Object.Destroy(value.gameObject);
		}
		Instance.runningScripts.Clear();
	}
}
