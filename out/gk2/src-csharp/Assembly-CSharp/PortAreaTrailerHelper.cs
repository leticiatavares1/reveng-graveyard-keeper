using UnityEngine;
using UnityEngine.Playables;

public class PortAreaTrailerHelper : MonoBehaviour
{
	public PlayableDirector director;

	public Transform cameraFollowGo;

	public void Play()
	{
		director.Play();
	}

	public void SetCameraFollowGo()
	{
		CameraSystem.Instance.ActiveCameraController.SetTarget(cameraFollowGo.transform);
		GUIElements.Instance.SetVisibilityState(isVisible: false);
	}
}
