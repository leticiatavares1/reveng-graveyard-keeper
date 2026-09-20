using System;
using System.Collections.Generic;
using UnityEngine;

public class Event_03_GoToDir : AgentAI
{
	public Vector3 Direction { get; set; }

	public float Distance { get; set; }

	public override MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		Vector3 position = agent.Wgo.Data.Position + Direction * Distance;
		return new MobCommandGoTo(new CombatEntityDestinationModifier()).ToPosition(position);
	}
}
