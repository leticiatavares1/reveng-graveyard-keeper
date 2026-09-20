using System;
using UnityEngine;

public class BaseCharacterIdle : WorldObjectPartComponent
{
	public enum IdleState
	{
		None,
		Waiting,
		Moving
	}

	[Serializable]
	public class SerializableCharacterIdle
	{
		public Vector2 spawner_coords;

		public Vector2 start_pos;

		public bool has_spawner;

		public IdleState state;

		public float delay;
	}

	public float radius = 3f;

	public float wait_time = 3f;

	[Range(0f, 1f)]
	public float wait_time_range = 0.2f;

	[Range(0f, 1f)]
	public float radius_range = 0.8f;

	protected Vector2 start_pos;

	protected BaseCharacterComponent ch;

	protected float delay;

	[NonSerialized]
	protected IdleState _state;

	public IdleState state => _state;

	public virtual void StartIdle()
	{
		base.enabled = true;
		if (_state == IdleState.None)
		{
			ch = base.wgo.components.character;
			ch.idle_used = true;
			MobSpawner spawner = base.wgo.components.character.spawner;
			start_pos = ((spawner == null) ? base.wgo.pos : ((Vector2)spawner.transform.position));
			Vector2 vector = start_pos;
			Debug.Log("StartIdle, start_pos = " + vector.ToString() + ", spawner is null: " + (spawner == null));
			Wait();
		}
	}

	public virtual void UpdateIdle(float delta_time)
	{
		if (NeedStateChange(delta_time))
		{
			IdleState idleState = _state;
			if (idleState != IdleState.Waiting)
			{
				_ = 2;
			}
			else
			{
				ChangeState();
			}
		}
	}

	protected bool NeedStateChange(float delta_time)
	{
		if (_state == IdleState.None)
		{
			return false;
		}
		delay -= delta_time;
		if (delay > 0f)
		{
			return false;
		}
		delay = 0f;
		return true;
	}

	public virtual void StopIdle()
	{
		if (_state == IdleState.None)
		{
			return;
		}
		switch (_state)
		{
		case IdleState.Waiting:
			delay = 0f;
			break;
		case IdleState.Moving:
			if (base.wgo.components.character.movement_state != MovementComponent.MovementState.AnimCurve)
			{
				base.wgo.components.character.StopMovement();
			}
			break;
		}
		_state = IdleState.None;
	}

	protected virtual void ChangeState()
	{
		switch (_state)
		{
		case IdleState.Waiting:
			MoveToRandomPos();
			break;
		case IdleState.Moving:
			Wait();
			break;
		}
	}

	protected virtual void MoveToRandomPos()
	{
	}

	protected virtual void Wait(bool stop = true)
	{
		if (stop)
		{
			ch.StopMovement();
		}
		_state = IdleState.Waiting;
		delay = wait_time * (1f + UnityEngine.Random.Range(0f - wait_time_range, wait_time_range));
	}

	protected Vector2 GetNextDest()
	{
		bool flag = true;
		int num = 5;
		Vector2 vector = start_pos;
		while (flag)
		{
			float f = UnityEngine.Random.Range(-(float)Math.PI, (float)Math.PI);
			vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			vector *= (radius_range + (1f - radius_range) * UnityEngine.Random.value) * radius * 96f;
			vector += start_pos;
			flag = (bool)Physics2D.OverlapPoint(vector, 1) && num > 0;
			if (flag && num == 0)
			{
				flag = false;
				vector = start_pos;
			}
		}
		if ((vector - start_pos).magnitude.EqualsTo(0f))
		{
			Debug.LogError("No free point for idle movement", base.wgo.gameObject);
			return Vector2.zero;
		}
		return vector;
	}

	public override void UpdateComponent(float delta_time)
	{
		UpdateIdle(delta_time);
	}

	public SerializableCharacterIdle Serialize()
	{
		MobSpawner spawner = base.wgo.components.character.spawner;
		return new SerializableCharacterIdle
		{
			has_spawner = (spawner != null),
			start_pos = start_pos,
			state = _state,
			delay = delay,
			spawner_coords = ((spawner == null) ? Vector2.zero : ((Vector2)spawner.transform.position))
		};
	}

	public void Deserialize(SerializableCharacterIdle data)
	{
		start_pos = data.start_pos;
		_state = data.state;
		delay = data.delay;
		if (data.has_spawner)
		{
			base.wgo.components.character.spawner = WorldMap.GetSpawnerByCoords(data.spawner_coords);
		}
	}
}
