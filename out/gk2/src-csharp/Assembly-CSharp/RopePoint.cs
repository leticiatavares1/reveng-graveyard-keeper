using UnityEngine;

[ExecuteInEditMode]
public class RopePoint : MonoBehaviour
{
	[Header("Point Settings")]
	[SerializeField]
	private bool isStartPoint = true;

	[SerializeField]
	private Vector3 localOffset = Vector3.zero;

	[Header("Gizmo Settings")]
	[SerializeField]
	private bool showGizmo = true;

	[SerializeField]
	private Color gizmoColor = Color.red;

	[SerializeField]
	private float gizmoSize = 0.1f;

	private RopeRenderer parentRope;

	private Vector3 cachedWorldPosition;

	private bool positionDirty = true;

	private void Awake()
	{
		parentRope = GetComponentInParent<RopeRenderer>();
		if (parentRope == null)
		{
			Debug.LogWarning("RopePoint '" + base.gameObject.name + "' is not a child of a RopeRenderer!");
		}
	}

	private void Start()
	{
		UpdateCachedPosition();
	}

	private void Update()
	{
		if (base.transform.hasChanged)
		{
			positionDirty = true;
			base.transform.hasChanged = false;
		}
	}

	public Vector3 GetWorldPosition()
	{
		if (positionDirty)
		{
			UpdateCachedPosition();
		}
		return cachedWorldPosition;
	}

	public Vector3 GetLocalOffset()
	{
		return localOffset;
	}

	public void SetLocalOffset(Vector3 offset)
	{
		localOffset = offset;
		positionDirty = true;
	}

	public bool IsStartPoint()
	{
		return isStartPoint;
	}

	public void SetAsStartPoint(bool isStart)
	{
		isStartPoint = isStart;
	}

	private void UpdateCachedPosition()
	{
		cachedWorldPosition = base.transform.position + base.transform.TransformDirection(localOffset);
		positionDirty = false;
	}

	public void SetGizmoSettings(bool show, Color color, float size)
	{
		showGizmo = show;
		gizmoColor = color;
		gizmoSize = size;
	}

	private void OnDrawGizmos()
	{
		if (showGizmo)
		{
			Gizmos.color = gizmoColor;
			Gizmos.DrawSphere(base.transform.position + base.transform.TransformDirection(localOffset), gizmoSize);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (showGizmo)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(base.transform.position + base.transform.TransformDirection(localOffset), gizmoSize * 1.5f);
		}
	}

	private void OnValidate()
	{
		positionDirty = true;
		if (isStartPoint)
		{
			gizmoColor = Color.green;
		}
		else
		{
			gizmoColor = Color.red;
		}
	}
}
