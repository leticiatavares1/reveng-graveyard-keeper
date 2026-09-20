using UnityEngine;

[ExecuteInEditMode]
public class SmartTransparentCamera : MonoBehaviour
{
	public Transform character;

	public float z_shift = -0.7f;

	public void Update()
	{
		if (!(character == null))
		{
			Vector3 position = base.transform.position;
			position.z = character.position.z + z_shift;
			base.transform.position = position;
		}
	}
}
