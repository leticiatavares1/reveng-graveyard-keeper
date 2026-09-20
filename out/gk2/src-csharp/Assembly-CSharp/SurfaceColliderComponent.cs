using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SurfaceColliderComponent : MonoBehaviour, IOrderedSurface
{
	[SerializeField]
	private int depth;

	[SerializeField]
	private SurfaceType surfaceType;

	public int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
		}
	}

	public SurfaceType SurfaceType
	{
		get
		{
			return surfaceType;
		}
		set
		{
			surfaceType = value;
		}
	}
}
