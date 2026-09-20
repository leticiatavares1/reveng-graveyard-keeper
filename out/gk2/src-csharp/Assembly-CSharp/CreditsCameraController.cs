using UnityEngine;

[DefaultExecutionOrder(-1)]
public class CreditsCameraController : MonoBehaviour
{
	private const string DefaultGdPointId = "gd_astrologer_tower_blackout";

	private static readonly Vector3[] cornersBuffer = new Vector3[4];

	[SerializeField]
	private CameraController cameraController;

	[SerializeField]
	private string gdPointId = "gd_astrologer_tower_blackout";

	private bool isActive;

	private bool isSticking;

	private Vector3 gdPointPosition;

	private float trackTopScreenYAtContact;

	private UICreditsWindow creditsWindow;

	private void Awake()
	{
		if (cameraController == null)
		{
			cameraController = GetComponent<CameraController>();
		}
	}

	public static void TryEnable(UICreditsWindow creditsWindow)
	{
		CreditsCameraController creditsCameraController = FindController();
		if (creditsCameraController == null)
		{
			Debug.LogError(string.Format("{0} is not found on {1} camera.", "CreditsCameraController", CameraType.Credits));
		}
		else
		{
			creditsCameraController.Enable(creditsWindow);
		}
	}

	public static void TryDisable()
	{
		FindController()?.Disable();
	}

	public void Enable(UICreditsWindow creditsWindow)
	{
		if (!(creditsWindow == null))
		{
			this.creditsWindow = creditsWindow;
			isSticking = false;
			trackTopScreenYAtContact = 0f;
			if (creditsWindow.CameraTrackRect == null)
			{
				Debug.LogError("CreditsCameraController: first credits element is not found.");
			}
			CameraSystem instance = CameraSystem.Instance;
			Vector3 liveFollowPosition = GetLiveFollowPosition(instance.ActiveCameraController);
			instance.SetActiveCamera(CameraType.Credits);
			instance.ActiveCameraController.SetPosition(liveFollowPosition);
			cameraController.ResetCameraSpaceOffset();
			GDPointData gDPointData = MainGame.Instance?.GameSave?.worldData?.gdPointsData?.GetGDPointDataById(gdPointId);
			if (gDPointData == null)
			{
				Debug.LogError("CreditsCameraController: GD point [" + gdPointId + "] is not found.");
				isActive = true;
			}
			else
			{
				gdPointPosition = gDPointData.Position;
				isActive = true;
			}
		}
	}

	public void Disable()
	{
		if (isActive || !(creditsWindow == null))
		{
			isActive = false;
			isSticking = false;
			creditsWindow = null;
			cameraController.ResetCameraSpaceOffset();
			CameraSystem instance = CameraSystem.Instance;
			if (!(instance == null))
			{
				Vector3 liveFollowPosition = GetLiveFollowPosition(cameraController);
				instance.SetActiveCamera(CameraType.Main);
				instance.ActiveCameraController.SetPosition(liveFollowPosition);
			}
		}
	}

	private void LateUpdate()
	{
		if (!isActive || creditsWindow == null || !creditsWindow.IsShown || !TryGetCreditsTrackTopScreenY(creditsWindow, out var screenY))
		{
			return;
		}
		float y = CameraSystem.WorldToScreenPoint(gdPointPosition).y;
		if (!isSticking)
		{
			if (!(screenY < y))
			{
				isSticking = true;
				trackTopScreenYAtContact = screenY;
			}
			return;
		}
		float num = screenY - trackTopScreenYAtContact;
		Vector3 cameraLocalOffset;
		if (num <= 0f)
		{
			cameraController.ResetCameraSpaceOffset();
		}
		else if (TryGetCameraLocalOffsetForScreenYDelta(num, gdPointPosition, out cameraLocalOffset))
		{
			cameraController.SetCameraSpaceOffset(cameraLocalOffset);
		}
	}

	private static Vector3 GetLiveFollowPosition(CameraController source)
	{
		if (source != null && source.VirtualCamera != null && source.VirtualCamera.Follow != null)
		{
			return source.VirtualCamera.Follow.position;
		}
		if (source != null && source.Target != null)
		{
			return source.Target.position;
		}
		if (MainGame.PlayerController != null)
		{
			return MainGame.PlayerController.PhysicalBody.PlayerView.transform.position;
		}
		return Vector3.zero;
	}

	private static CreditsCameraController FindController()
	{
		CameraController cameraController = ((CameraSystem.Instance != null) ? CameraSystem.Instance.GetCameraController(CameraType.Credits) : null);
		if (!(cameraController != null))
		{
			return null;
		}
		return cameraController.GetComponent<CreditsCameraController>();
	}

	private static bool TryGetCreditsTrackTopScreenY(UICreditsWindow creditsWindow, out float screenY)
	{
		screenY = 0f;
		RectTransform cameraTrackRect = creditsWindow.CameraTrackRect;
		if (cameraTrackRect == null)
		{
			return false;
		}
		cameraTrackRect.GetWorldCorners(cornersBuffer);
		screenY = ((cornersBuffer[1] + cornersBuffer[2]) * 0.5f).y;
		return true;
	}

	private static bool TryGetCameraLocalOffsetForScreenYDelta(float screenYDelta, Vector3 worldPoint, out Vector3 cameraLocalOffset)
	{
		cameraLocalOffset = Vector3.zero;
		if (CameraSystem.Instance == null)
		{
			return false;
		}
		Camera worldCamera = CameraSystem.Instance.WorldCamera;
		if (worldCamera == null)
		{
			return false;
		}
		Vector3 screenPoint;
		Vector3 screenPoint2 = (screenPoint = CameraSystem.WorldToScreenPoint(worldPoint));
		screenPoint.y += screenYDelta;
		Ray ray = CameraSystem.ScreenPointToRay(screenPoint2);
		Ray ray2 = CameraSystem.ScreenPointToRay(screenPoint);
		Plane plane = new Plane(-worldCamera.transform.forward, worldPoint);
		if (!plane.Raycast(ray, out float enter) || !plane.Raycast(ray2, out float enter2))
		{
			return false;
		}
		Vector3 point = ray.GetPoint(enter);
		Vector3 point2 = ray2.GetPoint(enter2);
		Vector3 vector = point - point2;
		cameraLocalOffset = Quaternion.Inverse(worldCamera.transform.rotation) * vector;
		return true;
	}
}
