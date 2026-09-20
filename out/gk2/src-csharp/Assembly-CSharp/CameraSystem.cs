using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-10)]
public class CameraSystem : MonoBehaviour
{
	private const float SIZE_MULTIPLIER = 100f;

	private static CameraSystem instance;

	[SerializeField]
	private MainCamera worldCamera;

	[SerializeField]
	private Transform groundPointTransform;

	private CameraController activeCameraController;

	private List<CameraController> cameraControllers = new List<CameraController>();

	[SerializeField]
	protected float followTime = 5f;

	private static int curScreenWidth;

	private static int curScreenHeight;

	public static CameraSystem Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<CameraSystem>();
			}
			return instance;
		}
	}

	public Camera WorldCamera => worldCamera.Camera;

	public CameraController ActiveCameraController => activeCameraController;

	public Vector3 CameraGroundPos => groundPointTransform.position;

	public Transform GroundPointTransform => groundPointTransform;

	public MainCamera MainCamera => worldCamera;

	public int ResolutionPiexelSize { get; private set; }

	public void SetActiveCamera(CameraType cameraType)
	{
		foreach (CameraController cameraController in cameraControllers)
		{
			if (cameraController.CameraType == cameraType)
			{
				activeCameraController?.SetActive(isActive: false);
				activeCameraController = cameraController;
				activeCameraController.VirtualCamera.PreviousStateIsValid = false;
				activeCameraController.SetActive(isActive: true);
				return;
			}
		}
		Debug.LogError($"Error activating camera with type {cameraType}");
	}

	public static float CalculateOrthographicSize(int resolutionHeight, int resolutionPixelSize)
	{
		return (float)resolutionHeight / 2f / 100f / ((float)resolutionPixelSize / 2f);
	}

	public void SetOrthographicSize(float size)
	{
		foreach (CameraController cameraController in cameraControllers)
		{
			cameraController.VirtualCamera.m_Lens.OrthographicSize = size;
			cameraController.VirtualCamera.PreviousStateIsValid = false;
		}
	}

	public CameraController GetCameraController(CameraType cameraType)
	{
		foreach (CameraController cameraController in cameraControllers)
		{
			if (cameraController.CameraType == cameraType)
			{
				return cameraController;
			}
		}
		return null;
	}

	private void Awake()
	{
		cameraControllers = GetComponentsInChildren<CameraController>().ToList();
		SetActiveCamera(CameraType.Main);
		ApplyRenderActiveState();
		GameSettings.OnResolutionChanged += OnResolutionChanged;
	}

	private void Start()
	{
		ApplyInitialResolution();
	}

	private void OnDestroy()
	{
		GameSettings.OnResolutionChanged -= OnResolutionChanged;
	}

	private void ApplyInitialResolution()
	{
		OnResolutionChanged(GameSettings.Instance.GetResolutionIntVector2());
	}

	public void OnResolutionChanged(IntVector2 res)
	{
		curScreenWidth = res.x;
		curScreenHeight = res.y;
		ResolutionPiexelSize = ResolutionConfig.PixelSize;
		float num = CalculateOrthographicSize(res.y, ResolutionPiexelSize);
		SetOrthographicSize(num);
		Camera camera = ((worldCamera != null) ? worldCamera.Camera : null);
		if (camera != null)
		{
			camera.orthographicSize = num;
		}
		if (MainGame.Instance != null && MainGame.Instance.gameState == MainGame.GameState.InGame)
		{
			activeCameraController?.UpdateTargetPosInstant();
			ProcessChunkVisibilityForUpcomingResolution(camera, num);
		}
		if (worldCamera != null)
		{
			worldCamera.OnResolutionChanged(res.x, res.y);
		}
	}

	private static void ProcessChunkVisibilityForUpcomingResolution(Camera cam, float orthoSize)
	{
		if (LazySingleton<ChunkManager>.Instance == null)
		{
			return;
		}
		if (cam == null)
		{
			LazySingleton<ChunkManager>.Instance.ForceProcessVisibility();
			return;
		}
		float num = ((ResolutionConfig.Height > 0) ? ((float)ResolutionConfig.Width / (float)ResolutionConfig.Height) : cam.aspect);
		bool flag = num > 0f;
		if (flag)
		{
			cam.projectionMatrix = Matrix4x4.Ortho((0f - orthoSize) * num, orthoSize * num, 0f - orthoSize, orthoSize, cam.nearClipPlane, cam.farClipPlane);
		}
		try
		{
			LazySingleton<ChunkManager>.Instance.ForceProcessVisibility();
		}
		finally
		{
			if (flag)
			{
				cam.ResetProjectionMatrix();
			}
		}
	}

	private void LateUpdate()
	{
		if (!GameSettings.Instance.GraphicSettingsAppliedThisFrame)
		{
			bool num = curScreenWidth != Screen.width || curScreenHeight != Screen.height;
			bool flag = false;
			flag = GameSettings.Instance.SyncScreenModeFromHardware(applySave: false);
			if (num || flag)
			{
				GameSettings.Instance.ApplyGraphicSettings(applySave: true, applyEditorGameView: false);
			}
		}
	}

	public void ApplyRenderActiveState()
	{
		SetCameraRenderActiveState();
	}

	private void SetCameraRenderActiveState()
	{
		worldCamera.SetRenderType(PlatformFeatures.Current.GetMainCameraRenderMode());
	}

	public static Vector3 WorldToScreenPoint(Vector3 worldPoint)
	{
		Vector3 result = Instance.WorldCamera.WorldToScreenPoint(worldPoint);
		if (Instance.MainCamera.GetRenderType() == MainCamera.RenderMode.Lightweight)
		{
			float num = (float)Screen.width / (float)Instance.MainCamera.RenderTexture.width;
			float num2 = (float)Screen.height / (float)Instance.MainCamera.RenderTexture.height;
			result.x *= num;
			result.y *= num2;
		}
		return result;
	}

	public static Ray ScreenPointToRay(Vector3 screenPoint)
	{
		if (Instance.MainCamera.GetRenderType() == MainCamera.RenderMode.Lightweight)
		{
			float num = (float)Screen.width / (float)Instance.MainCamera.RenderTexture.width;
			float num2 = (float)Screen.height / (float)Instance.MainCamera.RenderTexture.height;
			screenPoint.x /= num;
			screenPoint.y /= num2;
		}
		return Instance.WorldCamera.ScreenPointToRay(screenPoint);
	}
}
