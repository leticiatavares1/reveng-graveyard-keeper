using UnityEngine;

public class UpdateNormalMapGraphics : StateMachineBehaviour
{
	[SerializeField]
	private bool is_update_on_enter;

	[SerializeField]
	private bool is_update_on_exit;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		UpdateNormalMaps(animator);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		UpdateNormalMaps(animator);
	}

	private void UpdateNormalMaps(Animator animator)
	{
		NormalMapSprite[] componentsInChildren = animator.gameObject.GetComponentsInChildren<NormalMapSprite>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateRenderer();
		}
	}
}
