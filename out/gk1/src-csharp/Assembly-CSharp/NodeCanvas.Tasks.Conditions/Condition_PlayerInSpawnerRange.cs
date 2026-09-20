using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("Player")]
[Name("Player In Spawner Range", 0)]
public class Condition_PlayerInSpawnerRange : WGOBehaviourCondition
{
	public BBParameter<float> range = new BBParameter<float>(1f);

	protected override string info => "Player In Spawner Range " + range;

	protected override bool OnCheck()
	{
		if (base.self_ch == null)
		{
			Debug.LogError("Character of " + base.self_wgo.name + " is null!");
			return false;
		}
		if (base.self_ch.spawner == null)
		{
			Debug.LogError("Spawner of " + base.self_wgo.name + " is null!");
			return false;
		}
		if (base.player_wgo.is_dead)
		{
			return false;
		}
		return base.player_wgo.IsInRange(base.self_ch.spawner.gameObject, range.value);
	}
}
