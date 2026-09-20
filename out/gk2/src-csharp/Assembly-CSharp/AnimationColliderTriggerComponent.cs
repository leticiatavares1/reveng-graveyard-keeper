using System.Collections.Generic;
using UnityEngine;

public class AnimationColliderTriggerComponent : ColliderTriggerComponentBase
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private AnimationColliderTriggerData onEnter;

	[SerializeField]
	private AnimationColliderTriggerData onExit;

	public static List<string> triggerNamesArray;

	private List<string> stateNames = new List<string>();

	private List<string> triggerNames = new List<string>();

	private List<string> paramNames = new List<string>();

	private void Awake()
	{
		onEnter.Animator = animator;
		onExit.Animator = animator;
		Init(onEnter, onExit);
	}

	private void OnCustomInspectorGUI()
	{
		if (animator == null)
		{
			Debug.LogError("Animator not set!", this);
			return;
		}
		EditorAnimatorHelper.ScanAnimator(animator.gameObject, ref stateNames, ref triggerNames, ref paramNames);
		triggerNamesArray = triggerNames;
	}
}
