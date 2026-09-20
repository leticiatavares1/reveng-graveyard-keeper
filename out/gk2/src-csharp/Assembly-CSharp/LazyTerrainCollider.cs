using UnityEngine;

public class LazyTerrainCollider : MonoBehaviour, IOrderedSurface
{
	public int depth;

	public SurfaceType surfaceType;

	public Collider collider;

	public int Depth => depth;

	public SurfaceType SurfaceType => surfaceType;
}
