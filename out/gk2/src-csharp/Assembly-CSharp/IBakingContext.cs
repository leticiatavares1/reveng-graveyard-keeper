using System.Collections.Generic;

public interface IBakingContext
{
	List<BakedChunkableObjectComponentData> GetBakedData { get; }
}
