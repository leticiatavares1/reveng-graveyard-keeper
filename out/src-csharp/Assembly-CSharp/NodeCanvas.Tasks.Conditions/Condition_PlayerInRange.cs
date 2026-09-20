using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[Name("Player In Range", 0)]
[Category("Player")]
public class Condition_PlayerInRange : WGOBehaviourCondition
{
	public BBParameter<float> range = new BBParameter<float>(1f);

	protected override string info => "Player in range " + range;

	protected override bool OnCheck()
	{
		if (base.self_wgo.IsInRange(base.player_wgo, range.value))
		{
			return !base.player_wgo.is_dead;
		}
		return false;
	}
}
