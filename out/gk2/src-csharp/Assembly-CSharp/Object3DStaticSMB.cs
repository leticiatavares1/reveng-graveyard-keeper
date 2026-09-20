using UnityEngine;

public class Object3DStaticSMB : StateMachineBehaviour
{
	private Object3D object3D;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (object3D == null)
		{
			object3D = animator.GetComponent<Object3D>();
		}
		if (object3D != null)
		{
			object3D.ResetToStaticState();
		}
		base.OnStateEnter(animator, stateInfo, layerIndex);
	}
}
