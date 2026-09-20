using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Idle", 0)]
public class Action_Idle : WGOBehaviourAction
{
	protected override void OnExecute()
	{
		if (base.self_ch.movement_state == MovementComponent.MovementState.Following)
		{
			base.self_ch.StopTargetFollowing();
		}
		try
		{
			base.self_wgo.components.character.idle.StartIdle();
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception at Action_Idle, WGO \"" + base.self_wgo.name + "\": " + ex, base.self_wgo);
			Debug.LogError(ex.StackTrace);
		}
	}

	protected override void OnStop()
	{
		if (!(base.self_wgo == null) && !string.IsNullOrEmpty(base.self_wgo.obj_id) && !(base.self_wgo.obj_id == "0"))
		{
			base.self_wgo.components.character.idle.StopIdle();
		}
	}
}
