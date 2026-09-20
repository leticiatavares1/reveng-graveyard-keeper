using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBearTechnology;

public abstract class MultiFlagBase<T> where T : Enum
{
	protected readonly Dictionary<T, bool> flags = new Dictionary<T, bool>();

	private bool currentResultFlag;

	private Action<bool> onChangedAction;

	public bool ResultFlag => currentResultFlag;

	protected abstract bool DefaultValueForMissingFlag { get; }

	protected abstract bool ExceptResultOnMatch { get; }

	protected abstract bool ExceptResultDefault { get; }

	protected abstract bool ComparedFlagsResultOnMatch { get; }

	protected abstract bool ComparedFlagsResultDefault { get; }

	public void Init(Action<bool> onChangedAction, bool initialFlag = false)
	{
		this.onChangedAction = onChangedAction;
		currentResultFlag = initialFlag;
	}

	public void UpdateFlag(T t, bool newValue)
	{
		if (!flags.ContainsKey(t))
		{
			AddFlag(t, newValue);
			return;
		}
		flags[t] = newValue;
		CalcResult();
	}

	public bool HasFlag(T t)
	{
		return flags.ContainsKey(t);
	}

	public bool GetFlag(T state)
	{
		if (!flags.TryGetValue(state, out var value))
		{
			return DefaultValueForMissingFlag;
		}
		return value;
	}

	public void Reset(bool initialFlag = false)
	{
		flags.Clear();
		currentResultFlag = initialFlag;
		CalcResult();
	}

	public Dictionary<T, bool> GetAllFlags()
	{
		Dictionary<T, bool> dictionary = new Dictionary<T, bool>();
		foreach (KeyValuePair<T, bool> flag in flags)
		{
			dictionary.Add(flag.Key, flag.Value);
		}
		return dictionary;
	}

	public bool GetResultFlagExceptFlagTypes(params T[] ignoredFlagTypes)
	{
		foreach (KeyValuePair<T, bool> flag in flags)
		{
			if (!ignoredFlagTypes.Contains(flag.Key) && IsExceptMatch(flag.Value))
			{
				return ExceptResultOnMatch;
			}
		}
		return ExceptResultDefault;
	}

	public bool GetResultFlagExceptFlagTypesNonAlloc(params T[] ignoreStates)
	{
		Dictionary<T, bool>.Enumerator enumerator = flags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<T, bool> current = enumerator.Current;
			if (!ignoreStates.Contains(current.Key) && IsExceptMatch(current.Value))
			{
				return ExceptResultOnMatch;
			}
		}
		return ExceptResultDefault;
	}

	public bool AreFlagsSet(params T[] flagTypesToCompare)
	{
		foreach (KeyValuePair<T, bool> flag in flags)
		{
			if (flagTypesToCompare.Contains(flag.Key) && IsComparedFlagsMatch(flag.Value))
			{
				return ComparedFlagsResultOnMatch;
			}
		}
		return ComparedFlagsResultDefault;
	}

	protected abstract bool IsExceptMatch(bool flagValue);

	protected abstract bool IsComparedFlagsMatch(bool flagValue);

	protected abstract void CalcResult();

	private void AddFlag(T t, bool flagOnStart)
	{
		flags.Add(t, flagOnStart);
		CalcResult();
	}

	protected void SetResultFlagAndNotify(bool resultFlag)
	{
		if (currentResultFlag != resultFlag)
		{
			currentResultFlag = resultFlag;
			onChangedAction?.Invoke(currentResultFlag);
		}
	}
}
