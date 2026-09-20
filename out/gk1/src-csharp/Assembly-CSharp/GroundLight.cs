using UnityEngine;

[ExecuteInEditMode]
public class GroundLight : MonoBehaviour
{
	public float z_shift = -1f;

	public float intensity_k = 1f;

	public DynamicSpritePreset intensity_preset;

	public void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.z = 2000f + z_shift;
		base.transform.position = position;
	}
}
