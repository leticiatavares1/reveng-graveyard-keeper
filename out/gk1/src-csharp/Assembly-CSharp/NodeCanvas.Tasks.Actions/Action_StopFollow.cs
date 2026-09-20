using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Stop Follow", 0)]
public class Action_StopFollow : WGOBehaviourAction
{
	protected override void OnExecute()
	{
		base.self_ch.StopTargetFollowing();
		EndAction(success: true);
	}
}
