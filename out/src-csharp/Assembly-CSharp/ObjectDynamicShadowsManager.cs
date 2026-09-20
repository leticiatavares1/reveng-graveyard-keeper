using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class ObjectDynamicShadowsManager : MonoBehaviour
{
	private static bool _inited;

	private static ObjectDynamicShadowsManager _me;

	private Dictionary<Action, Action> _queue = new Dictionary<Action, Action>();

	private int _queue_size;

	private static ObjectDynamicShadowsManager me
	{
		get
		{
			if (!_inited)
			{
				_me = new GameObject("ObjectDynamicShadowsManager", typeof(ObjectDynamicShadowsManager)).GetComponent<ObjectDynamicShadowsManager>();
				_inited = true;
			}
			return _me;
		}
	}

	public static void QueueShadowCreation(Action action, Action on_done)
	{
		me._queue.Add(action, on_done);
	}

	public static void ForceShadowAction(Action action)
	{
		if (action != null)
		{
			if (!me._queue.ContainsKey(action))
			{
				Debug.LogWarning("Trying to force absent action");
				action();
				return;
			}
			Action action2 = me._queue[action];
			me._queue.Remove(action);
			action();
			action2();
		}
	}

	public void Update()
	{
		_queue_size = _queue.Count;
		if (_queue_size != 0)
		{
			int num = ((_queue_size > 4) ? 4 : _queue_size);
			for (int i = 0; i < num; i++)
			{
				KeyValuePair<Action, Action> keyValuePair = _queue.First();
				_queue.Remove(keyValuePair.Key);
				keyValuePair.Key();
				keyValuePair.Value();
			}
		}
	}

	public static void TerminateQueue()
	{
		me._queue.Clear();
	}
}
