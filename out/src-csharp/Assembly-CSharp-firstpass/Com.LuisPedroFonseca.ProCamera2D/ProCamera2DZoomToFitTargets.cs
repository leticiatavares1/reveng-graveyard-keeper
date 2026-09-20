using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-zoom-to-fit/")]
public class ProCamera2DZoomToFitTargets : BasePC2D, ISizeOverrider
{
	public static string ExtensionName = "Zoom To Fit";

	public float ZoomOutBorder = 0.6f;

	public float ZoomInBorder = 0.4f;

	public float ZoomInSmoothness = 2f;

	public float ZoomOutSmoothness = 1f;

	public float MaxZoomInAmount = 2f;

	public float MaxZoomOutAmount = 4f;

	public bool DisableWhenOneTarget = true;

	private float _zoomVelocity;

	private float _initialCamSize;

	private float _previousCamSize;

	private float _targetCamSize;

	private float _targetCamSizeSmoothed;

	private float _minCameraSize;

	private float _maxCameraSize;

	private int _soOrder;

	public int SOOrder
	{
		get
		{
			return _soOrder;
		}
		set
		{
			_soOrder = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!(ProCamera2D == null))
		{
			_initialCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
			_targetCamSize = _initialCamSize;
			_targetCamSizeSmoothed = _targetCamSize;
			ProCamera2D.AddSizeOverrider(this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ProCamera2D.RemoveSizeOverrider(this);
	}

	public float OverrideSize(float deltaTime, float originalSize)
	{
		if (!base.enabled)
		{
			return originalSize;
		}
		_targetCamSizeSmoothed = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
		if (DisableWhenOneTarget && ProCamera2D.CameraTargets.Count <= 1)
		{
			_targetCamSize = _initialCamSize;
		}
		else
		{
			if (_previousCamSize == ProCamera2D.ScreenSizeInWorldCoordinates.y)
			{
				_targetCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
				_targetCamSizeSmoothed = _targetCamSize;
				_zoomVelocity = 0f;
			}
			UpdateTargetCamSize();
		}
		_previousCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y;
		return _targetCamSizeSmoothed = Mathf.SmoothDamp(_targetCamSizeSmoothed, _targetCamSize, ref _zoomVelocity, (_targetCamSize < _targetCamSizeSmoothed) ? ZoomInSmoothness : ZoomOutSmoothness, float.MaxValue, deltaTime);
	}

	public override void OnReset()
	{
		_zoomVelocity = 0f;
		_previousCamSize = _initialCamSize;
		_targetCamSize = _initialCamSize;
		_targetCamSizeSmoothed = _initialCamSize;
	}

	private void UpdateTargetCamSize()
	{
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.PositiveInfinity;
		for (int i = 0; i < ProCamera2D.CameraTargets.Count; i++)
		{
			Vector2 vector = new Vector2(Vector3H(ProCamera2D.CameraTargets[i].TargetPosition) + ProCamera2D.CameraTargets[i].TargetOffset.x, Vector3V(ProCamera2D.CameraTargets[i].TargetPosition) + ProCamera2D.CameraTargets[i].TargetOffset.y);
			num = ((vector.x > num) ? vector.x : num);
			num2 = ((vector.x < num2) ? vector.x : num2);
			num3 = ((vector.y > num3) ? vector.y : num3);
			num4 = ((vector.y < num4) ? vector.y : num4);
		}
		float num5 = Mathf.Abs(num - num2) * 0.5f;
		float num6 = Mathf.Abs(num3 - num4) * 0.5f;
		if (num5 > ProCamera2D.ScreenSizeInWorldCoordinates.x * ZoomOutBorder * 0.5f || num6 > ProCamera2D.ScreenSizeInWorldCoordinates.y * ZoomOutBorder * 0.5f)
		{
			if (num5 / ProCamera2D.ScreenSizeInWorldCoordinates.x >= num6 / ProCamera2D.ScreenSizeInWorldCoordinates.y)
			{
				_targetCamSize = num5 / ProCamera2D.GameCamera.aspect / ZoomOutBorder;
			}
			else
			{
				_targetCamSize = num6 / ZoomOutBorder;
			}
		}
		else if (num5 < ProCamera2D.ScreenSizeInWorldCoordinates.x * ZoomInBorder * 0.5f && num6 < ProCamera2D.ScreenSizeInWorldCoordinates.y * ZoomInBorder * 0.5f)
		{
			if (num5 / ProCamera2D.ScreenSizeInWorldCoordinates.x >= num6 / ProCamera2D.ScreenSizeInWorldCoordinates.y)
			{
				_targetCamSize = num5 / ProCamera2D.GameCamera.aspect / ZoomInBorder;
			}
			else
			{
				_targetCamSize = num6 / ZoomInBorder;
			}
		}
		_minCameraSize = _initialCamSize / MaxZoomInAmount;
		_maxCameraSize = _initialCamSize * MaxZoomOutAmount;
		_targetCamSize = Mathf.Clamp(_targetCamSize, _minCameraSize, _maxCameraSize);
	}
}
