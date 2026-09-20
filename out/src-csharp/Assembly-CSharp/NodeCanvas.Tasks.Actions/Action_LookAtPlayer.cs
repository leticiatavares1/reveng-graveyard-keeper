using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Look At Player", 0)]
public class Action_LookAtPlayer : WGOBehaviourAction
{
	protected override void OnExecute()
	{
		base.self_ch.LookAt(base.player_wgo);
		EndAction(success: true);
	}
}
