using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Global Resources")]
public class GlobalResources : LazySingletonSO<GlobalResources>
{
	private const int DEFAULT_TRANSPARENT_MATERIAL_IDX = 4;

	private const int TRANSPARENT_MATERIALS_COUNT = 9;

	[Header("Materials")]
	public Material matObject3D;

	public Material matObject3DDeforming;

	public Material matObject3DLUT;

	public Material[] matsObject3DTransparent = new Material[9];

	public Material matBlackout;

	public Material matHorizontalSprite;

	public Material matHorizontalSpriteVertLight;

	public Material matVerticalSprite;

	public Material matVerticalSpriteShadowCaster;

	public Material matGround;

	public Material shadowMaterial;

	public Material cloudsMaterial;

	public Material windClothMaterial;

	public Material matDeformingGrass;

	public Material fullTransparent;

	public Material fakeLightMaterial;

	[Header("Shaders")]
	public Shader smartRasterizerShader;

	[Header("Meshes")]
	public Mesh humanoidSizedPlaneMesh;

	[Header("FX")]
	public FXSettings fxSettings;

	[Header("Fighting")]
	public FightingGlobal fighting;

	private (float, Object3DMesh)[] obj3DMeshWithZPos = new(float, Object3DMesh)[9];

	private Dictionary<Object3DMesh, int> object3DMeshes = new Dictionary<Object3DMesh, int>();

	private int lastOccupiedIdx = -1;

	private static GlobalResources aliveInstance;

	public Material TransparentMaterial => matsObject3DTransparent[4];

	public Material TransparentMaterialPlus1 => matsObject3DTransparent[5];

	private void OnEnable()
	{
		aliveInstance = this;
		ClearTransparentMaterialAllocations();
	}

	private void OnDisable()
	{
		if ((object)aliveInstance == this)
		{
			aliveInstance = null;
		}
	}

	public static void TryReleaseTransparentMaterialFor(Object3DMesh mesh)
	{
		if (!(aliveInstance == null))
		{
			aliveInstance.ReleaseTransparentMaterialFor(mesh);
		}
	}

	private void ClearTransparentMaterialAllocations()
	{
		object3DMeshes.Clear();
		obj3DMeshWithZPos = new(float, Object3DMesh)[9];
		lastOccupiedIdx = -1;
	}

	private void PurgeStaleTransparentMaterialAllocations()
	{
		if (lastOccupiedIdx < 0)
		{
			return;
		}
		(float, Object3DMesh)[] array = new(float, Object3DMesh)[9];
		int num = 0;
		bool flag = false;
		for (int i = 0; i <= lastOccupiedIdx; i++)
		{
			Object3DMesh item = obj3DMeshWithZPos[i].Item2;
			if (item == null)
			{
				flag = true;
				continue;
			}
			array[num] = obj3DMeshWithZPos[i];
			object3DMeshes[item] = num;
			num++;
		}
		if (!flag)
		{
			return;
		}
		List<Object3DMesh> list = null;
		foreach (KeyValuePair<Object3DMesh, int> object3DMesh in object3DMeshes)
		{
			if (object3DMesh.Key == null)
			{
				if (list == null)
				{
					list = new List<Object3DMesh>();
				}
				list.Add(object3DMesh.Key);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				object3DMeshes.Remove(list[j]);
			}
		}
		obj3DMeshWithZPos = array;
		lastOccupiedIdx = num - 1;
	}

	public bool GetTransparentMaterialFor(Object3DMesh mesh, Material defaultIfNotGet, out Material material, float zOffset = 0f)
	{
		material = defaultIfNotGet;
		if (mesh == null)
		{
			return false;
		}
		if (object3DMeshes.TryGetValue(mesh, out var value))
		{
			material = matsObject3DTransparent[value];
			return true;
		}
		PurgeStaleTransparentMaterialAllocations();
		if (lastOccupiedIdx + 1 >= 9)
		{
			return false;
		}
		for (int i = 0; i < 9; i++)
		{
			float num = mesh.transform.position.z + zOffset;
			if (i <= lastOccupiedIdx && !(obj3DMeshWithZPos[i].Item1 - num).EqualsOrMore(0f))
			{
				continue;
			}
			lastOccupiedIdx++;
			(float, Object3DMesh)[] array = new(float, Object3DMesh)[9];
			for (int j = 0; j < i; j++)
			{
				array[j] = obj3DMeshWithZPos[j];
			}
			array[i] = (num, mesh);
			object3DMeshes.Add(mesh, i);
			for (int k = i; k < Mathf.Min(8, lastOccupiedIdx); k++)
			{
				int num2 = k + 1;
				array[num2] = obj3DMeshWithZPos[k];
				Object3DMesh item = obj3DMeshWithZPos[k].Item2;
				if (!(item == null))
				{
					object3DMeshes[item] = num2;
					item.ApplyMaterials();
				}
			}
			obj3DMeshWithZPos = array;
			return true;
		}
		return false;
	}

	public void ReleaseTransparentMaterialFor(Object3DMesh mesh)
	{
		if (mesh == null || !object3DMeshes.Remove(mesh, out var value))
		{
			return;
		}
		(float, Object3DMesh)[] array = new(float, Object3DMesh)[9];
		for (int i = 0; i < value; i++)
		{
			array[i] = obj3DMeshWithZPos[i];
		}
		for (int j = value; j < Mathf.Min(8, lastOccupiedIdx); j++)
		{
			(float, Object3DMesh) tuple = (array[j] = obj3DMeshWithZPos[j + 1]);
			if (tuple.Item2 != null)
			{
				object3DMeshes[tuple.Item2] = j;
			}
		}
		obj3DMeshWithZPos = array;
		lastOccupiedIdx = Mathf.Clamp(--lastOccupiedIdx, -1, 8);
	}
}
