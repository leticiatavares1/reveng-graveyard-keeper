using UnityEngine;

public abstract class GoToDestinationModifier
{
	public virtual bool IsValid => true;

	public virtual float CustomDestinationOffset => -1f;

	protected FightingAgent Agent { get; private set; }

	protected Wgo Wgo => Agent.Wgo;

	public virtual bool StopWhenCrowded => false;

	public virtual bool IsStucked => false;

	public virtual bool ShouldAnchorOnArrival => false;

	public virtual void Init(FightingAgent agent)
	{
		Agent = agent;
	}

	public virtual void OnStart()
	{
	}

	public virtual void OnUpdate(float deltaTime)
	{
	}

	public abstract bool TryGetTargetPositionForPathfinding(out ICombatEntity combatEntity, out Vector3 position);

	public abstract Vector3 GetCurrentTargetPosition();

	public virtual void OnReachedDestination()
	{
	}

	public virtual void OnFinish()
	{
	}

	public virtual GoToDestinationModifierDebugInfo GetDebugInfo()
	{
		GoToDestinationModifierDebugInfo result = default(GoToDestinationModifierDebugInfo);
		result.ModifierType = GetType().Name;
		result.IsValid = IsValid;
		result.IsStucked = IsStucked;
		result.ShouldAnchorOnArrival = ShouldAnchorOnArrival;
		result.CurrentTargetPosition = ((Agent != null) ? GetCurrentTargetPosition() : default(Vector3));
		result.TargetWgo = GoToDestinationModifierDebugInfo.ResolveTargetWgoView(Agent?.MobCommand?.TargetEntity);
		return result;
	}
}
