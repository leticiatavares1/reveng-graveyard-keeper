using UnityEngine;

public class ConstantZ : MonoBehaviour
{
	public float z;

	public void LateUpdate()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z = z;
	}
}
