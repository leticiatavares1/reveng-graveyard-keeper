using Unity.Mathematics;
using UnityEngine;

public static class ChunkExtensions
{
	public const string KEY_DRAW_GIZMOS = "draw_chink_gizmos";

	public static bool DrawGizmos
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("draw_chink_gizmos", value.ToInt());
		}
	}

	public static void DrawChunkGizmos(this IChunkableObject chunkableObject)
	{
		if (DrawGizmos)
		{
			BurstableBounds chunkableData = chunkableObject.GetChunkableData();
			if (chunkableData.Extents.Equals(float3.zero))
			{
				Vector3 vector = chunkableData.center;
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(vector, vector + Vector3.up * 5f);
				Gizmos.DrawSphere(vector, 0.05f);
			}
			else
			{
				Gizmos.DrawWireCube(chunkableData.center, chunkableData.size);
			}
		}
	}
}
