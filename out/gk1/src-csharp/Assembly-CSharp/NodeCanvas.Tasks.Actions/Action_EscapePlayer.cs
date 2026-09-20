using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Name("Escape Player", 0)]
[Category("Player")]
public class Action_EscapePlayer : WGOBehaviourAction
{
	public BBParameter<float> speed = new BBParameter<float>(0f);

	public BBParameter<float> range = new BBParameter<float>(1f);

	protected override void OnUpdate()
	{
		if (base.self_wgo.IsInRange(base.player_wgo, range.value))
		{
			base.self_ch.GoToVector(-base.self_wgo.DirTo(base.player_wgo), 1f);
			base.self_ch.SetSpeed(speed.value);
		}
		else
		{
			base.self_ch.StopMovement();
		}
		EndAction(success: true);
	}
}
