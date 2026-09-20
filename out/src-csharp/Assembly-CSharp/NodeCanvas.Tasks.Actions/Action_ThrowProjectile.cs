using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Throw projectile", 0)]
public class Action_ThrowProjectile : WGOBehaviourAction
{
	public BBParameter<bool> look_at_player = new BBParameter<bool>(value: false);

	public BBParameter<bool> return_success_result;

	public BBParameter<bool> interruptable = new BBParameter<bool>(value: true);

	public BBParameter<bool> round_dir = new BBParameter<bool>(value: false);

	public BBParameter<Direction> direction = new BBParameter<Direction>(Direction.ToPlayer);

	protected override string info => "Throw projectile ";

	protected override void OnExecute()
	{
		if (look_at_player.value)
		{
			base.self_ch.LookAt(MainGame.me.player);
		}
		switch (direction.value)
		{
		case Direction.ToPlayer:
			if (round_dir.value)
			{
				Vector2 normalized = base.self_wgo.pos.DirTo(MainGame.me.player.pos).normalized;
				Vector2 dir = ((Mathf.Abs(normalized.x) > Mathf.Abs(normalized.y)) ? new Vector2((!(normalized.x < 0f)) ? 1 : (-1), 0f) : new Vector2(0f, (!(normalized.y < 0f)) ? 1 : (-1)));
				base.projectile_emitter.DoShot(dir, OnPerformed);
			}
			else
			{
				base.projectile_emitter.DoShot(MainGame.me.player.tf, OnPerformed);
			}
			break;
		case Direction.Right:
		case Direction.Up:
		case Direction.Left:
		case Direction.Down:
			base.projectile_emitter.DoShot(direction.value.ToVec(), OnPerformed);
			break;
		case Direction.IgnoreDirection:
			base.projectile_emitter.DoShot(base.self_ch.direction.normalized, OnPerformed);
			break;
		case Direction.None:
			Debug.LogError("Wrong Throw projectile direction on " + base.self_wgo.name + ": dir=" + direction.value);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.self_ch.ChangeDamageFlagIgnoring(!interruptable.value);
	}

	private void OnPerformed(bool success)
	{
		if (base.isRunning)
		{
			EndAction(!return_success_result.value || success);
		}
	}

	protected override void OnStop()
	{
		base.OnStop();
		base.self_ch.ChangeDamageFlagIgnoring(ignore: false);
	}
}
