using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/AIs/EnemyWithDoorAI")]
public class EnemyWithDoorAI : EnemyDefaultAI
{
	public readonly struct EnemyWithDoorContext
	{
		public EnemyDecisionContext Base { get; }

		public EnemyWithDoorContext(EnemyDecisionContext baseContext)
		{
			Base = baseContext;
		}
	}

	[SerializeField]
	private List<string> objGroupsToAttack = new List<string>();

	protected override EnemyDecisionContext BuildContext(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		Func<IEnumerable<ICombatEntity>> targets2 = () => targets().Where(delegate(ICombatEntity t)
		{
			Wgo tWgo2 = t as Wgo;
			return (object)tWgo2 != null && objGroupsToAttack.Any((string g) => tWgo2.Data.Definition.wgoGroup.Contains(g));
		});
		ICombatEntity nearestTargetOnLine = FindFrontmostTargetOnLine(agent, agent.ParentController.FightingLine, requireDirectVisibility: false, delegate(ICombatEntity t)
		{
			Wgo tWgo = t as Wgo;
			return (object)tWgo != null && objGroupsToAttack.Any((string g) => tWgo.Data.Definition.wgoGroup.Contains(g));
		});
		return new EnemyDecisionContext(this, agent, WrapPotentialTargets(agent, targets2), aggroDistance, nearestTargetOnLine);
	}
}
