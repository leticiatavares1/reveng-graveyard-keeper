using System.Collections.Generic;
using LazyBearTechnology;
using Unity.Collections;
using UnityEngine;

public class ConveyorSystemAnimationOrchestrator : LazySingleton<ConveyorSystemAnimationOrchestrator>
{
	public const string IDLE_STATE_NAME = "Idle";

	public const string OUT_STATE_NAME = "Out";

	public const string IN_STATE_NAME = "In";

	[SerializeField]
	[ReadOnly]
	private List<ConveyorSystemAnimator> conveyorAnimators = new List<ConveyorSystemAnimator>();

	private readonly HashSet<ConveyorSystemAnimator> conveyorAnimatorSet = new HashSet<ConveyorSystemAnimator>();

	[SerializeField]
	private AnimationClip idleAnimationClip;

	[SerializeField]
	private AnimationClip inAnimationClip;

	[SerializeField]
	private AnimationClip outAnimationClip;

	[SerializeField]
	private string globalState = "Idle";

	[SerializeField]
	public bool unscaled;

	[SerializeField]
	public float speed = 1f;

	private float clock;

	private float prevPhaseValue;

	private float clipLength = 1f;

	private float phase;

	private bool requiresViewUpdate;

	public bool TryAddAnimator(ConveyorSystemAnimator animator)
	{
		if (!conveyorAnimatorSet.Add(animator))
		{
			return false;
		}
		conveyorAnimators.Add(animator);
		animator.OnOrchestratorRegistered(globalState, phase);
		animator.UpdateView();
		return true;
	}

	public void RemoveAnimator(ConveyorSystemAnimator animator)
	{
		if (conveyorAnimatorSet.Remove(animator))
		{
			conveyorAnimators.Remove(animator);
		}
	}

	private void Update()
	{
		if (MainGame.IsGamePaused)
		{
			return;
		}
		for (int num = conveyorAnimators.Count - 1; num >= 0; num--)
		{
			ConveyorSystemAnimator conveyorSystemAnimator = conveyorAnimators[num];
			if (!conveyorSystemAnimator)
			{
				conveyorAnimatorSet.Remove(conveyorSystemAnimator);
				conveyorAnimators.RemoveAt(num);
			}
			else if (conveyorSystemAnimator.gameObject.activeInHierarchy)
			{
				if (requiresViewUpdate)
				{
					conveyorSystemAnimator.UpdateView();
				}
				conveyorSystemAnimator.CustomUpdate();
			}
		}
		requiresViewUpdate = false;
		clock += (unscaled ? Time.unscaledDeltaTime : Time.deltaTime) * speed;
		phase = clock % clipLength / clipLength;
		if (prevPhaseValue > phase)
		{
			if (globalState == "Out")
			{
				SetState("In");
			}
			else if (globalState == "In")
			{
				SetState("Idle");
			}
		}
		prevPhaseValue = phase;
		for (int i = 0; i < conveyorAnimators.Count; i++)
		{
			ConveyorSystemAnimator conveyorSystemAnimator2 = conveyorAnimators[i];
			if ((bool)conveyorSystemAnimator2 && conveyorSystemAnimator2.gameObject.activeInHierarchy)
			{
				conveyorSystemAnimator2.Play(phase);
			}
		}
	}

	public void SetState(string name)
	{
		globalState = name;
		if (globalState == "Idle")
		{
			clipLength = idleAnimationClip.length;
		}
		else if (globalState == "In")
		{
			clipLength = inAnimationClip.length;
		}
		else if (globalState == "Out")
		{
			clipLength = outAnimationClip.length;
		}
		phase = 0f;
		clock = 0f;
		prevPhaseValue = 0f;
		requiresViewUpdate = true;
		foreach (ConveyorSystemAnimator conveyorAnimator in conveyorAnimators)
		{
			if ((bool)conveyorAnimator)
			{
				conveyorAnimator.CurrentState = name;
				conveyorAnimator.MarkPlaybackDirty();
			}
		}
	}
}
