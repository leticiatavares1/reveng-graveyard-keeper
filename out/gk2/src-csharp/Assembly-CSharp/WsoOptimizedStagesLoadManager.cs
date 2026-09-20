using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;

public class WsoOptimizedStagesLoadManager : LazySingleton<WsoOptimizedStagesLoadManager>
{
	private readonly AsyncLoadRequestTracker<Wso> requestTracker = new AsyncLoadRequestTracker<Wso>();

	public bool HasActiveRequests => requestTracker.HasActiveRequests;

	public void RequestLoad(Wso wso, bool async = true)
	{
		if (!(wso == null))
		{
			int requestId = requestTracker.Begin(wso);
			if (async)
			{
				LoadAsync(wso, requestId).Forget();
			}
			else
			{
				LoadSync(wso, requestId);
			}
		}
	}

	public void CancelLoad(Wso wso)
	{
		requestTracker.Cancel(wso);
	}

	public async UniTask WaitForAllRequestsAsync()
	{
		while (HasActiveRequests)
		{
			await UniTask.NextFrame();
		}
	}

	public async UniTask WaitForRequestAsync(Wso wso)
	{
		if (!(wso == null))
		{
			while (requestTracker.IsTracking(wso))
			{
				await UniTask.NextFrame();
			}
		}
	}

	public async UniTask WaitForRequestsAsync(IEnumerable<Wso> wsos)
	{
		if (wsos == null)
		{
			return;
		}
		foreach (Wso wso in wsos)
		{
			await WaitForRequestAsync(wso);
		}
	}

	private async UniTask LoadAsync(Wso wso, int requestId)
	{
		if (!(wso == null))
		{
			WsoOptimizedStagesBuildResult buildResult = await wso.BuildOptimizedStagesAsync();
			if (!requestTracker.IsActual(wso, requestId))
			{
				wso.ReleasePendingOptimizedStages(buildResult);
				return;
			}
			requestTracker.Complete(wso, requestId);
			wso.CompleteOptimizedStagesLoad(buildResult);
		}
	}

	private void LoadSync(Wso wso, int requestId)
	{
		if (!(wso == null))
		{
			WsoOptimizedStagesBuildResult buildResult = wso.BuildOptimizedStagesSync();
			if (!requestTracker.IsActual(wso, requestId))
			{
				wso.ReleasePendingOptimizedStages(buildResult);
				return;
			}
			requestTracker.Complete(wso, requestId);
			wso.CompleteOptimizedStagesLoad(buildResult);
		}
	}
}
