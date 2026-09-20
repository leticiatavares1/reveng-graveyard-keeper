using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyCondition : MonoBehaviour
{
	private const int POOL_SIZE = 5;

	[SerializeField]
	private List<ConditionalChecker> activeCheckers = new List<ConditionalChecker>();

	private List<ConditionalChecker> inactiveCheckers = new List<ConditionalChecker>();

	private static LazyCondition instance;

	public static LazyCondition Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("LazyCondition").AddComponent<LazyCondition>();
				UnityEngine.Object.DontDestroyOnLoad(instance);
			}
			return instance;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < 5; i++)
		{
			inactiveCheckers.Add(new ConditionalChecker());
		}
	}

	private void Update()
	{
		for (int i = 0; i < activeCheckers.Count; i++)
		{
			ConditionalChecker conditionalChecker = activeCheckers[i];
			if (conditionalChecker.isActive)
			{
				conditionalChecker.Update();
				continue;
			}
			activeCheckers.Remove(conditionalChecker);
			inactiveCheckers.Add(conditionalChecker);
			i--;
		}
	}

	public static int AddConditionalChecker(Func<bool> condition, Action onUpdate, Action onComplete)
	{
		if (condition == null)
		{
			Debug.LogError("Trying to add conditional checker without condition");
			return -1;
		}
		if (onComplete == null)
		{
			Debug.LogError("Trying to add conditional checker without complete action");
			return -1;
		}
		ConditionalChecker conditionalChecker = GetConditionalChecker();
		conditionalChecker.Init(condition, onUpdate, onComplete);
		Instance.activeCheckers.Add(conditionalChecker);
		return conditionalChecker.id;
	}

	public static bool Stop(int id)
	{
		ConditionalChecker conditionalChecker = Instance.activeCheckers.Find((ConditionalChecker x) => x.id == id);
		if (conditionalChecker != null)
		{
			conditionalChecker.Stop();
			return true;
		}
		return false;
	}

	public static void StopAll()
	{
		foreach (ConditionalChecker activeChecker in Instance.activeCheckers)
		{
			activeChecker.Stop();
		}
	}

	public static void ForceCompleteAll()
	{
		foreach (ConditionalChecker activeChecker in Instance.activeCheckers)
		{
			activeChecker.OnComplete();
		}
	}

	private static ConditionalChecker GetConditionalChecker()
	{
		List<ConditionalChecker> list = Instance.inactiveCheckers;
		ConditionalChecker result;
		if (list.Count != 0)
		{
			result = list[0];
			list.RemoveAt(0);
		}
		else
		{
			result = new ConditionalChecker();
		}
		return result;
	}
}
