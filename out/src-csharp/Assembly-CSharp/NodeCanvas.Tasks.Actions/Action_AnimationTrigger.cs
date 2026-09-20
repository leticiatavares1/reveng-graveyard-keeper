using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Name("Animation Trigger", 0)]
[Category("Animation")]
public class Action_AnimationTrigger : WGOBehaviourAction
{
	public BBParameter<string> trigger_name = new BBParameter<string>("");

	public BBParameter<bool> return_true = new BBParameter<bool>(value: true);

	protected override string info => "Animation " + trigger_name?.ToString() + " trigger";

	protected override void OnExecute()
	{
		base.self_wgo.components.animator.SetTrigger(trigger_name.value);
		EndAction(return_true.value);
	}
}
