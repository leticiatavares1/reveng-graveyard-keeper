using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class SerializedMesh : MonoBehaviour
{
	private const int UVS_CHANNELS_COUNT = 8;

	[SerializeField]
	private UVs[] uvs;

	[SerializeField]
	private Vector3[] vertices;

	[SerializeField]
	private int[] triangles;

	[SerializeField]
	private bool isSerialized;

	[SerializeField]
	private MeshFilter meshFilter;

	public static bool debug;

	public MeshFilter MeshFilter
	{
		get
		{
			if (meshFilter != null)
			{
				return meshFilter;
			}
			meshFilter = GetComponent<MeshFilter>();
			return meshFilter;
		}
	}

	public Vector3[] Vertices => vertices;

	public void RebuildMesh()
	{
		if (!isSerialized)
		{
			Debug.LogError("Trying build the mesh before it's serialized. Be sure you were serialized it first");
			return;
		}
		Mesh mesh = new Mesh
		{
			vertices = vertices,
			triangles = triangles
		};
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		mesh.RecalculateBounds();
		for (int i = 0; i < 8; i++)
		{
			mesh.SetUVs(i, uvs[i].uvs);
		}
		MeshFilter.sharedMesh = mesh;
	}

	public void Serialize()
	{
		SerializeFrom(MeshFilter.sharedMesh);
	}

	public void SerializeFrom(Mesh mesh)
	{
		isSerialized = true;
		uvs = new UVs[8];
		vertices = mesh.vertices;
		triangles = mesh.triangles;
		for (int i = 0; i < 8; i++)
		{
			EnsureHasUVsByChannel(i);
			mesh.GetUVs(i, uvs[i].uvs);
		}
	}

	public void GetUVs(int channel, List<Vector4> uvs)
	{
		EnsureIsCorrectChannelValue(channel);
		uvs.Clear();
		uvs.AddRange(Enumerable.Repeat(Vector4.zero, vertices.Length));
		for (int i = 0; i < this.uvs[channel].uvs.Count; i++)
		{
			uvs[i] = this.uvs[channel].uvs[i];
		}
	}

	public void SetUVs(int channel, List<Vector4> uvs)
	{
		EnsureIsCorrectChannelValue(channel);
		EnsureHasUVsByChannel(channel);
		for (int i = 0; i < uvs.Count; i++)
		{
			this.uvs[channel].uvs[i] = uvs[i];
		}
		if (meshFilter != null)
		{
			meshFilter.sharedMesh.SetUVs(channel, uvs);
		}
	}

	public void SetUVs(int channel, Vector4[] uvs)
	{
		SetUVs(channel, uvs.ToList());
	}

	public void EnsureHasUVsByChannel(int channel)
	{
		EnsureIsCorrectChannelValue(channel);
		if (isSerialized)
		{
			int num = vertices.Length;
			UVs obj = uvs[channel];
			if (obj == null || obj.uvs.Count != num)
			{
				uvs[channel] = new UVs(num);
				uvs[channel].uvs.AddRange(Enumerable.Repeat(Vector4.zero, num));
			}
		}
	}

	private void Awake()
	{
		if (isSerialized)
		{
			RebuildMesh();
		}
	}

	private void Start()
	{
		_ = isSerialized;
	}

	private void EnsureIsCorrectChannelValue(int channel)
	{
		if (channel < 0 || channel > 7)
		{
			throw new ArgumentOutOfRangeException($"Invalid value for the channel: {channel}");
		}
	}
}
