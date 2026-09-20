using System;
using UnityEngine;

public class MobCommandFlagCapture : MobCommand
{
	private readonly FightingCapturePoint capturePoint;

	private Func<bool> customStopCondition;

	private bool hasReservedSlot;

	public FightingCapturePoint CapturePoint => capturePoint;

	public MobCommandFlagCapture(FightingCapturePoint capturePoint)
		: base(CommandType.FlagCapture)
	{
		this.capturePoint = capturePoint;
	}

	public override bool IsTheSameCommand(MobCommand other)
	{
		if (other is MobCommandFlagCapture mobCommandFlagCapture)
		{
			return mobCommandFlagCapture.capturePoint == capturePoint;
		}
		return false;
	}

	public override void OnStart()
	{
		agent.RichAI.SetPath(null);
		agent.RVO_Locked = false;
		agent.RVO_Enabled = true;
		agent.RvoStopAt(base.Wgo.Data.Position);
		agent.IsAnchoredAtDockPoint = false;
		agent.SetNavmeshCutActive(active: false);
		hasReservedSlot = capturePoint != null && capturePoint.TryReserveSlot(base.Wgo.Data.UniqueId, out var _, out var _);
		FaceCapturePoint();
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.FlagCapture);
	}

	public override void OnUpdate(float deltaTime)
	{
		if (capturePoint == null)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		}
		else if (!IsOnCapturePoint(base.Wgo.Data.Position, capturePoint))
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		}
		else if (customStopCondition != null && customStopCondition())
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		}
		else
		{
			FaceCapturePoint();
		}
	}

	public override void OnFinish()
	{
		if (hasReservedSlot && capturePoint != null)
		{
			capturePoint.ReleaseSlot(base.Wgo.Data.UniqueId);
		}
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
	}

	public MobCommandFlagCapture WithCustomStopCondition(Func<bool> condition)
	{
		customStopCondition = condition;
		return this;
	}

	private void FaceCapturePoint()
	{
		if (!(capturePoint == null))
		{
			Vector2 direction = (capturePoint.transform.position - base.Wgo.Data.Position).XZ2();
			if (direction.sqrMagnitude > 0.0001f)
			{
				SetFacingDirection(direction);
			}
		}
	}

	private static bool IsOnCapturePoint(Vector3 pos, FightingCapturePoint point)
	{
		if (point == null)
		{
			return false;
		}
		return (pos - point.transform.position).XZ().magnitude < point.Radius;
	}
}
