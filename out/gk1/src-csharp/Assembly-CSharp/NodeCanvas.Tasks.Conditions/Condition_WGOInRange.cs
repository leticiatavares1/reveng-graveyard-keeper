using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[Name("WGO In Range", 0)]
[Category("Movement")]
public class Condition_WGOInRange : WGOBehaviourCondition
{
	public BBParameter<float> range = new BBParameter<float>(1f);

	public BBParameter<WorldGameObject> target_wgo = new BBParameter<WorldGameObject>();

	protected override string info => "WGO in range " + range;

	protected override bool OnCheck()
	{
		if (target_wgo.value == null)
		{
			return false;
		}
		return base.self_wgo.IsInRange(target_wgo.value, range.value);
	}
}
