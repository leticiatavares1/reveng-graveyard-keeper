using UnityEngine;

public class CubeGizmo : MonoBehaviour
{
	public Color color = Color.green;

	private void OnDrawGizmosSelected()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if ((bool)component && (bool)component.sharedMesh)
		{
			Gizmos.color = color;
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(size: component.sharedMesh.bounds.size, center: Vector3.zero);
			Gizmos.matrix = matrix;
		}
	}
}
