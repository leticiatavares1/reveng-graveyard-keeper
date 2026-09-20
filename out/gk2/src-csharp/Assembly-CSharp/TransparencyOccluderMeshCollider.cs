using UnityEngine;

[ExecuteAlways]
public class TransparencyOccluderMeshCollider : MonoBehaviour
{
	[SerializeField]
	private MeshFilter meshFilter;

	[SerializeField]
	private MeshCollider meshCollider;

	private void ApplyParameters()
	{
		base.gameObject.layer = 15;
		if ((bool)meshFilter)
		{
			meshCollider.sharedMesh = meshFilter.sharedMesh;
		}
	}
}
