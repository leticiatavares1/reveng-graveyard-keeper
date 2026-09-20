using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
	private class FighterDefProvider
	{
		[CanBeNull]
		private FighterDef fighterDef;

		private AttackComponent attackComponent;

		public int Damage
		{
			get
			{
				if (fighterDef == null)
				{
					return attackComponent.weapon?.ItemDef.damage.EvaluateInt(attackComponent.combatEntity) ?? 0;
				}
				return fighterDef.atkDamage.EvaluateInt(attackComponent.combatEntity);
			}
		}

		public int Armor
		{
			get
			{
				if (fighterDef == null)
				{
					return 0;
				}
				return fighterDef.armor.EvaluateInt(attackComponent.combatEntity);
			}
		}

		public bool HasKnockback => fighterDef?.HasKnockback ?? attackComponent.weapon?.ItemDef.knockbackForce.More(0f) ?? false;

		public float KnockbackForce
		{
			get
			{
				if (fighterDef == null)
				{
					return attackComponent.weapon?.ItemDef.knockbackForce ?? 0f;
				}
				return fighterDef.knockbackForce.EvaluateFloat(attackComponent.combatEntity);
			}
		}

		public AttackContext GetAttackContext(Vector3 direction)
		{
			ICombatEntity combatEntity = attackComponent.combatEntity;
			LazyConsts.Fighting.TeamType teamType = attackComponent.teamType;
			FighterDef obj = fighterDef;
			ItemDef itemDef = attackComponent.weapon.ItemDef;
			Vector3 position = attackComponent.weapon.transform.position;
			AttackComponent attackersAttackComponent = attackComponent;
			return new AttackContext(combatEntity, teamType, obj, itemDef, position, direction, default(Vector3), -1, isReturnDamage: false, attackersAttackComponent);
		}

		public FighterDefProvider(FighterDef fighterDef, AttackComponent attackComponent)
		{
			this.fighterDef = fighterDef;
			this.attackComponent = attackComponent;
		}
	}

	public Action OnWeaponChanged;

	private Action OnAnimationFinished;

	public ICombatEntity combatEntity;

	private FighterDefProvider fighterDefProvider;

	public Weapon weapon;

	public AnimationComponent animationComponent;

	[SerializeField]
	private List<Weapon> availableWeapons = new List<Weapon>();

	public LazyConsts.Fighting.TeamType teamType;

	private bool useAnimation = true;

	public bool IsRangedWeapon => weapon is BowWeapon;

	public bool HasEquippedWeapon => weapon;

	public int Armor => fighterDefProvider.Armor;

	public bool IsInitialized { get; private set; }

	public void Init(ICombatEntity combatEntity, FighterDef fighterDef = null, List<Weapon> availableWeapons = null)
	{
		if (combatEntity == null)
		{
			Debug.LogError("[AttackComponent]: Attempting to initialize AttackComponent with null combatEntity", this);
			return;
		}
		this.combatEntity = combatEntity;
		if (!IsInitialized)
		{
			IsInitialized = true;
			fighterDefProvider = new FighterDefProvider(fighterDef, this);
			if (availableWeapons != null)
			{
				this.availableWeapons = availableWeapons;
			}
		}
	}

	public void EquipWeapon(ItemDef itemDef)
	{
		Weapon weapon = availableWeapons.Find((Weapon w) => w.ItemType == itemDef.type);
		if (!weapon)
		{
			Debug.LogError($"No suitable weapon for {itemDef} was found");
			return;
		}
		EquipWeapon(weapon);
		this.weapon.ItemDef = itemDef;
	}

	public void EquipWeapon(ItemType itemType)
	{
		Weapon weapon = availableWeapons.Find((Weapon w) => w.ItemType == itemType);
		if (!weapon)
		{
			Debug.LogError($"No suitable weapon for {itemType} was found");
		}
		else
		{
			EquipWeapon(weapon);
		}
	}

	public void EquipWeapon(Weapon weapon)
	{
		if ((bool)this.weapon)
		{
			UnequipWeapon();
		}
		if ((bool)weapon)
		{
			this.weapon = weapon;
			weapon.gameObject.SetActive(value: true);
			OnWeaponChanged?.Invoke();
			SubscribeToEvents();
		}
	}

	public void UnequipWeapon()
	{
		if ((bool)weapon)
		{
			weapon.gameObject.SetActive(value: false);
			UnsubscribeFromEvents();
			weapon = null;
			OnWeaponChanged?.Invoke();
		}
	}

	public void PerformAttack(bool useCustomDirection = false, Vector3 customDirection = default(Vector3), bool useAnimationFromWeapon = true, Action onAnimationFinished = null, bool activateWeapon = true)
	{
		PerformAttack(useCustomDirection, customDirection, useAnimationFromWeapon, onAnimationFinished, null, activateWeapon);
	}

	public void PerformAttackByTrigger(bool useCustomDirection = false, Vector3 customDirection = default(Vector3), string customTrigger = null, Action onAnimationFinished = null, bool activateWeapon = true)
	{
		PerformAttack(useCustomDirection, customDirection, useAnimationFromWeapon: false, onAnimationFinished, customTrigger, activateWeapon);
	}

	public void PerformAttackMelee(Action onAnimationFinished = null)
	{
		PerformAttackMeleeCustom(onAnimationFinished);
	}

	public void CancelAttack()
	{
		OnAnimationFinished = null;
		OnAttackAnimFinished(animationComponent);
	}

	public void ActivateWeapon(Vector3 direction)
	{
		if (fighterDefProvider != null)
		{
			AttackContext ctx = fighterDefProvider.GetAttackContext(direction);
			weapon.Dealers.ForEach(delegate(IDamageDealer dealer)
			{
				dealer.Activate(ctx);
			});
		}
	}

	public void OnAttackAnimFinished(AnimationComponentBase animationComponent)
	{
		if (this.animationComponent == animationComponent)
		{
			if (useAnimation)
			{
				animationComponent.SetState(AnimationState.Idle);
			}
			OnAnimationFinished?.Invoke();
		}
		if (animationComponent.Animator.layerCount > 6)
		{
			animationComponent.SetLayerWeight(6, 0f);
		}
	}

	public void DoHit(ICombatEntity target, AttackContext ctx)
	{
		HandleHit(target, ctx);
	}

	private void PerformAttack(bool useCustomDirection = false, Vector3 customDirection = default(Vector3), bool useAnimationFromWeapon = true, Action onAnimationFinished = null, string customTrigger = null, bool activateWeapon = true)
	{
		if ((bool)weapon)
		{
			if (onAnimationFinished != null)
			{
				OnAnimationFinished = onAnimationFinished;
			}
			useAnimation = useAnimationFromWeapon;
			if (useAnimationFromWeapon)
			{
				animationComponent?.SetState(weapon.animState);
			}
			else if (customTrigger != null)
			{
				animationComponent?.SetTrigger(customTrigger);
			}
			animationComponent?.SetLayerWeight(6, 1f);
			if (activateWeapon)
			{
				ActivateWeapon(useCustomDirection ? customDirection : (animationComponent?.GetDirection().XZ() ?? Vector3.zero));
			}
		}
	}

	private void PerformAttackMeleeCustom(Action onAnimationFinished = null)
	{
		if (onAnimationFinished != null)
		{
			OnAnimationFinished = onAnimationFinished;
		}
		animationComponent?.SetState(AnimationState.AttackMelee);
	}

	private void OnEnable()
	{
		if ((bool)weapon)
		{
			EquipWeapon(weapon);
		}
	}

	private void OnDestroy()
	{
		OnWeaponChanged = null;
		UnsubscribeFromEvents();
	}

	private void HandleHit(ICombatEntity target, AttackContext attackContext)
	{
		int damage = attackContext.Damage;
		damage = Mathf.Clamp(damage - target.ArmorValue, 0, int.MaxValue);
		if (target.IsActiveCombatant)
		{
			target.CombatEntityHpComponent.ApplyDamage(damage);
			if (target is IKnockbackable knockbackable && fighterDefProvider != null)
			{
				knockbackable.ApplyKnockback(attackContext, fighterDefProvider.HasKnockback ? fighterDefProvider.KnockbackForce : 0f);
			}
		}
		if (damage != 0)
		{
			target.OnOtherCombatTargetHitMe(attackContext);
		}
		if (attackContext.Damage <= 0)
		{
			return;
		}
		if (attackContext.weaponDef != null)
		{
			string text = "";
			switch (attackContext.weaponDef.type)
			{
			case ItemType.Sword:
				text = "sword_hit";
				break;
			case ItemType.Pike:
				text = "spear_hit_zombie";
				break;
			case ItemType.Bow:
				text = "bow_hit_zombie";
				break;
			case ItemType.Spit:
				text = "spitter_hit";
				break;
			case ItemType.None:
				text = "zombie_hit_player";
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (attackContext.weaponDef.type == ItemType.Spit)
				{
					LazyAudio.PlayAtPos(text, attackContext.hitPosition);
				}
				else
				{
					LazyAudio.PlayAtGameObject(text, base.transform, SpatialType.sound3D);
				}
			}
		}
		DamageEffectComponent.TryPlayEffect(target, attackContext);
	}

	private void HandleMiss()
	{
	}

	private void SubscribeToEvents()
	{
		foreach (IDamageDealer dealer in weapon.Dealers)
		{
			dealer.OnHit += HandleHit;
			dealer.OnMiss += HandleMiss;
		}
	}

	private void UnsubscribeFromEvents()
	{
		if (!weapon)
		{
			return;
		}
		foreach (IDamageDealer dealer in weapon.Dealers)
		{
			dealer.OnHit -= HandleHit;
			dealer.OnMiss -= HandleMiss;
		}
	}
}
