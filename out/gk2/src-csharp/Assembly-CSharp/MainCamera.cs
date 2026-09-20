using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[ExecuteAlways]
public class MainCamera : MonoBehaviour, ISoundZoneRecognizable
{
	public enum RenderMode
	{
		Native,
		Lightweight
	}

	[SerializeField]
	private PostProcessVolume postProcessVolume;

	[SerializeField]
	private Camera cameraComponent;

	[SerializeField]
	private DeformTextureCamera deformTextureCamera;

	[Space]
	[SerializeField]
	private RenderMode renderMode;

	[SerializeField]
	private BlitCameraRT blitCameraRT;

	[SerializeField]
	[Space]
	private float bloomBaseThreshold = 1.08f;

	[SerializeField]
	[Space]
	private List<PostProcessProfile> postProcessProfiles = new List<PostProcessProfile>();

	private List<Func<float>> additionalThresholdGetters = new List<Func<float>>();

	private Bloom bloom;

	private PostProcessProfile defaultPostProcessProfile;

	private RenderTexture renderTexture;

	private int prevPixelSize;

	private CommandBuffer depthBindCmd;

	private RaycastHit[] results = new RaycastHit[1];

	public float freeCameraSizeK = 1f;

	private float lastFreeCameraSizeK = 1f;

	private bool isFreeCamera;

	private float storedOrthographicSize;

	private string currentAnimationName = "";

	private bool waitingForEnterAnimation;

	private bool isPlayingAnimation;

	private Action onAnimationFinished;

	private Animator animator;

	public RenderTexture RenderTexture => renderTexture;

	public Camera Camera => cameraComponent;

	public PostProcessVolume PostProcessVolume => postProcessVolume;

	public BlitCameraRT BlitCameraRT => blitCameraRT;

	private Animator Animator
	{
		get
		{
			if (animator == null)
			{
				animator = GetComponent<Animator>();
			}
			return animator;
		}
	}

	public bool IsDepthCopyActive => depthBindCmd != null;

	public static event Action<RenderMode> OnRenderModeChanged;

