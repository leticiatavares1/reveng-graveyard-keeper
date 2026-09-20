using Cinemachine;
using UnityEngine;

public class CinemachineCustomConfiner : CinemachineExtension
{
	[SerializeField]
	private CinemachineCore.Stage applyOnStage;

	[SerializeField]
	private Collider boundingCollider;

	public bool IsValid
	{
		get
		{
			if (boundingCollider != null && boundingCollider.enabled)
			{
				return boundingCollider.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	public Collider BoundingCollider
	{
		set
		{
			boundingCollider = value;
		}
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (IsValid && stage == applyOnStage)
		{
			Vector3 vector = ConfineScreenEdges(ref state);
			state.PositionCorrection += vector;
		}
	}

	private Vector3 ConfineScreenEdges(ref CameraState state)
	{
		Quaternion correctedOrientation = state.CorrectedOrientation;
		float orthographicSize = state.Lens.OrthographicSize;
		float num = orthographicSize * state.Lens.Aspect;
		Vector3 vector = correctedOrientation * Vector3.right * num;
		Vector3 vector2 = correctedOrientation * Vector3.up * orthographicSize;
		Vector3 zero = Vector3.zero;
		Vector3 correctedPosition = state.CorrectedPosition;
		Vector3 lineDir = correctedOrientation * Vector3.forward;
		Bounds lowFaceBoundsFromBoxCollider = GetLowFaceBoundsFromBoxCollider();
		bool flag = false;
		bool flag2 = false;
		Vector3 posOnYBoundsPlane = GetPosOnYBoundsPlane(correctedPosition, lineDir, lowFaceBoundsFromBoxCollider);
		if (lowFaceBoundsFromBoxCollider.size.x <= num * 2f)
		{
			flag = true;
			zero += Vector3.right * (lowFaceBoundsFromBoxCollider.center.x - posOnYBoundsPlane.x);
		}
		if (lowFaceBoundsFromBoxCollider.size.z / 1.25f <= orthographicSize * 2f)
		{
			flag2 = true;
			zero += Vector3.forward * (lowFaceBoundsFromBoxCollider.center.z - posOnYBoundsPlane.z);
		}
		Vector3 vector3 = correctedPosition + vector2 + vector;
		Vector3 vector4 = correctedPosition - vector2 - vector;
		Vector3 vector5 = Clamp(vector3, lineDir, lowFaceBoundsFromBoxCollider);
		Vector3 vector6 = Clamp(vector4, lineDir, lowFaceBoundsFromBoxCollider);
		Vector3 a = vector5 - vector3;
		Vector3 a2 = vector6 - vector4;
		float x = (flag ? 0f : 1f);
		float z = (flag2 ? 0f : 1f);
		if (a.magnitude > 0.0001f)
		{
			zero += Vector3.Scale(a, new Vector3(x, 1f, z));
		}
		if (a2.magnitude > 0.0001f)
		{
			zero += Vector3.Scale(a2, new Vector3(x, 1f, z));
		}
		return zero;
	}

	private Vector3 Clamp(Vector3 linePos, Vector3 lineDir, Bounds yBounds)
	{
		float num = (yBounds.center.y - linePos.y) / lineDir.y;
		Vector3 point = new Vector3(linePos.x + num * lineDir.x, yBounds.center.y, linePos.z + num * lineDir.z);
		if (!yBounds.Contains(point))
		{
			return yBounds.ClosestPoint(point) - num * lineDir;
		}
		return linePos;
	}

	private Vector3 GetPosOnYBoundsPlane(Vector3 linePos, Vector3 lineDir, Bounds yBounds)
	{
		float num = (yBounds.center.y - linePos.y) / lineDir.y;
		return new Vector3(linePos.x + num * lineDir.x, yBounds.center.y, linePos.z + num * lineDir.z);
	}

	private Bounds GetLowFaceBoundsFromBoxCollider()
	{
		Bounds result = default(Bounds);
		Bounds bounds = boundingCollider.bounds;
		result.center = new Vector3(bounds.center.x, bounds.center.y - bounds.size.y / 2f, bounds.center.z);
		result.size = new Vector3(bounds.size.x, 0.01f, bounds.size.z);
		return result;
	}
}
