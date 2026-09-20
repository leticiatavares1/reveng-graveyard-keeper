using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{
	public static Mesh ConvertWorldMeshToLocalSpace(this Mesh sourceMesh, Transform targetTransform, Vector3 offset = default(Vector3))
	{
		if (sourceMesh == null)
		{
			return null;
		}
		Mesh mesh = new Mesh();
		Vector3[] vertices = sourceMesh.vertices;
		Vector3[] normals = sourceMesh.normals;
		Vector3[] array = new Vector3[vertices.Length];
		Vector3[] array2 = new Vector3[vertices.Length];
		Matrix4x4 worldToLocalMatrix = targetTransform.worldToLocalMatrix;
		Matrix4x4 transpose = worldToLocalMatrix.transpose;
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i] = worldToLocalMatrix.MultiplyPoint3x4(vertices[i]) + offset;
			if (normals != null && normals.Length != 0)
			{
				array2[i] = transpose.MultiplyVector(normals[i]).normalized;
			}
		}
		mesh.vertices = array;
		if (normals != null && normals.Length != 0)
		{
			mesh.normals = array2;
		}
		mesh.uv = sourceMesh.uv;
		mesh.uv2 = sourceMesh.uv2;
		mesh.subMeshCount = sourceMesh.subMeshCount;
		for (int j = 0; j < sourceMesh.subMeshCount; j++)
		{
			mesh.SetTriangles(sourceMesh.GetTriangles(j), j);
		}
		mesh.RecalculateBounds();
		return mesh;
	}

	public static Mesh GenerateMeshFromSubMesh(this Mesh sourceMesh, int subMeshIndex)
	{
		if (sourceMesh == null)
		{
			Debug.LogError("Source mesh cannot be null.");
			return null;
		}
		if (subMeshIndex < 0 || subMeshIndex >= sourceMesh.subMeshCount)
		{
			Debug.LogError($"Submesh index {subMeshIndex} is out of range for the specified mesh.");
			return null;
		}
		Mesh mesh = new Mesh();
		int[] triangles = sourceMesh.GetTriangles(subMeshIndex);
		Vector3[] vertices = sourceMesh.vertices;
		Vector3[] normals = sourceMesh.normals;
		Vector2[] uv = sourceMesh.uv;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		List<int> list4 = new List<int>();
		foreach (int num in triangles)
		{
			if (!dictionary.TryGetValue(num, out var value))
			{
				value = (dictionary[num] = list.Count);
				list.Add(vertices[num]);
				if (normals.Length != 0)
				{
					list2.Add(normals[num]);
				}
				if (uv.Length != 0)
				{
					list3.Add(uv[num]);
				}
			}
			list4.Add(value);
		}
		mesh.vertices = list.ToArray();
		if (list2.Count > 0)
		{
			mesh.normals = list2.ToArray();
		}
		if (list3.Count > 0)
		{
			mesh.uv = list3.ToArray();
		}
		mesh.triangles = list4.ToArray();
		mesh.RecalculateBounds();
		return mesh;
	}

	public static Mesh ApplyRotationAndScale(this Mesh mesh, Transform sourceTransform)
	{
		if (mesh == null || sourceTransform == null)
		{
			Debug.LogError("Mesh or sourceTransform cannot be null.");
			return null;
		}
		Mesh mesh2 = new Mesh
		{
			vertices = mesh.vertices,
			uv = mesh.uv,
			uv2 = mesh.uv2,
			colors = mesh.colors,
			tangents = mesh.tangents,
			normals = mesh.normals,
			subMeshCount = mesh.subMeshCount
		};
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			mesh2.SetTriangles(mesh.GetTriangles(i), i);
		}
		Vector3 lossyScale = sourceTransform.lossyScale;
		Quaternion rotation = sourceTransform.rotation;
		Vector3[] vertices = mesh2.vertices;
		for (int j = 0; j < vertices.Length; j++)
		{
			vertices[j] = rotation * Vector3.Scale(vertices[j], lossyScale);
		}
		mesh2.vertices = vertices;
		Vector3[] normals = mesh2.normals;
		if (normals != null && normals.Length != 0)
		{
			Matrix4x4 transpose = Matrix4x4.TRS(Vector3.zero, rotation, lossyScale).inverse.transpose;
			for (int k = 0; k < normals.Length; k++)
			{
				normals[k] = transpose.MultiplyVector(normals[k]).normalized;
			}
			mesh2.normals = normals;
		}
		if (lossyScale.x * lossyScale.y * lossyScale.z < 0f)
		{
			for (int l = 0; l < mesh2.subMeshCount; l++)
			{
				int[] triangles = mesh2.GetTriangles(l);
				for (int m = 0; m < triangles.Length; m += 3)
				{
					int num = triangles[m];
					triangles[m] = triangles[m + 1];
					triangles[m + 1] = num;
				}
				mesh2.SetTriangles(triangles, l);
			}
		}
		mesh2.RecalculateBounds();
		return mesh2;
	}
}
