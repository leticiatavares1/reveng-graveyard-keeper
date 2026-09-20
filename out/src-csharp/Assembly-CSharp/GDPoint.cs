using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GDPoint : MonoBehaviour
{
	public enum IdlePointPrefix
	{
		None = -1,
		Tavern,
		Village,
		Camp,
		CampChickens
	}

	public static readonly string[] IDLE_POINT_PREFIXES = new string[4] { "tavern_idle_", "village_idle_", "camp_idle_", "camp_chicken_" };

	[SerializeField]
	public string gd_tag;

	[SerializeField]
	public List<GDPoint> next_gd_points = new List<GDPoint>();

	[SerializeField]
	private Vector3 _pos = Vector3.zero;

	private bool _node_inited;

	[NonSerialized]
	public bool is_graph_waypoint;

	public int idle_animation;

	public Direction direction;

	public string smart_anim_trigger = string.Empty;

	[NonSerialized]
	public bool? default_enable_state;

	private static AstarPath _astar_path;

	private GDPointNode _node;

	[HideInInspector]
	public Vector3 pos
	{
		get
		{
			if (_pos == Vector3.zero)
			{
				_pos = base.transform.position;
			}
			return _pos;
		}
	}

	public static AstarPath astar_path => _astar_path ?? (_astar_path = AstarPath.active);

	public GDPointNode node => _node;

	public void ResetPos()
	{
		_pos = Vector3.zero;
	}

	public bool IsTPPoint()
	{
		return pos.z > 1000f;
	}

	public static float Distance(GDPoint from, GDPoint to)
	{
		if (from == null || to == null)
		{
			return 0f;
		}
		return from.transform.position.DistTo(to.transform.position);
	}

	public float Distance(GDPoint to)
	{
		return Distance(this, to);
	}

	public void InitNode()
	{
		if (_node_inited)
		{
			return;
		}
		_node_inited = true;
		_node = new GDPointNode(astar_path, this);
		if (next_gd_points == null || next_gd_points.Count == 0)
		{
			return;
		}
		foreach (GDPoint next_gd_point in next_gd_points)
		{
			if (next_gd_point == null)
			{
				Debug.LogError("Found a null next_gd_point at object name = " + base.name, this);
				continue;
			}
			next_gd_point.InitNode();
			_node.LinkAdjacentNode(next_gd_point.node, (uint)Mathf.CeilToInt(Distance(next_gd_point)));
		}
	}

	public void DeInitNode()
	{
		_node_inited = false;
		_node = null;
	}

	public static string GetIdlePrefix(IdlePointPrefix prefix_enum)
	{
		if (prefix_enum == IdlePointPrefix.None)
		{
			return string.Empty;
		}
		return IDLE_POINT_PREFIXES[(int)prefix_enum];
	}

	public static bool TryParseIdlePointTag(string gd_tag, out IdlePointPrefix idle_prefix, out int idle_num, bool log_errors = false)
	{
		idle_prefix = IdlePointPrefix.None;
		idle_num = 0;
		if (string.IsNullOrEmpty(gd_tag))
		{
			if (log_errors)
			{
				Debug.Log("#ipm# GD_tag is null!");
			}
			return false;
		}
		for (int i = 0; i < IDLE_POINT_PREFIXES.Length; i++)
		{
			if (!gd_tag.StartsWith(IDLE_POINT_PREFIXES[i]))
			{
				continue;
			}
			string text = gd_tag.Substring(IDLE_POINT_PREFIXES[i].Length);
			idle_prefix = (IdlePointPrefix)i;
			if (!int.TryParse(text, out idle_num))
			{
				if (log_errors)
				{
					Debug.LogError("Can not parce num of GD_tag \"" + gd_tag + "\": prefis=" + idle_prefix.ToString() + "; num=" + text);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsDisabled()
	{
		GDPoint[] componentsInParent = base.gameObject.GetComponentsInParent<GDPoint>(includeInactive: true);
		foreach (GDPoint gDPoint in componentsInParent)
		{
			if (gDPoint != this && !gDPoint.gameObject.activeSelf)
			{
				return true;
			}
		}
		return false;
	}

	public static void StoreGDPointsState()
	{
		GDPoint[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<GDPoint>(includeInactive: true);
		foreach (GDPoint gDPoint in componentsInChildren)
		{
			gDPoint.default_enable_state = gDPoint.gameObject.activeSelf;
		}
	}

	public static void RestoreGDPointsState()
	{
		GDPoint[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<GDPoint>(includeInactive: true);
		foreach (GDPoint gDPoint in componentsInChildren)
		{
			if (gDPoint.default_enable_state.HasValue)
			{
				gDPoint.gameObject.SetActive(gDPoint.default_enable_state.GetValueOrDefault());
			}
		}
	}
}
