using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/AIs/Barricade")]
public class BarricadeAI : AgentAI
{
	public override MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		return null;
	}
}
