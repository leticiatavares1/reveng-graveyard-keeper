using UnityEngine;
using UnityEngine.Events;

public class AnimationEventReceiver : MonoBehaviour
{
	[SerializeField]
	private bool isPlayerRelatedAnimationEvents;

	public UnityEvent onEvent1;

	public UnityEvent onEvent2;

	public UnityEvent onEvent3;

	public UnityEvent onEvent4;

	public UnityEvent onEvent5;

	public UnityEvent onEvent6;

	public UnityEvent onEvent7;

	public UnityEvent onEvent8;

	public UnityEvent onEvent9;

	public UnityEvent onEvent10;

	[Space]
	public UnityEvent onFishingEvent1;

	public UnityEvent onFishingEvent2;

	public UnityEvent onSermonEvent1;

	public void CallEvent1()
	{
		onEvent1?.Invoke();
	}

	public void CallEvent2()
	{
		onEvent2?.Invoke();
	}

	public void CallEvent3()
	{
		onEvent3?.Invoke();
	}

	public void CallEvent4()
	{
		onEvent4?.Invoke();
	}

	public void CallEvent5()
	{
		onEvent5?.Invoke();
	}

	public void CallEvent6()
	{
		onEvent6?.Invoke();
	}

	public void CallEvent7()
	{
		onEvent7?.Invoke();
	}

	public void CallEvent8()
	{
		onEvent8?.Invoke();
	}

	public void CallEvent9()
	{
		onEvent9?.Invoke();
	}

	public void CallEvent10()
	{
		onEvent10?.Invoke();
	}

	public void CallFishingEvent1()
	{
		onFishingEvent1?.Invoke();
	}

	public void CallFishingEvent2()
	{
		onFishingEvent2?.Invoke();
	}

	public void CallSermonEvent1()
	{
		onSermonEvent1?.Invoke();
	}
}
