using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SetActiveManyAction : ConditionalDrawerActionBase
{
	[Tooltip("The list of GameObjects to activate/deactivate")]
	public List<GameObject> targets;

	[Tooltip("If true, the logic is inverted (active when condition is false)")]
	public bool invertLogic;

	[Tooltip("Default active state when reset")]
	public bool defaultActiveState = true;

	public override void Execute(ConditionalDrawerContext context, bool conditionMet)
	{
		if (targets == null || targets.Count == 0)
		{
			return;
		}
		bool active = (invertLogic ? (!conditionMet) : conditionMet);
		foreach (GameObject target in targets)
		{
			target.SetActive(active);
		}
	}

	public override void Reset(ConditionalDrawerContext context)
	{
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		foreach (GameObject target in targets)
		{
			target.SetActive(defaultActiveState);
		}
	}
}
