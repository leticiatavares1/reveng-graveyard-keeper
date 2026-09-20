using System;
using UnityEngine;

namespace LazyBearTechnology;

public static class LazyMesh
{
	public static Mesh CloneMesh(Mesh mesh)
	{
		Mesh mesh2 = new Mesh();
		CloneMeshInto(mesh2, mesh);
		mesh2.name = mesh.name;
		return mesh2;
	}

	public static void CloneMeshInto(Mesh dest, Mesh src)
	{
		dest.colors = src.colors;
		dest.vertices = src.vertices;
		dest.normals = src.normals;
		dest.triangles = src.triangles;
		dest.tangents = src.tangents;
		dest.uv = src.uv;
		dest.uv2 = src.uv2;
		dest.uv3 = src.uv3;
		dest.uv4 = src.uv4;
		dest.uv5 = src.uv5;
		dest.uv6 = src.uv6;
		dest.uv7 = src.uv7;
		dest.uv8 = src.uv8;
	}

	private static T[] MergeArrays<T>(T[] array1, T[] array2)
	{
		T[] array3 = new T[array1.Length + array2.Length];
		Array.Copy(array1, array3, array1.Length);
		Array.Copy(array2, 0, array3, array1.Length, array2.Length);
		return array3;
	}
}
