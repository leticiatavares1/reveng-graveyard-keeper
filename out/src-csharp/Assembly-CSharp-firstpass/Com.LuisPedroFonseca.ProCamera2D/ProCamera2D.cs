using System;
using System.Collections;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/core/")]
[RequireComponent(typeof(Camera))]
public class ProCamera2D : MonoBehaviour, ISerializationCallbackReceiver
{
	public const string Title = "Pro Camera 2D";

	public static readonly Version Version = new Version("2.4.4");

	public List<CameraTarget> CameraTargets = new List<CameraTarget>();

	public bool CenterTargetOnStart;

	public MovementAxis Axis;

	public UpdateType UpdateType;

	public bool FollowHorizontal = true;

	public float HorizontalFollowSmoothness = 0.15f;

	public bool FollowVertical = true;

	public float VerticalFollowSmoothness = 0.15f;

	[Range(-1f, 1f)]
	public float OffsetX;

	[Range(-1f, 1f)]
	public float OffsetY;

	public bool IsRelativeOffset = true;

	public bool ZoomWithFOV;

	private static ProCamera2D _instance;

	private float _cameraTargetHorizontalPositionSmoothed;

	private float _cameraTargetVerticalPositionSmoothed;

	private Vector2 _screenSizeInWorldCoordinates;

	private Vector3 _previousTargetsMidPoint;

	private Vector3 _targetsMidPoint;

	private Vector3 _cameraTargetPosition;

	private float _deltaTime;

	private Vector3 _parentPosition;

	private Vector3 _influencesSum = Vector3.zero;

	public Action<float> PreMoveUpdate;

	public Action<float> PostMoveUpdate;

	public Action<Vector2> OnCameraResize;

	public Action OnReset;

	public Vector3? ExclusiveTargetPosition;

	public int CurrentZoomTriggerID;

	public bool IsCameraPositionLeftBounded;

	public bool IsCameraPositionRightBounded;

	public bool IsCameraPositionTopBounded;

	public bool IsCameraPositionBottomBounded;

	public Camera GameCamera;

	private Func<Vector3, float> Vector3H;

	private Func<Vector3, float> Vector3V;

	private Func<Vector3, float> Vector3D;

	private Func<float, float, Vector3> VectorHV;

	private Func<float, float, float, Vector3> VectorHVD;

	private Vector2 _startScreenSizeInWorldCoordinates;

	private Coroutine _updateScreenSizeCoroutine;

	private Coroutine _dollyZoomRoutine;

	private List<Vector3> _influences = new List<Vector3>();

	private float _originalCameraDepthSign;

	private float _previousCameraTargetHorizontalPositionSmoothed;

	private float _previousCameraTargetVerticalPositionSmoothed;

	private int _previousScreenWidth;

	private int _previousScreenHeight;

	private Vector3 _previousCameraPosition;

	private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

	private Transform _transform;

	private List<IPreMover> _preMovers = new List<IPreMover>();

	private List<IPositionDeltaChanger> _positionDeltaChangers = new List<IPositionDeltaChanger>();

	private List<IPositionOverrider> _positionOverriders = new List<IPositionOverrider>();

	private List<ISizeDeltaChanger> _sizeDeltaChangers = new List<ISizeDeltaChanger>();

	private List<ISizeOverrider> _sizeOverriders = new List<ISizeOverrider>();

	private List<IPostMover> _postMovers = new List<IPostMover>();

	public static ProCamera2D Instance
	{
		get
		{
			if (object.Equals(_instance, null))
			{
				_instance = UnityEngine.Object.FindObjectOfType(typeof(ProCamera2D)) as ProCamera2D;
				if (object.Equals(_instance, null))
				{
					throw new UnityException("ProCamera2D does not exist.");
				}
			}
			return _instance;
		}
	}

	public static bool Exists => _instance != null;

	public bool IsMoving
	{
		get
		{
			if (Vector3H(_transform.localPosition) == Vector3H(_previousCameraPosition))
			{
				return Vector3V(_transform.localPosition) != Vector3V(_previousCameraPosition);
			}
			return true;
		}
	}

