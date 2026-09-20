using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[EventReceiver(new string[] { "OnAnimatorIK" })]
[Name("Set IK", 0)]
[Category("Animator")]
public class MecanimSetIK : ActionTask<Animator>
{
	public AvatarIKGoal IKGoal;

	[RequiredField]
	public BBParameter<GameObject> goal;

	public BBParameter<float> weight;

	protected override string info => "Set '" + IKGoal.ToString() + "' " + goal;

	public void OnAnimatorIK()
	{
		base.agent.SetIKPositionWeight(IKGoal, weight.value);
		base.agent.SetIKPosition(IKGoal, goal.value.transform.position);
		EndAction();
	}
}
