using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/AIs/DevTestPlayerAttack")]
public class DevTestPlayerAttackAI : AgentAI
{
	private const float TINY_EPSILON = 0.0001f;

	public float attackDistance = 1f;

	public int meleeDamage = 10;

	[CanBeNull]
	public override MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		ICombatEntity combatEntity = MainGame.PlayerController?.PhysicalBody;
		if (combatEntity == null)
		{
			return null;
		}
		if (combatEntity.CombatEntityHpComponent == null || combatEntity.CombatEntityHpComponent.Hp <= 0)
		{
			return null;
		}
		return DoGoAndAttackPlayer(agent, combatEntity);
	}

	private MobCommand DoGoAndAttackPlayer(FightingAgent agent, ICombatEntity player)
	{
		float num = DistToPos(agent, player.CombatEntityPosition);
		if ((num - attackDistance).More(0f, 0.0001f))
		{
			return new MobCommandGoTo(new CombatEntityDestinationModifier()).WithCustomTargetDestinationOffset(attackDistance).ToTarget(player);
		}
		if (num.More(0f, 0.0001f))
		{
			return new ZombieMeleeAttackCommand(attackDistance).WithDamage(meleeDamage).ToTarget(player);
		}
		return null;
	}

	private float DistToPos(FightingAgent agent, Vector3 pos)
	{
		return (agent.Wgo.Data.Position - pos).XZ().magnitude;
	}
}
