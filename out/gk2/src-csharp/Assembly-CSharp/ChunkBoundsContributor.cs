using UnityEngine;

[DisallowMultipleComponent]
public class ChunkBoundsContributor : MonoBehaviour
{
	public static readonly Color gizmoBoundsColor = new Color(1f, 0.5f, 0f, 0.85f);

	public static readonly Color gizmoWithShadowsColor = new Color(1f, 0.85f, 0f, 0.85f);

	[SerializeField]
	[Tooltip("When enabled, With Shadows and Without Shadows use separate bounds.")]
	private bool useSeparateBounds;

	[SerializeField]
	[Tooltip("Local bounds merged into both chunk bounds entries. Zero size = ignored.")]
	private Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	[SerializeField]
	[Tooltip("Local bounds for with-shadows chunk bounds only. Zero size = ignored.")]
	private Bounds boundsWithShadows = new Bounds(Vector3.zero, Vector3.one);

	[SerializeField]
	[Tooltip("Local bounds for without-shadows chunk bounds only. Zero size = ignored.")]
	private Bounds boundsWithoutShadows = new Bounds(Vector3.zero, Vector3.one);

	public bool UseSeparateBounds => useSeparateBounds;

	public bool TryGetBounds(out Bounds worldBounds)
	{
		if (useSeparateBounds)
		{
			worldBounds = default(Bounds);
			return false;
		}
		return TryGetWorldBounds(bounds, out worldBounds);
	}

	public bool TryGetBoundsWithShadows(out Bounds worldBounds)
	{
		return TryGetWorldBounds(useSeparateBounds ? boundsWithShadows : bounds, out worldBounds);
	}

	public bool TryGetBoundsWithoutShadows(out Bounds worldBounds)
	{
		return TryGetWorldBounds(useSeparateBounds ? boundsWithoutShadows : bounds, out worldBounds);
	}

	private bool TryGetWorldBounds(Bounds localBounds, out Bounds worldBounds)
	{
		worldBounds = default(Bounds);
		if (localBounds.size.sqrMagnitude <= 0f)
		{
			return false;
		}
		worldBounds = TransformLocalBounds(localBounds);
		return true;
	}

	private Bounds TransformLocalBounds(Bounds localBounds)
	{
		Transform obj = base.transform;
		Vector3 center = obj.TransformPoint(localBounds.center);
		Vector3 extents = localBounds.extents;
		Vector3 vector = obj.TransformVector(new Vector3(extents.x, 0f, 0f));
		Vector3 vector2 = obj.TransformVector(new Vector3(0f, extents.y, 0f));
		Vector3 vector3 = obj.TransformVector(new Vector3(0f, 0f, extents.z));
		Vector3 vector4 = new Vector3(Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x), Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y), Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z));
		return new Bounds(center, vector4 * 2f);
	}
}
