using System.Collections.Generic;
using UnityEngine;

public static class LazyTerrainSurfaceUtility
{
	public static GameObject GenerateObjectWithColliderFromSprite(Sprite sprite, float height)
	{
		GameObject gameObject = new GameObject(sprite.name);
		List<Vector2> list = new List<Vector2>();
		sprite.GetPhysicsShape(0, list);
		List<Vector3> list2 = new List<Vector3>();
		List<int> list3 = new List<int>();
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(new Vector3(list[i].x, list[i].y, 0f));
			list2.Add(new Vector3(list[i].x, list[i].y, height));
		}
		for (int j = 0; j < list.Count; j++)
		{
			int num2 = num + j * 2;
			int num3 = num + (j + 1) % list.Count * 2;
			list3.Add(num2);
			list3.Add(num2 + 1);
			list3.Add(num3);
			list3.Add(num3);
			list3.Add(num2 + 1);
			list3.Add(num3 + 1);
		}
		int count = list2.Count;
		int item = count + 1;
		list2.Add(new Vector3(0f, 0f, 0f));
		list2.Add(new Vector3(0f, 0f, height));
		for (int k = 0; k < list.Count; k++)
		{
			int num4 = (k + 1) % list.Count;
			int num5 = k * 2;
			int item2 = num5 + 1;
			list3.Add(count);
			list3.Add(num5);
			list3.Add(num4 * 2);
			list3.Add(item2);
			list3.Add(item);
			list3.Add(num4 * 2 + 1);
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list2.ToArray();
		mesh.triangles = list3.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
		meshCollider.convex = true;
		meshRenderer.enabled = false;
		meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
		return gameObject;
	}
}
