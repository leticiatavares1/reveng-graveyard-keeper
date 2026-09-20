using JetBrains.Annotations;
using UnityEngine;

public readonly struct AttackContext
{
	public readonly ICombatEntity attacker;

	public readonly LazyConsts.Fighting.TeamType teamType;

	[CanBeNull]
	public readonly ItemDef weaponDef;

	[CanBeNull]
	public readonly FighterDef fighterDef;

	public readonly Vector3 origin;

	public readonly Vector3 direction;

	public readonly Vector3 hitPosition;

	public readonly bool hasCustomDamage;

	public readonly int customDamage;

	public readonly bool isReturnDamage;

	[CanBeNull]
	public readonly AttackComponent attackersAttackComponent;

	[CanBeNull]
	public readonly DamageEffectSettings damageEffectOverride;

	public int Damage
	{
		get
		{
			if (hasCustomDamage)
			{
				return customDamage;
			}
			return fighterDef?.atkDamage.EvaluateInt(attacker) ?? weaponDef?.damage.EvaluateInt(attacker) ?? 0;
		}
	}

	public AttackContext(ICombatEntity attacker, LazyConsts.Fighting.TeamType teamType, FighterDef fighterDef, ItemDef weaponDef, Vector3 origin, Vector3 direction, Vector3 hitPosition = default(Vector3), int customDamage = -1, bool isReturnDamage = false, AttackComponent attackersAttackComponent = null, DamageEffectSettings damageEffectOverride = null)
	{
		this.attacker = attacker;
		this.teamType = teamType;
		this.fighterDef = fighterDef;
		this.weaponDef = weaponDef;
		this.origin = origin;
		this.direction = direction.normalized;
		this.hitPosition = hitPosition;
		hasCustomDamage = customDamage >= 0;
		this.customDamage = customDamage;
		this.isReturnDamage = isReturnDamage;
		this.attackersAttackComponent = attackersAttackComponent;
		this.damageEffectOverride = damageEffectOverride;
	}

	public AttackContext(ICombatEntity attacker, LazyConsts.Fighting.TeamType teamType, FighterDef fighterDef, ItemDef weaponDef, Vector3 origin, Vector3 direction, Vector3 hitPosition)
		: this(attacker, teamType, fighterDef, weaponDef, origin, direction, hitPosition, -1, isReturnDamage: false, null, null)
	{
	}

	public AttackContext(AttackContext context, Vector3 hitPosition)
		: this(context.attacker, context.teamType, context.fighterDef, context.weaponDef, context.origin, context.direction, hitPosition, context.hasCustomDamage ? context.customDamage : (-1), context.isReturnDamage, context.attackersAttackComponent, context.damageEffectOverride)
	{
	}

	public AttackContext(AttackContext context, int customDamage)
		: this(context.attacker, context.teamType, context.fighterDef, context.weaponDef, context.origin, context.direction, context.hitPosition, customDamage, context.isReturnDamage, context.attackersAttackComponent, context.damageEffectOverride)
	{
	}

	public static AttackContext WithHit(AttackContext context, Vector3 hitPosition, DamageEffectSettings damageEffectOverride = null)
	{
		return new AttackContext(context.attacker, context.teamType, context.fighterDef, context.weaponDef, context.origin, context.direction, hitPosition, context.hasCustomDamage ? context.customDamage : (-1), context.isReturnDamage, context.attackersAttackComponent, damageEffectOverride ?? context.damageEffectOverride);
	}

	public static AttackContext WithCustomDamage(AttackContext context, int customDamage)
	{
		return new AttackContext(context.attacker, context.teamType, context.fighterDef, context.weaponDef, context.origin, context.direction, context.hitPosition, customDamage, context.isReturnDamage, context.attackersAttackComponent, context.damageEffectOverride);
	}
}
