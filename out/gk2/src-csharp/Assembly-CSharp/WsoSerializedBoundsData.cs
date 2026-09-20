using UnityEngine;

public class WsoSerializedBoundsData : WsoComponentDataBase
{
	[SerializeField]
	private ChunkBoundsPair bounds;

	public ChunkBoundsPair Bounds
	{
		get
		{
			return bounds;
		}
		set
		{
			bounds = value;
		}
	}
}
