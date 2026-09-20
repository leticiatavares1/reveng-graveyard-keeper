using System;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public abstract class MobCommand
{
	public enum CommandType
	{
		None,
		GoTo,
		ZombieMeleeAttack,
		ZombieBowAttack,
		ZombiePikeAttack,
		ZombieSpitAttack,
		ZombieJump,
		FlagCapture
	}

	public CommandType commandType;

	public Vector3 customPosition;

	protected FightingAgent agent;

	[CanBeNull]
	public ICombatEntity TargetEntity { get; set; }

	public Vector3 Position => TargetEntity?.CombatEntityPosition ?? customPosition;

	public virtual Vector2 DirectionToTarget => (Position - Wgo.Data.Position).XZ2().normalized;

	public Wgo Wgo => agent.Wgo;

	protected MobCommand(CommandType type)
	{
		commandType = type;
	}

	public virtual void Init(FightingAgent agent)
	{
		this.agent = agent;
	}

	public virtual bool IsTheSameCommand(MobCommand other)
	{
		if (TargetEntity != other.TargetEntity)
		{
			return other.TargetEntity == null;
		}
		return true;
	}

	public abstract void OnStart();

	public abstract void OnUpdate(float deltaTime);

	public abstract void OnFinish();

	public virtual void CompensatePause(float pausedFor)
	{
	}

	protected void SetFacingDirection(Vector2 direction, bool instant = false)
	{
		agent?.SetFacingDirection(direction, instant);
	}

	protected void SetDirectionToTarget()
	{
		if (TargetEntity != null)
		{
			SetFacingDirection(DirectionToTarget);
		}
	}
}