	public Vector3 GetGroundPointPos()
	{
		Vector3 zero = Vector3.zero;
		if (Physics.RaycastNonAlloc(cameraComponent.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), results, float.PositiveInfinity, 15) > 0)
		{
			return results[0].point;
		}
		return zero;
	}

	public void OnResolutionChanged(int width, int height)
	{
		if (renderMode == RenderMode.Lightweight)
		{
			prevPixelSize = ResolutionConfig.PixelSize;
			ReCreateRenderTexture();
		}
		if (deformTextureCamera != null && cameraComponent != null)
		{
			int width2 = ResolutionConfig.Width;
			int height2 = ResolutionConfig.Height;
			deformTextureCamera.ChangeRenderTargetSize(width2, height2, cameraComponent.orthographicSize);
		}
	}

	public void SetRenderType(RenderMode renderType)
	{
		CleanupDepthCopy();
		ReleaseRenderTexture();
		blitCameraRT.gameObject.SetActive(value: false);
		cameraComponent.targetTexture = null;
		if (renderType != 0 && renderType == RenderMode.Lightweight)
		{
			blitCameraRT.gameObject.SetActive(value: true);
			ReCreateRenderTexture();
		}
		renderMode = renderType;
		MainCamera.OnRenderModeChanged?.Invoke(renderType);
	}

	public RenderMode GetRenderType()
	{
		return renderMode;
	}

	private void Awake()
	{
		base.transform.eulerAngles = new Vector3(53.130104f, 0f, 0f);
		if (!TryGetComponent<PostProcessVolume>(out postProcessVolume))
		{
			Debug.LogError("PostProcessVolume wasn't found");
		}
		prevPixelSize = ResolutionConfig.PixelSize;
		defaultPostProcessProfile = ((postProcessVolume != null) ? postProcessVolume.sharedProfile : null);
		CacheBloomFromActiveProfile();
	}

	public void SetPostProcessProfile(string profileName)
	{
		if (string.IsNullOrEmpty(profileName))
		{
			ResetPostProcessProfile();
			return;
		}
		PostProcessProfile postProcessProfile = FindPostProcessProfile(profileName);
		if (postProcessProfile == null)
		{
			Debug.LogError("Post process profile [" + profileName + "] wasn't found");
		}
		else
		{
			ApplyPostProcessProfile(postProcessProfile);
		}
	}

	public void ResetPostProcessProfile()
	{
		ApplyPostProcessProfile(defaultPostProcessProfile);
	}

	private PostProcessProfile FindPostProcessProfile(string profileName)
	{
		if (postProcessProfiles == null)
		{
			return null;
		}
		for (int i = 0; i < postProcessProfiles.Count; i++)
		{
			PostProcessProfile postProcessProfile = postProcessProfiles[i];
			if (postProcessProfile != null && postProcessProfile.name == profileName)
			{
				return postProcessProfile;
			}
		}
		return null;
	}

	private void ApplyPostProcessProfile(PostProcessProfile asset)
	{
		if (asset == null || postProcessVolume == null)
		{
			return;
		}
		if (postProcessVolume.HasInstantiatedProfile())
		{
			PostProcessProfile profile = postProcessVolume.profile;
			postProcessVolume.profile = null;
			if (profile != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(profile);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(profile);
				}
			}
		}
		postProcessVolume.sharedProfile = asset;
		CacheBloomFromActiveProfile();
		if (EnvironmentEngine.Instance != null)
		{
			EnvironmentEngine.Instance.RefreshAppliedLightPreset();
		}
		Debug.Log("Set post process profile to: " + asset.name);
	}

	private void CacheBloomFromActiveProfile()
	{
		bloom = ((PostProcessVolume != null) ? PostProcessVolume.profile.GetSetting<Bloom>() : null);
		UpdateBloomThreshold();
	}

	private void Start()
	{
		if (renderMode == RenderMode.Lightweight)
		{
			ReCreateRenderTexture();
		}
	}

	private void OnPreRender()
	{
		Shader.EnableKeyword("WORLD_RENDER");
		if (depthBindCmd != null)
		{
			Graphics.ExecuteCommandBuffer(depthBindCmd);
		}
	}

	private void OnPostRender()
	{
		Shader.DisableKeyword("WORLD_RENDER");
	}

	private void Update()
	{
		if (isFreeCamera)
		{
			cameraComponent.orthographicSize = storedOrthographicSize * freeCameraSizeK;
			if (isPlayingAnimation && !string.IsNullOrEmpty(currentAnimationName))
			{
				string text = ((Animator.GetCurrentAnimatorClipInfo(0).Length != 0) ? Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name : string.Empty);
				if (waitingForEnterAnimation && text == currentAnimationName)
				{
					waitingForEnterAnimation = false;
				}
				else if (text != currentAnimationName && !waitingForEnterAnimation)
				{
					Debug.Log("Camera.Update: Animation finished: " + text + " != " + currentAnimationName);
					OnAnimationFinished();
				}
			}
		}
		if (renderMode == RenderMode.Lightweight && prevPixelSize != ResolutionConfig.PixelSize)
		{
			prevPixelSize = ResolutionConfig.PixelSize;
			ReCreateRenderTexture();
		}
	}

	private void SetFreeCameraMode(bool isFreeCamera)
	{
		Debug.Log($"Camera.SetFreeCameraMode: {isFreeCamera}");
		this.isFreeCamera = isFreeCamera;
		GetComponent<CinemachineBrain>().enabled = !isFreeCamera;
		if (isFreeCamera)
		{
			storedOrthographicSize = cameraComponent.orthographicSize;
			Update();
		}
		else
		{
			cameraComponent.orthographicSize = storedOrthographicSize;
			freeCameraSizeK = 1f;
		}
	}

	public void PlayAnimation(string animationName, Action onFinished)
	{
		Debug.Log("Camera.PlayAnimation: " + animationName);
		currentAnimationName = animationName;
		waitingForEnterAnimation = true;
		isPlayingAnimation = true;
		onAnimationFinished = onFinished;
		SetFreeCameraMode(isFreeCamera: true);
		Animator.enabled = true;
		Animator.Play(animationName);
	}

	public void UpdateBloomThreshold()
	{
		if (bloom == null)
		{
			return;
		}
		float num = 0f;
		foreach (Func<float> additionalThresholdGetter in additionalThresholdGetters)
		{
			if (additionalThresholdGetter != null)
			{
				num += additionalThresholdGetter();
			}
		}
		bloom.threshold.value = bloomBaseThreshold + num;
	}

	public void AddAdditionalBloomThresholdGetter(Func<float> getter)
	{
		if (getter != null)
		{
			additionalThresholdGetters.Add(getter);
		}
	}

	private void OnAnimationFinished()
	{
		Debug.Log("Camera.OnAnimationFinished");
		isPlayingAnimation = false;
		Animator.enabled = false;
		SetFreeCameraMode(isFreeCamera: false);
		Action action = onAnimationFinished;
		onAnimationFinished = null;
		action?.Invoke();
	}

	private void ReCreateRenderTexture()
	{
		CleanupDepthCopy();
		ReleaseRenderTexture();
		int width = ResolutionConfig.Width;
		int height = ResolutionConfig.Height;
		renderTexture = new RenderTexture(width, height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt);
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.Create();
		cameraComponent.targetTexture = renderTexture;
		SetupDepthCopy();
		LightRTManager.NotifyWorldRenderTargetChanged();
	}

	public void SetDepthCopyEnabled(bool enabled)
	{
		if (enabled)
		{
			SetupDepthCopy();
		}
		else
		{
			CleanupDepthCopy();
		}
	}

	private void SetupDepthCopy()
	{
		CleanupDepthCopy();
		if (!(renderTexture == null))
		{
			depthBindCmd = new CommandBuffer
			{
				name = "Bind RT Depth"
			};
			depthBindCmd.SetGlobalTexture("_CameraDepthTexture", renderTexture, RenderTextureSubElement.Depth);
			Graphics.ExecuteCommandBuffer(depthBindCmd);
		}
	}

	private void CleanupDepthCopy()
	{
		if (depthBindCmd != null)
		{
			depthBindCmd.Release();
			depthBindCmd = null;
			Shader.SetGlobalTexture("_CameraDepthTexture", Texture2D.whiteTexture);
		}
	}

	private void ReleaseRenderTexture()
	{
		if (!(renderTexture == null))
		{
			if (cameraComponent != null && cameraComponent.targetTexture == renderTexture)
			{
				cameraComponent.targetTexture = null;
			}
			renderTexture.Release();
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(renderTexture);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(renderTexture);
			}
			renderTexture = null;
		}
	}

	private void OnDestroy()
	{
		CleanupDepthCopy();
		ReleaseRenderTexture();
	}
}
