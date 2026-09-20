using System;
using System.Collections.Generic;
using UnityEngine;

public class LightRTManager : MonoBehaviour
{
	public const int RT_RESOLUTION_SWITCH = 256;

	public const int RT_RESOLUTION_DEFAULT = 512;

	private static LightRTManager instance;

	private static readonly int idLightRT = Shader.PropertyToID("_LightRT");

	private static readonly int idLightRTViewDir = Shader.PropertyToID("_LightRTViewDir");

	private static readonly int idLightRTIntensity = Shader.PropertyToID("_LightRTIntensity");

	private static readonly int idLightRTCameraVPMatrix = Shader.PropertyToID("_LightRTCameraVPMatrix");

	[SerializeField]
	private float worldHalfExtent = 40f;

	[SerializeField]
	private float cameraHeight = 50f;

	[SerializeField]
	private Camera lightCamera;

	private RenderTexture lightRT;

	private readonly HashSet<LightFaker> fakers = new HashSet<LightFaker>();

	private bool keywordApplied;

	private bool preRenderHooked;

	private static readonly Rect FullViewportRect = new Rect(0f, 0f, 1f, 1f);

	public static LightRTManager Instance
	{
		get
		{
			if (instance == null)
			{
				EnsureInstance();
			}
			return instance;
		}
	}

	public static void NotifyWorldRenderTargetChanged()
	{
		if (!(instance == null))
		{
			instance.ReleaseRT();
		}
	}

