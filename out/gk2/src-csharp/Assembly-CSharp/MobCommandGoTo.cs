using System;
using UnityEngine;

public class MobCommandGoTo : MobCommand
{
	public const float DEST_ALMOST_PRECISE_OFFSET = 0.06666668f;

	private const float MOVEMENT_AGENT_RVO_PRIORITY_ADD = 0.1f;

	private const float MAX_DISTANCE_TO_TARGET_THRESHOLD = 100f;

	private GoToDestinationModifier destinationModifier;

	private Action action;

	private float destinationCompletionOffset = 0.06666668f;

	private float pathCompletionOffset = 0.3f;

	private float repathSensitivity = 0.5f;

	private float lastRepathTime;

	private float repathCooldown = 0.3f;

	private float adaptiveSensitivityMultiplier = 1f;

	private Vector3 lastRepathPosition;

	private Vector3 pathTransitionVelocity;

	private float pathTransitionTime;

	private float pathTransitionDuration = 0.25f;

	private bool isTransitioningPath;

	private Quaternion previousRotation;

	private Vector3 targetPos;

	private Vector3 prevPosition;

	private Func<bool> customStopCondition;

	private float customConditionUpdateTime = -1f;

	private float customConditionUpdateAccumulatedTime;

	private float cachedRvoControllerPriority;

	private AnimationState customAnimationState = AnimationState.Walk;

	private float distanceToTargetWhenStartToApplyPreFinishRvoPriority = 5f;

	private float maxRvoPriorityOnDestReached = 0.2f;

	private float cachedMaxSpeed = -1f;

	private const float MOVEMENT_WALK_SPEED_THRESHOLD = 0.12f;

	private const float MOVEMENT_INTENT_SPEED_THRESHOLD = 0.05f;

	private const float MOVEMENT_SPEED_SMOOTH_TIME = 0.08f;

	private const float IDLE_AFTER_STILL_TIME = 0.12f;

	private float animationStillTime;

	private float smoothedMoveSpeed;

	private float lastPathProgressRemaining = -1f;

	private float lastPathProgressTime;

	public GoToDestinationModifier DestinationModifier => destinationModifier;

	private RichAI_Custom RichAI => agent.RichAI;

	private string DestinationModifierType => destinationModifier?.GetType().Name;

	private GoToDestinationModifierDebugInfo DestinationModifierDebug => destinationModifier?.GetDebugInfo() ?? default(GoToDestinationModifierDebugInfo);

	private Vector3 PathfindingTargetPosition => targetPos;

	private float SmoothedMoveSpeed => smoothedMoveSpeed;

	public MobCommandGoTo(GoToDestinationModifier goToDestinationModifier)
		: base(CommandType.GoTo)
	{
		destinationModifier = goToDestinationModifier;
	}

	public override void Init(FightingAgent agent)
	{
		base.Init(agent);
		prevPosition = base.Wgo.Data.Position;
		lastRepathPosition = base.Wgo.Data.Position;
		lastRepathTime = Time.time;
		previousRotation = RichAI.rotation;
		destinationModifier.Init(agent);
		cachedMaxSpeed = RichAI.maxSpeed;
		isTransitioningPath = false;
		pathTransitionTime = 0f;
		pathTransitionVelocity = Vector3.zero;
		animationStillTime = 0f;
		smoothedMoveSpeed = 0f;
		lastPathProgressRemaining = -1f;
		lastPathProgressTime = Time.time;
	}

	public override bool IsTheSameCommand(MobCommand other)
	{
		if (other is MobCommandGoTo mobCommandGoTo)
		{
			Vector3 currentTargetPosition = destinationModifier.GetCurrentTargetPosition();
			Vector3 currentTargetPosition2 = mobCommandGoTo.destinationModifier.GetCurrentTargetPosition();
			return (currentTargetPosition - currentTargetPosition2).XZ().magnitude < 0.01f;
		}
		return base.IsTheSameCommand(other);
	}

	public override void OnStart()
	{
		RichAI.SetPath(null);
		agent.RVO_Locked = false;
		agent.RVO_Enabled = true;
		agent.SetNavmeshCutActive(active: false);
		agent.IsAnchoredAtDockPoint = false;
		agent.RichAI.simulateMovement = false;
		agent.TeleportToNavmesh(base.Wgo.Data.Position);
		prevPosition = RichAI.position;
		lastRepathPosition = RichAI.position;
		lastPathProgressRemaining = -1f;
		lastPathProgressTime = Time.time;
		animationStillTime = 0f;
		smoothedMoveSpeed = 0f;
		if (cachedMaxSpeed > 0f)
		{
			RichAI.maxSpeed = cachedMaxSpeed;
			try
			{
				base.Wgo.MainWgoPart.AnimationComponent.SetWalkAnimationSpeedMultiplier(1f);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				throw;
			}
		}
		StartNewPathCalculation();
		base.Wgo.MainWgoPart.AnimationComponent.SetState(AnimationState.Walk);
		cachedRvoControllerPriority = agent.RVO_Priority;
	}

