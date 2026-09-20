using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Do Zero HP", 0)]
public class Action_DoZeroHP : WGOBehaviourAction
{
	protected override void OnExecute()
	{
		base.self_wgo.DoPreZeroHPActivity();
		EndAction(success: true);
	}
}
