using System.Collections.Generic;

public class AsyncLoadRequestTracker<TTarget> where TTarget : class
{
	private readonly Dictionary<TTarget, int> activeRequestIds = new Dictionary<TTarget, int>();

	public bool HasActiveRequests => activeRequestIds.Count > 0;

	public int Begin(TTarget target)
	{
		if (target == null)
		{
			return -1;
		}
		int num = NextRequestId(target);
		activeRequestIds[target] = num;
		return num;
	}

	public void Cancel(TTarget target)
	{
		if (target != null)
		{
			activeRequestIds.Remove(target);
		}
	}

	public bool IsActual(TTarget target, int requestId)
	{
		if (target != null && activeRequestIds.TryGetValue(target, out var value))
		{
			return value == requestId;
		}
		return false;
	}

	public bool IsTracking(TTarget target)
	{
		if (target != null)
		{
			return activeRequestIds.ContainsKey(target);
		}
		return false;
	}

	public void Complete(TTarget target, int requestId)
	{
		if (IsActual(target, requestId))
		{
			activeRequestIds.Remove(target);
		}
	}

	private int NextRequestId(TTarget target)
	{
		if (activeRequestIds.TryGetValue(target, out var value))
		{
			return value + 1;
		}
		return 1;
	}
}
