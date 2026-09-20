using System;
using System.Collections;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/trigger-zoom/")]
public class ProCamera2DTriggerZoom : BaseTrigger
{
	public static string TriggerName = "Zoom Trigger";

	public bool SetSizeAsMultiplier = true;

	public float TargetZoom = 1.5f;

	public float ZoomSmoothness = 1f;

	[Range(0f, 1f)]
	public float ExclusiveInfluencePercentage = 0.25f;

	public bool ResetSizeOnExit;

	public float ResetSizeSmoothness = 1f;

	private float _startCamSize;

	private float _initialCamSize;

	private float _targetCamSize;

	private float _targetCamSizeSmoothed;

	private float _previousCamSize;

	private float _zoomVelocity;

	private float _initialCamDepth;

	private void Start()
	{
		if (!(ProCamera2D == null))
		{
			_startCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
			_initialCamSize = _startCamSize;
			_targetCamSize = _startCamSize;
			_targetCamSizeSmoothed = _startCamSize;
			_initialCamDepth = Vector3D(ProCamera2D.LocalPosition);
		}
	}

	protected override void EnteredTrigger()
	{
		base.EnteredTrigger();
		ProCamera2D.CurrentZoomTriggerID = _instanceID;
		if (ResetSizeOnExit)
		{
			_initialCamSize = _startCamSize;
			_targetCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
			_targetCamSizeSmoothed = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
		}
		else
		{
			_initialCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
			_targetCamSize = _initialCamSize;
			_targetCamSizeSmoothed = _initialCamSize;
		}
		StartCoroutine(InsideTriggerRoutine());
	}

	protected override void ExitedTrigger()
	{
		base.ExitedTrigger();
		if (ResetSizeOnExit)
		{
			_targetCamSize = _startCamSize;
			StartCoroutine(OutsideTriggerRoutine());
		}
	}

	private IEnumerator InsideTriggerRoutine()
	{
		while (_insideTrigger && _instanceID == ProCamera2D.CurrentZoomTriggerID)
		{
			_exclusiveInfluencePercentage = ExclusiveInfluencePercentage;
			Vector2 point = new Vector2(Vector3H(UseTargetsMidPoint ? ProCamera2D.TargetsMidPoint : TriggerTarget.position), Vector3V(UseTargetsMidPoint ? ProCamera2D.TargetsMidPoint : TriggerTarget.position));
			float distanceToCenterPercentage = GetDistanceToCenterPercentage(point);
			float num = (SetSizeAsMultiplier ? (_startCamSize / TargetZoom) : ((!ProCamera2D.GameCamera.orthographic) ? (Mathf.Abs(_initialCamDepth) * Mathf.Tan(TargetZoom * 0.5f * ((float)Math.PI / 180f))) : TargetZoom));
			float num2 = _initialCamSize * distanceToCenterPercentage + num * (1f - distanceToCenterPercentage);
			if ((num > ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f && num2 > _targetCamSize) || (num < ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f && num2 < _targetCamSize) || ResetSizeOnExit)
			{
				_targetCamSize = num2;
			}
			_previousCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y;
			yield return ProCamera2D.GetYield();
			if (Mathf.Abs(ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f - _targetCamSize) > 0.0001f)
			{
				UpdateScreenSize(ResetSizeOnExit ? ResetSizeSmoothness : ZoomSmoothness);
			}
			if (_previousCamSize == ProCamera2D.ScreenSizeInWorldCoordinates.y)
			{
				_targetCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
				_targetCamSizeSmoothed = _targetCamSize;
				_zoomVelocity = 0f;
			}
		}
	}

	private IEnumerator OutsideTriggerRoutine()
	{
		while (!_insideTrigger && _instanceID == ProCamera2D.CurrentZoomTriggerID && Mathf.Abs(ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f - _targetCamSize) > 0.0001f)
		{
			UpdateScreenSize(ResetSizeOnExit ? ResetSizeSmoothness : ZoomSmoothness);
			yield return ProCamera2D.GetYield();
		}
		_zoomVelocity = 0f;
	}

	protected void UpdateScreenSize(float smoothness)
	{
		_targetCamSizeSmoothed = Mathf.SmoothDamp(_targetCamSizeSmoothed, _targetCamSize, ref _zoomVelocity, smoothness, float.MaxValue, ProCamera2D.DeltaTime);
		ProCamera2D.UpdateScreenSize(_targetCamSizeSmoothed);
	}
}
