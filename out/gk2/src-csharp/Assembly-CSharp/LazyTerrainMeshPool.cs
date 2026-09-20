using System;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LazyTerrainMeshPool : LazySingleton<LazyTerrainMeshPool>, IProgress<float>
{
	public const string PATH_TO_MESH_PREFAB = "Assets/AddressableAssets/Prefabs/LazyTerrainMeshPrefab.prefab";

	public const string PATH_TO_GRASS_PREFAB = "Assets/AddressableAssets/Prefabs/DeformingGrassPrefab.prefab";

	private Pool meshesPool;

	private Pool grassPool;

	private LazyTerrainMesh meshPrefab;

	private DeformingGrass grassPrefab;

	public float LocalProgress { get; private set; }

	public async UniTask InitAsync()
	{
		GameShutdown.ThrowIfRequested();
		Report(0f);
		AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Prefabs/LazyTerrainMeshPrefab.prefab");
		AsyncOperationHandle<GameObject> handle2 = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Prefabs/DeformingGrassPrefab.prefab");
		float mesh = 0f;
		float grass = 0f;
		Progress<float> progress = new Progress<float>(delegate(float p)
		{
			mesh = p;
			OnAddressablesProgressChanged();
		});
		Progress<float> progress2 = new Progress<float>(delegate(float p)
		{
			grass = p;
			OnAddressablesProgressChanged();
		});
		var (goMesh, goGrass) = await UniTask.WhenAll(handle.ToUniTask(progress, PlayerLoopTiming.Update, GameShutdown.Token, cancelImmediately: true, autoReleaseWhenCanceled: true), handle2.ToUniTask(progress2, PlayerLoopTiming.Update, GameShutdown.Token, cancelImmediately: true, autoReleaseWhenCanceled: true));
		GameShutdown.ThrowIfRequested();
		Report(0.8f);
		if (BackgroundLoading.IsActive)
		{
			await UniTask.Yield(PlayerLoopTiming.Update, GameShutdown.Token);
		}
		meshPrefab = goMesh.GetComponent<LazyTerrainMesh>();
		if (meshesPool == null)
		{
			meshesPool = LazyPooler.CreatePool(meshPrefab, 450, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		}
		Report(0.9f);
		if (BackgroundLoading.IsActive)
		{
			await UniTask.Yield(PlayerLoopTiming.Update, GameShutdown.Token);
		}
		grassPrefab = goGrass.GetComponent<DeformingGrass>();
		if (grassPool == null)
		{
			grassPool = LazyPooler.CreatePool(grassPrefab, 80, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		}
		Report(1f);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		void OnAddressablesProgressChanged()
		{
			float num = (mesh + grass) * 0.5f;
			Report(num * 0.8f);
		}
	}

	public static LazyTerrainMesh GetMesh()
	{
		return LazySingleton<LazyTerrainMeshPool>.Instance.meshesPool.GetOrCreateObject<LazyTerrainMesh>();
	}

	public static void ReleaseMesh(LazyTerrainMesh mesh)
	{
		if (!(mesh == null) && !(LazySingleton<LazyTerrainMeshPool>.Instance == null) && LazySingleton<LazyTerrainMeshPool>.Instance.meshesPool != null)
		{
			LazySingleton<LazyTerrainMeshPool>.Instance.meshesPool.ReleaseObject(mesh);
		}
	}

	public static DeformingGrass GetGrass()
	{
		return LazySingleton<LazyTerrainMeshPool>.Instance.grassPool.GetOrCreateObject<DeformingGrass>();
	}

	public static void ReleaseGrass(DeformingGrass grass)
	{
		if (!(grass == null) && !(LazySingleton<LazyTerrainMeshPool>.Instance == null) && LazySingleton<LazyTerrainMeshPool>.Instance.grassPool != null)
		{
			LazySingleton<LazyTerrainMeshPool>.Instance.grassPool.ReleaseObject(grass);
		}
	}

	public void Report(float value)
	{
		LocalProgress = Mathf.Clamp01(value);
	}
}
