using System.Collections.Generic;
using UnityEngine;

public class Object3DAnimSMB : StateMachineBehaviour
{
	private Object3D object3D;

	private MultiObject3DAnimatable multiObject3DAnimatable;

	private HashSet<Object3D> object3Ds = new HashSet<Object3D>();

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (object3D == null)
		{
			object3D = animator.GetComponent<Object3D>();
		}
		if (object3D == null)
		{
			multiObject3DAnimatable = animator.GetComponent<MultiObject3DAnimatable>();
			object3Ds = new HashSet<Object3D>(multiObject3DAnimatable.Object3Ds);
		}
		if (object3D != null)
		{
			object3D.SetAnimatableState(isAnimatable: true);
		}
		if (multiObject3DAnimatable != null)
		{
			foreach (Object3D object3D in object3Ds)
			{
				object3D.SetAnimatableState(isAnimatable: true);
			}
		}
		base.OnStateEnter(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (object3D != null)
		{
			object3D.SetAnimatableState(isAnimatable: false);
		}
		if (multiObject3DAnimatable != null)
		{
			foreach (Object3D object3D in object3Ds)
			{
				object3D.SetAnimatableState(isAnimatable: false);
			}
		}
		base.OnStateExit(animator, stateInfo, layerIndex);
	}
}
