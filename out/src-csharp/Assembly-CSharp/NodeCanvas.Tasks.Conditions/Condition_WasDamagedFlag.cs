using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[Category("Player")]
[Name("Was Damaged Flag", 0)]
public class Condition_WasDamagedFlag : WGOBehaviourCondition
{
	protected override bool OnCheck()
	{
		return base.self_ch.WasDamaged(clear_flag: true);
	}
}
