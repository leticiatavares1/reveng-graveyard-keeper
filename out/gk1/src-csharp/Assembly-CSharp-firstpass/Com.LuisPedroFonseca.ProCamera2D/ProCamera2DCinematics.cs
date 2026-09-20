using System.Collections;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;
using UnityEngine.Events;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-cinematics/")]
public class ProCamera2DCinematics : BasePC2D, IPositionOverrider, ISizeOverrider
{
	public static string ExtensionName = "Cinematics";

	public UnityEvent OnCinematicStarted;

	public CinematicEvent OnCinematicTargetReached;

	public UnityEvent OnCinematicFinished;

	private bool _isPlaying;

	public List<CinematicTarget> CinematicTargets = new List<CinematicTarget>();

	public float EndDuration = 1f;

	public EaseType EndEaseType = EaseType.EaseOut;

	public bool UseNumericBoundaries;

	public bool UseLetterbox = true;

	[Range(0f, 0.5f)]
	public float LetterboxAmount = 0.1f;

	public float LetterboxAnimDuration = 1f;

	public Color LetterboxColor = Color.black;

	private float _initialCameraSize;

	private ProCamera2DNumericBoundaries _numericBoundaries;

	private ProCamera2DLetterbox _letterbox;

	private Coroutine _startCinematicRoutine;

	private Coroutine _goToCinematicRoutine;

	private Coroutine _endCinematicRoutine;

	private bool _skipTarget;

	private Vector3 _newPos;

	private Vector3 _originalPos;

	private Vector3 _startPos;

	private float _newSize;

	private bool _paused;

	private int _poOrder;

	private int _soOrder = 3000;

	public bool IsPlaying => _isPlaying;

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
		if (UseLetterbox)
		{
			SetupLetterbox();
		}
		ProCamera2D.AddPositionOverrider(this);
		ProCamera2D.AddSizeOverrider(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ProCamera2D.RemovePositionOverrider(this);
		ProCamera2D.RemoveSizeOverrider(this);
	}

	public Vector3 OverridePosition(float deltaTime, Vector3 originalPosition)
	{
		if (!base.enabled)
		{
			return originalPosition;
		}
		_originalPos = originalPosition;
		if (_isPlaying)
		{
			return _newPos;
		}
		return originalPosition;
	}

	public float OverrideSize(float deltaTime, float originalSize)
	{
		if (!base.enabled)
		{
			return originalSize;
		}
		if (_isPlaying)
		{
			return _newSize;
		}
		return originalSize;
	}

	public void Play()
	{
		if (_isPlaying)
		{
			return;
		}
		_paused = false;
		if (CinematicTargets.Count == 0)
		{
			Debug.LogWarning("No cinematic targets added to the list");
			return;
		}
		_initialCameraSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
		if (UseNumericBoundaries && _numericBoundaries == null)
		{
			_numericBoundaries = ProCamera2D.GetComponentInChildren<ProCamera2DNumericBoundaries>();
		}
		if (_numericBoundaries == null)
		{
			UseNumericBoundaries = false;
		}
		_isPlaying = true;
		if (_endCinematicRoutine != null)
		{
			StopCoroutine(_endCinematicRoutine);
			_endCinematicRoutine = null;
		}
		if (_startCinematicRoutine == null)
		{
			_startCinematicRoutine = StartCoroutine(StartCinematicRoutine());
		}
	}

	public void Stop()
	{
		if (_isPlaying)
		{
			if (_startCinematicRoutine != null)
			{
				StopCoroutine(_startCinematicRoutine);
				_startCinematicRoutine = null;
			}
			if (_goToCinematicRoutine != null)
			{
				StopCoroutine(_goToCinematicRoutine);
				_goToCinematicRoutine = null;
			}
			if (_endCinematicRoutine == null)
			{
				_endCinematicRoutine = StartCoroutine(EndCinematicRoutine());
			}
		}
	}

	public void Toggle()
	{
		if (_isPlaying)
		{
			Stop();
		}
		else
		{
			Play();
		}
	}

	public void GoToNextTarget()
	{
		_skipTarget = true;
	}

	public void Pause()
	{
		_paused = true;
	}

	public void Unpause()
	{
		_paused = false;
	}

