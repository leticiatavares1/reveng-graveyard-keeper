using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Throw projectiles around", 0)]
public class Action_ThrowProjectilesAround : WGOBehaviourAction
{
	public enum ShotType
	{
		Around,
		Fan
	}

	public BBParameter<bool> look_at_player = new BBParameter<bool>(value: false);

	public BBParameter<bool> interruptable = new BBParameter<bool>(value: true);

	public BBParameter<int> projectiles_count = new BBParameter<int>(6);

	public BBParameter<Direction> direction = new BBParameter<Direction>(Direction.ToPlayer);

	public BBParameter<ShotType> shot_type = new BBParameter<ShotType>(ShotType.Around);

	public BBParameter<float> fan_shot_angle = new BBParameter<float>(10f);

	public BBParameter<bool> do_round = new BBParameter<bool>(value: false);

	protected override string info => "Throw " + projectiles_count?.ToString() + " projectiles around";

	protected override void OnExecute()
	{
		Debug.Log("#AI# Executing \"Throw projectiles around\" {took_at_player=" + look_at_player.value + "; interruptable=" + interruptable.value + ";projectiles_count=" + projectiles_count.value + "}\n on WGO \"" + base.self_wgo.name + "\"", base.self_wgo);
		if (projectiles_count.value < 2)
		{
			Debug.LogError("[" + base.self_wgo.name + "] projectiles_count < 2");
			EndAction(success: false);
			return;
		}
		if (look_at_player.value)
		{
			base.self_ch.LookAt(MainGame.me.player);
		}
		List<Vector2> list = new List<Vector2>();
		Vector2 pos = base.self_wgo.pos;
		switch (shot_type.value)
		{
		case ShotType.Around:
		{
			switch (direction.value)
			{
			case Direction.None:
				Debug.LogError("Can not Throw Projectiles Around: direction is None");
				break;
			case Direction.Right:
			case Direction.Up:
			case Direction.Left:
			case Direction.Down:
				list.Add(direction.value.ToVec());
				break;
			case Direction.IgnoreDirection:
				list.Add(base.self_ch.direction.normalized);
				break;
			case Direction.ToPlayer:
				list.Add(pos.DirTo(MainGame.me.player.pos).normalized);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (do_round.value)
			{
				Vector2 vector = list[0];
				Vector2 value = ((Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) ? new Vector2((!(vector.x < 0f)) ? 1 : (-1), 0f) : new Vector2(0f, (!(vector.y < 0f)) ? 1 : (-1)));
				list[0] = value;
			}
			float num2 = Mathf.Atan2(list[0].y, list[0].x);
			float num3 = (float)Math.PI * 2f / (float)projectiles_count.value;
			for (int j = 1; j < projectiles_count.value; j++)
			{
				float f3 = num2 + num3 * (float)j;
				Vector2 item2 = new Vector2(Mathf.Cos(f3), Mathf.Sin(f3));
				list.Add(item2);
			}
			break;
		}
		case ShotType.Fan:
		{
			float num = fan_shot_angle.value * ((float)Math.PI / 180f);
			Vector2 item = pos.DirTo(MainGame.me.player.pos).normalized;
			if (do_round.value)
			{
				item = ((Mathf.Abs(item.x) > Mathf.Abs(item.y)) ? new Vector2((!(item.x < 0f)) ? 1 : (-1), 0f) : new Vector2(0f, (!(item.y < 0f)) ? 1 : (-1)));
			}
			if (projectiles_count.value % 2 == 1)
			{
				list.Add(item);
			}
			else
			{
				float f = Mathf.Atan2(item.y, item.x) - num / 2f;
				list.Add(new Vector2(Mathf.Cos(f), Mathf.Sin(f)));
			}
			for (int i = 1; i < projectiles_count.value; i++)
			{
				bool flag = i % 2 == 0;
				float f2 = Mathf.Atan2(list[i - 1].y, list[i - 1].x) + num * (float)(flag ? (-i) : i);
				list.Add(new Vector2(Mathf.Cos(f2), Mathf.Sin(f2)));
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.projectile_emitter.DoShots(list, OnPerformed);
		base.self_ch.ChangeDamageFlagIgnoring(!interruptable.value);
	}

	private void OnPerformed(bool success)
	{
		if (base.isRunning)
		{
			Debug.Log("#AI# OnPerformed \"Throw projectiles around\" {took_at_player=" + look_at_player.value + "; interruptable=" + interruptable.value + ";projectiles_count=" + projectiles_count.value + "}\n on WGO \"" + base.self_wgo.name + "\"", base.self_wgo);
			EndAction(success);
		}
	}

	protected override void OnStop()
	{
		base.OnStop();
		base.self_ch.ChangeDamageFlagIgnoring(ignore: false);
	}
}
