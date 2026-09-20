using UnityEngine;

namespace DefaultNamespace;

public class TestCamera : MonoBehaviour
{
	private void OutputRotationMatrix()
	{
		Camera component = GetComponent<Camera>();
		Matrix4x4 worldToCameraMatrix = component.worldToCameraMatrix;
		Debug.Log(worldToCameraMatrix);
		string text = $"#define CUSTOM_WORLD_TO_CAMERA float3x3({worldToCameraMatrix[0, 0]:0.000000}; {worldToCameraMatrix[0, 1]:0.000000}; {worldToCameraMatrix[0, 2]:0.000000}; {worldToCameraMatrix[1, 0]:0.000000}; {worldToCameraMatrix[1, 1]:0.000000}; {worldToCameraMatrix[1, 2]:0.000000}; {worldToCameraMatrix[2, 0]:0.000000}; {worldToCameraMatrix[2, 1]:0.000000}; {worldToCameraMatrix[2, 2]:0.000000})";
		text = text.Replace(",", ".");
		text = text.Replace(";", ",");
		Vector3 eulerAngles = component.transform.rotation.eulerAngles;
		if (eulerAngles.x > 180f)
		{
			eulerAngles.x -= 360f;
		}
		if (eulerAngles.y > 180f)
		{
			eulerAngles.y -= 360f;
		}
		if (eulerAngles.z > 180f)
		{
			eulerAngles.z -= 360f;
		}
		text = $"// rotation matrix for camera ({eulerAngles.x}, {eulerAngles.y}, {eulerAngles.z}) calculated with camera.worldToCameraMatrix():\n{text}";
		Debug.Log(text);
	}
}
