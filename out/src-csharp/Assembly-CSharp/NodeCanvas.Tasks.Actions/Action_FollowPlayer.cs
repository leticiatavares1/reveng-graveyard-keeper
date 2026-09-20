using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Name("Follow Player", 0)]
[Category("Player")]
public class Action_FollowPlayer : WGOBehaviourAction
{
	public BBParameter<float> speed = new BBParameter<float>(0f);

	public BBParameter<float> min_dist = new BBParameter<float>(0.5f);

	protected override void OnExecute()
	{
		try
		{
			base.self_wgo.components.character.idle.StopIdle();
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception at Action_FollowPlayer, WGO \"" + base.self_wgo.name + "\": " + ex, base.self_wgo);
			Debug.LogError(ex.StackTrace);
		}
		base.self_ch.FollowTarget(base.player_wgo, min_dist.value, delegate
		{
			if (base.isRunning)
			{
				EndAction();
			}
		});
		base.self_ch.SetSpeed(speed.value);
	}
}
