using System;
using System.Collections;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/trigger-boundaries/")]
public class ProCamera2DTriggerBoundaries : BaseTrigger, IPositionOverrider
{
	public static string TriggerName = "Boundaries Trigger";

	public ProCamera2DNumericBoundaries NumericBoundaries;

	public bool AreBoundariesRelative = true;

	public bool UseTopBoundary = true;

	public float TopBoundary = 10f;

	public bool UseBottomBoundary = true;

	public float BottomBoundary = -10f;

	public bool UseLeftBoundary = true;

	public float LeftBoundary = -10f;

	public bool UseRightBoundary = true;

	public float RightBoundary = 10f;

	public float TransitionDuration = 1f;

	public EaseType TransitionEaseType;

	public bool ChangeZoom;

	public float TargetZoom = 1.5f;

	public float ZoomSmoothness = 1f;

	public bool _setAsStartingBoundaries;

	private float _initialCamSize;

	private BoundariesAnimator _boundsAnim;

	private float _targetTopBoundary;

	private float _targetBottomBoundary;

	private float _targetLeftBoundary;

	private float _targetRightBoundary;

	private bool _transitioning;

	private Vector3 _newPos;

	private int _poOrder = 1000;

	public bool IsCurrentTrigger => NumericBoundaries.CurrentBoundariesTrigger._instanceID == _instanceID;

