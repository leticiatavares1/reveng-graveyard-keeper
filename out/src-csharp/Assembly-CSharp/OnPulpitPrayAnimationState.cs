using UnityEngine;

public class OnPulpitPrayAnimationState : StateMachineBehaviour
{
	public enum Type
	{
		EndOfAnimation,
		MiddleOfAnimation
	}

	public Type type;

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		ChurchPulpit component = animator.gameObject.GetComponent<ChurchPulpit>();
		if (component == null)
		{
			Debug.LogError("Not found ChurchPulpit for " + animator.gameObject.name, animator);
			return;
		}
		switch (type)
		{
		case Type.EndOfAnimation:
			component.OnPrayAnimationDone();
			break;
		case Type.MiddleOfAnimation:
			component.OnMiddleAnimation();
			break;
		}
	}
}
