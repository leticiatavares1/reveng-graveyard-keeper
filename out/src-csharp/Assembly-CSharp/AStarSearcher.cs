using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class AStarSearcher
{
	private Seeker _seeker;

	private Path _path;

	private MovementComponent _movement_comp;

	private bool _finding;

	private Vector2 _dest;

	private GJCommons.VoidDelegate _on_completed;

	private GJCommons.VoidDelegate _on_failed;

	public Seeker seeker => _seeker;

	public bool finding => _finding;

	public Vector2 destination => _dest;

	public List<Vector3> path
	{
		get
		{
			if (_path != null)
			{
				return _path.vectorPath;
			}
			return null;
		}
	}

	public bool not_avaible
	{
		get
		{
			if (_path != null)
			{
				return _seeker == null;
			}
			return true;
		}
	}

	public AStarSearcher(MovementComponent movement_component)
	{
		_movement_comp = movement_component;
		_seeker = _movement_comp.wgo.gameObject.GetComponent<Seeker>();
		if (_seeker == null)
		{
			Debug.Log("Seeker not found, adding", _movement_comp.wgo);
			_seeker = _movement_comp.wgo.gameObject.AddComponent<Seeker>();
			AddDefaultSeekerModifier(_movement_comp.wgo.gameObject);
		}
	}

	public void EnablePathSmoother(bool enabled)
	{
		SimpleSmoothModifierXY componentInChildren = _movement_comp.wgo.GetComponentInChildren<SimpleSmoothModifierXY>();
		if (componentInChildren == null)
		{
			Debug.LogError("No path smoother found", _movement_comp.wgo);
		}
		else
		{
			componentInChildren.enabled = enabled;
		}
	}

	public void Find(Vector2 dest, GJCommons.VoidDelegate on_completed, GJCommons.VoidDelegate on_failed, int graph_mask = 1)
	{
		_finding = true;
		_dest = dest;
		_on_completed = on_completed;
		_on_failed = on_failed;
		Vector2 pos = _movement_comp.wgo.pos;
		seeker.StartPath(pos, _dest, OnPathComplete, graph_mask);
	}

	public static void AddDefaultSeekerModifier(GameObject go, bool is_player = false)
	{
		SimpleSmoothModifierXY simpleSmoothModifierXY = go.AddComponent<SimpleSmoothModifierXY>();
		simpleSmoothModifierXY.subdivisions = 2;
		simpleSmoothModifierXY.iterations = 12;
		simpleSmoothModifierXY.strength = 5f;
		simpleSmoothModifierXY.uniformLength = false;
	}

	public void SetDest(Vector2 dest)
	{
		_dest = dest;
		_on_completed = null;
		_on_failed = null;
	}

	public void Find(Vector2 start_pos, Vector2 dest, GJCommons.VoidDelegate on_completed, GJCommons.VoidDelegate on_failed, int graph_mask = 1)
	{
		_finding = true;
		_dest = dest;
		_on_completed = on_completed;
		_on_failed = on_failed;
		seeker.StartPath(start_pos, _dest, OnPathComplete, graph_mask);
	}

	public void Clear()
	{
		_finding = false;
		_path = null;
		_dest = Vector2.zero;
		_on_completed = (_on_failed = null);
	}

	private void OnPathComplete(Path p)
	{
		if (!_finding)
		{
			return;
		}
		if (p.error)
		{
			Debug.Log("#path# Path finding failed", _movement_comp.wgo);
			OnPathFailed(p.errorLog);
			return;
		}
		float magnitude = (p.path.LastElement().position.ToVector2() - _dest).magnitude;
		if (_movement_comp.wgo.is_player && magnitude > 17f)
		{
			OnPathFailed("end point is too far, mag = " + magnitude);
			return;
		}
		_finding = false;
		_path = p;
		_on_completed.TryInvoke();
	}

	private void OnPathFailed(string error_message)
	{
		Debug.LogWarning(error_message, _movement_comp.wgo.gameObject);
		_on_failed.TryInvoke();
		Clear();
	}

	public void RestoreSerialized(Vector2 dest)
	{
		_dest = dest;
	}
}
