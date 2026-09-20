using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Name("Has An Anchor", 0)]
[Category("Mob")]
public class Condition_HasAnchor : WGOBehaviourCondition
{
	protected override string info => "Has an anchor";

	protected override bool OnCheck()
	{
		if (base.self_wgo == null)
		{
			Debug.LogError("self_wgo is null");
			return false;
		}
		if (base.self_wgo.components.character.anchor_obj == null)
		{
			return false;
		}
		return true;
	}
}
