using Cinemachine;
using UnityEngine;

public class CinemachineCorrectPixelExtension : CinemachineExtension
{
	private const float PLANE_INTERSECTION_DIR_EPSILON = 1E-05f;

	[SerializeField]
	private CinemachineCore.Stage applyCorrectionOnStage;

	[Space]
	[SerializeField]
	private bool useSmoothing;

	[SerializeField]
	private bool applyMaxDistanceConstraint;

	[SerializeField]
	private float maxDistance = 2f;

	[SerializeField]
	private float smoothingFactor = 10f;

	[SerializeField]
	private bool smoothWorldInsteadOfCharacter;

	private bool isActiveExtension = true;

	private Vector3 smoothedOffset;

	private Vector3 smoothedWorldPosition;

	private Transform cachedFollowTarget;

	private bool cachedWasActiveCamera;

	private CameraController cameraController;

	private CameraController CameraController
	{
		get
		{
			if (!cameraController)
			{
				cameraController = GetComponent<CameraController>();
			}
			return cameraController;
		}
	}

	public bool SmoothWorldInsteadOfCharacter
	{
		get
		{
			return smoothWorldInsteadOfCharacter;
		}
		set
		{
			smoothWorldInsteadOfCharacter = value;
		}
	}

	public float SmoothingFactor
	{
		get
		{
			return smoothingFactor;
		}
		set
		{
			smoothingFactor = value;
		}
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (!isActiveExtension || stage != applyCorrectionOnStage || !(base.VirtualCamera.Follow != null))
		{
			return;
		}
		vcam.MoveToTopOfPrioritySubqueue();
		bool flag = CameraSystem.Instance.ActiveCameraController == CameraController;
		Vector3 camDir = state.CorrectedOrientation * Vector3.forward;
		if (!useSmoothing)
		{
			Vector3 posOnYPlaneProjected = GetPosOnYPlaneProjected(state.CorrectedPosition, camDir, base.VirtualCamera.Follow.transform.position.y);
			Vector3 roundedCameraPosition = CameraController.GetRoundedCameraPosition(posOnYPlaneProjected, CameraSystem.Instance.ResolutionPiexelSize, flag);
			state.PositionCorrection += roundedCameraPosition - posOnYPlaneProjected;
			return;
		}
		Vector3 roundedCameraPosition2 = CameraController.GetRoundedCameraPosition(base.VirtualCamera.Follow.transform.position, CameraSystem.Instance.ResolutionPiexelSize, flag);
		Vector3 posOnYPlaneProjected2 = GetPosOnYPlaneProjected(state.CorrectedPosition, camDir, roundedCameraPosition2.y);
		if (ShouldResetSmoothing(base.VirtualCamera.Follow, flag))
		{
			ResetSmoothingState(posOnYPlaneProjected2, roundedCameraPosition2);
		}
		UpdateSmoothingCache(base.VirtualCamera.Follow, flag);
		if (smoothWorldInsteadOfCharacter)
		{
			smoothedWorldPosition = Vector3.Lerp(smoothedWorldPosition, posOnYPlaneProjected2, smoothingFactor * deltaTime);
			Vector3 roundedCameraPosition3 = CameraController.GetRoundedCameraPosition(smoothedWorldPosition, CameraSystem.Instance.ResolutionPiexelSize, flag);
			state.PositionCorrection += roundedCameraPosition3 - posOnYPlaneProjected2;
		}
		else
		{
			Vector3 b = posOnYPlaneProjected2 - roundedCameraPosition2;
			smoothedOffset = Vector3.Lerp(smoothedOffset, b, smoothingFactor * deltaTime);
			Vector3 roundedCameraPosition4 = CameraController.GetRoundedCameraPosition(roundedCameraPosition2 + smoothedOffset, CameraSystem.Instance.ResolutionPiexelSize, flag);
			state.PositionCorrection += roundedCameraPosition4 - posOnYPlaneProjected2;
		}
		if (applyMaxDistanceConstraint)
		{
			Vector3 posOnYPlaneProjected3 = GetPosOnYPlaneProjected(roundedCameraPosition2, camDir, roundedCameraPosition2.y);
			Vector3 posOnYPlaneProjected4 = GetPosOnYPlaneProjected(state.FinalPosition, camDir, roundedCameraPosition2.y);
			Vector2 vector = Vector2.zero;
			float x = posOnYPlaneProjected3.x - posOnYPlaneProjected4.x;
			float y = posOnYPlaneProjected3.z - posOnYPlaneProjected4.z;
			Vector2 vector2 = new Vector2(x, y);
			if (vector2.sqrMagnitude.EqualsOrMore(maxDistance))
			{
				vector = vector2 - vector2.normalized * maxDistance;
				Vector3 roundedPosXZ = VisualConsts.GetRoundedPosXZ(new Vector3(vector.x, 0f, vector.y), CameraSystem.Instance.ResolutionPiexelSize);
				vector.x = roundedPosXZ.x;
				vector.y = roundedPosXZ.z;
			}
			state.PositionCorrection.x += vector.x;
			state.PositionCorrection.z += vector.y;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		HandleMainCameraRenderTypeChanged(CameraSystem.Instance.MainCamera.GetRenderType());
		MainCamera.OnRenderModeChanged += HandleMainCameraRenderTypeChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MainCamera.OnRenderModeChanged -= HandleMainCameraRenderTypeChanged;
	}

	private void HandleMainCameraRenderTypeChanged(MainCamera.RenderMode renderType)
	{
		isActiveExtension = PlatformFeatures.Current.renderMode != PlatformRenderMode.Lightweight;
		useSmoothing = renderType == MainCamera.RenderMode.Lightweight;
		cachedFollowTarget = null;
	}

	private bool ShouldResetSmoothing(Transform follow, bool isActiveCamera)
	{
		if (follow != cachedFollowTarget)
		{
			return true;
		}
		if (isActiveCamera && !cachedWasActiveCamera)
		{
			return true;
		}
		return false;
	}

	private void UpdateSmoothingCache(Transform follow, bool isActiveCamera)
	{
		cachedFollowTarget = follow;
		cachedWasActiveCamera = isActiveCamera;
	}

	private void ResetSmoothingState(Vector3 curPos, Vector3 roundedCharacterPos)
	{
		smoothedOffset = curPos - roundedCharacterPos;
		smoothedWorldPosition = curPos;
	}

	private Vector3 GetPosOnYPlaneProjected(Vector3 camPos, Vector3 camDir, float yPlane)
	{
		if (Mathf.Abs(camDir.y) < 1E-05f)
		{
			return new Vector3(camPos.x, yPlane, camPos.z);
		}
		float num = (yPlane - camPos.y) / camDir.y;
		return new Vector3(camPos.x + num * camDir.x, yPlane, camPos.z + num * camDir.z);
	}

	private void OnDrawGizmosSelected()
	{
		if (applyMaxDistanceConstraint && !(base.VirtualCamera.Follow == null))
		{
			Gizmos.DrawWireCube(base.VirtualCamera.Follow.position, Vector3.one * maxDistance * 2f);
		}
	}
}
