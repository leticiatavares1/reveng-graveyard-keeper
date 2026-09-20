using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class MirrorMeshCollider : MonoBehaviour
{
	[SerializeField]
	private Vector3 mirrorScale = new Vector3(-1f, 1f, 1f);

	private MeshCollider meshCol;

	private MeshCollider MeshCol
	{
		get
		{
			if (meshCol == null)
			{
				meshCol = GetComponent<MeshCollider>();
			}
			return meshCol;
		}
	}

	private void OnEnable()
	{
		if (MeshCol != null && MeshCol.IsLossyScaleNegative())
		{
			MirrorCollider();
		}
	}

	public void MirrorCollider()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (component == null || MeshCol == null || (TryGetComponent<VHACD_ColliderDecomposer>(out var component2) && component2.IsValid))
		{
			return;
		}
		Mesh sharedMesh = component.sharedMesh;
		if (!(sharedMesh == null))
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x * mirrorScale.x, base.transform.localScale.y * mirrorScale.y, base.transform.localScale.z * mirrorScale.z);
			Mesh mesh = Object.Instantiate(sharedMesh);
			List<Vector3> list = new List<Vector3>();
			Vector3[] vertices = mesh.vertices;
			foreach (Vector3 a in vertices)
			{
				list.Add(Vector3.Scale(a, mirrorScale));
			}
			mesh.vertices = list.ToArray();
			int[] triangles = mesh.triangles;
			for (int j = 0; j < triangles.Length; j += 3)
			{
				ref int reference = ref triangles[j];
				ref int reference2 = ref triangles[j + 1];
				int i = triangles[j + 1];
				int num = triangles[j];
				reference = i;
				reference2 = num;
			}
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			MeshCol.sharedMesh = mesh;
		}
	}
}
