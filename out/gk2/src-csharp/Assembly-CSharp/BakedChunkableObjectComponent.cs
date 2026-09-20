using UnityEngine;

public abstract class BakedChunkableObjectComponent : MonoBehaviour
{
	public abstract BakedChunkableObjectComponentData GetData();

	public abstract void SetData(BakedChunkableObjectComponentData data);

	public virtual void BakeData()
	{
		BakedChunkableObjectComponentData data = GetData();
		ChunkBoundsPair chunkBoundsPair = ChunkSizeCalculator.CalculateChunkBounds(base.gameObject);
		data.lossyScale = base.transform.lossyScale;
		data.rotation = base.transform.rotation;
		data.worldPos = base.transform.position;
		data.chunkBounds = new BurstableChunkBoundsPair(new BurstableBounds(chunkBoundsPair.withShadows.center + base.transform.position - base.transform.position, chunkBoundsPair.withShadows.size), new BurstableBounds(chunkBoundsPair.withoutShadows.center + base.transform.position - base.transform.position, chunkBoundsPair.withoutShadows.size));
		GDPoint componentInParent = GetComponentInParent<GDPoint>(includeInactive: true);
		data.parentGdPointId = ((componentInParent != null) ? componentInParent.Id : null);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		GetData().DrawChunkGizmos();
	}

	public virtual void ApplyData()
	{
		BakedChunkableObjectComponentData data = GetData();
		base.transform.rotation = data.rotation;
		base.transform.position = data.worldPos;
		base.transform.localScale = data.lossyScale;
	}
}
