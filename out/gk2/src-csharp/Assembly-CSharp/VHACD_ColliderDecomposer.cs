using System.Collections.Generic;
using MeshProcess;
using UnityEngine;
using UnityEngine.ProBuilder;

[RequireComponent(typeof(MeshCollider))]
public class VHACD_ColliderDecomposer : VHACD
{
	public const string DecomposedConvexMeshColliderName = "DecomposedConvexMeshCollider";

	[SerializeField]
	private MeshCollider mainMeshCollider;

	[SerializeField]
	private MeshFilter mainMeshFilter;

	[SerializeField]
	private PolyShape mainPolyShape;

	[SerializeField]
	private GameObject decomposedMeshColliderObj;

	[SerializeField]
	private List<MeshCollider> decomposedMeshColliders = new List<MeshCollider>();

	public bool IsValid
	{
		get
		{
			if (decomposedMeshColliderObj != null)
			{
				return decomposedMeshColliders.Count > 0;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (IsValid)
		{
			if ((bool)mainMeshCollider)
			{
				mainMeshCollider.sharedMesh = null;
				mainMeshCollider.enabled = false;
			}
			if (mainMeshFilter != null && !TryGetComponent<MeshRenderer>(out var _))
			{
				Object.Destroy(mainMeshFilter);
			}
			if (mainPolyShape != null)
			{
				Object.Destroy(mainPolyShape);
			}
		}
	}
}
