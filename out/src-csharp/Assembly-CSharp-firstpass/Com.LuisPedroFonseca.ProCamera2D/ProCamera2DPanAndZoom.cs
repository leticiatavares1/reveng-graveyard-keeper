using UnityEngine;
using UnityEngine.EventSystems;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-pan-and-zoom/")]
public class ProCamera2DPanAndZoom : BasePC2D, ISizeDeltaChanger, IPreMover
{
	public static string ExtensionName = "Pan And Zoom";

	public bool DisableOverUGUI = true;

	public bool AllowZoom = true;

	public float MouseZoomSpeed = 10f;

	public float PinchZoomSpeed = 50f;

	[Range(0f, 2f)]
	public float ZoomSmoothness = 0.2f;

	public float MaxZoomInAmount = 2f;

	public float MaxZoomOutAmount = 2f;

	public bool ZoomToInputCenter = true;

	private float _zoomAmount;

	private float _initialCamSize;

	private bool _zoomStarted;

	private float _origFollowSmoothnessX;

	private float _origFollowSmoothnessY;

	private float _prevZoomAmount;

	private float _zoomVelocity;

	private Vector3 _zoomPoint;

	private float _touchZoomTime;

	public bool AllowPan = true;

	public bool UsePanByDrag = true;

	[Range(0f, 1f)]
	public float StopSpeedOnDragStart = 0.95f;

	public Rect DraggableAreaRect = new Rect(0f, 0f, 1f, 1f);

	public Vector2 DragPanSpeedMultiplier = new Vector2(1f, 1f);

	public bool UsePanByMoveToEdges;

	public Vector2 EdgesPanSpeed = new Vector2(2f, 2f);

	[Range(0f, 0.99f)]
	public float TopPanEdge = 0.9f;

	[Range(0f, 0.99f)]
	public float BottomPanEdge = 0.9f;

	[Range(0f, 0.99f)]
	public float LeftPanEdge = 0.9f;

	[Range(0f, 0.99f)]
	public float RightPanEdge = 0.9f;

	[HideInInspector]
	public bool ResetPrevPanPoint;

	private Vector2 _panDelta;

	private Transform _panTarget;

	private Vector3 _prevMousePosition;

	private Vector3 _prevTouchPosition;

	private bool _onMaxZoom;

	private bool _onMinZoom;

	private EventSystem _eventSystem;

	private bool _skip;

	private int _prmOrder;

	private int _sdcOrder;

	public int PrMOrder
	{
		get
		{
			return _prmOrder;
		}
		set
		{
			_prmOrder = value;
		}
	}

	public int SDCOrder
	{
		get
		{
			return _sdcOrder;
		}
		set
		{
			_sdcOrder = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UpdateCurrentFollowSmoothness();
		_eventSystem = EventSystem.current;
		_panTarget = new GameObject("PC2DPanTarget").transform;
		ProCamera2D.AddPreMover(this);
		ProCamera2D.AddSizeDeltaChanger(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ProCamera2D.RemovePreMover(this);
		ProCamera2D.RemoveSizeDeltaChanger(this);
	}

	private void Start()
	{
		_initialCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		CenterPanTargetOnCamera();
		ProCamera2D.Instance.AddCameraTarget(_panTarget);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ResetPrevPanPoint = true;
		_onMaxZoom = false;
		_onMinZoom = false;
		ProCamera2D.RemoveCameraTarget(_panTarget);
	}

	public void PreMove(float deltaTime)
	{
		_skip = DisableOverUGUI && (bool)_eventSystem && _eventSystem.IsPointerOverGameObject();
		if (_skip)
		{
			_prevMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Vector3D(ProCamera2D.LocalPosition)));
			CancelZoom();
		}
		if (base.enabled && AllowPan && !_skip)
		{
			Pan(deltaTime);
		}
	}

	public float AdjustSize(float deltaTime, float originalDelta)
	{
		if (base.enabled && AllowZoom && !_skip)
		{
			return Zoom(deltaTime) + originalDelta;
		}
		return originalDelta;
	}

