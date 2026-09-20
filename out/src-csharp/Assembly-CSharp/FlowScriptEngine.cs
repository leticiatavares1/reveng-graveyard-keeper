using FlowCanvas;
using UnityEngine;

public class FlowScriptEngine : MonoBehaviour
{
	private static FlowScriptEngine _me;

	private FlowScriptController[] _scripts;

	public void Awake()
	{
		_me = this;
		_scripts = GetComponentsInChildren<FlowScriptController>(includeInactive: true);
	}

	private static bool Assert()
	{
		if (_me == null)
		{
			Debug.LogError("FlowScriptEngine has not been initialized");
			return true;
		}
		return false;
	}

	public static void SendEvent(string event_name)
	{
		if (!Assert())
		{
			Debug.Log("FlowScriptEngine.SendEvent: " + event_name);
			FlowScriptController[] scripts = _me._scripts;
			for (int i = 0; i < scripts.Length; i++)
			{
				scripts[i].SendEvent(event_name);
			}
		}
	}

	public static void StopAllBehaviours()
	{
		if (!Assert())
		{
			FlowScriptController[] scripts = _me._scripts;
			for (int i = 0; i < scripts.Length; i++)
			{
				scripts[i].StopBehaviour();
			}
		}
	}

	public static void StartAllBehaviours()
	{
		if (!Assert())
		{
			FlowScriptController[] scripts = _me._scripts;
			for (int i = 0; i < scripts.Length; i++)
			{
				scripts[i].StartBehaviour();
			}
		}
	}
}
