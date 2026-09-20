using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(10)]
public class FollowMainCamera : MonoBehaviour
{
	private MainCamera mainCamera;

	public Vector3 positionDelta = Vector3.zero;

	public bool followY = true;

	private void Awake()
	{
		mainCamera = CameraSystem.Instance.MainCamera;
	}

	private void Update()
	{
		Vector3 position = mainCamera.transform.position;
		if (!followY)
		{
			position.y = base.transform.position.y - positionDelta.y;
		}
		base.transform.position = position + positionDelta;
	}

	private void OnDrawGizmosSelected()
	{
		if (!(mainCamera == null))
		{
			Camera camera = mainCamera.Camera;
			Gizmos.color = Color.gray;
			float orthographicSize = camera.orthographicSize;
			float aspect = camera.aspect;
			float nearClipPlane = camera.nearClipPlane;
			float farClipPlane = camera.farClipPlane;
			float num = orthographicSize;
			float num2 = orthographicSize * aspect;
			Vector3[] array = new Vector3[8]
			{
				camera.transform.position + camera.transform.rotation * new Vector3(0f - num2, 0f - num, nearClipPlane),
				camera.transform.position + camera.transform.rotation * new Vector3(num2, 0f - num, nearClipPlane),
				camera.transform.position + camera.transform.rotation * new Vector3(num2, num, nearClipPlane),
				camera.transform.position + camera.transform.rotation * new Vector3(0f - num2, num, nearClipPlane),
				camera.transform.position + camera.transform.rotation * new Vector3(0f - num2, 0f - num, farClipPlane),
				camera.transform.position + camera.transform.rotation * new Vector3(num2, 0f - num, farClipPlane),
				camera.transform.position + camera.transform.rotation * new Vector3(num2, num, farClipPlane),
				camera.transform.position + camera.transform.rotation * new Vector3(0f - num2, num, farClipPlane)
			};
			for (int i = 0; i < 4; i++)
			{
				Gizmos.DrawLine(array[i], array[(i + 1) % 4]);
				Gizmos.DrawLine(array[i + 4], array[(i + 1) % 4 + 4]);
				Gizmos.DrawLine(array[i], array[i + 4]);
			}
		}
	}
}
