using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyTimer : MonoBehaviour
{
	private const int POOL_SIZE = 15;

	[SerializeField]
	private List<Timer> activeTimers = new List<Timer>();

	private List<Timer> inactiveTimers = new List<Timer>();

	private static LazyTimer instance;

	private static LazyTimer Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("LazyTimer").AddComponent<LazyTimer>();
				UnityEngine.Object.DontDestroyOnLoad(instance);
			}
			return instance;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < 15; i++)
		{
			inactiveTimers.Add(new Timer());
		}
	}

	private void Update()
	{
		float time = Time.time;
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < activeTimers.Count; i++)
		{
			Timer timer = activeTimers[i];
			if (timer.isActive)
			{
				timer.Update(time, deltaTime);
				continue;
			}
			activeTimers.Remove(timer);
			inactiveTimers.Add(timer);
			i--;
		}
	}

	public static int AddTimer(float seconds, Action onComplete, Action<float> onUpdate = null)
	{
		if (onComplete == null)
		{
			Debug.LogError("Trying to add timer without complete action");
			return -1;
		}
		Timer timer = GetTimer();
		timer.Init(seconds, onComplete, onUpdate);
		Instance.activeTimers.Add(timer);
		return timer.id;
	}

	public static bool Stop(int id)
	{
		Timer timer = Instance.activeTimers.Find((Timer x) => x.id == id);
		if (timer != null)
		{
			timer.Stop();
			return true;
		}
		return false;
	}

	public static void StopAll()
	{
		foreach (Timer activeTimer in Instance.activeTimers)
		{
			activeTimer.Stop();
		}
	}

	public static void ForceCompleteAll()
	{
		foreach (Timer activeTimer in Instance.activeTimers)
		{
			activeTimer.OnComplete();
		}
	}

	private static Timer GetTimer()
	{
		List<Timer> list = Instance.inactiveTimers;
		Timer result;
		if (list.Count != 0)
		{
			result = list[0];
			list.RemoveAt(0);
		}
		else
		{
			result = new Timer();
		}
		return result;
	}
}