	public static void EnsureInstance()
	{
		if (!(instance != null))
		{
			GameObject obj = new GameObject("LightRTManager");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<LightRTManager>();
			instance.Init();
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Init();
	}

	private void OnEnable()
	{
		HookPreRender();
	}

	private void OnDisable()
	{
		UnhookPreRender();
	}

	private void OnDestroy()
	{
		UnhookPreRender();
		if (instance == this)
		{
			instance = null;
		}
		ReleaseRT();
		SetKeyword(enable: false);
	}

	private void Init()
	{
		CreateLightCamera();
		EnsureRT(GetSourceCamera());
		if ((bool)lightCamera)
		{
			lightCamera.targetTexture = lightRT;
		}
		HookPreRender();
		UpdateKeyword();
	}

	private void HookPreRender()
	{
		if (!preRenderHooked)
		{
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnCameraPreRender));
			preRenderHooked = true;
		}
	}

	private void UnhookPreRender()
	{
		if (preRenderHooked)
		{
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnCameraPreRender));
			preRenderHooked = false;
		}
	}

	private Camera GetSourceCamera()
	{
		if (CameraSystem.Instance != null)
		{
			return CameraSystem.Instance.WorldCamera;
		}
		return Camera.main;
	}

	private void OnCameraPreRender(Camera cam)
	{
		if (!SwitchLightPolicy.UseLightRT)
		{
			ClearGlobals();
		}
		else if (fakers.Count != 0)
		{
			Camera sourceCamera = GetSourceCamera();
			if (!(sourceCamera == null) && !(cam != sourceCamera))
			{
				EnsureRT(sourceCamera);
				SyncLightCameraFromSource(sourceCamera);
				RenderLightRT();
				PublishGlobals();
			}
		}
	}

	private void LateUpdate()
	{
		if (fakers.Count == 0)
		{
			ClearGlobals();
		}
	}

	private void EnsureRT(Camera source)
	{
		if (!TryGetWorldRenderTargetSize(source, out var width, out var height))
		{
			height = (SwitchLightPolicy.IsSwitchPlatform ? 256 : 512);
			float num = ((source != null) ? source.aspect : 1f);
			width = Mathf.Max(1, Mathf.RoundToInt((float)height * num));
		}
		if (!(lightRT != null) || lightRT.width != width || lightRT.height != height)
		{
			ReleaseRT();
			lightRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf)
			{
				name = "_LightRT",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				useMipMap = false
			};
			lightRT.Create();
			if (lightCamera != null)
			{
				lightCamera.targetTexture = lightRT;
				lightCamera.rect = FullViewportRect;
			}
		}
	}

	private static bool TryGetWorldRenderTargetSize(Camera source, out int width, out int height)
	{
		RenderTexture renderTexture = ResolveSourceRenderTexture(source);
		if (renderTexture != null)
		{
			width = renderTexture.width;
			height = renderTexture.height;
			return true;
		}
		width = 0;
		height = 0;
		return false;
	}

	private static RenderTexture ResolveSourceRenderTexture(Camera source)
	{
		if (source != null && source.targetTexture != null)
		{
			return source.targetTexture;
		}
		if (CameraSystem.Instance != null)
		{
			RenderTexture renderTexture = CameraSystem.Instance.MainCamera.RenderTexture;
			if (renderTexture != null)
			{
				return renderTexture;
			}
		}
		return null;
	}

	private void CreateLightCamera()
	{
		if (!(lightCamera != null))
		{
			lightCamera = base.gameObject.AddComponent<Camera>();
			lightCamera.enabled = false;
			lightCamera.orthographic = true;
			lightCamera.orthographicSize = worldHalfExtent;
			lightCamera.clearFlags = CameraClearFlags.Color;
			lightCamera.backgroundColor = Color.black;
			lightCamera.cullingMask = 512;
			lightCamera.depth = -100f;
			lightCamera.rect = FullViewportRect;
			lightCamera.nearClipPlane = 0.1f;
			lightCamera.farClipPlane = cameraHeight + 10f;
			lightCamera.allowHDR = true;
		}
	}

	private void SyncLightCameraFromSource(Camera source)
	{
		if (lightCamera == null)
		{
			return;
		}
		if (source != null)
		{
			Transform transform = lightCamera.transform;
			Transform transform2 = source.transform;
			if (transform.parent != transform2)
			{
				transform.SetPositionAndRotation(transform2.position, transform2.rotation);
			}
			lightCamera.orthographic = source.orthographic;
			lightCamera.fieldOfView = source.fieldOfView;
			lightCamera.orthographicSize = source.orthographicSize;
			lightCamera.nearClipPlane = source.nearClipPlane;
			lightCamera.farClipPlane = source.farClipPlane;
			lightCamera.rect = FullViewportRect;
			lightCamera.ResetWorldToCameraMatrix();
			lightCamera.worldToCameraMatrix = source.worldToCameraMatrix;
			lightCamera.ResetProjectionMatrix();
			lightCamera.projectionMatrix = source.projectionMatrix;
			PublishCameraMatrices(source);
		}
		else
		{
			lightCamera.orthographicSize = worldHalfExtent;
			lightCamera.rect = FullViewportRect;
		}
	}

	private static void PublishCameraMatrices(Camera source)
	{
		Matrix4x4 value = GL.GetGPUProjectionMatrix(source.projectionMatrix, renderIntoTexture: true) * source.worldToCameraMatrix;
		Shader.SetGlobalMatrix(idLightRTCameraVPMatrix, value);
	}

	private void RenderLightRT()
	{
		if (!(lightCamera == null) && !(lightRT == null))
		{
			lightCamera.rect = FullViewportRect;
			lightCamera.Render();
		}
	}

	private void PublishGlobals()
	{
		Shader.SetGlobalTexture(idLightRT, lightRT);
		Shader.SetGlobalVector(idLightRTViewDir, lightCamera.transform.forward);
		Shader.SetGlobalFloat(idLightRTIntensity, 1f);
		UpdateKeyword();
	}

	private void ClearGlobals()
	{
		Shader.SetGlobalTexture(idLightRT, Texture2D.blackTexture);
		Shader.SetGlobalVector(idLightRTViewDir, Vector4.zero);
		Shader.SetGlobalFloat(idLightRTIntensity, 0f);
		Shader.SetGlobalMatrix(idLightRTCameraVPMatrix, Matrix4x4.zero);
		SetKeyword(enable: false);
	}

	private void UpdateKeyword()
	{
		bool keyword = SwitchLightPolicy.UseLightRT && fakers.Count > 0;
		SetKeyword(keyword);
	}

	private void SetKeyword(bool enable)
	{
		if (keywordApplied != enable)
		{
			keywordApplied = enable;
			if (enable)
			{
				Shader.EnableKeyword("USE_LIGHT_RT");
			}
			else
			{
				Shader.DisableKeyword("USE_LIGHT_RT");
			}
		}
	}

	private void ReleaseRT()
	{
		if (lightRT != null)
		{
			lightRT.Release();
			UnityEngine.Object.Destroy(lightRT);
			lightRT = null;
		}
	}

	public void Register(LightFaker faker)
	{
		EnsureInstance();
		fakers.Add(faker);
	}

	public void Unregister(LightFaker faker)
	{
		fakers.Remove(faker);
		if (fakers.Count == 0)
		{
			ClearGlobals();
		}
	}

	public static void ApplyPolicy()
	{
		if (instance != null)
		{
			instance.ApplyPolicyInternal();
		}
		else if (!SwitchLightPolicy.UseLightRT)
		{
			Shader.SetGlobalTexture(idLightRT, Texture2D.blackTexture);
			Shader.SetGlobalVector(idLightRTViewDir, Vector4.zero);
			Shader.SetGlobalFloat(idLightRTIntensity, 0f);
			Shader.SetGlobalMatrix(idLightRTCameraVPMatrix, Matrix4x4.zero);
			Shader.DisableKeyword("USE_LIGHT_RT");
		}
	}

	private void ApplyPolicyInternal()
	{
		if (!SwitchLightPolicy.UseLightRT || fakers.Count == 0)
		{
			keywordApplied = true;
			ClearGlobals();
		}
		else
		{
			keywordApplied = false;
			UpdateKeyword();
		}
	}
}
