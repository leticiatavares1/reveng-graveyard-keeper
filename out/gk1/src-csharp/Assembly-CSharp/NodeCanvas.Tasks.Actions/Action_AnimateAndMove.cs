using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Name("Animate and move", 0)]
[Category("Player")]
public class Action_AnimateAndMove : WGOBehaviourAction
{
	public enum Direction
	{
		None,
		ToPlayer,
		FromPlayer,
		Random,
		ToAnchor
	}

	public BBParameter<CharAnimState> animation = new BBParameter<CharAnimState>(CharAnimState.Idle);

	public BBParameter<Direction> direction = new BBParameter<Direction>(Direction.None);

	public BBParameter<float> dist = new BBParameter<float>(1f);

	public BBParameter<float> min_dist = new BBParameter<float>(0f);

	public BBParameter<float> time = new BBParameter<float>(0f);

	public BBParameter<bool> use_astar = new BBParameter<bool>(value: true);

	public BBParameter<bool> interruptable = new BBParameter<bool>(value: true);

	public BBParameter<bool> look_at_player = new BBParameter<bool>(value: false);

	private bool finding_path;

	protected override string info
	{
		get
		{
			string text = "Animate " + animation?.ToString() + " and\nmove " + direction;
			if (animation.value == CharAnimState.Attack)
			{
				text += "\n(For Attack use \"Perform attack\" block)";
			}
			return text;
		}
	}

	protected override void OnExecute()
	{
		base.self_ch.ChangeDamageFlagIgnoring(!interruptable.value);
		if (base.self_ch.anim_state == animation.value && base.self_ch.movement_state == MovementComponent.MovementState.AnimCurve)
		{
			EndAction(success: true);
			return;
		}
		if (look_at_player.value)
		{
			base.self_ch.LookAt(MainGame.me.player);
		}
		base.OnExecute();
		switch (direction.value)
		{
		case Direction.None:
			ProcessDir(Vector2.zero);
			break;
		case Direction.Random:
			ProcessDir(Random.insideUnitCircle);
			break;
		case Direction.FromPlayer:
			ProcessDir(base.player_wgo.tf.DirTo(base.self_wgo.tf));
			break;
		case Direction.ToPlayer:
		{
			Vector2 dir = base.self_wgo.tf.DirTo(base.player_wgo.tf);
			float current_dist = dir.magnitude;
			if (current_dist < min_dist.value)
			{
				EndAction(success: true);
				break;
			}
			bool flag = false;
			if (use_astar.value)
			{
				Physics2D.LinecastAll(base.self_wgo.pos, base.player_wgo.pos, 1);
				flag = false;
			}
			if (dist.value + min_dist.value > current_dist)
			{
				if (use_astar.value && flag)
				{
					JumpToPlayerByAstar(delegate
					{
						ProcessDir(dir, current_dist - min_dist.value);
					});
				}
				else
				{
					ProcessDir(dir, current_dist - min_dist.value);
				}
			}
			else if (use_astar.value && flag)
			{
				JumpToPlayerByAstar(delegate
				{
					ProcessDir(dir);
				});
			}
			else
			{
				ProcessDir(dir);
			}
			break;
		}
		case Direction.ToAnchor:
			if (base.self_wgo != null && base.self_wgo.components != null && base.self_wgo.components.character.enabled && base.self_wgo.components.character.anchor_obj != null)
			{
				ProcessDir(base.self_wgo.tf.DirTo(base.self_wgo.components.character.anchor_obj.transform));
			}
			break;
		}
	}

	private void JumpToPlayerByAstar(GJCommons.VoidDelegate on_failed)
	{
		finding_path = true;
		base.self_ch.astar.Find(base.player_wgo.pos, delegate
		{
			if (finding_path)
			{
				ProcessAstar(on_failed);
			}
		}, delegate
		{
			if (finding_path)
			{
				on_failed.TryInvoke();
			}
		});
	}

	private void ProcessAstar(GJCommons.VoidDelegate on_failed)
	{
		Vector3 vector = (base.self_ch.cur_astar_path[1] - (Vector3)base.self_wgo.pos) / 96f;
		ProcessDir(vector);
	}

	private void ProcessDir(Vector2 dir, float dist = 0f)
	{
		if (dist.EqualsTo(0f))
		{
			dist = this.dist.value;
		}
		base.self_ch.SetAnimationState(animation.value);
		if (time.value > 0f)
		{
			base.self_wgo.components.timer.Play(time.value);
		}
		base.self_ch.CurveMove(dir, base.self_wgo.GetWOP().GetCurve(animation.value), dist, delegate
		{
			if (base.isRunning)
			{
				if (base.self_ch.anim_state == animation.value)
				{
					base.self_ch.SetAnimationState(CharAnimState.Idle);
				}
				EndAction(success: true);
			}
		}, time.value.EqualsTo(0f), "", force_state_change: true, is_dir_change: true);
	}

	protected override void OnStop()
	{
		base.OnStop();
		if (base.self_ch.anim_state == animation.value)
		{
			base.self_ch.SetAnimationState(CharAnimState.Idle);
		}
		base.self_ch.ChangeDamageFlagIgnoring(ignore: false);
		finding_path = false;
	}
}
