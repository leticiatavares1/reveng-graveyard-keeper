using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationColliderTriggerData : ColliderTriggerDataBase
{
	[SerializeField]
	private string trigger;

	public static List<string> Triggers => AnimationColliderTriggerComponent.triggerNamesArray;

	public Animator Animator { get; set; }

	protected override bool IsSetupCompleted()
	{
		if (Animator != null)
		{
			return !string.IsNullOrEmpty(trigger);
		}
		return false;
	}

	protected override void TriggerSetAction()
	{
		Debug.Log("AnimationColliderTriggerData Trigger:[" + trigger + "] animation");
		Animator.SetTrigger(trigger);
	}

	protected override void TriggerResetAction()
	{
		Animator.ResetTrigger(trigger);
	}
}