	public bool SetAsStartingBoundaries
	{
		get
		{
			return _setAsStartingBoundaries;
		}
		set
		{
			if (value && !_setAsStartingBoundaries)
			{
				UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(ProCamera2DTriggerBoundaries));
				for (int i = 0; i < array.Length; i++)
				{
					((ProCamera2DTriggerBoundaries)array[i]).SetAsStartingBoundaries = false;
				}
			}
			_setAsStartingBoundaries = value;
		}
	}

	public int POOrder
	{
		get
		{
			return _poOrder;
		}
		set
		{
			_poOrder = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		ProCamera2D.Instance.AddPositionOverrider(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ProCamera2D.RemovePositionOverrider(this);
	}

	private void Start()
	{
		if (ProCamera2D == null)
		{
			return;
		}
		if (NumericBoundaries == null)
		{
			ProCamera2DNumericBoundaries proCamera2DNumericBoundaries = UnityEngine.Object.FindObjectOfType<ProCamera2DNumericBoundaries>();
			NumericBoundaries = ((proCamera2DNumericBoundaries == null) ? ProCamera2D.gameObject.AddComponent<ProCamera2DNumericBoundaries>() : proCamera2DNumericBoundaries);
		}
		_boundsAnim = new BoundariesAnimator(ProCamera2D, NumericBoundaries);
		BoundariesAnimator boundsAnim = _boundsAnim;
		boundsAnim.OnTransitionStarted = (Action)Delegate.Combine(boundsAnim.OnTransitionStarted, (Action)delegate
		{
			if (NumericBoundaries.OnBoundariesTransitionStarted != null)
			{
				NumericBoundaries.OnBoundariesTransitionStarted();
			}
		});
		BoundariesAnimator boundsAnim2 = _boundsAnim;
		boundsAnim2.OnTransitionFinished = (Action)Delegate.Combine(boundsAnim2.OnTransitionFinished, (Action)delegate
		{
			if (NumericBoundaries.OnBoundariesTransitionFinished != null)
			{
				NumericBoundaries.OnBoundariesTransitionFinished();
			}
		});
		GetTargetBoundaries();
		if (SetAsStartingBoundaries)
		{
			SetBoundaries();
		}
		_initialCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
	}

	public Vector3 OverridePosition(float deltaTime, Vector3 originalPosition)
	{
		if (!base.enabled)
		{
			return originalPosition;
		}
		if (_transitioning)
		{
			return _newPos;
		}
		return originalPosition;
	}

	protected override void EnteredTrigger()
	{
		base.EnteredTrigger();
		if (NumericBoundaries.CurrentBoundariesTrigger != null)
		{
			StartCoroutine(TurnOffPreviousTrigger(NumericBoundaries.CurrentBoundariesTrigger));
		}
		if ((NumericBoundaries.CurrentBoundariesTrigger != null && NumericBoundaries.CurrentBoundariesTrigger._instanceID != _instanceID) || NumericBoundaries.CurrentBoundariesTrigger == null)
		{
			NumericBoundaries.CurrentBoundariesTrigger = this;
			StartCoroutine(Transition());
		}
	}

	protected override void ExitedTrigger()
	{
		base.ExitedTrigger();
		if (NumericBoundaries.CurrentBoundariesTrigger == this)
		{
			StartCoroutine(ExitTriggerRoutine());
		}
	}

	private IEnumerator TurnOffPreviousTrigger(ProCamera2DTriggerBoundaries trigger)
	{
		yield return new WaitForEndOfFrame();
		trigger._transitioning = false;
	}

	private IEnumerator ExitTriggerRoutine()
	{
		yield return new WaitForEndOfFrame();
		if (!(NumericBoundaries.CurrentBoundariesTrigger != this))
		{
			_transitioning = false;
			BoundariesAnimator boundsAnim = _boundsAnim;
			bool useLeftBoundary = false;
			boundsAnim.UseRightBoundary = false;
			boundsAnim.UseTopBoundary = (boundsAnim.UseBottomBoundary = (boundsAnim.UseLeftBoundary = useLeftBoundary));
			boundsAnim.TransitionDuration = 1f;
			boundsAnim.TransitionEaseType = EaseType.EaseInOut;
			boundsAnim.Transition();
			NumericBoundaries.CurrentBoundariesTrigger = null;
		}
	}

	public void SetBoundaries()
	{
		if (NumericBoundaries != null)
		{
			NumericBoundaries.CurrentBoundariesTrigger = this;
			NumericBoundaries.UseLeftBoundary = UseLeftBoundary;
			if (UseLeftBoundary)
			{
				NumericBoundaries.LeftBoundary = (NumericBoundaries.TargetLeftBoundary = _targetLeftBoundary);
			}
			NumericBoundaries.UseRightBoundary = UseRightBoundary;
			if (UseRightBoundary)
			{
				NumericBoundaries.RightBoundary = (NumericBoundaries.TargetRightBoundary = _targetRightBoundary);
			}
			NumericBoundaries.UseTopBoundary = UseTopBoundary;
			if (UseTopBoundary)
			{
				NumericBoundaries.TopBoundary = (NumericBoundaries.TargetTopBoundary = _targetTopBoundary);
			}
			NumericBoundaries.UseBottomBoundary = UseBottomBoundary;
			if (UseBottomBoundary)
			{
				NumericBoundaries.BottomBoundary = (NumericBoundaries.TargetBottomBoundary = _targetBottomBoundary);
			}
			if (!UseTopBoundary && !UseBottomBoundary && !UseLeftBoundary && !UseRightBoundary)
			{
				NumericBoundaries.UseNumericBoundaries = false;
			}
			else
			{
				NumericBoundaries.UseNumericBoundaries = true;
			}
		}
	}

	private void GetTargetBoundaries()
	{
		if (AreBoundariesRelative)
		{
			_targetTopBoundary = Vector3V(base.transform.position) + TopBoundary;
			_targetBottomBoundary = Vector3V(base.transform.position) + BottomBoundary;
			_targetLeftBoundary = Vector3H(base.transform.position) + LeftBoundary;
			_targetRightBoundary = Vector3H(base.transform.position) + RightBoundary;
		}
		else
		{
			_targetTopBoundary = TopBoundary;
			_targetBottomBoundary = BottomBoundary;
			_targetLeftBoundary = LeftBoundary;
			_targetRightBoundary = RightBoundary;
		}
	}

	private IEnumerator Transition()
	{
		if (!UseTopBoundary && !UseBottomBoundary && !UseLeftBoundary && !UseRightBoundary)
		{
			NumericBoundaries.UseNumericBoundaries = false;
			yield break;
		}
		bool flag = true;
		if (UseTopBoundary && (NumericBoundaries.TopBoundary != TopBoundary || !NumericBoundaries.UseTopBoundary))
		{
			flag = false;
		}
		if (UseBottomBoundary && (NumericBoundaries.BottomBoundary != BottomBoundary || !NumericBoundaries.UseBottomBoundary))
		{
			flag = false;
		}
		if (UseLeftBoundary && (NumericBoundaries.LeftBoundary != LeftBoundary || !NumericBoundaries.UseLeftBoundary))
		{
			flag = false;
		}
		if (UseRightBoundary && (NumericBoundaries.RightBoundary != RightBoundary || !NumericBoundaries.UseRightBoundary))
		{
			flag = false;
		}
		if (flag)
		{
			yield break;
		}
		NumericBoundaries.UseNumericBoundaries = true;
		GetTargetBoundaries();
		_boundsAnim.UseTopBoundary = UseTopBoundary;
		_boundsAnim.TopBoundary = _targetTopBoundary;
		_boundsAnim.UseBottomBoundary = UseBottomBoundary;
		_boundsAnim.BottomBoundary = _targetBottomBoundary;
		_boundsAnim.UseLeftBoundary = UseLeftBoundary;
		_boundsAnim.LeftBoundary = _targetLeftBoundary;
		_boundsAnim.UseRightBoundary = UseRightBoundary;
		_boundsAnim.RightBoundary = _targetRightBoundary;
		_boundsAnim.TransitionDuration = TransitionDuration;
		_boundsAnim.TransitionEaseType = TransitionEaseType;
		if (ChangeZoom && _initialCamSize / TargetZoom != ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f)
		{
			ProCamera2D.UpdateScreenSize(_initialCamSize / TargetZoom, ZoomSmoothness, TransitionEaseType);
		}
		if (_boundsAnim.GetAnimsCount() > 1)
		{
			if (NumericBoundaries.MoveCameraToTargetRoutine != null)
			{
				NumericBoundaries.StopCoroutine(NumericBoundaries.MoveCameraToTargetRoutine);
			}
			NumericBoundaries.MoveCameraToTargetRoutine = NumericBoundaries.StartCoroutine(MoveCameraToTarget());
		}
		yield return new WaitForEndOfFrame();
		_boundsAnim.Transition();
	}

	private IEnumerator MoveCameraToTarget()
	{
		float initialCamPosH = Vector3H(ProCamera2D.LocalPosition);
		float initialCamPosV = Vector3V(ProCamera2D.LocalPosition);
		_newPos = VectorHVD(initialCamPosH, initialCamPosV, 0f);
		_transitioning = true;
		float t = 0f;
		while (t <= 1f)
		{
			t += ProCamera2D.DeltaTime / TransitionDuration;
			float horizontalPos = Utils.EaseFromTo(initialCamPosH, ProCamera2D.CameraTargetPositionSmoothed.x, t, TransitionEaseType);
			float verticalPos = Utils.EaseFromTo(initialCamPosV, ProCamera2D.CameraTargetPositionSmoothed.y, t, TransitionEaseType);
			LimitToNumericBoundaries(ref horizontalPos, ref verticalPos);
			_newPos = VectorHVD(horizontalPos, verticalPos, 0f);
			yield return ProCamera2D.GetYield();
		}
		NumericBoundaries.MoveCameraToTargetRoutine = null;
		_transitioning = false;
	}

	private void LimitToNumericBoundaries(ref float horizontalPos, ref float verticalPos)
	{
		if (NumericBoundaries.UseLeftBoundary && horizontalPos - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f < NumericBoundaries.LeftBoundary)
		{
			horizontalPos = NumericBoundaries.LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f;
		}
		else if (NumericBoundaries.UseRightBoundary && horizontalPos + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f > NumericBoundaries.RightBoundary)
		{
			horizontalPos = NumericBoundaries.RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f;
		}
		if (NumericBoundaries.UseBottomBoundary && verticalPos - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f < NumericBoundaries.BottomBoundary)
		{
			verticalPos = NumericBoundaries.BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f;
		}
		else if (NumericBoundaries.UseTopBoundary && verticalPos + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f > NumericBoundaries.TopBoundary)
		{
			verticalPos = NumericBoundaries.TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f;
		}
	}
}
