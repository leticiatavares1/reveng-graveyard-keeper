using System;

public interface IDamageDealer
{
	event Action<ICombatEntity, AttackContext> OnHit;

	event Action OnMiss;

	void Activate(AttackContext ctx);

	void Cancel();
}
