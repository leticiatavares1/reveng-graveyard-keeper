using UnityEngine;

public class BuildModeCameraController : MonoBehaviour
{
	private Transform followTarget;

	public void Enable(Transform followTarget, Collider boundingVolume)
	{
		CameraSystem instance = CameraSystem.Instance;
		instance.SetActiveCamera(CameraType.BuildMode);
		if (!instance.ActiveCameraController.TrySet3DConfinerBounds(boundingVolume))
		{
			Debug.LogError("The build mode camera must have bounding volume collider");
		}
		this.followTarget = followTarget;
		instance.ActiveCameraController.SetTarget(followTarget);
	}

	public void Disable()
	{
		CameraSystem.Instance.SetActiveCamera(CameraType.Main);
		followTarget = null;
	}

	public void SetPauseState(bool isPaused)
	{
		if (isPaused)
		{
			CameraSystem.Instance.ActiveCameraController.SetTargetInstant(null);
		}
		else
		{
			CameraSystem.Instance.ActiveCameraController.SetTargetInstant(followTarget);
		}
	}
}
