using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class Timer
{
	public int id;

	public bool isActive;

	private bool justStarted;

	private float endTime;

	private Action onComplete;

	private Action<float> onUpdate;

	public void Init(float seconds, Action onComplete, Action<float> onUpdate = null)
	{
		Refresh();
		endTime = Time.time + seconds;
		this.onComplete = onComplete;
		this.onUpdate = onUpdate;
		id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		justStarted = true;
		isActive = true;
	}

	public void Update(float currentTime, float deltaTime)
	{
		if (justStarted)
		{
			justStarted = false;
			return;
		}
		onUpdate?.Invoke(deltaTime);
		if (isActive && currentTime >= endTime)
		{
			OnComplete();
		}
	}

	public void Stop()
	{
		isActive = false;
		onComplete = null;
		onUpdate = null;
	}

	public void OnComplete()
	{
		onComplete();
		isActive = false;
	}

	private void Refresh()
	{
		onComplete = null;
		isActive = false;
		justStarted = true;
		endTime = 0f;
	}
}
