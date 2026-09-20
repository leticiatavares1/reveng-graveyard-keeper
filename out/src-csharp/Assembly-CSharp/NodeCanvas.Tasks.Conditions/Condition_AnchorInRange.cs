using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Name("Anchor In Range", 0)]
[Category("Mob")]
public class Condition_AnchorInRange : WGOBehaviourCondition
{
	public BBParameter<float> range = new BBParameter<float>(1f);

	protected override string info => "Anchor in range " + range;

	protected override bool OnCheck()
	{
		if (base.self_wgo == null)
		{
			Debug.LogError("self_wgo is null");
			return false;
		}
		GameObject anchor_obj = base.self_wgo.components.character.anchor_obj;
		if (anchor_obj == null)
		{
			Debug.LogWarning("Anchor is null", base.self_wgo);
			return false;
		}
		return base.self_wgo.IsInRange(anchor_obj, range.value);
	}
}
