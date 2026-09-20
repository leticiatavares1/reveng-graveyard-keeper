using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Perform Attack", 0)]
public class Action_PerformAttack : WGOBehaviourAction
{
	public BBParameter<int> type = new BBParameter<int>(0);

	public BBParameter<bool> look_at_player = new BBParameter<bool>(value: false);

	public BBParameter<bool> return_success_result;

	public BBParameter<bool> interruptable = new BBParameter<bool>(value: true);

	public BBParameter<Direction> direction = new BBParameter<Direction>(Direction.ToPlayer);

	protected override string info => "Perform attack " + type;

	protected override void OnExecute()
	{
		if (look_at_player.value)
		{
			base.self_ch.LookAt(MainGame.me.player);
		}
		switch (direction.value)
		{
		case Direction.ToPlayer:
			base.self_ch.components.character.attack.Perform(MainGame.me.player, type.value, OnPerformed);
			break;
		case Direction.Right:
		case Direction.Up:
		case Direction.Left:
		case Direction.Down:
		case Direction.IgnoreDirection:
			base.self_ch.components.character.attack.Perform(direction.value, type.value, OnPerformed);
			break;
		case Direction.None:
			Debug.LogError("Wrong perform attack direction on " + base.self_wgo.name + ": dir=" + direction.value);
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