	public void AddCinematicTarget(Transform targetTransform, float easeInDuration = 1f, float holdDuration = 1f, float zoom = 1f, EaseType easeType = EaseType.EaseOut, string sendMessageName = "", string sendMessageParam = "", int index = -1)
	{
		CinematicTarget item = new CinematicTarget
		{
			TargetTransform = targetTransform,
			EaseInDuration = easeInDuration,
			HoldDuration = holdDuration,
			Zoom = zoom,
			EaseType = easeType,
			SendMessageName = sendMessageName,
			SendMessageParam = sendMessageParam
		};
		if (index == -1 || index > CinematicTargets.Count)
		{
			CinematicTargets.Add(item);
		}
		else
		{
			CinematicTargets.Insert(index, item);
		}
	}

	public void RemoveCinematicTarget(Transform targetTransform)
	{
		for (int i = 0; i < CinematicTargets.Count; i++)
		{
			if (CinematicTargets[i].TargetTransform.GetInstanceID() == targetTransform.GetInstanceID())
			{
				CinematicTargets.Remove(CinematicTargets[i]);
			}
		}
	}

	private IEnumerator StartCinematicRoutine()
	{
		if (OnCinematicStarted != null)
		{
			OnCinematicStarted.Invoke();
		}
		_startPos = ProCamera2D.LocalPosition;
		_newPos = ProCamera2D.LocalPosition;
		_newSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
		if (UseLetterbox)
		{
			LetterboxEffect(show: true);
		}
		int count = -1;
		while (count < CinematicTargets.Count - 1)
		{
			count++;
			_skipTarget = false;
			_goToCinematicRoutine = StartCoroutine(GoToCinematicTargetRoutine(CinematicTargets[count], count));
			yield return _goToCinematicRoutine;
		}
		Stop();
	}

	public void LetterboxEffect(bool show)
	{
		if (_letterbox == null)
		{
			SetupLetterbox();
		}
		if (show)
		{
			_letterbox.Color = LetterboxColor;
			_letterbox.TweenTo(LetterboxAmount, LetterboxAnimDuration);
		}
		else if (_letterbox != null && LetterboxAmount > 0f)
		{
			_letterbox.TweenTo(0f, LetterboxAnimDuration);
		}
	}

	private IEnumerator GoToCinematicTargetRoutine(CinematicTarget cinematicTarget, int targetIndex)
	{
		if (cinematicTarget.TargetTransform == null)
		{
			yield break;
		}
		float initialPosH = Vector3H(ProCamera2D.LocalPosition);
		float initialPosV = Vector3V(ProCamera2D.LocalPosition);
		float currentCameraSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
		float t2 = 0f;
		if (cinematicTarget.EaseInDuration > 0f)
		{
			while (t2 <= 1f)
			{
				if (!_paused)
				{
					t2 += ProCamera2D.DeltaTime / cinematicTarget.EaseInDuration;
					float horizontalPos = Utils.EaseFromTo(initialPosH, Vector3H(cinematicTarget.TargetTransform.position) - Vector3H(ProCamera2D.ParentPosition), t2, cinematicTarget.EaseType);
					float verticalPos = Utils.EaseFromTo(initialPosV, Vector3V(cinematicTarget.TargetTransform.position) - Vector3V(ProCamera2D.ParentPosition), t2, cinematicTarget.EaseType);
					if (UseNumericBoundaries)
					{
						LimitToNumericBoundaries(ref horizontalPos, ref verticalPos);
					}
					_newPos = VectorHVD(horizontalPos, verticalPos, 0f);
					_newSize = Utils.EaseFromTo(currentCameraSize, _initialCameraSize / cinematicTarget.Zoom, t2, cinematicTarget.EaseType);
					if (_skipTarget)
					{
						yield break;
					}
				}
				yield return ProCamera2D.GetYield();
			}
		}
		else
		{
			float arg = Vector3H(cinematicTarget.TargetTransform.position) - Vector3H(ProCamera2D.ParentPosition);
			float arg2 = Vector3V(cinematicTarget.TargetTransform.position) - Vector3V(ProCamera2D.ParentPosition);
			_newPos = VectorHVD(arg, arg2, 0f);
			_newSize = _initialCameraSize / cinematicTarget.Zoom;
		}
		if (OnCinematicTargetReached != null)
		{
			OnCinematicTargetReached.Invoke(targetIndex);
		}
		if (!string.IsNullOrEmpty(cinematicTarget.SendMessageName))
		{
			cinematicTarget.TargetTransform.SendMessage(cinematicTarget.SendMessageName, cinematicTarget.SendMessageParam, SendMessageOptions.DontRequireReceiver);
		}
		t2 = 0f;
		while (cinematicTarget.HoldDuration < 0f || t2 <= cinematicTarget.HoldDuration)
		{
			if (!_paused)
			{
				t2 += ProCamera2D.DeltaTime;
				float horizontalPos2 = Vector3H(cinematicTarget.TargetTransform.position) - Vector3H(ProCamera2D.ParentPosition);
				float verticalPos2 = Vector3V(cinematicTarget.TargetTransform.position) - Vector3V(ProCamera2D.ParentPosition);
				if (UseNumericBoundaries)
				{
					LimitToNumericBoundaries(ref horizontalPos2, ref verticalPos2);
				}
				_newPos = VectorHVD(horizontalPos2, verticalPos2, 0f);
				if (_skipTarget)
				{
					break;
				}
			}
			yield return ProCamera2D.GetYield();
		}
	}

