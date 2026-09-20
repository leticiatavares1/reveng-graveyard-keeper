using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;

public class WgoPartLoadManager : LazySingleton<WgoPartLoadManager>
{
	private readonly AsyncLoadRequestTracker<Wgo> requestTracker = new AsyncLoadRequestTracker<Wgo>();

	public bool HasActiveRequests => requestTracker.HasActiveRequests;

	public int RequestLoad(Wgo wgo, bool applyDefaultWgoPartState, bool async = true)
	{
		if (wgo == null)
		{
			return -1;
		}
		int num = requestTracker.Begin(wgo);
		if (async)
		{
			LoadAndApplyAsync(wgo, num, applyDefaultWgoPartState).Forget();
		}
		else
		{
			LoadAndApplySync(wgo, num, applyDefaultWgoPartState);
		}
		return num;
	}

	public void CancelLoad(Wgo wgo)
	{
		requestTracker.Cancel(wgo);
	}

	public async UniTask WaitForAllRequestsAsync()
	{
		while (HasActiveRequests)
		{
			await UniTask.NextFrame();
		}
	}

	public async UniTask WaitForRequestAsync(Wgo wgo)
	{
		if (!(wgo == null))
		{
			while (requestTracker.IsTracking(wgo))
			{
				await UniTask.NextFrame();
			}
		}
	}

	public async UniTask WaitForRequestsAsync(IEnumerable<Wgo> wgos)
	{
		if (wgos == null)
		{
			return;
		}
		foreach (Wgo wgo in wgos)
		{
			await WaitForRequestAsync(wgo);
		}
	}

	private bool IsRequestActual(Wgo wgo, int requestId)
	{
		return requestTracker.IsActual(wgo, requestId);
	}

	private void LoadAndApplySync(Wgo wgo, int requestId, bool applyDefaultWgoPartState)
	{
		if (wgo == null)
		{
			return;
		}
		string mainWgoPartAssetId = wgo.GetMainWgoPartAssetId();
		string addressableKey = "Assets/AddressableAssets/WGOs/" + mainWgoPartAssetId + ".prefab";
		WgoPart sync = LazySingleton<WgoPartPool>.Instance.GetSync(addressableKey, wgo);
		if (sync == null)
		{
			if (IsRequestActual(wgo, requestId))
			{
				requestTracker.Complete(wgo, requestId);
				wgo.HandleVisualPartsLoadFailed();
			}
			return;
		}
		List<WgoPart> list = new List<WgoPart>();
		List<WgoPartData> list2 = new List<WgoPartData>();
		List<WgoPartData> additionalWgoPartsData = wgo.Data.AdditionalWgoPartsData;
		for (int i = 0; i < additionalWgoPartsData.Count; i++)
		{
			WgoPartData wgoPartData = additionalWgoPartsData[i];
			string addressableKey2 = "Assets/AddressableAssets/WGOs/" + wgoPartData.id + ".prefab";
			WgoPart sync2 = LazySingleton<WgoPartPool>.Instance.GetSync(addressableKey2, wgo);
			if (sync2 != null)
			{
				list.Add(sync2);
				list2.Add(wgoPartData);
			}
		}
		if (!IsRequestActual(wgo, requestId))
		{
			ReleaseLoadedParts(sync, list);
			return;
		}
		requestTracker.Complete(wgo, requestId);
		wgo.CompleteVisualPartsLoad(sync, list, list2, applyDefaultWgoPartState);
	}

	private async UniTask LoadAndApplyAsync(Wgo wgo, int requestId, bool applyDefaultWgoPartState)
	{
		if (wgo == null)
		{
			return;
		}
		string mainWgoPartAssetId = wgo.GetMainWgoPartAssetId();
		string addressableKey = "Assets/AddressableAssets/WGOs/" + mainWgoPartAssetId + ".prefab";
		WgoPart mainPart = await LazySingleton<WgoPartPool>.Instance.GetAsync(addressableKey, wgo);
		if (mainPart == null)
		{
			if (IsRequestActual(wgo, requestId))
			{
				requestTracker.Complete(wgo, requestId);
				wgo.HandleVisualPartsLoadFailed();
			}
			return;
		}
		List<WgoPart> additionalParts = new List<WgoPart>();
		List<WgoPartData> loadedAdditionalData = new List<WgoPartData>();
		List<WgoPartData> additionalData = wgo.Data.AdditionalWgoPartsData;
		for (int i = 0; i < additionalData.Count; i++)
		{
			WgoPartData addData = additionalData[i];
			string addressableKey2 = "Assets/AddressableAssets/WGOs/" + addData.id + ".prefab";
			WgoPart wgoPart = await LazySingleton<WgoPartPool>.Instance.GetAsync(addressableKey2, wgo);
			if (wgoPart != null)
			{
				additionalParts.Add(wgoPart);
				loadedAdditionalData.Add(addData);
			}
		}
		if (!IsRequestActual(wgo, requestId))
		{
			ReleaseLoadedParts(mainPart, additionalParts);
			return;
		}
		requestTracker.Complete(wgo, requestId);
		wgo.CompleteVisualPartsLoad(mainPart, additionalParts, loadedAdditionalData, applyDefaultWgoPartState);
	}

	private static void ReleaseLoadedParts(WgoPart mainPart, List<WgoPart> additionalParts)
	{
		if (mainPart != null && !string.IsNullOrEmpty(mainPart.PooledAddressableKey))
		{
			LazySingleton<WgoPartPool>.Instance.Release(mainPart.PooledAddressableKey, mainPart);
		}
		else if (mainPart != null)
		{
			Object.Destroy(mainPart.gameObject);
		}
		for (int i = 0; i < additionalParts.Count; i++)
		{
			WgoPart wgoPart = additionalParts[i];
			if (!(wgoPart == null))
			{
				if (!string.IsNullOrEmpty(wgoPart.PooledAddressableKey))
				{
					LazySingleton<WgoPartPool>.Instance.Release(wgoPart.PooledAddressableKey, wgoPart);
				}
				else
				{
					Object.Destroy(wgoPart.gameObject);
				}
			}
		}
	}
}
