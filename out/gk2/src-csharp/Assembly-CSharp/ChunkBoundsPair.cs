using System;
using UnityEngine;

[Serializable]
public struct ChunkBoundsPair
{
	public Bounds withShadows;

	public Bounds withoutShadows;

	public Bounds GetBounds()
	{
		if (PlatformFeatureConfig.Get(GamePlatformResolver.Current).shadowMode == PlatformShadowMode.Off)
		{
			return withoutShadows;
		}
		return withShadows;
	}

	public ChunkBoundsPair(Bounds withShadows, Bounds withoutShadows)
	{
		this.withShadows = withShadows;
		this.withoutShadows = withoutShadows;
	}
}
