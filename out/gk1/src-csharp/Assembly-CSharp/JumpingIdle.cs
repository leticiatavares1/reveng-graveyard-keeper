using UnityEngine;

public class JumpingIdle : BaseCharacterIdle
{
	[Range(0f, 1f)]
	[Space]
	public float at_place_probability = 0.2f;

	public float jump_dist = 0.5f;

	public override void StartIdle()
	{
		if (_state == IdleState.None)
		{
			ch = base.wgo.components.character;
			start_pos = base.wgo.pos;
			Wait(ch.anim_state != CharAnimState.Jump && ch.movement_state != MovementComponent.MovementState.AnimCurve);
		}
	}

	protected override void MoveToRandomPos()
	{
		if (Random.value < at_place_probability)
		{
			ProcessDir(Vector2.zero);
			return;
		}
		Vector2 nextDest = GetNextDest();
		if (nextDest.magnitude > 0f)
		{
			ProcessDir(base.wgo.pos.DirTo(nextDest).normalized);
		}
		else
		{
			Wait();
		}
	}

	protected override void Wait(bool stop = true)
	{
		base.Wait(stop);
		ch.SetAnimationState(CharAnimState.Idle);
	}

	private void ProcessDir(Vector2 dir)
	{
		ch.SetAnimationState(CharAnimState.Jump);
		ch.CurveMove(dir, base.wgo.wop.GetCurve(CharAnimState.Jump), jump_dist, ChangeState);
		_state = IdleState.Moving;
	}
}
