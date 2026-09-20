using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RiverBodyReceiver : MonoBehaviour
{
	[SerializeField]
	private SplineContainer splineContainer;

	[SerializeField]
	private AnimationCurve throwHeightCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	[SerializeField]
	[Min(0.01f)]
	private float throwDuration = 0.45f;

	[SerializeField]
	[Min(0.01f)]
	private float flowSpeed = 1.5f;

	[SerializeField]
	[Min(0.01f)]
	private float fallSpeed = 4f;

	[SerializeField]
	[Min(0f)]
	private float bubbleHeight = 1f;

	private float[] cachedSplineWorldLengths;

	private Wgo wgo;

	public Wgo Wgo => wgo;

	public SplineContainer SplineContainer => splineContainer;

	public AnimationCurve ThrowHeightCurve => throwHeightCurve;

	public float ThrowDuration => throwDuration;

	public float FlowSpeed => flowSpeed;

	public float FallSpeed => fallSpeed;

	public bool UseDynamicBubble { get; private set; }

	public int SplineCount
	{
		get
		{
			if (!(splineContainer != null))
			{
				return 0;
			}
			return splineContainer.Splines.Count;
		}
	}

	public bool HasFlowSpline
	{
		get
		{
			if (SplineCount == 0)
			{
				return false;
			}
			Spline spline = splineContainer.Splines[0];
			if (spline != null)
			{
				return spline.Count >= 2;
			}
			return false;
		}
	}

	public void Init(Wgo owner)
	{
		if (!(owner == null))
		{
			wgo = owner;
			RecacheSplineLengths();
			MainGame.Instance?.riverDropSystem?.HandleReceiverReady(this);
		}
	}

	public void DeInit()
	{
		UseDynamicBubble = false;
		wgo = null;
	}

	public void SetDynamicBubbleEnabled(bool enabled)
	{
		UseDynamicBubble = enabled;
	}

	public float[] CopySplineWorldLengths()
	{
		RecacheSplineLengths();
		if (cachedSplineWorldLengths == null || cachedSplineWorldLengths.Length == 0)
		{
			return Array.Empty<float>();
		}
		float[] array = new float[cachedSplineWorldLengths.Length];
		Array.Copy(cachedSplineWorldLengths, array, array.Length);
		return array;
	}

	public bool GetNearestFlowPoint(Vector3 worldPos, out Vector3 worldPoint, out float t)
	{
		worldPoint = worldPos;
		t = 0f;
		if (!HasFlowSpline)
		{
			return false;
		}
		Spline spline = splineContainer.Splines[0];
		if (spline == null || spline.Count < 2)
		{
			return false;
		}
		float3 point = splineContainer.transform.InverseTransformPoint(worldPos);
		SplineUtility.GetNearestPoint(spline, point, out var nearest, out t);
		worldPoint = splineContainer.transform.TransformPoint(nearest);
		return true;
	}

	public bool TryEvaluatePose(int splineIndex, float t, out Vector3 worldPos)
	{
		worldPos = default(Vector3);
		if (splineContainer == null || splineIndex < 0 || splineIndex >= splineContainer.Splines.Count)
		{
			return false;
		}
		worldPos = splineContainer.EvaluatePosition(splineIndex, Mathf.Clamp01(t));
		return true;
	}

	public Vector3 GetDynamicBubblePos(Vector3 playerWorldPos)
	{
		return playerWorldPos + Vector3.up * bubbleHeight;
	}

	private void RecacheSplineLengths()
	{
		if (splineContainer == null || splineContainer.Splines == null)
		{
			cachedSplineWorldLengths = Array.Empty<float>();
			return;
		}
		int count = splineContainer.Splines.Count;
		cachedSplineWorldLengths = new float[count];
		for (int i = 0; i < count; i++)
		{
			cachedSplineWorldLengths[i] = splineContainer.CalculateLength(i);
		}
	}
}
