using System;
using System.Collections.Generic;
using UnityEngine;

public class EasySpriteCollectionManager : MonoBehaviour
{
	private struct AtlasRequest
	{
		public ResourceRequest req;

		public string name;
	}

	private static bool _inited;

	private static EasySpriteCollectionManager _me;

	private Dictionary<AtlasRequest, Action<UnityEngine.Object>> _reqs = new Dictionary<AtlasRequest, Action<UnityEngine.Object>>();

	private float _time;

	private Action _on_all_loaded;

	private static EasySpriteCollectionManager me
	{
		get
		{
			if (!_inited)
			{
				_me = new GameObject("ESC Manager", typeof(EasySpriteCollectionManager)).GetComponent<EasySpriteCollectionManager>();
				_inited = true;
			}
			return _me;
		}
	}

	public static void StartTrackingResourceRequest(string name, ResourceRequest rq, Action<UnityEngine.Object> on_done)
	{
		me._reqs.Add(new AtlasRequest
		{
			req = rq,
			name = name
		}, on_done);
	}

	public void Update()
	{
		_time += Time.deltaTime;
		if (_on_all_loaded != null && _reqs.Count == 0)
		{
			Action on_all_loaded = _on_all_loaded;
			_on_all_loaded = null;
			on_all_loaded();
		}
		if (_time < 0.1f)
		{
			return;
		}
		_time = 0f;
		List<AtlasRequest> list = new List<AtlasRequest>();
		foreach (KeyValuePair<AtlasRequest, Action<UnityEngine.Object>> req2 in _reqs)
		{
			ResourceRequest req = req2.Key.req;
			if (req.isDone)
			{
				if (req.asset == null)
				{
					Debug.LogError("Error loading sprite atlas: " + req2.Key.name);
				}
				list.Add(req2.Key);
			}
		}
		foreach (AtlasRequest item in list)
		{
			Action<UnityEngine.Object> action = _reqs[item];
			_reqs.Remove(item);
			action?.Invoke(item.req.asset);
			if (list.Count > 1)
			{
				_time = 1f;
				break;
			}
		}
	}

	public static void EnsureAllAtlasesLoaded(Action on_loaded)
	{
		if (me._reqs.Count == 0)
		{
			on_loaded();
		}
		else
		{
			me._on_all_loaded = on_loaded;
		}
	}
}
