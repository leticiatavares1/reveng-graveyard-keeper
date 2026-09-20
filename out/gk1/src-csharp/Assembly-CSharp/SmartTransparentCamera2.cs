using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class SmartTransparentCamera2 : GJShaderEffect
{
	public SmartTransparentCamera camera_1;

	private Camera _camera_1;

	private Camera _c;

	public float range = 1f;

	public void Update()
	{
		if (!(camera_1 == null))
		{
			if (_camera_1 == null)
			{
				_camera_1 = camera_1.GetComponent<Camera>();
			}
			if (_c == null)
			{
				_c = GetComponent<Camera>();
			}
			if ((int)_camera_1.orthographicSize != (int)_c.orthographicSize)
			{
				_c.orthographicSize = _camera_1.orthographicSize;
			}
			_c.nearClipPlane = -2000f;
			_c.farClipPlane = 0f - camera_1.z_shift;
		}
	}

	protected override void SetValues(Material mat)
	{
		mat.SetFloat("_Range", range);
	}
}
