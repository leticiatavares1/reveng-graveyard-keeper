using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[Serializable]
	public class NoiseProfile
	{
		public CameraNoiseType type;

		public NoiseSettings profile;
	}

	private enum State
	{
		Default,
		TrackingDolly
	}

	private const int INACTIVE_VIRTUAL_CAM_PRIORITY = 0;

	private const int ACTIVE_VIRTUAL_CAM_PRIORITY = 10;

	private static readonly int idCameraSubOffset = Shader.PropertyToID("_CameraSubOffset");

	private static readonly int idCameraOffset = Shader.PropertyToID("_CameraOffset");

	private Action onTargetFollowCompleted;

	[SerializeField]
	private CameraType cameraType;

	[SerializeField]
	private CinemachineVirtualCamera virtualCamera;

	[SerializeField]
	private List<NoiseProfile> noiseProfiles;

	[SerializeField]
	private Transform dollyTarget;

	[SerializeField]
	private Transform fakeFollowTarget;

	private Action noiseCallback;

	private Coroutine noiseCoroutine;

	private CinemachineBasicMultiChannelPerlin noise;

	private CinemachineFramingTransposer framingTransposer;

	private CinemachineHardLockToTarget hardLockToTarget;

	private CinemachineCameraOffset cameraSpaceOffset;

	private Vector3 defaultFramingDamping;

	private bool defaultFramingDampingCached;

	private Transform actualTarget;

	private bool initialized;

	private float currentTime;

	private float followTargetTime;

	private Vector3 followStartPosition;

	private bool isFollowingFakeTransform;

	private State currentState;

	private static bool subpixelPosition = true;

	private static bool canMoveBySubpixel = false;

	public CameraType CameraType => cameraType;

	public CinemachineVirtualCamera VirtualCamera => virtualCamera;

	public Transform Target => actualTarget;

	private CinemachineCameraOffset CameraSpaceOffset
	{
		get
		{
			if (cameraSpaceOffset == null && virtualCamera != null)
			{
				cameraSpaceOffset = virtualCamera.GetComponent<CinemachineCameraOffset>();
			}
			return cameraSpaceOffset;
		}
	}

	private void Awake()
	{
		MainCamera.OnRenderModeChanged += HandleMainCameraRenderTypeChanged;
		ApplyFollowBodyModeIfReady();
	}

	private void Initialize()
	{
		dollyTarget = new GameObject().GetComponent<Transform>();
		dollyTarget.parent = CameraSystem.Instance.transform;
		dollyTarget.gameObject.SetActive(value: false);
		fakeFollowTarget = new GameObject().GetComponent<Transform>();
		fakeFollowTarget.parent = CameraSystem.Instance.transform;
		noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		initialized = true;
		ApplyFollowBodyModeIfReady();
	}

	public void SetTarget(Transform targetTransform, float duration = 0f, Action completeCallback = null)
	{
		if (!initialized)
		{
			Initialize();
		}
		if (duration.EqualsTo(0f))
		{
			SetTargetInstant(targetTransform);
			completeCallback?.Invoke();
			return;
		}
		actualTarget = targetTransform;
		dollyTarget.position = base.transform.position;
		followStartPosition = dollyTarget.position;
		currentTime = 0f;
		followTargetTime = duration;
		virtualCamera.Follow = dollyTarget;
		currentState = State.TrackingDolly;
		onTargetFollowCompleted = completeCallback;
	}

	public void SetTargetInstant(Transform targetTransform)
	{
		if (!initialized)
		{
			Initialize();
		}
		actualTarget = targetTransform;
		SetDefaultParams();
		UpdateTargetPosInstant();
	}

	public void SetPosition(Vector3 pos, float duration = 0f, Action completeCallback = null)
	{
		if (!initialized)
		{
			Initialize();
		}
		fakeFollowTarget.position = pos;
		isFollowingFakeTransform = true;
		SetTarget(fakeFollowTarget, duration, completeCallback);
	}

	public void UpdateTargetPosInstant()
	{
		virtualCamera.UpdateCameraState(Vector3.up, -1f);
		virtualCamera.CancelDamping(updateNow: true);
	}

	public void SetActive(bool isActive)
	{
		virtualCamera.Priority = (isActive ? 10 : 0);
	}

	public void SetCameraSpaceOffset(Vector3 offset)
	{
		if (!(CameraSpaceOffset == null))
		{
			CameraSpaceOffset.m_Offset = offset;
		}
	}

	public void ResetCameraSpaceOffset()
	{
		SetCameraSpaceOffset(Vector3.zero);
	}

	public void TrуReset()
	{
		if (currentState == State.TrackingDolly)
		{
			onTargetFollowCompleted = null;
			SetDefaultParams();
		}
	}

	public static Vector3 GetRoundedCameraPosition(Vector3 position, int pixelSize = 2, bool writeOffsetToShader = false)
	{
		float num = 2f / (float)Mathf.Max(1, pixelSize);
		float num2 = 0.01f * num;
		float num3 = 0.01666667f * num;
		float num4 = 0.0125f * num;
		Vector3 vector = new Vector3(Mathf.Round(position.x / num2) * num2, Mathf.Round(position.y / num3) * num3, Mathf.Round(position.z / num4) * num4);
		Vector3 roundedPosXYZ = VisualConsts.GetRoundedPosXYZ(vector);
		Vector3 vector2 = Vector3.Scale(vector - roundedPosXYZ, VisualConsts.XYZ_STEP_INV) * ((float)ResolutionConfig.PixelSize / 2f);
		Shader.SetGlobalVector(idCameraSubOffset, subpixelPosition ? vector2 : Vector3.zero);
		if (writeOffsetToShader)
		{
			Shader.SetGlobalVector(idCameraOffset, vector2);
		}
		if (!canMoveBySubpixel)
		{
			return roundedPosXYZ;
		}
		return vector;
	}

	public void ShakeCamera(float duration, float fadeDuration = 0f, float amplitude = 1f)
	{
		StopAllCoroutines();
		StartCoroutine(DoShakeCamera(duration, amplitude, fadeDuration));
	}

	public void ShakeCamera(CameraNoiseType type, float duration, float fadeDuration = 0f, float amplitude = 1f, Action callback = null)
	{
		if (noiseCoroutine != null)
		{
			StopCoroutine(noiseCoroutine);
			noiseCallback?.Invoke();
		}
		noiseCallback = callback;
		noise.m_NoiseProfile = noiseProfiles.Find((NoiseProfile x) => x.type == type)?.profile;
		if (noise.m_NoiseProfile == null)
		{
			Debug.LogError(string.Format("[{0}]: noise type [{1}] is not set up.", "CameraController", type));
		}
		else
		{
			noiseCoroutine = StartCoroutine(DoShakeCamera(duration, fadeDuration, amplitude));
		}
	}

	public bool TrySet3DConfinerBounds(Collider collider)
	{
		if (TryGetComponent<CinemachineCustomConfiner>(out var component))
		{
			component.BoundingCollider = collider;
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (!MainGame.IsGamePaused && currentState == State.TrackingDolly)
		{
			float num = currentTime / followTargetTime;
			dollyTarget.transform.position = Vector3.Lerp(followStartPosition, isFollowingFakeTransform ? fakeFollowTarget.transform.position : actualTarget.transform.position, num);
			if (num >= 1f)
			{
				SetDefaultParams();
				onTargetFollowCompleted?.Invoke();
			}
			currentTime += Time.deltaTime;
		}
	}

	private void SetDefaultParams()
	{
		virtualCamera.Follow = actualTarget;
		currentState = State.Default;
		isFollowingFakeTransform = false;
	}

	private IEnumerator DoShakeCamera(float duration, float fadeDuration, float amplitude)
	{
		bool hasFade = fadeDuration == 0f;
		if (hasFade)
		{
			noise.m_AmplitudeGain = amplitude;
		}
		else
		{
			yield return DoAmplitudeEase(0f, amplitude, fadeDuration);
		}
		yield return new WaitForSeconds(duration);
		if (hasFade)
		{
			noise.m_AmplitudeGain = 0f;
		}
		else
		{
			yield return DoAmplitudeEase(amplitude, 0f, fadeDuration);
		}
		noise.m_NoiseProfile = null;
		noiseCoroutine = null;
		noiseCallback?.Invoke();
	}

	private IEnumerator DoAmplitudeEase(float amplitudeValueFrom, float amplitudeValueTo, float easeDuration)
	{
		for (float t = 0f; t < easeDuration; t += Time.deltaTime)
		{
			noise.m_AmplitudeGain = Mathf.Lerp(amplitudeValueFrom, amplitudeValueTo, t / easeDuration);
			yield return null;
		}
		noise.m_AmplitudeGain = amplitudeValueTo;
		yield return null;
	}

	private void HandleMainCameraRenderTypeChanged(MainCamera.RenderMode renderType)
	{
		canMoveBySubpixel = renderType == MainCamera.RenderMode.Native;
		ApplyFollowBodyMode(renderType);
	}

	private void ApplyFollowBodyModeIfReady()
	{
		if (!(CameraSystem.Instance == null) && !(CameraSystem.Instance.MainCamera == null))
		{
			subpixelPosition = false;
			HandleMainCameraRenderTypeChanged(CameraSystem.Instance.MainCamera.GetRenderType());
		}
	}

	private void ApplyFollowBodyMode(MainCamera.RenderMode renderType)
	{
		if (virtualCamera == null)
		{
			return;
		}
		Transform componentOwner = virtualCamera.GetComponentOwner();
		if (!(componentOwner == null))
		{
			CacheFollowBodyComponents(componentOwner);
			if (renderType == MainCamera.RenderMode.Lightweight && cameraType != CameraType.BuildMode)
			{
				EnableHardLockBody(componentOwner);
			}
			else
			{
				EnableFramingTransposerBody(componentOwner, renderType == MainCamera.RenderMode.Lightweight);
			}
			virtualCamera.InvalidateComponentPipeline();
			virtualCamera.PreviousStateIsValid = false;
		}
	}

	private void CacheFollowBodyComponents(Transform pipelineOwner)
	{
		if (framingTransposer == null)
		{
			framingTransposer = pipelineOwner.GetComponent<CinemachineFramingTransposer>();
		}
		if (hardLockToTarget == null)
		{
			hardLockToTarget = pipelineOwner.GetComponent<CinemachineHardLockToTarget>();
		}
	}

	private void EnableHardLockBody(Transform pipelineOwner)
	{
		if (hardLockToTarget == null)
		{
			hardLockToTarget = pipelineOwner.gameObject.AddComponent<CinemachineHardLockToTarget>();
		}
		hardLockToTarget.m_Damping = 0f;
		if (framingTransposer != null)
		{
			framingTransposer.enabled = false;
		}
		hardLockToTarget.enabled = true;
	}

	private void EnableFramingTransposerBody(Transform pipelineOwner, bool zeroDamping)
	{
		if (hardLockToTarget != null)
		{
			hardLockToTarget.enabled = false;
		}
		if (framingTransposer == null)
		{
			framingTransposer = pipelineOwner.gameObject.AddComponent<CinemachineFramingTransposer>();
		}
		CacheDefaultFramingDamping();
		framingTransposer.m_XDamping = (zeroDamping ? 0f : defaultFramingDamping.x);
		framingTransposer.m_YDamping = (zeroDamping ? 0f : defaultFramingDamping.y);
		framingTransposer.m_ZDamping = (zeroDamping ? 0f : defaultFramingDamping.z);
		framingTransposer.enabled = true;
	}

	private void CacheDefaultFramingDamping()
	{
		if (!defaultFramingDampingCached && !(framingTransposer == null))
		{
			defaultFramingDamping = new Vector3(framingTransposer.m_XDamping, framingTransposer.m_YDamping, framingTransposer.m_ZDamping);
			defaultFramingDampingCached = true;
		}
	}

	private void OnDestroy()
	{
		MainCamera.OnRenderModeChanged -= HandleMainCameraRenderTypeChanged;
	}

	public static void SetFollowTarget(Transform targetTransform, float duration = 0f, Action callback = null)
	{
		CameraSystem.Instance.ActiveCameraController.SetTarget(targetTransform, duration, callback);
	}

	public static void SetFollowTargetInstant(Transform targetTransform)
	{
		CameraSystem.Instance.ActiveCameraController.SetTargetInstant(targetTransform);
	}
}
