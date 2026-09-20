using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Stop Any Movement", 0)]
public class Action_StopAnyMovement : WGOBehaviourAction
{
	protected override void OnExecute()
	{
		base.self_ch.StopMovement();
		EndAction(success: true);
	}
}
