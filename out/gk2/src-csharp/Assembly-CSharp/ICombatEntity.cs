using UnityEngine;

public interface ICombatEntity
{
	SGuid CombatEntityUID { get; }

	LazyConsts.Fighting.TeamType TeamType { get; }

	LazyConsts.Fighting.EntityType EntityType { get; }

	Vector3 CombatEntityPosition { get; }

	HPComponent CombatEntityHpComponent { get; }

	int ArmorValue { get; }

	int CombatEntityQuality { get; }

	int AttackPriority { get; set; }

	bool HasAnyDockPoint { get; }

	bool IsActiveCombatant { get; set; }

	float GetCombatEntityDistance(Vector3 from, LazyConsts.Fighting.TeamType teamType);

	float GetCombatEntityGameRes(string resId);

	void OnOtherCombatTargetReachedToMe(ICombatEntity other);

	void OnOtherCombatTargetHitMe(AttackContext ctx);
}
