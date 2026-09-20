using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WorldZoneElevationArea : MonoBehaviour
{
	[SerializeField]
	private BoxCollider footprintCollider;

	[SerializeField]
	[Tooltip("Point whose projected build-grid phase the footprint aligns to (camera perspective). Optional.")]
	private Transform elevationReference;

	[SerializeField]
	private bool showGrid = true;

	[SerializeField]
	private bool showGroundFootprint;

	public bool ShowGrid => showGrid;

	public bool ShowGroundFootprint => showGroundFootprint;

	public BoxCollider FootprintCollider => footprintCollider;

	public float ElevationY
	{
		get
		{
			if (!(footprintCollider != null))
			{
				return base.transform.position.y;
			}
			return footprintCollider.bounds.center.y;
		}
	}

	public float GroundPlaneY
	{
		get
		{
			WorldZone componentInParent = GetComponentInParent<WorldZone>();
			if (!(componentInParent != null))
			{
				return 0f;
			}
			return componentInParent.transform.position.y;
		}
	}

	public Rect GetXZRect()
	{
		if (footprintCollider == null)
		{
			return default(Rect);
		}
		Bounds bounds = footprintCollider.bounds;
		float num = (bounds.center.y - GroundPlaneY) * 0.75f;
		return new Rect(bounds.min.x, bounds.min.z + num, bounds.size.x, bounds.size.z);
	}

	public bool ContainsXZ(Vector2 xz)
	{
		return GetXZRect().Contains(xz);
	}

	public WorldZoneElevationAreaBakedData ToBakedData()
	{
		WorldZoneElevationAreaBakedData result = default(WorldZoneElevationAreaBakedData);
		result.xzRect = GetXZRect();
		result.elevationY = ElevationY;
		return result;
	}

	private void Awake()
	{
		if (footprintCollider == null)
		{
			TryGetComponent<BoxCollider>(out footprintCollider);
		}
		if (footprintCollider != null)
		{
			footprintCollider.isTrigger = true;
			Vector3 size = footprintCollider.size;
			if (size.y > 0.02f)
			{
				footprintCollider.size = new Vector3(size.x, 0.01f, size.z);
			}
		}
	}
}