	public Rect Rect
	{
		get
		{
			return GameCamera.rect;
		}
		set
		{
			GameCamera.rect = value;
			ProCamera2DParallax componentInChildren = GetComponentInChildren<ProCamera2DParallax>();
			if (componentInChildren != null)
			{
				for (int i = 0; i < componentInChildren.ParallaxLayers.Count; i++)
				{
					componentInChildren.ParallaxLayers[i].ParallaxCamera.rect = value;
				}
			}
		}
	}

	public Vector2 CameraTargetPositionSmoothed
	{
		get
		{
			return new Vector2(_cameraTargetHorizontalPositionSmoothed, _cameraTargetVerticalPositionSmoothed);
		}
		set
		{
			_cameraTargetHorizontalPositionSmoothed = value.x;
			_cameraTargetVerticalPositionSmoothed = value.y;
		}
	}

	public Vector3 LocalPosition
	{
		get
		{
			return _transform.localPosition;
		}
		set
		{
			_transform.localPosition = value;
		}
	}

	public Vector2 ScreenSizeInWorldCoordinates => _screenSizeInWorldCoordinates;

	public Vector3 PreviousTargetsMidPoint => _previousTargetsMidPoint;

	public Vector3 TargetsMidPoint => _targetsMidPoint;

	public Vector3 CameraTargetPosition => _cameraTargetPosition;

	public float DeltaTime => _deltaTime;

	public Vector3 ParentPosition => _parentPosition;

	public Vector3 InfluencesSum => _influencesSum;

	protected void Awake()
	{
		Debug.Log("ProCamera2D.Awake()", this);
		_instance = this;
		_transform = base.transform;
		if (_transform.parent != null)
		{
			_parentPosition = _transform.parent.position;
		}
		if (GameCamera == null)
		{
			GameCamera = GetComponent<Camera>();
		}
		if (GameCamera == null)
		{
			Debug.LogError("Unity Camera not set and not found on the GameObject: " + base.gameObject.name);
		}
		ResetAxisFunctions();
		for (int i = 0; i < CameraTargets.Count; i++)
		{
			if (CameraTargets[i].TargetTransform == null)
			{
				CameraTargets.RemoveAt(i);
			}
		}
		CalculateScreenSize();
		_startScreenSizeInWorldCoordinates = _screenSizeInWorldCoordinates;
		_originalCameraDepthSign = Mathf.Sign(Vector3D(_transform.localPosition));
	}

	private void Start()
	{
		SortPreMovers();
		SortPositionDeltaChangers();
		SortPositionOverriders();
		SortSizeDeltaChangers();
		SortSizeOverriders();
		SortPostMovers();
		_targetsMidPoint = GetTargetsWeightedMidPoint(ref CameraTargets);
		_cameraTargetHorizontalPositionSmoothed = Vector3H(_targetsMidPoint);
		_cameraTargetVerticalPositionSmoothed = Vector3V(_targetsMidPoint);
		_deltaTime = Time.deltaTime;
		if (CenterTargetOnStart && CameraTargets.Count > 0)
		{
			Vector3 targetsWeightedMidPoint = GetTargetsWeightedMidPoint(ref CameraTargets);
			float x = (FollowHorizontal ? Vector3H(targetsWeightedMidPoint) : Vector3H(_transform.localPosition));
			float y = (FollowVertical ? Vector3V(targetsWeightedMidPoint) : Vector3V(_transform.localPosition));
			Vector2 cameraPos = new Vector2(x, y);
			cameraPos += new Vector2(GetOffsetX() - Vector3H(_parentPosition), GetOffsetY() - Vector3V(_parentPosition));
			MoveCameraInstantlyToPosition(cameraPos);
		}
		else
		{
			_cameraTargetPosition = _transform.position - _parentPosition;
			_cameraTargetHorizontalPositionSmoothed = Vector3H(_cameraTargetPosition);
			_previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;
			_cameraTargetVerticalPositionSmoothed = Vector3V(_cameraTargetPosition);
			_previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;
		}
	}

