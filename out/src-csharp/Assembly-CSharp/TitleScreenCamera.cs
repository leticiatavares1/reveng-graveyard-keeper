using UnityEngine;

[ExecuteInEditMode]
public class TitleScreenCamera : MonoBehaviour
{
	private Camera _cam;

	public void Update()
	{
		float num = (float)Screen.height / 96f / 2f;
		if (_cam == null)
		{
			_cam = GetComponent<Camera>();
		}
		if (!_cam.orthographicSize.EqualsTo(num))
		{
			_cam.orthographicSize = num;
		}
	}
}
