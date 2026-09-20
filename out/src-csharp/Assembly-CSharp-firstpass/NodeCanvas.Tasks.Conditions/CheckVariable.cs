using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[Description("It's best to use the respective Condition for a type if existant since they support operations as well")]
[Category("✫ Blackboard")]
public class CheckVariable<T> : ConditionTask
{
	[BlackboardOnly]
	public BBParameter<T> valueA;

	public BBParameter<T> valueB;

	protected override string info => valueA?.ToString() + " == " + valueB;

	protected override bool OnCheck()
	{
		return object.Equals(valueA.value, valueB.value);
	}
}
