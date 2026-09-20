using System;

[Serializable]
public struct BurstableChunkBoundsPair
{
	public BurstableBounds withShadows;

	public BurstableBounds withoutShadows;

	public BurstableBounds GetBounds()
	{
		if (PlatformFeatureConfig.Get(GamePlatformResolver.Current).shadowMode == PlatformShadowMode.Off)
		{
			return withoutShadows;
		}
		return withShadows;
	}

	public BurstableChunkBoundsPair(BurstableBounds withShadows, BurstableBounds withoutShadows)
	{
		this.withShadows = withShadows;
		this.withoutShadows = withoutShadows;
	}
}
