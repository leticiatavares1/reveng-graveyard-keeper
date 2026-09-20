using JetBrains.Annotations;
using LazyBearTechnology;

public interface IChunkableObject
{
	[CanBeNull]
	MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	bool IgnoreChunkVisibility
	{
		get
		{
			if (IgnoreMultiFlag != null)
			{
				return IgnoreMultiFlag.ResultFlag;
			}
			return false;
		}
	}

	BurstableBounds GetChunkableData();

	void UpdateChunkVisibility(bool isVisible);
}
