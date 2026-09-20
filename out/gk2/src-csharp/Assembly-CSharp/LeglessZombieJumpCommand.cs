using System.Collections.Generic;
using UnityEngine;

public class LeglessZombieJumpCommand : MobCommand
{
	private enum JumpPhase
	{
		Prepare,
		Jump,
		Land
	}

	private readonly Vector3 landingPosition;

	private readonly float jumpDuration;

	private readonly float jumpArcHeight;

	private readonly AnimationCurve jumpHeightCurve;

	private JumpPhase phase;

	private Vector3 jumpStartPosition;

	private float jumpTime;

	private bool jumpInterruptedByDamage;

	private int hpOnPrepareStart;

	private bool isSubscribedToHpChanged;

	private readonly List<Collider> disabledNonTriggerColliders = new List<Collider>();

	private RichAI_Custom richAICustom;

	public override Vector2 DirectionToTarget => (landingPosition - base.Wgo.Data.Position).XZ2().normalized;

	public LeglessZombieJumpCommand(Vector3 landingPosition, float jumpDuration, float jumpArcHeight, AnimationCurve jumpHeightCurve)
		: base(CommandType.ZombieJump)
	{
		this.landingPosition = landingPosition;
		this.jumpDuration = Mathf.Max(0.01f, jumpDuration);
		this.jumpArcHeight = Mathf.Max(0f, jumpArcHeight);
		this.jumpHeightCurve = jumpHeightCurve;
	}

	public override void Init(FightingAgent agent)
	{
		base.Init(agent);
		hpOnPrepareStart = agent.Wgo.Data.HpComponent.Hp;
		richAICustom = agent.RichAI;
	}

	public override void OnStart()
	{
		phase = JumpPhase.Prepare;
		agent.RVO_Locked = true;
		agent.RichAI.SetPath(null, updateDestinationFromPath: false);
		base.Wgo.MainWgoPart.AnimationComponent.SetState(AnimationState.JumpPrepare);
		SetFacingDirection(DirectionToTarget);
		SubscribeToDamage();
	}

	public override void OnUpdate(float deltaTime)
	{
		switch (phase)
		{
		case JumpPhase.Prepare:
			SetFacingDirection(DirectionToTarget);
			if (jumpInterruptedByDamage)
			{
				FinishCommand();
			}
			else if (IsCurrentAnimationFinished())
			{
				BeginJump();
			}
			break;
		case JumpPhase.Jump:
			UpdateJump(deltaTime);
			break;
		case JumpPhase.Land:
			if (IsCurrentAnimationFinished())
			{
				FinishCommand();
			}
			break;
		}
	}

	public override void OnFinish()
	{
		RestoreNonTriggerColliders();
		SetGroundSnappingEnabled(isEnabled: true);
		UnsubscribeFromDamage();
		agent.RVO_Locked = false;
		agent.TeleportToNavmesh(base.Wgo.Data.Position);
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
	}

	private void BeginJump()
	{
		phase = JumpPhase.Jump;
		jumpStartPosition = base.Wgo.Data.Position;
		jumpTime = 0f;
		DisableNonTriggerColliders();
		SetGroundSnappingEnabled(isEnabled: false);
		base.Wgo.MainWgoPart.AnimationComponent.SetState(AnimationState.Jump);
	}

	private void UpdateJump(float deltaTime)
	{
		jumpTime += deltaTime;
		float num = Mathf.Clamp01(jumpTime / jumpDuration);
		Vector3 vector = Vector3.Lerp(jumpStartPosition, landingPosition, num);
		float num2 = ((jumpHeightCurve != null) ? jumpHeightCurve.Evaluate(num) : (4f * num * (1f - num)));
		Vector3 position = vector + Vector3.up * (num2 * jumpArcHeight);
		agent.TeleportToNavmesh(position);
		SetFacingDirection(DirectionToTarget);
		if (num >= 1f)
		{
			agent.TeleportToNavmesh(landingPosition);
			SetGroundSnappingEnabled(isEnabled: true);
			RestoreNonTriggerColliders();
			phase = JumpPhase.Land;
			base.Wgo.MainWgoPart.AnimationComponent.SetState(AnimationState.JumpLand);
		}
	}

	private bool IsCurrentAnimationFinished()
	{
		Animator animator = base.Wgo.MainWgoPart.AnimationComponent.Animator;
		if (!animator)
		{
			return true;
		}
		return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f;
	}

	private void FinishCommand()
	{
		agent.StopCommandExecution(reportAlsoAsCompletion: true);
	}

	private void SubscribeToDamage()
	{
		if (!isSubscribedToHpChanged)
		{
			agent.Wgo.Data.HpComponent.OnHpChanged += HandleHpChanged;
			isSubscribedToHpChanged = true;
		}
	}

	private void UnsubscribeFromDamage()
	{
		if (isSubscribedToHpChanged)
		{
			agent.Wgo.Data.HpComponent.OnHpChanged -= HandleHpChanged;
			isSubscribedToHpChanged = false;
		}
	}

	private void HandleHpChanged(HPComponent hpComponent)
	{
		if (phase == JumpPhase.Prepare)
		{
			if (hpComponent.Hp < hpOnPrepareStart)
			{
				jumpInterruptedByDamage = true;
			}
			hpOnPrepareStart = hpComponent.Hp;
		}
	}

	private void DisableNonTriggerColliders()
	{
		disabledNonTriggerColliders.Clear();
		Collider[] componentsInChildren = base.Wgo.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (!(collider == null) && !collider.isTrigger && collider.enabled)
			{
				collider.enabled = false;
				disabledNonTriggerColliders.Add(collider);
			}
		}
	}

	private void RestoreNonTriggerColliders()
	{
		if (disabledNonTriggerColliders.Count == 0)
		{
			return;
		}
		for (int i = 0; i < disabledNonTriggerColliders.Count; i++)
		{
			Collider collider = disabledNonTriggerColliders[i];
			if (collider != null)
			{
				collider.enabled = true;
			}
		}
		disabledNonTriggerColliders.Clear();
	}

	private void SetGroundSnappingEnabled(bool isEnabled)
	{
		if ((Object)(object)richAICustom != null)
		{
			richAICustom.GroundSnapEnabled = isEnabled;
		}
	}
}
