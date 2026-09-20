using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;

public class WsoConstructorPartsLoadManager : LazySingleton<WsoConstructorPartsLoadManager>
{
	private readonly AsyncLoadRequestTracker<Wso> requestTracker = new AsyncLoadRequestTracker<Wso>();

	public bool HasActiveRequests => requestTracker.HasActiveRequests;

	public void RequestRebuild(Wso wso, bool async = true)
	{
		if (!(wso == null))
		{
			int requestId = requestTracker.Begin(wso);
			if (async)
			{
				RebuildAsync(wso, requestId).Forget();
			}
			else
			{
				RebuildSync(wso, requestId);
			}
		}
	}

	public void CancelRebuild(Wso wso)
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

	private async UniTask RebuildAsync(Wso wso, int requestId)
	{
		if (!(wso == null))
		{
			WsoConstructorPartsBuildResult buildResult = await wso.BuildRuntimeConstructorPartsAsync();
			if (!requestTracker.IsActual(wso, requestId))
			{
				wso.ReleasePendingRuntimeParts(buildResult);
				return;
			}
			requestTracker.Complete(wso, requestId);
			wso.CompleteRuntimePartsRebuild(buildResult);
		}
	}

	private void RebuildSync(Wso wso, int requestId)
	{
		if (!(wso == null))
		{
			WsoConstructorPartsBuildResult buildResult = wso.BuildRuntimeConstructorPartsSync();
			if (!requestTracker.IsActual(wso, requestId))
			{
				wso.ReleasePendingRuntimeParts(buildResult);
				return;
			}
			requestTracker.Complete(wso, requestId);
			wso.CompleteRuntimePartsRebuild(buildResult);
		}
	}
}
