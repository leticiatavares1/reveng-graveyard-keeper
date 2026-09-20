using UnityEngine;

public class TestMeshDraw : MonoBehaviour
{
	public void OnDrawGizmos()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[3]
		{
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(0f, -1f, 0f)
		};
		mesh.triangles = new int[3] { 0, 1, 2 };
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		Gizmos.DrawWireMesh(mesh, base.transform.position, Quaternion.identity, base.transform.lossyScale);
	}
}