	public override void OnUpdate(float deltaTime)
	{
		if (RichAI.IsMovementPaused || agent.RVO_Locked)
		{
			return;
		}
		destinationModifier.OnUpdate(deltaTime);
		if (!destinationModifier.IsValid)
		{
			OnCameToDestination();
			return;
		}
		float num = cachedRvoControllerPriority;
		Vector3 currentTargetPosition = destinationModifier.GetCurrentTargetPosition();
		if (customStopCondition != null)
		{
			if (customConditionUpdateTime > 0f)
			{
				customConditionUpdateAccumulatedTime += deltaTime;
				if (customConditionUpdateAccumulatedTime > customConditionUpdateTime)
				{
					if (customStopCondition())
					{
						OnCameToDestination();
						return;
					}
					customConditionUpdateAccumulatedTime = 0f;
				}
			}
			else if (customStopCondition())
			{
				OnCameToDestination();
				return;
			}
		}
		Vector3 position = RichAI.position;
		float magnitude = (currentTargetPosition - position).XZ().magnitude;
		if (destinationModifier.IsStucked)
		{
			OnCameToDestination();
			return;
		}
		float magnitude2 = (currentTargetPosition - targetPos).XZ().magnitude;
		float num2 = (prevPosition - position).XZ().magnitude / deltaTime;
		adaptiveSensitivityMultiplier = Mathf.Clamp(1f + num2 * 0.1f, 0.5f, 2f);
		float num3 = repathSensitivity * adaptiveSensitivityMultiplier;
		bool num4 = magnitude2.EqualsOrMore(num3);
		bool flag = false;
		if (num4 && Time.time - lastRepathTime > repathCooldown && (position - lastRepathPosition).XZ().magnitude > num3 * 0.5f)
		{
			lastRepathTime = Time.time;
			lastRepathPosition = position;
			DoRepath();
			flag = true;
		}
		if ((magnitude - destinationCompletionOffset).EqualsOrLess(0f, 0.0001f) || magnitude < destinationCompletionOffset)
		{
			OnCameToDestination();
			return;
		}
		float currentPathDistance = GetCurrentPathDistance(magnitude);
		float num5 = currentPathDistance;
		if (currentPathDistance > 100f)
		{
			num5 %= maxRvoPriorityOnDestReached;
		}
		num += Mathf.Lerp(maxRvoPriorityOnDestReached, 0f, (100f - currentPathDistance) / 100f);
		if (currentPathDistance < pathCompletionOffset)
		{
			Vector3 vector = currentTargetPosition - position;
			vector.y = 0f;
			float magnitude3 = vector.magnitude;
			float num6 = destinationCompletionOffset * 0.5f;
			bool num7 = magnitude3 < pathCompletionOffset;
			if (num7 && magnitude3 > num6)
			{
				Vector3 vector2 = position + vector.normalized * (magnitude3 - num6);
				RichAI.FinalizeMovement(vector2, RichAI.rotation);
				UpdateMovementAndAnimation(deltaTime, vector2, RichAI.rotation);
			}
			if (num7)
			{
				OnCameToDestination();
				return;
			}
		}
		num += 0.1f;
		agent.RVO_Priority = num;
		Vector3 position2 = RichAI.position;
		RichAI.MovementUpdate(deltaTime, out var nextPosition, out var nextRotation);
		nextPosition = ApplyRvoFunnelFallbackIfNeeded(position2, nextPosition, deltaTime);
		if (!flag)
		{
			flag = TryRepathIfPathProgressStalled();
		}
		if (!flag && RichAI.RequiredRepath)
		{
			lastRepathTime = Time.time;
			lastRepathPosition = position;
			DoRepath();
		}
		if (isTransitioningPath)
		{
			pathTransitionTime += deltaTime;
			float num8 = Mathf.Clamp01(pathTransitionTime / pathTransitionDuration);
			nextPosition = Vector3.Lerp(position + pathTransitionVelocity * deltaTime, nextPosition, num8);
			nextRotation = Quaternion.Slerp(previousRotation, nextRotation, num8);
			if (num8 >= 1f)
			{
				isTransitioningPath = false;
				pathTransitionTime = 0f;
				pathTransitionVelocity = Vector3.zero;
				agent.RVO_Priority = cachedRvoControllerPriority;
			}
		}
		RichAI.FinalizeMovement(nextPosition, nextRotation);
		Vector3 position3 = RichAI.position;
		UpdateMovementAndAnimation(deltaTime, position3, nextRotation);
		previousRotation = nextRotation;
	}

