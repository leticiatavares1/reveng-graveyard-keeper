using System.Collections.Generic;
using UnityEngine;

public class KickComponent : WorldGameObjectComponent
{
	public enum KickType
	{
		Kicker,
		Kickable
	}

	private const float VERTICAL_K = 0.8f;

	private const float K = 96f;

	private const float MIN_DELTA = 0.01f;

	private const float RADIUS = 0.14f;

	private const float KICK_DIST = 0.4f;

	private const float KICK_FACTOR = 1f;

	private const float DROP_KICK_SPEED = 1f;

	private const float DROP_KICK_FRICTION = 0.96f;

	private const int WALLS_MASK = 1;

	private KickType _type = KickType.Kickable;

	[RuntimeValue]
	public bool active = true;

	[RuntimeValue]
	[SerializeField]
	private bool _in_process;

	[RuntimeValue]
	public Vector2 delta_vec;

	private GJCommons.VoidDelegate _on_stoped;

	private bool _is_drop;

	private bool _was_in_wall_durint_kick;

	private DropResGameObject _drop_go;

	private static HashSet<KickComponent> _all_kicks = new HashSet<KickComponent>();

	public bool is_kickable => _type == KickType.Kickable;

	public bool is_kicker => _type == KickType.Kicker;

	public bool in_process => _in_process;

	public void SetDropResGameObject(DropResGameObject dgo)
	{
		_drop_go = dgo;
		ForceSetGameObjectLinks(null, dgo.gameObject);
	}

	public override void StartComponent()
	{
		_is_drop = base.wgo == null;
		SetTf(_is_drop ? _drop_go.transform : base.wgo.transform);
		_type = ((_is_drop || !base.components.character.enabled) ? KickType.Kickable : KickType.Kicker);
		if (!_is_drop)
		{
			CheckKickParams();
		}
		active = !_is_drop && !base.components.character.enabled;
	}

	private void CheckKickParams()
	{
		if (base.wgo == null)
		{
			Debug.LogError("WGO is null");
			return;
		}
		if (base.wgo.obj_def == null)
		{
			Debug.LogError("Obj def is null for WGO " + base.wgo.name, base.wgo);
			return;
		}
		if (!base.wgo.obj_def.res.Has("kick_speed"))
		{
			base.wgo.obj_def.res.Set("kick_speed", 1f);
		}
		if (!base.wgo.obj_def.res.Has("kick_friction"))
		{
			base.wgo.obj_def.res.Set("kick_friction", 0.9f);
		}
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		if (is_kickable || DelayedUpdate(delta_time))
		{
			return;
		}
		Vector3 position = base.tf.position;
		foreach (KickComponent all_kick in _all_kicks)
		{
			if (all_kick.enabled && !all_kick.is_kicker && all_kick.active)
			{
				all_kick.CheckKick(position, 0.4f, 1f);
			}
		}
	}

	public override bool HasFixedUpdate()
	{
		return true;
	}

	public override void FixedUpdateComponent(float delta_time)
	{
		if (!_in_process || base.wgo == null || base.wgo.is_dead)
		{
			return;
		}
		if (InWall())
		{
			if (!_was_in_wall_durint_kick)
			{
				InterruptKick(call_on_stoped: true);
				return;
			}
		}
		else if (_was_in_wall_durint_kick)
		{
			_was_in_wall_durint_kick = false;
		}
		delta_vec *= (_is_drop ? 0.96f : base.wgo.obj_def.kick_friction);
		if (Mathf.Abs(delta_vec.x) < 0.01f)
		{
			delta_vec.x = 0f;
		}
		if (Mathf.Abs(delta_vec.y) < 0.01f)
		{
			delta_vec.y = 0f;
		}
		if (delta_vec.magnitude.EqualsTo(0f))
		{
			_in_process = false;
			_on_stoped.TryInvoke();
			_on_stoped = null;
		}
		else
		{
			base.body.MovePosition(base.body.position + new Vector2(delta_vec.x, delta_vec.y * 0.8f) * (_is_drop ? 1f : base.wgo.obj_def.kick_speed) * delta_time * 96f);
		}
	}

	private bool InWall()
	{
		return Physics2D.OverlapCircle(base.tf.position, 0.14f, 1) != null;
	}

	public void CheckKick(Vector3 kicker_pos, float min_dist, float factor)
	{
		if (is_kicker)
		{
			return;
		}
		if (base.body == null)
		{
			Debug.LogWarning("no rigidbody", base.go);
			base.enabled = false;
			return;
		}
		Vector2 direction = (base.tf.position - kicker_pos) / 96f;
		if (!(direction.magnitude > min_dist))
		{
			Kick(direction, factor);
		}
	}

	public KickComponent KickFrom(Vector2 from_pos, float force_factor = 1f, GJCommons.VoidDelegate on_stoped = null)
	{
		return Kick(base.wgo.pos - from_pos, force_factor, on_stoped);
	}

	public KickComponent Kick(Vector2 direction, float force_factor = 1f, GJCommons.VoidDelegate on_stoped = null)
	{
		delta_vec = direction.normalized * force_factor;
		_on_stoped = on_stoped;
		_in_process = true;
		_was_in_wall_durint_kick = InWall();
		FixedUpdateComponent(Time.fixedDeltaTime);
		return this;
	}

	public KickComponent SetSpeed(float value)
	{
		if (value > 0f)
		{
			base.wgo.obj_def.res.Set("kick_speed", value);
		}
		return this;
	}

	public KickComponent SetFriction(float value)
	{
		if (value > 0f)
		{
			base.wgo.obj_def.res.Set("kick_friction", value);
		}
		return this;
	}

	public void InterruptKick(bool call_on_stoped = false)
	{
		_in_process = false;
		delta_vec = Vector2.zero;
		if (call_on_stoped)
		{
			_on_stoped.TryInvoke();
		}
		_on_stoped = null;
	}

	public static void ResetAtGameStart()
	{
		_all_kicks.Clear();
	}

	public override void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = obj_type == ObjectDefinition.ObjType.Mob || obj_type == ObjectDefinition.ObjType.NPC || (base.wgo != null && base.wgo.is_player) || base.go.GetComponent<DropResGameObject>() != null;
		OnEnableStateChanged();
	}

	public void OnEnableStateChanged()
	{
		if (base.enabled)
		{
			if (!_all_kicks.Contains(this))
			{
				_all_kicks.Add(this);
			}
		}
		else if (_all_kicks.Contains(this))
		{
			_all_kicks.Remove(this);
		}
	}
}
