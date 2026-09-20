using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class ConditionalChecker
{
	public int id;

	public bool isActive;

	public bool justStarted;

	private Action onComplete;

	private Action onUpdate;

	private Func<bool> condition;

	public void Init(Func<bool> condition, Action onUpdate, Action onComplete)
	{
		Refresh();
		this.onComplete = onComplete;
		this.onUpdate = onUpdate;
		this.condition = condition;
		id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		isActive = true;
		justStarted = true;
	}

	public void Update()
	{
		if (justStarted)
		{
			justStarted = false;
			return;
		}
		onUpdate?.Invoke();
		if (condition())
		{
			OnComplete();
		}
	}

	public void Stop()
	{
		isActive = false;
	}

	public void OnComplete()
	{
		onComplete();
		isActive = false;
	}

	private void Refresh()
	{
		onComplete = null;
		onUpdate = null;
		condition = null;
		isActive = false;
		justStarted = true;
	}
}