	private Vector3 ApplyRvoFunnelFallbackIfNeeded(Vector3 positionBeforeUpdate, Vector3 nextPosition, float deltaTime)
	{
		if (!agent.RVO_Enabled)
		{
			return nextPosition;
		}
		float magnitude = RichAI.desiredVelocityWithoutLocalAvoidance.magnitude;
		if (magnitude < 0.05f)
		{
			return nextPosition;
		}
		float magnitude2 = (nextPosition - positionBeforeUpdate).XZ().magnitude;
		float num = magnitude * deltaTime;
		if (magnitude2 >= num * 0.05f)
		{
			return nextPosition;
		}
		Vector3 vector = RichAI.desiredVelocityWithoutLocalAvoidance * deltaTime;
		return positionBeforeUpdate + vector;
	}

	private bool TryRepathIfPathProgressStalled()
	{
		if (!RichAI.hasPath || RichAI.pathPending)
		{
			return false;
		}
		if (agent.RVO_NeighbourCount == 0)
		{
			return false;
		}
		float remainingDistance = RichAI.remainingDistance;
		if (float.IsNaN(remainingDistance) || float.IsInfinity(remainingDistance) || remainingDistance < 0f)
		{
			return false;
		}
		if (RichAI.desiredVelocityWithoutLocalAvoidance.magnitude < 0.05f)
		{
			return false;
		}
		if (lastPathProgressRemaining < 0f || remainingDistance < lastPathProgressRemaining - 0.05f)
		{
			lastPathProgressRemaining = remainingDistance;
			lastPathProgressTime = Time.time;
			return false;
		}
		if (Time.time - lastPathProgressTime < RichAI.stuckThreshold)
		{
			return false;
		}
		if (Time.time - lastRepathTime < RichAI.repathCooldownDuration)
		{
			return false;
		}
		lastRepathTime = Time.time;
		lastRepathPosition = RichAI.position;
		lastPathProgressTime = Time.time;
		DoRepath();
		return true;
	}

	private void UpdateMovementAndAnimation(float deltaTime, Vector3 currentPosition, Quaternion? facingRotation)
	{
		AnimationComponentBase animationComponentBase = base.Wgo.MainWgoPart?.AnimationComponent;
		float num = 0f;
		if (facingRotation.HasValue)
		{
			float num2 = Mathf.Max(deltaTime, 0.0001f);
			num = (currentPosition - prevPosition).XZ().magnitude / num2;
			float t = 1f - Mathf.Exp((0f - num2) / 0.08f);
			smoothedMoveSpeed = Mathf.Lerp(smoothedMoveSpeed, num, t);
			prevPosition = currentPosition;
			base.Wgo.Data.Position = currentPosition;
		}
		if (animationComponentBase != null && customAnimationState != AnimationState.Walk)
		{
			animationComponentBase.SetState(customAnimationState);
			if ((animationComponentBase.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f).EqualsTo(1f, 0.05f))
			{
				customAnimationState = AnimationState.Walk;
				animationComponentBase.SetState(AnimationState.Walk);
			}
		}
		else if (facingRotation.HasValue && !(animationComponentBase == null))
		{
			bool flag = smoothedMoveSpeed >= 0.12f;
			bool flag2 = RichAI.hasPath && GetCurrentPathDistance(0f) > destinationCompletionOffset;
			bool flag3 = isTransitioningPath || RichAI.pathPending || flag2 || RichAI.desiredVelocityWithoutLocalAvoidance.magnitude >= 0.05f;
			if (flag || flag3)
			{
				animationStillTime = 0f;
			}
			else
			{
				animationStillTime += deltaTime;
			}
			if (flag3 || flag || animationStillTime < 0.12f)
			{
				animationComponentBase.SetState(AnimationState.Walk);
				Quaternion value = facingRotation.Value;
				SetFacingDirection(new Vector2(Mathf.Sin(value.eulerAngles.y * (MathF.PI / 180f)), Mathf.Cos(value.eulerAngles.y * (MathF.PI / 180f))));
			}
			else
			{
				animationComponentBase.SetState(AnimationState.Idle);
			}
		}
	}

