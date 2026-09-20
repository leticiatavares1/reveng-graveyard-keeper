using System;
using System.Collections.Generic;
using FlowCanvas;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GDZone : MonoBehaviour
{
	[Serializable]
	public class GDZoneEvent
	{
		public FlowGraph flow_script;

		public EnvironmentPreset environment_preset;

		public string ovr_music;
	}

	public enum DistanceType
	{
		None,
		Point,
		Line
	}

	private bool _entered;

	private Vector2 _enter_point;

	public GDZoneEvent on_enter;

	public GDZoneEvent on_exit;

	public GDZoneEvent on_crossed_to_right;

	public GDZoneEvent on_crossed_to_left;

	public GDZoneEvent on_crossed_to_up;

	public GDZoneEvent on_crossed_to_down;

	public const float COLLIDER_MIN_SIZE = 100f;

	public List<string> sound_loops = new List<string>();

	private bool _is_player_inside;

	public DistanceType distance_counter;

	public Vector2 dist_p1;

	public Vector2 dist_p2;

	public float dist_size = 1f;

	public float dist_fade_size = 2f;

	[SerializeField]
	private Vector2 _p1;

	[SerializeField]
	private Vector2 _p2;

	[SerializeField]
	private Vector2[] _inner_poly;

	[SerializeField]
	private Vector2 _perpendicular;

	[SerializeField]
	private Vector2 _line_direction;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision == null)
		{
			return;
		}
		WorldGameObject componentInParent = collision.gameObject.GetComponentInParent<WorldGameObject>();
		if (componentInParent == null || !componentInParent.is_player || _is_player_inside)
		{
			return;
		}
		_is_player_inside = true;
		_enter_point = componentInParent.transform.position;
		Debug.Log("GDZone.OnTriggerEnter2D " + base.name + ", obj = " + componentInParent.name, this);
		ExecuteEvent(on_enter);
		if (!string.IsNullOrEmpty(on_enter.ovr_music))
		{
			SmartAudioEngine.me.PlayOvrMusic(on_enter.ovr_music);
		}
		foreach (string sound_loop in sound_loops)
		{
			SmartAudioEngine.me.PlaySoundWithFade(sound_loop);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision == null)
		{
			return;
		}
		WorldGameObject componentInParent = collision.gameObject.GetComponentInParent<WorldGameObject>();
		if (componentInParent == null || !componentInParent.is_player || !_is_player_inside)
		{
			return;
		}
		_is_player_inside = false;
		Vector2 vector = (Vector2)componentInParent.transform.position - _enter_point;
		string[] obj = new string[6] { "GDZone.OnTriggerExit2D ", base.name, ", obj = ", componentInParent.name, ", diff_vector = ", null };
		Vector2 vector2 = vector;
		obj[5] = vector2.ToString();
		Debug.Log(string.Concat(obj), this);
		ExecuteEvent(on_exit);
		if (!string.IsNullOrEmpty(on_enter.ovr_music))
		{
			SmartAudioEngine.me.StopOvrMusic(on_enter.ovr_music);
		}
		if (vector.x * 2f > 100f)
		{
			ExecuteEvent(on_crossed_to_right);
		}
		if (vector.x * 2f < -100f)
		{
			ExecuteEvent(on_crossed_to_left);
		}
		if (vector.y * 2f > 100f)
		{
			ExecuteEvent(on_crossed_to_down);
		}
		if (vector.y * 2f < -100f)
		{
			ExecuteEvent(on_crossed_to_up);
		}
		foreach (string sound_loop in sound_loops)
		{
			SmartAudioEngine.me.StopSoundWithFade(sound_loop);
		}
	}

	private void ExecuteEvent(GDZoneEvent e)
	{
		if (e != null)
		{
			if (e.flow_script != null)
			{
				CustomFlowScript.Create(MainGame.me.world_root.gameObject, e.flow_script, is_global: true);
			}
			if (e.environment_preset != null)
			{
				EnvironmentEngine.me.ApplyEnvironmentPreset(e.environment_preset);
			}
		}
	}

	public void Update()
	{
		if (!_is_player_inside)
		{
			return;
		}
		switch (distance_counter)
		{
		case DistanceType.Line:
		{
			float volume = CalculateDistanceK(MainGame.me.player_pos);
			{
				foreach (string sound_loop in sound_loops)
				{
					SmartAudioEngine.me.SetSoundVolume(sound_loop, volume);
				}
				break;
			}
		}
		case DistanceType.Point:
			Debug.LogError("Point not supported");
			break;
		}
	}

	public void RecalculateBounds()
	{
		_p1 = base.transform.position + (Vector3)dist_p1 * 96f;
		_p2 = base.transform.position + (Vector3)dist_p2 * 96f;
		_line_direction = (_p2 - _p1).normalized;
		_perpendicular = Quaternion.AngleAxis(90f, _line_direction) * Vector3.forward;
		_inner_poly = new Vector2[4]
		{
			_p1 + _perpendicular * dist_size * 96f,
			_p2 + _perpendicular * dist_size * 96f,
			_p2 - _perpendicular * dist_size * 96f,
			_p1 - _perpendicular * dist_size * 96f
		};
	}

	private static float DistanceFromPointToLine(Vector2 point, Vector2 l1, Vector2 l2)
	{
		return Mathf.Abs((l2.x - l1.x) * (l1.y - point.y) - (l1.x - point.x) * (l2.y - l1.y)) / Mathf.Sqrt(Mathf.Pow(l2.x - l1.x, 2f) + Mathf.Pow(l2.y - l1.y, 2f));
	}

	public float CalculateDistanceK(Vector2 p)
	{
		if (ExtentionTools.PolygonContainsPoint(_inner_poly, p))
		{
			return 1f;
		}
		float a = DistanceFromPointToLine(p, _inner_poly[0], _inner_poly[1]);
		float b = DistanceFromPointToLine(p, _inner_poly[2], _inner_poly[3]);
		float num = 1f - Mathf.Min(a, b) / 96f / dist_fade_size;
		if (!(num < 0f))
		{
			return num;
		}
		return 0f;
	}

	public void OnDrawGizmosSelected()
	{
		RecalculateBounds();
		Gizmos.color = Color.red;
		Gizmos.color = Color.blue;
		if (dist_fade_size < 0f)
		{
			dist_fade_size = 0f;
		}
		switch (distance_counter)
		{
		case DistanceType.Point:
			Gizmos.DrawWireSphere(_p1, dist_size * 96f);
			break;
		case DistanceType.Line:
		{
			Gizmos.DrawSphere(_p1, 10f);
			Gizmos.DrawSphere(_p2, 10f);
			Gizmos.color = Color.white;
			Gizmos.DrawLine(_p1, _p2);
			float num = dist_size + dist_fade_size;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(_p1 + _perpendicular * num * 96f, _p2 + _perpendicular * num * 96f);
			Gizmos.DrawLine(_p1 - _perpendicular * num * 96f, _p2 - _perpendicular * num * 96f);
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(_inner_poly[0], _inner_poly[1]);
			Gizmos.DrawLine(_inner_poly[1], _inner_poly[2]);
			Gizmos.DrawLine(_inner_poly[2], _inner_poly[3]);
			Gizmos.DrawLine(_inner_poly[3], _inner_poly[0]);
			break;
		}
		}
		Gizmos.color = Color.white;
	}
}
