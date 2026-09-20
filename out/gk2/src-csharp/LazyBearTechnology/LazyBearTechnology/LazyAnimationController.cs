using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[RequireComponent(typeof(Animator))]
public class LazyAnimationController : MonoBehaviour
{
	[SerializeField]
	private float minIdleTime;

	[SerializeField]
	private List<string> idleAnimStateNames;

	[SerializeField]
	private List<AnimationEventData> animationEvents;

	[SerializeField]
	private bool resetTriggersBeforeFireNewTrigger;

	[SerializeField]
	private List<string> onStartTriggerList;

	private Animator animator;

	private bool idleState;

	private float minIdleTimeCounter;

	private List<AnimationEventData> possibleEventsToFire = new List<AnimationEventData>();

	private Action OnTriggerToIdleReturned;

	private Action<string> onEnterClip;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		foreach (AnimationEventData animationEvent in animationEvents)
		{
			animationEvent.PlanStartPlayingTime();
		}
		TryStartEventFromStartTriggerList();
	}

	private void TryStartEventFromStartTriggerList()
	{
		if (onStartTriggerList.Count != 0)
		{
			string random = onStartTriggerList.GetRandom();
			ForceStartEvent(random);
		}
	}

	private void Update()
	{
		CheckIsIdleState();
		if (idleState)
		{
			minIdleTimeCounter += Time.deltaTime;
			UpdateEventTimers();
			CheckAnimationEventsToFire();
		}
	}

	private void CheckIsIdleState()
	{
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		bool flag = false;
		for (int i = 0; i < idleAnimStateNames.Count; i++)
		{
			if (currentAnimatorStateInfo.IsName(idleAnimStateNames[i]))
			{
				flag = true;
				break;
			}
		}
		if (flag && !idleState)
		{
			minIdleTimeCounter = 0f;
			OnTriggerToIdleReturned?.Invoke();
			OnTriggerToIdleReturned = null;
			idleState = true;
		}
		else if (!flag && idleState)
		{
			idleState = false;
		}
	}

	private void UpdateEventTimers()
	{
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < animationEvents.Count; i++)
		{
			animationEvents[i].nextTimeToPlay -= deltaTime;
		}
	}

	private void CheckAnimationEventsToFire()
	{
		if (!idleState || minIdleTimeCounter < minIdleTime)
		{
			return;
		}
		possibleEventsToFire.Clear();
		for (int i = 0; i < animationEvents.Count; i++)
		{
			if (animationEvents[i].nextTimeToPlay <= 0f)
			{
				possibleEventsToFire.Add(animationEvents[i]);
			}
		}
		if (possibleEventsToFire.Count <= 0)
		{
			return;
		}
		if (possibleEventsToFire.Count == 1)
		{
			FireEvent(possibleEventsToFire[0]);
			return;
		}
		AnimationEventData animationEventData = possibleEventsToFire[0];
		float planningTime = animationEventData.planningTime;
		for (int j = 1; j < possibleEventsToFire.Count; j++)
		{
			if (possibleEventsToFire[j].planningTime < planningTime)
			{
				animationEventData = possibleEventsToFire[j];
				planningTime = animationEventData.planningTime;
			}
		}
		FireEvent(animationEventData);
	}

	private void FireEvent(AnimationEventData animationEvent)
	{
		animationEvent.PlanStartPlayingTime();
		if (resetTriggersBeforeFireNewTrigger)
		{
			for (int i = 0; i < animationEvent.triggers.Count; i++)
			{
				animator.ResetTrigger(animationEvent.triggers[i].triggerName);
			}
		}
		animator.SetTrigger(animationEvent.PickTrigger());
		idleState = false;
		minIdleTimeCounter = 0f;
	}

	public void ForceStartEvent(string eventId)
	{
		AnimationEventData animationEventData = animationEvents.Find((AnimationEventData x) => x.id == eventId);
		if (animationEventData == null)
		{
			Debug.LogError("AnimationData for event id " + eventId + " not defined.");
		}
		else
		{
			FireEvent(animationEventData);
		}
	}

	public void SetTrigger(string trigger, Action callback, Action<string> onEnterClip = null)
	{
		animator.SetTrigger(trigger);
		OnTriggerToIdleReturned = callback;
		this.onEnterClip = onEnterClip;
		idleState = false;
		minIdleTimeCounter = 0f;
	}

	public void InvokeClipEnter(string clipName)
	{
		onEnterClip?.Invoke(clipName);
	}
}