	private float GetCurrentPathDistance(float fallbackDistance)
	{
		if (!RichAI.hasPath)
		{
			return fallbackDistance;
		}
		float remainingDistance = RichAI.remainingDistance;
		if (float.IsNaN(remainingDistance) || float.IsInfinity(remainingDistance) || remainingDistance < 0f)
		{
			return fallbackDistance;
		}
		return remainingDistance;
	}

	public override void OnFinish()
	{
		RichAI.SetPath(null);
		agent.TeleportToNavmesh(base.Wgo.Data.Position);
		agent.RvoStopAt(base.Wgo.Data.Position);
		agent.RVO_Enabled = !agent.IsAnchoredAtDockPoint;
		if (cachedMaxSpeed > 0f)
		{
			RichAI.maxSpeed = cachedMaxSpeed;
			base.Wgo.MainWgoPart?.AnimationComponent?.SetWalkAnimationSpeedMultiplier(1f);
		}
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
		agent.RVO_Priority = cachedRvoControllerPriority;
		agent.RVO_Locked = agent.IsAnchoredAtDockPoint;
		destinationModifier?.OnFinish();
		agent.RichAI.simulateMovement = true;
	}

	public override void CompensatePause(float pausedFor)
	{
		if (!(pausedFor <= 0f))
		{
			lastRepathTime += pausedFor;
			lastPathProgressTime += pausedFor;
		}
	}

	public MobCommandGoTo WithCustomTargetDestinationOffset(float offset)
	{
		destinationCompletionOffset = offset;
		return this;
	}

	public MobCommandGoTo WithCustomStopCondition(Func<bool> condition, float customUpdateTime = -1f, float initialCounter = 0f)
	{
		Func<bool> previousStopCondition = customStopCondition;
		customStopCondition = ((previousStopCondition == null) ? condition : ((Func<bool>)(() => previousStopCondition() || condition())));
		if (previousStopCondition == null)
		{
			customConditionUpdateTime = customUpdateTime;
			customConditionUpdateAccumulatedTime = Mathf.Clamp(initialCounter, 0f, customUpdateTime);
		}
		else if (customUpdateTime > 0f)
		{
			customConditionUpdateTime = ((customConditionUpdateTime > 0f) ? Mathf.Min(customConditionUpdateTime, customUpdateTime) : customUpdateTime);
		}
		return this;
	}

	public MobCommandGoTo WithCustomActionOnDestReached(Action action)
	{
		this.action = action;
		return this;
	}

	public void SetCustomAnimationState(AnimationState animationState)
	{
		if (customAnimationState != animationState)
		{
			customAnimationState = animationState;
		}
	}

	private void OnCameToDestination()
	{
		float magnitude = (destinationModifier.GetCurrentTargetPosition() - base.Wgo.Data.Position).XZ().magnitude;
		if ((magnitude - destinationCompletionOffset).EqualsTo(0f, 0.0001f) || magnitude < destinationCompletionOffset)
		{
			destinationModifier.OnReachedDestination();
			if (destinationModifier.ShouldAnchorOnArrival)
			{
				agent.IsAnchoredAtDockPoint = true;
			}
		}
		action?.Invoke();
		agent.StopCommandExecution(reportAlsoAsCompletion: true);
	}

	private void DoRepath()
	{
		if (RichAI.hasPath && RichAI.velocity.sqrMagnitude > 0.01f)
		{
			if (!isTransitioningPath)
			{
				pathTransitionVelocity = RichAI.velocity;
				isTransitioningPath = true;
				pathTransitionTime = 0f;
			}
			else
			{
				pathTransitionVelocity = RichAI.velocity;
			}
		}
		else if (!isTransitioningPath)
		{
			agent.RVO_Priority = cachedRvoControllerPriority;
		}
		StartNewPathCalculation();
	}

	private void StartNewPathCalculation()
	{
		if (destinationModifier.TryGetTargetPositionForPathfinding(out var combatEntity, out targetPos))
		{
			if (combatEntity != null)
			{
				agent.ParentController.AddPathCalculation(new PathCalculationData(base.Wgo.Data.UniqueId, RichAI, combatEntity));
			}
			else
			{
				agent.ParentController.AddPathCalculation(new PathCalculationData(base.Wgo.Data.UniqueId, RichAI, targetPos));
			}
		}
	}
}
