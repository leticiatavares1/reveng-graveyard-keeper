using System;
using System.Collections;
using LazyBearTechnology;
using UnityEngine;

public class AutomaticTurret : MonoBehaviour
{
	[SerializeField]
	private BowWeapon bow;

	[Header("Settings")]
	[SerializeField]
	private TurretSettings settings;

	[Header("Debug Settings")]
	[SerializeField]
	private bool debugModeEnabled;

	private FighterDef fighterDef;

	private Wgo wgo;

	private bool isAIActive;

	private FightingAgent currentTarget;

	private Coroutine updateAICoroutine;

	private float shootRange;

	private float shotIntervalTime;

	private Collider[] overlapColliders = new Collider[60];

	private WaitForSeconds waitScanIntervalTime;

	private WaitForSeconds waitShootIntervalTime;

	private WaitForSeconds waitForSecondaryShoot;

	public bool IsAIActive => isAIActive;

	public void Activate()
	{
		if (IsAIActive)
		{
			return;
		}
		if (settings == null)
		{
			Debug.LogError("[AutomaticTurret] Turret settings not assigned on " + base.gameObject.name + "!", base.gameObject);
			return;
		}
		wgo = GetComponentInParent<Wgo>();
		if ((bool)wgo)
		{
			fighterDef = GameBalance.Me.GetData<FighterDef>(wgo.Id);
		}
		if (fighterDef == null)
		{
			Debug.LogError("[AutomaticTurret] FighterDef not found for wgo: " + wgo.Id + ".");
			return;
		}
		isAIActive = true;
		SubscribeToEvents();
		shotIntervalTime = fighterDef.atkPause.EvaluateFloat(wgo);
		waitScanIntervalTime = new WaitForSeconds(settings.scanInterval);
		waitShootIntervalTime = new WaitForSeconds(shotIntervalTime);
		if (updateAICoroutine != null)
		{
			StopCoroutine(updateAICoroutine);
		}
		updateAICoroutine = StartCoroutine(UpdateAI());
	}

	public void Deactivate()
	{
		UnsubscribeFromEvents();
		isAIActive = false;
		if (updateAICoroutine != null)
		{
			StopCoroutine(updateAICoroutine);
		}
		currentTarget = null;
	}

	private void OnDisable()
	{
		Deactivate();
	}

	private IEnumerator UpdateAI()
	{
		while (isAIActive)
		{
			currentTarget = ScanForTarget();
			if (currentTarget == null)
			{
				yield return waitScanIntervalTime;
				continue;
			}
			while (currentTarget != null)
			{
				if (!IsTargetInRange(currentTarget))
				{
					currentTarget = null;
					yield return waitScanIntervalTime;
					break;
				}
				yield return DoShootingLogic();
				yield return waitShootIntervalTime;
			}
		}
		Deactivate();
		yield return null;
	}

	private FightingAgent ScanForTarget()
	{
		shootRange = fighterDef.atkRange.EvaluateFloat(wgo);
		Array.Clear(overlapColliders, 0, overlapColliders.Length);
		int num = Physics.OverlapCapsuleNonAlloc(base.transform.position + Vector3.down * 2f, base.transform.position + Vector3.up * 2f, shootRange, overlapColliders, 536870912, QueryTriggerInteraction.Ignore);
		FightingAgent result = null;
		float num2 = float.MaxValue;
		for (int i = 0; i < num; i++)
		{
			FightingAgent componentInParent = overlapColliders[i].GetComponentInParent<FightingAgent>();
			if (!(componentInParent == null) && (!bow || AgentAI.TryLineCastByRecast(bow.transform.position, componentInParent.Wgo.CombatEntityPosition)) && componentInParent.Wgo.TeamType != 0)
			{
				float num3 = Vector3.Distance(base.transform.position, componentInParent.transform.position);
				if (num3 < num2 && num3 <= shootRange)
				{
					result = componentInParent;
					num2 = num3;
				}
			}
		}
		return result;
	}

	private IEnumerator DoShootingLogic()
	{
		foreach (float shotSampleTiming in settings.shotSampleTimings)
		{
			yield return new WaitForSeconds(shotSampleTiming);
			ShootAtTarget(ref currentTarget);
		}
	}

	private bool IsTargetInRange(FightingAgent target)
	{
		if ((GetAimPosition(target.transform.position) - base.transform.position).magnitude <= shootRange)
		{
			return true;
		}
		return false;
	}

	private void ShootAtTarget(ref FightingAgent target)
	{
		if ((bool)target && target.Wgo.Data.HpComponent.Hp > 0)
		{
			Vector3 position = bow.transform.position;
			Vector3 normalized = (GetAimPosition(target.Wgo.Data.Position) - position).normalized;
			AttackContext ctx = new AttackContext(wgo, LazyConsts.Fighting.TeamType.Player, fighterDef, bow.ItemDef, bow.transform.position, normalized);
			LazyAudio.PlayAtGameObject("bow_aim_shot", bow.transform, SpatialType.sound3D);
			bow.Activate(ctx);
		}
		else
		{
			target = null;
		}
	}

	private Vector3 GetAimPosition(Vector3 targetPosition)
	{
		return targetPosition + Vector3.up * 1.666667f / 2f;
	}

	private void Update()
	{
	}

	private void SubscribeToEvents()
	{
		bow.Dealer.OnHit += HandleHit;
		bow.Dealer.OnMiss += HandleMiss;
	}

	private void UnsubscribeFromEvents()
	{
		if ((bool)bow)
		{
			bow.Dealer.OnHit -= HandleHit;
			bow.Dealer.OnMiss -= HandleMiss;
		}
	}

	private void HandleHit(ICombatEntity combatEntity, AttackContext attackContext)
	{
		int num = fighterDef.atkDamage.EvaluateInt(wgo);
		num = ((!attackContext.hasCustomDamage) ? num : attackContext.customDamage);
		Debug.Log($"Handle hit to {combatEntity.CombatEntityUID}, damage: {num}");
		num = Mathf.Clamp(num - combatEntity.ArmorValue, 0, int.MaxValue);
		if (combatEntity.IsActiveCombatant)
		{
			combatEntity.CombatEntityHpComponent.ApplyDamage(num);
			if (combatEntity is IKnockbackable knockbackable)
			{
				knockbackable.ApplyKnockback(attackContext, fighterDef.HasKnockback ? fighterDef.knockbackForce.EvaluateFloat(wgo) : 0f);
			}
		}
		AttackContext ctx = new AttackContext(wgo, wgo.TeamType, fighterDef, bow.ItemDef, wgo.CombatEntityPosition, attackContext.direction, attackContext.hitPosition);
		if (num != 0)
		{
			combatEntity.OnOtherCombatTargetHitMe(ctx);
		}
	}

	private void HandleMiss()
	{
		Debug.Log("Miss happened");
	}
}
