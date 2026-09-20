using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Events;

public class BowAttackEmulationComponent : MonoBehaviour
{
	[SerializeField]
	private AttackComponent attackComponent;

	[SerializeField]
	private AnimationComponent animationComponent;

	[SerializeField]
	private float testLoopDelay = 1f;

	[SerializeField]
	private float turnToTargetDelay = 0.12f;

	[SerializeField]
	private List<WgoData> possibleTargets = new List<WgoData>();

	private bool isAttacking;

	private Coroutine loopCoroutine;

	private Coroutine attackCoroutine;

	private UnityAction emitArrowAction;

	public bool IsAttacking => isAttacking;

	public IReadOnlyList<WgoData> PossibleTargets => possibleTargets;

	public void Init(AttackComponent attack, AnimationComponent anim)
	{
		attackComponent = attack;
		animationComponent = anim;
		Wgo componentInParent = base.gameObject.GetComponentInParent<Wgo>();
		if (componentInParent == null)
		{
			Debug.LogError("Wgo not found");
			return;
		}
		FighterDef data = GameBalance.Me.GetData<FighterDef>(componentInParent.Id);
		if (data == null)
		{
			Debug.LogError("FighterDef not found");
		}
		else
		{
			attackComponent.Init(attackComponent.combatEntity, data);
		}
	}

	public void SetTargets(List<WgoData> targets)
	{
		possibleTargets = targets ?? new List<WgoData>();
	}

	public void PerformAttack()
	{
		PerformAttack(null);
	}

	public void PerformAttack(float delay, Action onFinished = null)
	{
		PerformAttack(delegate
		{
			if (delay > 0f)
			{
				StartCoroutine(DelayRoutine(delay, onFinished));
			}
			else
			{
				onFinished?.Invoke();
			}
		});
	}

	public void PerformAttack(Action onFinished = null)
	{
		if (isAttacking)
		{
			return;
		}
		if ((object)attackComponent == null)
		{
			attackComponent = GetComponent<AttackComponent>();
		}
		if ((object)animationComponent == null)
		{
			animationComponent = GetComponentInChildren<AnimationComponent>();
		}
		WgoData randomTarget = GetRandomTarget();
		if (randomTarget == null || attackComponent == null)
		{
			onFinished?.Invoke();
			return;
		}
		BowWeapon bowWeapon = attackComponent.weapon as BowWeapon;
		if (bowWeapon == null)
		{
			onFinished?.Invoke();
			return;
		}
		isAttacking = true;
		Vector3 vector = (GetTargetPosition(randomTarget) - bowWeapon.transform.position).XZ().normalized;
		if (vector.sqrMagnitude < 0.0001f)
		{
			vector = ((animationComponent != null) ? animationComponent.GetDirection().XZ().normalized : Vector3.forward);
		}
		if (animationComponent == null || animationComponent.AnimationEventReceiver == null)
		{
			EmitArrow(vector);
			isAttacking = false;
			onFinished?.Invoke();
		}
		else
		{
			animationComponent.SetLayerWeight(AnimationComponent.Layers.BowAttack, 1f);
			animationComponent.SetLayerWeight(AnimationComponent.Layers.ArmorWithBow, 1f);
			animationComponent.SetDirection(vector.XZ2());
			animationComponent.Animator?.Update(0f);
			attackCoroutine = StartCoroutine(TurnToTargetAndShootRoutine(vector, onFinished));
		}
	}

	public void StartLoop()
	{
		AttackComponent component = GetComponent<AttackComponent>();
		AnimationComponent componentInChildren = GetComponentInChildren<AnimationComponent>();
		if (component == null || componentInChildren == null)
		{
			Debug.LogError("AttackComponent or AnimationComponent not found");
			return;
		}
		Init(component, componentInChildren);
		StartLoop(testLoopDelay);
	}

	public void StartLoop(float delay)
	{
		StopLoop();
		loopCoroutine = StartCoroutine(AttackLoopRoutine(delay));
	}

	public void StopLoop()
	{
		if (loopCoroutine != null)
		{
			StopCoroutine(loopCoroutine);
			loopCoroutine = null;
		}
		if (attackCoroutine != null)
		{
			StopCoroutine(attackCoroutine);
			attackCoroutine = null;
		}
		ClearEmitArrowListener();
		isAttacking = false;
		if (animationComponent != null)
		{
			animationComponent.SetLayerWeight(AnimationComponent.Layers.BowAttack, 0f);
		}
	}

	private IEnumerator TurnToTargetAndShootRoutine(Vector3 direction, Action onFinished)
	{
		if (turnToTargetDelay > 0f)
		{
			yield return new WaitForSeconds(turnToTargetDelay);
		}
		if (animationComponent == null || attackComponent == null)
		{
			isAttacking = false;
			onFinished?.Invoke();
			yield break;
		}
		if (animationComponent.AnimationEventReceiver.onEvent4 == null)
		{
			animationComponent.AnimationEventReceiver.onEvent4 = new UnityEvent();
		}
		ClearEmitArrowListener();
		emitArrowAction = delegate
		{
			ClearEmitArrowListener();
			EmitArrow(direction);
		};
		animationComponent.AnimationEventReceiver.onEvent4.AddListener(emitArrowAction);
		attackComponent.PerformAttack(useCustomDirection: false, default(Vector3), useAnimationFromWeapon: true, delegate
		{
			ClearEmitArrowListener();
			isAttacking = false;
			attackCoroutine = null;
			if (animationComponent != null)
			{
				animationComponent.SetLayerWeight(AnimationComponent.Layers.BowAttack, 0f);
			}
			onFinished?.Invoke();
		}, activateWeapon: false);
	}

	private IEnumerator AttackLoopRoutine(float delay)
	{
		while (true)
		{
			bool done = false;
			PerformAttack(delegate
			{
				done = true;
			});
			float timeout = Time.time + 5f;
			while (!done && Time.time < timeout)
			{
				yield return null;
			}
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}
		}
	}

	private IEnumerator DelayRoutine(float time, Action onFinished)
	{
		yield return new WaitForSeconds(time);
		onFinished?.Invoke();
	}

	private WgoData GetRandomTarget()
	{
		if (possibleTargets == null || possibleTargets.Count == 0)
		{
			return null;
		}
		List<WgoData> list = new List<WgoData>(possibleTargets.Count);
		for (int i = 0; i < possibleTargets.Count; i++)
		{
			WgoData wgoData = possibleTargets[i];
			if (wgoData != null)
			{
				list.Add(wgoData);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private Vector3 GetTargetPosition(WgoData target)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(target.UniqueId);
		if (wgoViewGlobal != null)
		{
			return wgoViewGlobal.CombatEntityPosition + Vector3.up * 1.666667f / 2f;
		}
		return target.Position + Vector3.up * 1.666667f / 2f;
	}

	private void EmitArrow(Vector3 direction)
	{
		if (!(attackComponent?.weapon as BowWeapon == null) && !(direction.sqrMagnitude < 0.0001f))
		{
			LazyAudio.PlayAtGameObject("bow_aim_shot", attackComponent.transform, SpatialType.sound3D);
			attackComponent.ActivateWeapon(direction);
		}
	}

	private void ClearEmitArrowListener()
	{
		if (animationComponent?.AnimationEventReceiver?.onEvent4 != null && emitArrowAction != null)
		{
			animationComponent.AnimationEventReceiver.onEvent4.RemoveListener(emitArrowAction);
		}
		emitArrowAction = null;
	}

	private void OnDisable()
	{
		StopLoop();
	}
}