	private void LateUpdate()
	{
		if (UpdateType == UpdateType.LateUpdate)
		{
			Move(Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (UpdateType == UpdateType.FixedUpdate)
		{
			Move(Time.fixedDeltaTime);
		}
	}

	private void OnApplicationQuit()
	{
		_instance = null;
	}

	public void ApplyInfluence(Vector2 influence)
	{
		if (!(Time.deltaTime < 0.0001f) && !float.IsNaN(influence.x) && !float.IsNaN(influence.y))
		{
			_influences.Add(VectorHV(influence.x, influence.y));
		}
	}

	public Coroutine ApplyInfluencesTimed(Vector2[] influences, float[] durations)
	{
		return StartCoroutine(ApplyInfluencesTimedRoutine(influences, durations));
	}

	public CameraTarget AddCameraTarget(Transform targetTransform, float targetInfluenceH = 1f, float targetInfluenceV = 1f, float duration = 0f, Vector2 targetOffset = default(Vector2))
	{
		CameraTarget cameraTarget = new CameraTarget
		{
			TargetTransform = targetTransform,
			TargetInfluenceH = targetInfluenceH,
			TargetInfluenceV = targetInfluenceV,
			TargetOffset = targetOffset
		};
		CameraTargets.Add(cameraTarget);
		if (duration > 0f)
		{
			cameraTarget.TargetInfluence = 0f;
			StartCoroutine(AdjustTargetInfluenceRoutine(cameraTarget, targetInfluenceH, targetInfluenceV, duration));
		}
		return cameraTarget;
	}

	public void AddCameraTargets(IList<Transform> targetsTransforms, float targetsInfluenceH = 1f, float targetsInfluenceV = 1f, float duration = 0f, Vector2 targetOffset = default(Vector2))
	{
		for (int i = 0; i < targetsTransforms.Count; i++)
		{
			AddCameraTarget(targetsTransforms[i], targetsInfluenceH, targetsInfluenceV, duration, targetOffset);
		}
	}

	public void AddCameraTargets(IList<CameraTarget> cameraTargets)
	{
		CameraTargets.AddRange(cameraTargets);
	}

	public CameraTarget GetCameraTarget(Transform targetTransform)
	{
		for (int i = 0; i < CameraTargets.Count; i++)
		{
			if (CameraTargets[i].TargetTransform.GetInstanceID() == targetTransform.GetInstanceID())
			{
				return CameraTargets[i];
			}
		}
		return null;
	}

	public void RemoveCameraTarget(Transform targetTransform, float duration = 0f)
	{
		for (int i = 0; i < CameraTargets.Count; i++)
		{
			if (CameraTargets[i].TargetTransform.GetInstanceID() == targetTransform.GetInstanceID())
			{
				if (duration > 0f)
				{
					StartCoroutine(AdjustTargetInfluenceRoutine(CameraTargets[i], 0f, 0f, duration, removeIfZeroInfluence: true));
				}
				else
				{
					CameraTargets.Remove(CameraTargets[i]);
				}
			}
		}
	}

	public void RemoveAllCameraTargets(float duration = 0f)
	{
		if (duration == 0f)
		{
			CameraTargets.Clear();
			return;
		}
		for (int i = 0; i < CameraTargets.Count; i++)
		{
			StartCoroutine(AdjustTargetInfluenceRoutine(CameraTargets[i], 0f, 0f, duration, removeIfZeroInfluence: true));
		}
	}

	public Coroutine AdjustCameraTargetInfluence(CameraTarget cameraTarget, float targetInfluenceH, float targetInfluenceV, float duration = 0f)
	{
		if (duration > 0f)
		{
			return StartCoroutine(AdjustTargetInfluenceRoutine(cameraTarget, targetInfluenceH, targetInfluenceV, duration));
		}
		cameraTarget.TargetInfluenceH = targetInfluenceH;
		cameraTarget.TargetInfluenceV = targetInfluenceV;
		return null;
	}

	public Coroutine AdjustCameraTargetInfluence(Transform cameraTargetTransf, float targetInfluenceH, float targetInfluenceV, float duration = 0f)
	{
		CameraTarget cameraTarget = GetCameraTarget(cameraTargetTransf);
		if (cameraTarget == null)
		{
			return null;
		}
		return AdjustCameraTargetInfluence(cameraTarget, targetInfluenceH, targetInfluenceV, duration);
	}

	public void MoveCameraInstantlyToPosition(Vector2 cameraPos)
	{
		if (_transform == null)
		{
			_transform = base.transform;
		}
		_transform.localPosition = VectorHVD(cameraPos.x, cameraPos.y, Vector3D(_transform.localPosition));
		ResetMovement();
	}

	public void Reset(bool centerOnTargets = true, bool resetSize = true, bool resetExtensions = true)
	{
		if (centerOnTargets)
		{
			CenterOnTargets();
		}
		else
		{
			ResetMovement();
		}
		if (resetSize)
		{
			ResetSize();
		}
		if (resetExtensions)
		{
			ResetExtensions();
		}
	}

	public void ResetMovement()
	{
		_cameraTargetPosition = _transform.localPosition;
		_cameraTargetHorizontalPositionSmoothed = Vector3H(_cameraTargetPosition);
		_cameraTargetVerticalPositionSmoothed = Vector3V(_cameraTargetPosition);
		_previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;
		_previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;
	}

	public void ResetSize()
	{
		SetScreenSize(_startScreenSizeInWorldCoordinates.y / 2f);
	}

	public void ResetExtensions()
	{
		if (OnReset != null)
		{
			OnReset();
		}
	}

	public void CenterOnTargets()
	{
		Vector3 targetsWeightedMidPoint = GetTargetsWeightedMidPoint(ref CameraTargets);
		Vector2 cameraPos = new Vector2(Vector3H(targetsWeightedMidPoint), Vector3V(targetsWeightedMidPoint));
		cameraPos += new Vector2(GetOffsetX(), GetOffsetY());
		MoveCameraInstantlyToPosition(cameraPos);
	}

	public void UpdateScreenSize(float newSize, float duration = 0f, EaseType easeType = EaseType.EaseInOut)
	{
		if (base.enabled)
		{
			if (_updateScreenSizeCoroutine != null)
			{
				StopCoroutine(_updateScreenSizeCoroutine);
			}
			if (duration > 0f)
			{
				_updateScreenSizeCoroutine = StartCoroutine(UpdateScreenSizeRoutine(newSize, duration, easeType));
			}
			else
			{
				SetScreenSize(newSize);
			}
		}
	}

	public void CalculateScreenSize()
	{
		GameCamera.ResetAspect();
		_screenSizeInWorldCoordinates = Utils.GetScreenSizeInWorldCoords(GameCamera, Mathf.Abs(Vector3D(_transform.localPosition)));
		_previousScreenWidth = Screen.width;
		_previousScreenHeight = Screen.height;
	}

	public void Zoom(float zoomAmount, float duration = 0f, EaseType easeType = EaseType.EaseInOut)
	{
		UpdateScreenSize(_screenSizeInWorldCoordinates.y * 0.5f + zoomAmount, duration, easeType);
	}

	public void DollyZoom(float targetFOV, float duration = 1f, EaseType easeType = EaseType.EaseInOut)
	{
		if (!base.enabled)
		{
			return;
		}
		if (GameCamera.orthographic)
		{
			Debug.LogWarning("Dolly zooming is only supported on perspective cameras");
			return;
		}
		if (_dollyZoomRoutine != null)
		{
			StopCoroutine(_dollyZoomRoutine);
		}
		targetFOV = Mathf.Clamp(targetFOV, 0.1f, 179.9f);
		if (duration <= 0f)
		{
			GameCamera.fieldOfView = targetFOV;
			_transform.localPosition = VectorHVD(Vector3H(_transform.localPosition), Vector3V(_transform.localPosition), GetCameraDistanceForFOV(GameCamera.fieldOfView, _screenSizeInWorldCoordinates.y) * _originalCameraDepthSign);
		}
		else
		{
			StartCoroutine(DollyZoomRoutine(targetFOV, duration, easeType));
		}
	}

	public void Move(float deltaTime)
	{
		_previousCameraPosition = _transform.localPosition;
		if (Screen.width != _previousScreenWidth || Screen.height != _previousScreenHeight)
		{
			CalculateScreenSize();
		}
		_deltaTime = deltaTime;
		if (!(_deltaTime < 0.0001f))
		{
			if (PreMoveUpdate != null)
			{
				PreMoveUpdate(_deltaTime);
			}
			for (int i = 0; i < _preMovers.Count; i++)
			{
				_preMovers[i].PreMove(deltaTime);
			}
			_previousTargetsMidPoint = _targetsMidPoint;
			_targetsMidPoint = GetTargetsWeightedMidPoint(ref CameraTargets);
			_cameraTargetPosition = _targetsMidPoint;
			_influencesSum = Utils.GetVectorsSum(_influences);
			_cameraTargetPosition += _influencesSum;
			_influences.Clear();
			float num = (FollowHorizontal ? Vector3H(_cameraTargetPosition) : Vector3H(_transform.localPosition));
			float num2 = (FollowVertical ? Vector3V(_cameraTargetPosition) : Vector3V(_transform.localPosition));
			_cameraTargetPosition = VectorHV(num - Vector3H(_parentPosition), num2 - Vector3V(_parentPosition));
			if (ExclusiveTargetPosition.HasValue)
			{
				_cameraTargetPosition = VectorHV(Vector3H(ExclusiveTargetPosition.Value) - Vector3H(_parentPosition), Vector3V(ExclusiveTargetPosition.Value) - Vector3V(_parentPosition));
				ExclusiveTargetPosition = null;
			}
			_cameraTargetPosition += VectorHV(FollowHorizontal ? GetOffsetX() : 0f, FollowVertical ? GetOffsetY() : 0f);
			_cameraTargetHorizontalPositionSmoothed = Utils.SmoothApproach(_cameraTargetHorizontalPositionSmoothed, _previousCameraTargetHorizontalPositionSmoothed, Vector3H(_cameraTargetPosition), 1f / HorizontalFollowSmoothness, _deltaTime);
			_previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;
			_cameraTargetVerticalPositionSmoothed = Utils.SmoothApproach(_cameraTargetVerticalPositionSmoothed, _previousCameraTargetVerticalPositionSmoothed, Vector3V(_cameraTargetPosition), 1f / VerticalFollowSmoothness, _deltaTime);
			_previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;
			float arg = _cameraTargetHorizontalPositionSmoothed - Vector3H(_transform.localPosition);
			float arg2 = _cameraTargetVerticalPositionSmoothed - Vector3V(_transform.localPosition);
			Vector3 vector = VectorHV(arg, arg2);
			float num3 = 0f;
			for (int j = 0; j < _sizeDeltaChangers.Count; j++)
			{
				num3 = _sizeDeltaChangers[j].AdjustSize(deltaTime, num3);
			}
			float num4 = _screenSizeInWorldCoordinates.y * 0.5f + num3;
			for (int k = 0; k < _sizeOverriders.Count; k++)
			{
				num4 = _sizeOverriders[k].OverrideSize(deltaTime, num4);
			}
			if (num4 != _screenSizeInWorldCoordinates.y * 0.5f)
			{
				SetScreenSize(num4);
			}
			for (int l = 0; l < _positionDeltaChangers.Count; l++)
			{
				vector = _positionDeltaChangers[l].AdjustDelta(deltaTime, vector);
			}
			Vector3 vector2 = LocalPosition + vector;
			for (int m = 0; m < _positionOverriders.Count; m++)
			{
				vector2 = _positionOverriders[m].OverridePosition(deltaTime, vector2);
			}
			_transform.localPosition = VectorHVD(Vector3H(vector2), Vector3V(vector2), Vector3D(_transform.localPosition));
			for (int n = 0; n < _postMovers.Count; n++)
			{
				_postMovers[n].PostMove(deltaTime);
			}
			if (PostMoveUpdate != null)
			{
				PostMoveUpdate(_deltaTime);
			}
		}
	}

	public YieldInstruction GetYield()
	{
		if (UpdateType == UpdateType.FixedUpdate)
		{
			return _waitForFixedUpdate;
		}
		return null;
	}

	private void ResetAxisFunctions()
	{
		switch (Axis)
		{
		case MovementAxis.XY:
			Vector3H = (Vector3 vector) => vector.x;
			Vector3V = (Vector3 vector) => vector.y;
			Vector3D = (Vector3 vector) => vector.z;
			VectorHV = (float h, float v) => new Vector3(h, v, 0f);
			VectorHVD = (float h, float v, float d) => new Vector3(h, v, d);
			break;
		case MovementAxis.XZ:
			Vector3H = (Vector3 vector) => vector.x;
			Vector3V = (Vector3 vector) => vector.z;
			Vector3D = (Vector3 vector) => vector.y;
			VectorHV = (float h, float v) => new Vector3(h, 0f, v);
			VectorHVD = (float h, float v, float d) => new Vector3(h, d, v);
			break;
		case MovementAxis.YZ:
			Vector3H = (Vector3 vector) => vector.z;
			Vector3V = (Vector3 vector) => vector.y;
			Vector3D = (Vector3 vector) => vector.x;
			VectorHV = (float h, float v) => new Vector3(0f, v, h);
			VectorHVD = (float h, float v, float d) => new Vector3(d, v, h);
			break;
		}
	}

	private Vector3 GetTargetsWeightedMidPoint(ref List<CameraTarget> targets)
	{
		float num = 0f;
		float num2 = 0f;
		if (targets.Count == 0)
		{
			return base.transform.localPosition;
		}
		float num3 = 0f;
		float num4 = 0f;
		int num5 = 0;
		int num6 = 0;
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] == null || targets[i].TargetTransform == null)
			{
				targets.RemoveAt(i);
				continue;
			}
			num += (Vector3H(targets[i].TargetPosition) + targets[i].TargetOffset.x) * targets[i].TargetInfluenceH;
			num2 += (Vector3V(targets[i].TargetPosition) + targets[i].TargetOffset.y) * targets[i].TargetInfluenceV;
			num3 += targets[i].TargetInfluenceH;
			num4 += targets[i].TargetInfluenceV;
			if (targets[i].TargetInfluenceH > 0f)
			{
				num5++;
			}
			if (targets[i].TargetInfluenceV > 0f)
			{
				num6++;
			}
		}
		if (num3 < 1f && num5 == 1)
		{
			num3 += 1f - num3;
		}
		if (num4 < 1f && num6 == 1)
		{
			num4 += 1f - num4;
		}
		if (num3 > 0.0001f)
		{
			num /= num3;
		}
		if (num4 > 0.0001f)
		{
			num2 /= num4;
		}
		return VectorHV(num, num2);
	}

	private IEnumerator ApplyInfluencesTimedRoutine(IList<Vector2> influences, float[] durations)
	{
		int count = -1;
		while (count < durations.Length - 1)
		{
			count++;
			float duration = durations[count];
			yield return StartCoroutine(ApplyInfluenceTimedRoutine(influences[count], duration));
		}
	}

	private IEnumerator ApplyInfluenceTimedRoutine(Vector2 influence, float duration)
	{
		while (duration > 0f)
		{
			duration -= _deltaTime;
			ApplyInfluence(influence);
			yield return GetYield();
		}
	}

	public IEnumerator AdjustTargetInfluenceRoutine(CameraTarget cameraTarget, float influenceH, float influenceV, float duration, bool removeIfZeroInfluence = false)
	{
		float startInfluenceH = cameraTarget.TargetInfluenceH;
		float startInfluenceV = cameraTarget.TargetInfluenceV;
		float t = 0f;
		while (t <= 1f)
		{
			t += _deltaTime / duration;
			cameraTarget.TargetInfluenceH = Utils.EaseFromTo(startInfluenceH, influenceH, t, EaseType.Linear);
			cameraTarget.TargetInfluenceV = Utils.EaseFromTo(startInfluenceV, influenceV, t, EaseType.Linear);
			yield return GetYield();
		}
		if (removeIfZeroInfluence && cameraTarget.TargetInfluenceH <= 0f && cameraTarget.TargetInfluenceV <= 0f)
		{
			CameraTargets.Remove(cameraTarget);
		}
	}

	private IEnumerator UpdateScreenSizeRoutine(float finalSize, float duration, EaseType easeType)
	{
		float startSize = _screenSizeInWorldCoordinates.y * 0.5f;
		float t = 0f;
		while (t <= 1f)
		{
			t += _deltaTime / duration;
			float screenSize = Utils.EaseFromTo(startSize, finalSize, t, easeType);
			SetScreenSize(screenSize);
			yield return GetYield();
		}
		_updateScreenSizeCoroutine = null;
	}

	private IEnumerator DollyZoomRoutine(float finalFOV, float duration, EaseType easeType)
	{
		float startFOV = GameCamera.fieldOfView;
		float t = 0f;
		while (t <= 1f)
		{
			t += _deltaTime / duration;
			float num = Utils.EaseFromTo(startFOV, finalFOV, t, easeType);
			GameCamera.fieldOfView = num;
			_transform.localPosition = VectorHVD(Vector3H(_transform.localPosition), Vector3V(_transform.localPosition), GetCameraDistanceForFOV(num, _screenSizeInWorldCoordinates.y) * _originalCameraDepthSign);
			yield return GetYield();
		}
		_dollyZoomRoutine = null;
	}

	private void SetScreenSize(float newSize)
	{
		if (GameCamera.orthographic)
		{
			newSize = Mathf.Max(newSize, 0.1f);
			GameCamera.orthographicSize = newSize;
		}
		else if (ZoomWithFOV)
		{
			float value = 2f * Mathf.Atan(newSize / Mathf.Abs(Vector3D(_transform.localPosition))) * 57.29578f;
			GameCamera.fieldOfView = Mathf.Clamp(value, 0.1f, 179.9f);
		}
		else
		{
			_transform.localPosition = VectorHVD(Vector3H(_transform.localPosition), Vector3V(_transform.localPosition), newSize / Mathf.Tan(GameCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f)) * _originalCameraDepthSign);
		}
		_screenSizeInWorldCoordinates = new Vector2(newSize * 2f * GameCamera.aspect, newSize * 2f);
		if (OnCameraResize != null)
		{
			OnCameraResize(_screenSizeInWorldCoordinates);
		}
	}

	private float GetCameraDistanceForFOV(float fov, float cameraHeight)
	{
		return cameraHeight / (2f * Mathf.Tan(0.5f * fov * ((float)Math.PI / 180f)));
	}

	private float GetOffsetX()
	{
		if (!IsRelativeOffset)
		{
			return OffsetX;
		}
		return OffsetX * _screenSizeInWorldCoordinates.x * 0.5f;
	}

	private float GetOffsetY()
	{
		if (!IsRelativeOffset)
		{
			return OffsetY;
		}
		return OffsetY * _screenSizeInWorldCoordinates.y * 0.5f;
	}

	public void AddPreMover(IPreMover mover)
	{
		_preMovers.Add(mover);
	}

	public void RemovePreMover(IPreMover mover)
	{
		_preMovers.Remove(mover);
	}

	public void SortPreMovers()
	{
		_preMovers = _preMovers.OrderBy((IPreMover a) => a.PrMOrder).ToList();
	}

	public void AddPositionDeltaChanger(IPositionDeltaChanger changer)
	{
		_positionDeltaChangers.Add(changer);
	}

	public void RemovePositionDeltaChanger(IPositionDeltaChanger changer)
	{
		_positionDeltaChangers.Remove(changer);
	}

	public void SortPositionDeltaChangers()
	{
		_positionDeltaChangers = _positionDeltaChangers.OrderBy((IPositionDeltaChanger a) => a.PDCOrder).ToList();
	}

	public void AddPositionOverrider(IPositionOverrider overrider)
	{
		_positionOverriders.Add(overrider);
	}

	public void RemovePositionOverrider(IPositionOverrider overrider)
	{
		_positionOverriders.Remove(overrider);
	}

	public void SortPositionOverriders()
	{
		_positionOverriders = _positionOverriders.OrderBy((IPositionOverrider a) => a.POOrder).ToList();
	}

	public void AddSizeDeltaChanger(ISizeDeltaChanger changer)
	{
		_sizeDeltaChangers.Add(changer);
	}

	public void RemoveSizeDeltaChanger(ISizeDeltaChanger changer)
	{
		_sizeDeltaChangers.Remove(changer);
	}

	public void SortSizeDeltaChangers()
	{
		_sizeDeltaChangers = _sizeDeltaChangers.OrderBy((ISizeDeltaChanger a) => a.SDCOrder).ToList();
	}

	public void AddSizeOverrider(ISizeOverrider overrider)
	{
		_sizeOverriders.Add(overrider);
	}

	public void RemoveSizeOverrider(ISizeOverrider overrider)
	{
		_sizeOverriders.Remove(overrider);
	}

	public void SortSizeOverriders()
	{
		_sizeOverriders = _sizeOverriders.OrderBy((ISizeOverrider a) => a.SOOrder).ToList();
	}

	public void AddPostMover(IPostMover mover)
	{
		_postMovers.Add(mover);
	}

	public void RemovePostMover(IPostMover mover)
	{
		_postMovers.Remove(mover);
	}

	public void SortPostMovers()
	{
		_postMovers = _postMovers.OrderBy((IPostMover a) => a.PMOrder).ToList();
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		ResetAxisFunctions();
	}

	public static void SetInstance(ProCamera2D ins)
	{
		ins.Awake();
	}
}
