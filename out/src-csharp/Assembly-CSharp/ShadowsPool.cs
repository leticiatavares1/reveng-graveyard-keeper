using System.Collections.Generic;
using UnityEngine;

public class ShadowsPool : MonoBehaviour
{
	private static ShadowsPool _me;

	private static bool _inited;

	private Stack<ObjectDynamicShadowChild> _objs = new Stack<ObjectDynamicShadowChild>();

	private const int POOL_SIZE = 100;

	private int _total_created;

	public static void Init()
	{
		if (!_inited)
		{
			_inited = true;
			GameObject obj = new GameObject("* Shadows Pool");
			_me = obj.AddComponent<ShadowsPool>();
			Object.DontDestroyOnLoad(obj);
		}
	}

	public void Update()
	{
		if (MainGame.game_started && _inited && _objs.Count < 100 && !(_me != this))
		{
			CreateObject();
			CreateObject();
		}
	}

	private ObjectDynamicShadowChild CreateObject()
	{
		GameObject obj = new GameObject("shadow");
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		obj.SetActive(value: false);
		obj.AddComponent<SpriteRenderer>();
		ObjectDynamicShadowChild objectDynamicShadowChild = obj.AddComponent<ObjectDynamicShadowChild>();
		_objs.Push(objectDynamicShadowChild);
		_total_created++;
		return objectDynamicShadowChild;
	}

	public static ObjectDynamicShadowChild GetShadow()
	{
		if (!_inited)
		{
			Init();
		}
		if (_me._objs.Count == 0)
		{
			_me.CreateObject();
		}
		ObjectDynamicShadowChild objectDynamicShadowChild = _me._objs.Pop();
		objectDynamicShadowChild.gameObject.SetActive(value: true);
		return objectDynamicShadowChild;
	}

	public static void CreateObjects(int n)
	{
		for (int i = 0; i < n; i++)
		{
			_me.CreateObject();
		}
	}

	private void CustomInspector()
	{
		GUILayout.Label("Total shadows: " + _total_created);
		GUILayout.Label("Available shadows: " + _objs.Count);
	}
}