	private void Pan(float deltaTime)
	{
		_panDelta = Vector2.zero;
		Vector2 vector = DragPanSpeedMultiplier;
		if (UsePanByDrag && Input.GetMouseButtonDown(0))
		{
			CenterPanTargetOnCamera(StopSpeedOnDragStart);
		}
		Vector3 vector2 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Vector3D(ProCamera2D.LocalPosition)));
		if (UsePanByDrag && Input.GetMouseButton(0))
		{
			Vector2 normalizedInput = new Vector2(Input.mousePosition.x / (float)Screen.width, Input.mousePosition.y / (float)Screen.height);
			if (InsideDraggableArea(normalizedInput))
			{
				Vector3 vector3 = ProCamera2D.GameCamera.ScreenToWorldPoint(_prevMousePosition);
				if (ResetPrevPanPoint)
				{
					vector3 = ProCamera2D.GameCamera.ScreenToWorldPoint(vector2);
					ResetPrevPanPoint = false;
				}
				Vector3 vector4 = ProCamera2D.GameCamera.ScreenToWorldPoint(vector2);
				Vector3 arg = vector3 - vector4;
				_panDelta = new Vector2(Vector3H(arg), Vector3V(arg));
			}
		}
		else if (UsePanByMoveToEdges && !Input.GetMouseButton(0))
		{
			float num = ((float)(-Screen.width) * 0.5f + Input.mousePosition.x) / (float)Screen.width;
			float num2 = ((float)(-Screen.height) * 0.5f + Input.mousePosition.y) / (float)Screen.height;
			if (num < 0f)
			{
				num = num.Remap(-0.5f, (0f - LeftPanEdge) * 0.5f, -0.5f, 0f);
			}
			else if (num > 0f)
			{
				num = num.Remap(RightPanEdge * 0.5f, 0.5f, 0f, 0.5f);
			}
			if (num2 < 0f)
			{
				num2 = num2.Remap(-0.5f, (0f - BottomPanEdge) * 0.5f, -0.5f, 0f);
			}
			else if (num2 > 0f)
			{
				num2 = num2.Remap(TopPanEdge * 0.5f, 0.5f, 0f, 0.5f);
			}
			_panDelta = new Vector2(num, num2) * deltaTime;
			if (_panDelta != Vector2.zero)
			{
				vector = EdgesPanSpeed;
			}
		}
		_prevMousePosition = vector2;
		if (_panDelta != Vector2.zero)
		{
			Vector3 translation = VectorHV(_panDelta.x * vector.x, _panDelta.y * vector.y);
			_panTarget.Translate(translation);
		}
		if ((ProCamera2D.IsCameraPositionLeftBounded && Vector3H(_panTarget.position) < Vector3H(ProCamera2D.LocalPosition)) || (ProCamera2D.IsCameraPositionRightBounded && Vector3H(_panTarget.position) > Vector3H(ProCamera2D.LocalPosition)))
		{
			_panTarget.position = VectorHVD(Vector3H(ProCamera2D.LocalPosition), Vector3V(_panTarget.position), Vector3D(_panTarget.position));
		}
		if ((ProCamera2D.IsCameraPositionBottomBounded && Vector3V(_panTarget.position) < Vector3V(ProCamera2D.LocalPosition)) || (ProCamera2D.IsCameraPositionTopBounded && Vector3V(_panTarget.position) > Vector3V(ProCamera2D.LocalPosition)))
		{
			_panTarget.position = VectorHVD(Vector3H(_panTarget.position), Vector3V(ProCamera2D.LocalPosition), Vector3D(_panTarget.position));
		}
	}

	private float Zoom(float deltaTime)
	{
		if (_panDelta != Vector2.zero)
		{
			CancelZoom();
			RestoreFollowSmoothness();
			return 0f;
		}
		float num = 0f;
		num = Input.GetAxis("Mouse ScrollWheel");
		_zoomPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Vector3D(ProCamera2D.LocalPosition)));
		float num2 = 0f;
		num2 = MouseZoomSpeed;
		if ((_onMaxZoom && num * num2 < 0f) || (_onMinZoom && num * num2 > 0f))
		{
			CancelZoom();
			return 0f;
		}
		_zoomAmount = Mathf.SmoothDamp(_prevZoomAmount, num * num2 * deltaTime, ref _zoomVelocity, ZoomSmoothness, float.MaxValue, deltaTime);
		if (Mathf.Abs(_zoomAmount) <= 0.0001f)
		{
			if (_zoomStarted)
			{
				RestoreFollowSmoothness();
			}
			_zoomStarted = false;
			_prevZoomAmount = 0f;
			return 0f;
		}
		if (!_zoomStarted)
		{
			_zoomStarted = true;
			_panTarget.position = ProCamera2D.LocalPosition - ProCamera2D.InfluencesSum;
			UpdateCurrentFollowSmoothness();
			RemoveFollowSmoothness();
		}
		float num3 = ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f + _zoomAmount;
		float num4 = _initialCamSize / MaxZoomInAmount;
		float num5 = MaxZoomOutAmount * _initialCamSize;
		_onMaxZoom = false;
		_onMinZoom = false;
		if (num3 < num4)
		{
			_zoomAmount -= num3 - num4;
			_onMaxZoom = true;
		}
		else if (num3 > num5)
		{
			_zoomAmount -= num3 - num5;
			_onMinZoom = true;
		}
		_prevZoomAmount = _zoomAmount;
		if (ZoomToInputCenter && _zoomAmount != 0f)
		{
			float num6 = _zoomAmount / (ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f);
			_panTarget.position += (_panTarget.position - ProCamera2D.GameCamera.ScreenToWorldPoint(_zoomPoint)) * num6;
		}
		return _zoomAmount;
	}

	public void UpdateCurrentFollowSmoothness()
	{
		_origFollowSmoothnessX = ProCamera2D.HorizontalFollowSmoothness;
		_origFollowSmoothnessY = ProCamera2D.VerticalFollowSmoothness;
	}

	public void CenterPanTargetOnCamera(float interpolant = 1f)
	{
		if (_panTarget != null)
		{
			_panTarget.position = Vector3.Lerp(_panTarget.position, VectorHV(Vector3H(ProCamera2D.LocalPosition), Vector3V(ProCamera2D.LocalPosition)), interpolant);
		}
	}

	private void CancelZoom()
	{
		_zoomAmount = 0f;
		_prevZoomAmount = 0f;
		_zoomVelocity = 0f;
	}

	private void RestoreFollowSmoothness()
	{
		ProCamera2D.HorizontalFollowSmoothness = _origFollowSmoothnessX;
		ProCamera2D.VerticalFollowSmoothness = _origFollowSmoothnessY;
	}

	private void RemoveFollowSmoothness()
	{
		ProCamera2D.HorizontalFollowSmoothness = 0f;
		ProCamera2D.VerticalFollowSmoothness = 0f;
	}

	private bool InsideDraggableArea(Vector2 normalizedInput)
	{
		if (DraggableAreaRect.x == 0f && DraggableAreaRect.y == 0f && DraggableAreaRect.width == 1f && DraggableAreaRect.height == 1f)
		{
			return true;
		}
		if (normalizedInput.x > DraggableAreaRect.x + (1f - DraggableAreaRect.width) / 2f && normalizedInput.x < DraggableAreaRect.x + DraggableAreaRect.width + (1f - DraggableAreaRect.width) / 2f && normalizedInput.y > DraggableAreaRect.y + (1f - DraggableAreaRect.height) / 2f && normalizedInput.y < DraggableAreaRect.y + DraggableAreaRect.height + (1f - DraggableAreaRect.height) / 2f)
		{
			return true;
		}
		return false;
	}
}