	private IEnumerator EndCinematicRoutine()
	{
		LetterboxEffect(show: false);
		float initialPosH = Vector3H(_newPos);
		float initialPosV = Vector3V(_newPos);
		float currentCameraSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
		float t = 0f;
		while (t <= 1f)
		{
			if (!_paused)
			{
				t += ProCamera2D.DeltaTime / EndDuration;
				float horizontalPos;
				float verticalPos;
				if (ProCamera2D.CameraTargets.Count > 0)
				{
					horizontalPos = Utils.EaseFromTo(initialPosH, Vector3H(_originalPos), t, EndEaseType);
					verticalPos = Utils.EaseFromTo(initialPosV, Vector3V(_originalPos), t, EndEaseType);
				}
				else
				{
					horizontalPos = Utils.EaseFromTo(initialPosH, Vector3H(_startPos), t, EndEaseType);
					verticalPos = Utils.EaseFromTo(initialPosV, Vector3V(_startPos), t, EndEaseType);
				}
				if (UseNumericBoundaries)
				{
					LimitToNumericBoundaries(ref horizontalPos, ref verticalPos);
				}
				_newPos = VectorHVD(horizontalPos, verticalPos, 0f);
				_newSize = Utils.EaseFromTo(currentCameraSize, _initialCameraSize, t, EndEaseType);
			}
			yield return ProCamera2D.GetYield();
		}
		_isPlaying = false;
		if (ProCamera2D.CameraTargets.Count == 0)
		{
			ProCamera2D.Reset();
		}
		if (OnCinematicFinished != null)
		{
			OnCinematicFinished.Invoke();
		}
	}

	private void SetupLetterbox()
	{
		ProCamera2DLetterbox componentInChildren = ProCamera2D.gameObject.GetComponentInChildren<ProCamera2DLetterbox>();
		if (componentInChildren == null)
		{
			(from c in ProCamera2D.gameObject.GetComponentsInChildren<Camera>()
				orderby c.depth descending
				select c).ToArray()[0].gameObject.AddComponent<ProCamera2DLetterbox>();
		}
		_letterbox = componentInChildren;
	}

	private void LimitToNumericBoundaries(ref float horizontalPos, ref float verticalPos)
	{
		if (_numericBoundaries.UseLeftBoundary && horizontalPos - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f < _numericBoundaries.LeftBoundary)
		{
			horizontalPos = _numericBoundaries.LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f;
		}
		else if (_numericBoundaries.UseRightBoundary && horizontalPos + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f > _numericBoundaries.RightBoundary)
		{
			horizontalPos = _numericBoundaries.RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f;
		}
		if (_numericBoundaries.UseBottomBoundary && verticalPos - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f < _numericBoundaries.BottomBoundary)
		{
			verticalPos = _numericBoundaries.BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f;
		}
		else if (_numericBoundaries.UseTopBoundary && verticalPos + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f > _numericBoundaries.TopBoundary)
		{
			verticalPos = _numericBoundaries.TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f;
		}
	}
}
