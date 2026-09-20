using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Blackboard")]
[Name("Set Integer Random", 0)]
[Description("Set a blackboard integer variable at random between min and max value")]
public class SetIntRandom : ActionTask
{
	public BBParameter<int> minValue;

	public BBParameter<int> maxValue;

	[BlackboardOnly]
	public BBParameter<int> intVariable;

	protected override string info => "Set " + intVariable?.ToString() + " Random(" + minValue?.ToString() + ", " + maxValue?.ToString() + ")";

	protected override void OnExecute()
	{
		intVariable.value = Random.Range(minValue.value, maxValue.value + 1);
		EndAction();
	}
}
