using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-geometry-boundaries/")]
public class ProCamera2DGeometryBoundaries : BasePC2D, IPositionDeltaChanger
{
	public static string ExtensionName = "Geometry Boundaries";

	[Tooltip("The layer that contains the (3d) colliders that define the boundaries for the camera")]
	public LayerMask BoundariesLayerMask;

	private MoveInColliderBoundaries _cameraMoveInColliderBoundaries;

	private int _pdcOrder = 3000;

	public int PDCOrder
	{
		get
		{
			return _pdcOrder;
		}
		set
		{
			_pdcOrder = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_cameraMoveInColliderBoundaries = new MoveInColliderBoundaries(ProCamera2D);
		_cameraMoveInColliderBoundaries.CameraTransform = ProCamera2D.transform;
		_cameraMoveInColliderBoundaries.CameraCollisionMask = BoundariesLayerMask;
		ProCamera2D.AddPositionDeltaChanger(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ProCamera2D.RemovePositionDeltaChanger(this);
	}

	public Vector3 AdjustDelta(float deltaTime, Vector3 originalDelta)
	{
		if (!base.enabled)
		{
			return originalDelta;
		}
		_cameraMoveInColliderBoundaries.CameraSize = ProCamera2D.ScreenSizeInWorldCoordinates;
		return _cameraMoveInColliderBoundaries.Move(originalDelta);
	}
}
