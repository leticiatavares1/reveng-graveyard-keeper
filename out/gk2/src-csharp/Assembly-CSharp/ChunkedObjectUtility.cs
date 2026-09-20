using LazyBearTechnology;
using UnityEngine;

public static class ChunkedObjectUtility
{
	public static void UpdateFlag(this IChunkableObject chunkableObject, ChunkingIgnoreType type, bool newValue)
	{
		if (chunkableObject != null && (!(chunkableObject is Object @object) || !(@object == null)))
		{
			if (chunkableObject.IgnoreMultiFlag == null)
			{
				MultiFlagOR<ChunkingIgnoreType> multiFlagOR2 = (chunkableObject.IgnoreMultiFlag = new MultiFlagOR<ChunkingIgnoreType>());
			}
			chunkableObject.IgnoreMultiFlag.UpdateFlag(type, newValue);
			if (chunkableObject.IgnoreMultiFlag.ResultFlag)
			{
				chunkableObject.UpdateChunkVisibility(isVisible: true);
			}
			else
			{
				LazySingleton<ChunkManager>.Instance.RequestVisibilityRecheck(chunkableObject);
			}
		}
	}
}
