using System;
using System.Collections;
using UnityEngine;

public class SwordAttackEmulationComponent : MonoBehaviour
{
	private const string ATTACK_TRIGGER = "attack";

	[SerializeField]
	private AttackComponent attackComponent;

	[SerializeField]
	private AnimationComponent animationComponent;

	[SerializeField]
	private float testLoopDelay = 0.5f;

	private bool isAttacking;

	private Coroutine loopCoroutine;

	public bool IsAttacking => isAttacking;

	public void Init(AttackComponent attack, AnimationComponent anim)
	{
		attackComponent = attack;
		animationComponent = anim;
	}

	public void PerformAttack()
	{
		PerformAttack(null);
	}

	public void PerformAttack(Action onFinished = null)
	{
		if (isAttacking)
		{
			return;
		}
		isAttacking = true;
		if (animationComponent != null)
		{
			animationComponent.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 1f);
			animationComponent.SetLayerWeight(AnimationComponent.Layers.ArmorWithSword, 1f);
			if (animationComponent.Animator != null)
			{
				animationComponent.Animator.Update(0f);
			}
			animationComponent.SetTrigger("attack");
			StartCoroutine(WaitForAnimationRoutine(onFinished));
		}
		else
		{
			isAttacking = false;
			onFinished?.Invoke();
		}
	}

	private IEnumerator WaitForAnimationRoutine(Action onFinished)
	{
		Animator animator = animationComponent.Animator;
		int layerIndex = 11;
		float timeout3 = Time.time + 0.5f;
		while (!animator.IsInTransition(layerIndex) && Time.time < timeout3)
		{
			yield return null;
		}
		timeout3 = Time.time + 1f;
		while (animator.IsInTransition(layerIndex) && Time.time < timeout3)
		{
			yield return null;
		}
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
		timeout3 = Time.time + 5f;
		while (stateInfo.normalizedTime < 0.95f && Time.time < timeout3)
		{
			stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
			yield return null;
		}
		isAttacking = false;
		if (animationComponent != null)
		{
			animationComponent.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 0f);
		}
		onFinished?.Invoke();
	}

	public void StartLoop()
	{
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
		isAttacking = false;
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
			while (!done)
			{
				yield return null;
			}
			Debug.Log("AttackLoopRoutine: done");
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}
		}
	}

	private void OnDisable()
	{
		StopLoop();
	}
}
