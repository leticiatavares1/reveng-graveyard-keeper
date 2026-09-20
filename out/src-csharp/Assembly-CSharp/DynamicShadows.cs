using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

[ExecuteInEditMode]
public class DynamicShadows : MonoBehaviour
{
	public static DynamicShadows me;

	[NonSerialized]
	private static bool _me_is_set;

	[NonSerialized]
	public List<DynamicLight> lights = new List<DynamicLight>();

	[HideInInspector]
	public bool editor_fake_shadows_on;

	public void Start()
	{
		me = this;
		_me_is_set = true;
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			lights = UnityEngine.Object.FindObjectsOfType<DynamicLight>().ToList();
			me = this;
		}
	}

	public static GameObject GetNearestLight(out bool found, Vector2 pos, int n, List<GameObject> lights_list)
	{
		found = false;
		if (!_me_is_set)
		{
			return null;
		}
		if (n >= lights_list.Count)
		{
			return null;
		}
		if (lights_list.Count == 1)
		{
			found = true;
			return lights_list[0];
		}
		lights_list.Sort((GameObject v1, GameObject v2) => ((Vector2)v1.transform.position - pos).sqrMagnitude.CompareTo(((Vector2)v2.transform.position - pos).sqrMagnitude));
		found = true;
		return lights_list[n];
	}

	public void UpdateEditorFakeShadowsMode()
	{
		World world = UnityEngine.Object.FindObjectOfType<World>();
		if (world == null)
		{
			return;
		}
		ObjectDynamicShadow[] componentsInChildren = world.GetComponentsInChildren<ObjectDynamicShadow>(includeInactive: true);
		foreach (ObjectDynamicShadow objectDynamicShadow in componentsInChildren)
		{
			objectDynamicShadow.InstantiateAdditionalShadows(editor_fake_shadows_on ? 1 : 0);
			if (!editor_fake_shadows_on)
			{
				objectDynamicShadow.shadow_n = 0;
			}
		}
	}
}
