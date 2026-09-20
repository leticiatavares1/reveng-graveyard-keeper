using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[Category("Player")]
[Name("Was Damaged", 0)]
public class Condition_WasDamaged : WGOBehaviourCondition
{
	protected override bool OnCheck()
	{
		return base.self_ch.WasDamaged(clear_flag: false);
	}
}
