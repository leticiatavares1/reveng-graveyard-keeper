using System;
using UnityEngine;

[Serializable]
public class SetActiveAction : ConditionalDrawerActionBase
{
	[Tooltip("The target GameObject to activate/deactivate")]
	public GameObject target;

	[Tooltip("If true, the logic is inverted (active when condition is false)")]
	public bool invertLogic;

	[Tooltip("Default active state when reset")]
	public bool defaultActiveState = true;

	public override void Execute(ConditionalDrawerContext context, bool conditionMet)
	{
		if (!(target == null))
		{
			bool active = (invertLogic ? (!conditionMet) : conditionMet);
			target.SetActive(active);
		}
	}

	public override void Reset(ConditionalDrawerContext context)
	{
		if (target != null)
		{
			target.SetActive(defaultActiveState);
		}
	}
}
