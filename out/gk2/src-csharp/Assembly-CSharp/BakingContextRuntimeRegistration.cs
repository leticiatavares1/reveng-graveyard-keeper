using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public static class BakingContextRuntimeRegistration
{
	public static void Register(IBakingContext context, List<BakedChunkableObjectComponentData> registrationCache)
	{
		if (context == null || !Application.isPlaying)
		{
			return;
		}
		Unregister(registrationCache);
		foreach (BakedChunkableObjectComponentData getBakedDatum in context.GetBakedData)
		{
			if (getBakedDatum != null)
			{
				registrationCache.Add(getBakedDatum);
				getBakedDatum.UpdateChunkVisibility(isVisible: false);
			}
		}
		if (registrationCache.Count != 0)
		{
			ChunkManager instance = LazySingleton<ChunkManager>.Instance;
			if (!(instance == null))
			{
				instance.RegisterChunks(registrationCache, ChunkManagerLayerType.FightingLevelStaticObjects);
			}
		}
	}

	public static void Unregister(List<BakedChunkableObjectComponentData> registrationCache)
	{
		if (registrationCache == null || registrationCache.Count == 0)
		{
			return;
		}
		if (Application.isPlaying)
		{
			ChunkManager instance = LazySingleton<ChunkManager>.Instance;
			if (instance != null)
			{
				instance.UnregisterChunks(registrationCache, ChunkManagerLayerType.FightingLevelStaticObjects);
			}
		}
		foreach (BakedChunkableObjectComponentData item in registrationCache)
		{
			item.UpdateChunkVisibility(isVisible: false);
		}
		registrationCache.Clear();
	}
}
