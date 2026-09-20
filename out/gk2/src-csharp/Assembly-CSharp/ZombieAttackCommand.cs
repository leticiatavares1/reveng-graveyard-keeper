using System;

public abstract class ZombieAttackCommand : MobCommand
{
	protected Func<bool> customStopCondition;

	protected Action onStartAction;

	protected Action onStopAction;

	protected bool applyPause;

	protected float pauseTime;

	protected float pauseTimeAccumulated;

	protected ZombieAttackCommand(CommandType type)
		: base(type)
	{
	}

	public override void Init(FightingAgent agent)
	{
		base.Init(agent);
		pauseTime = agent.FighterDef.atkPause.EvaluateFloat(agent.Wgo);
		if (agent.PreviousCommand == null)
		{
			applyPause = false;
		}
		else
		{
			applyPause = agent.PreviousCommand is ZombieAttackCommand;
		}
		pauseTimeAccumulated = 0f;
	}

	public override void OnStart()
	{
	}

	public override void OnUpdate(float deltaTime)
	{
		SetDirectionToTarget();
	}

	public override void OnFinish()
	{
		agent.RVO_Locked = agent.IsAnchoredAtDockPoint;
		pauseTimeAccumulated = 0f;
	}

	public ZombieAttackCommand WithCustomStopCondition(Func<bool> condition)
	{
		customStopCondition = condition;
		return this;
	}

	public ZombieAttackCommand WithCustomOnStartAction(Action action)
	{
		onStartAction = action;
		return this;
	}

	public ZombieAttackCommand WithCustomOnStopAction(Action action)
	{
		onStopAction = action;
		return this;
	}

	protected bool ShouldPause(float deltaTime)
	{
		if (!applyPause)
		{
			return false;
		}
		pauseTimeAccumulated += deltaTime;
		if (pauseTime.EqualsOrMore(0f))
		{
			return pauseTimeAccumulated < pauseTime;
		}
		return false;
	}
}
