using System;
using System.Text.RegularExpressions;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class Object3DSubMesh
{
	public enum MeshType
	{
		Regular,
		TreeCrown
	}

	private enum VegetationType
	{
		Tree,
		Bush
	}

	private struct MeshNameFlags
	{
		public bool IsCached;

		public bool IsWaterSurfaceMesh;

		public bool IsShadowMesh;

		public bool IsGardenPlant;

		public bool IsBlackoutMesh;

		public bool IsWindClothMesh;

		public bool IsTreeMesh;

		public bool IsTreeLeavesMesh;

		public static MeshNameFlags FromRenderer(Renderer renderer)
		{
			if (renderer == null || renderer.gameObject == null)
			{
				return default(MeshNameFlags);
			}
			string name = renderer.gameObject.name;
			bool flag = name.StartsWith("tree_");
			MeshNameFlags result = default(MeshNameFlags);
			result.IsCached = true;
			result.IsWaterSurfaceMesh = name.EndsWith("-ws") || name.EndsWith("_ws");
			result.IsShadowMesh = name.EndsWith("_sh") || name.EndsWith("-sh") || name.EndsWith("_sh2") || name.EndsWith("-sh2");
			result.IsGardenPlant = name.Contains("_plant_") && name.Contains("_stage_");
			result.IsBlackoutMesh = name.EndsWith("-blackout");
			result.IsWindClothMesh = name.EndsWith("-wind");
			result.IsTreeMesh = flag;
			result.IsTreeLeavesMesh = flag && name.Contains("_fm");
			return result;
		}
	}

	private const bool isEditor = false;

	private static readonly int matIdDestructionPhase = Shader.PropertyToID("_TreeDestrPhase");

	private static readonly int matIdChoppingPhase = Shader.PropertyToID("_TreeChopPhase");

	private static readonly int matIdIsTreeDestructing = Shader.PropertyToID("_TreeDestruction");

	private static readonly int matIdTreeHeight = Shader.PropertyToID("_TreeHeight");

	private static readonly float treeChopAmplitude = 0.45f;

	private static readonly float bushChopAmplitude = 0.125f;

	private LocalKeyword useReplaceColorLocalKeyword;

	[SerializeField]
	private MeshType type;

	[SerializeField]
	private Texture2D texture;

	[SerializeField]
	[HideInInspector]
	private bool isTextureSet;

	[SerializeField]
	private bool useCrownDeformOnly;

	[SerializeField]
	private VegetationType vegetationType;

	[SerializeField]
	private Texture2D texture2;

	[SerializeField]
	[HideInInspector]
	private bool isTexture2Set;

	[SerializeField]
	private Texture2D normalMap;

	[SerializeField]
	[HideInInspector]
	private bool isNormalSet;

	[SerializeField]
	private Texture2D lutTexture;

	[SerializeField]
	[HideInInspector]
	private bool isLutTextureSet;

	[SerializeField]
	private bool isOpaqueMesh;

	[SerializeField]
	private bool useBlueReplace;

	[SerializeField]
	private bool isWorldWindEnabled;

	[SerializeField]
	private bool ignoreTransparencySet;

	[SerializeField]
	[HideInInspector]
	private bool isUnlit;

	private MaterialPropertyBlock matPropertyBlock;

	private Material[] cachedSharedMaterials;

	private int materialIndex;

	private Renderer renderer;

	private Object3DMesh parentMesh;

	private MeshNameFlags meshNameFlags;

	private static readonly Regex TreeFsTextureRegex = new Regex("_fs\\d{1,2}$", RegexOptions.Compiled);

	private static readonly Regex TreeFmNumericTextureRegex = new Regex("_fm(\\d{1,2})$", RegexOptions.Compiled);

	public MeshType Type => type;

	public string MainTextureName => Texture?.name;

	public Texture2D Texture => texture;

	private Texture2D Texture2 => texture2;

	private Texture2D NormalMap => normalMap;

	public bool UseCrownDeformOnly => useCrownDeformOnly;

	public bool HasLutTexture => lutTexture;

	public Texture2D LutTexture => lutTexture;

	public bool IsShadowMesh
	{
		get
		{
			EnsureMeshNameFlagsCached();
			return meshNameFlags.IsShadowMesh;
		}
	}

	public bool IsGardenPlant
	{
		get
		{
			EnsureMeshNameFlagsCached();
			return meshNameFlags.IsGardenPlant;
		}
	}

	public void Init(Object3DMesh parentMesh, Renderer renderer, int materialIndex)
	{
		this.parentMesh = parentMesh;
		this.materialIndex = materialIndex;
		this.renderer = renderer;
		cachedSharedMaterials = null;
		CacheMeshNameFlags();
	}

	public void SetTexture(Texture2D texture)
	{
		if (!(texture == null))
		{
			if (type != MeshType.TreeCrown && IsTreeFsTexture(texture.name))
			{
				type = MeshType.TreeCrown;
			}
			this.texture = texture;
			ApplyMaterial();
		}
	}

	public void SetTextureOnly(Texture2D texture)
	{
		if (!(texture == null) && !(renderer == null))
		{
			this.texture = texture;
			if (matPropertyBlock == null)
			{
				matPropertyBlock = new MaterialPropertyBlock();
			}
			renderer.GetPropertyBlock(matPropertyBlock, materialIndex);
			matPropertyBlock.SetTexture(Object3DMesh.matIdMainTexture, texture);
			renderer.SetPropertyBlock(matPropertyBlock, materialIndex);
			isTextureSet = true;
		}
	}

	public void TrySetTexture2(Texture2D texture)
	{
		if (!(texture == null) && (!IsTreeFmNumericTexture(texture.name, out var digits) || this.texture.name.EndsWith("_fs" + digits)))
		{
			texture2 = texture;
			ApplyMaterial();
		}
	}

	public void SetNormalMap(Texture2D normalMap)
	{
		this.normalMap = normalMap;
		ApplyMaterial();
	}

	public void SetUnlit(bool isUnlit)
	{
		this.isUnlit = isUnlit;
		ApplyMaterial();
	}

	public void SetLutTexture(Texture2D texture)
	{
		lutTexture = texture;
		ApplyMaterial();
	}

	public void ApplyMaterial()
	{
		if (!(renderer == null))
		{
			ApplyAppropriateMaterial();
			EnsureInitialized();
			ForceSharedMaterials();
			ValidatePropertyBlock();
			matPropertyBlock.SetInt(Object3DMesh.matIdOpaqueShadowMesh, isOpaqueMesh ? 1 : 0);
			isTextureSet = TrySetPropertyBlockTexture(Object3DMesh.matIdMainTexture, Texture);
			isTexture2Set = TrySetPropertyBlockTexture(Object3DMesh.matIdMainTexture2, Texture2);
			isNormalSet = TrySetPropertyBlockTexture(Object3DMesh.matIdNormalTexture, NormalMap);
			isLutTextureSet = TrySetPropertyBlockTexture(Object3DMesh.matIdLutTexture, lutTexture);
			matPropertyBlock.SetFloat(Object3DMesh.matIdUseNormalMap, isNormalSet.ToInt());
			matPropertyBlock.SetFloat(Object3DMesh.matIdIsTreeCrown, (type == MeshType.TreeCrown).ToInt());
			switch (vegetationType)
			{
			case VegetationType.Tree:
				matPropertyBlock.SetFloat(Object3DMesh.matIdTreeChopAmplitude, treeChopAmplitude);
				break;
			case VegetationType.Bush:
				matPropertyBlock.SetFloat(Object3DMesh.matIdTreeChopAmplitude, bushChopAmplitude);
				break;
			}
			matPropertyBlock.SetFloat(Object3DMesh.matIdUseCrownDeformOnly, useCrownDeformOnly.ToInt());
			matPropertyBlock.SetFloat(Object3DMesh.matIdIsUnlit, isUnlit.ToInt());
			if (parentMesh.isTreeDestructing)
			{
				matPropertyBlock.SetFloat(matIdIsTreeDestructing, 1f);
				matPropertyBlock.SetFloat(matIdChoppingPhase, parentMesh.treeChoppingPhase);
				matPropertyBlock.SetFloat(matIdDestructionPhase, parentMesh.treeDestructionPhase);
				matPropertyBlock.SetFloat(matIdTreeHeight, parentMesh.treeHeight);
			}
			matPropertyBlock.SetFloat(Object3DMesh.matIdReplaceBlue, useBlueReplace.ToInt());
			if (useBlueReplace)
			{
				matPropertyBlock.SetColor(Object3DMesh.matIdReplaceBlueColor, parentMesh.isBlueReplacingToColor ? Object3DMesh.replaceBlueColor : Object3DMesh.replaceBlueTransparent);
			}
			matPropertyBlock.SetFloat(Object3DMesh.matIdAlphaCutoff, isUnlit ? 0f : 0.5f);
			matPropertyBlock.SetFloat(Object3DMesh.matIdTransparencyOcclusionValue, (!ignoreTransparencySet) ? parentMesh.transparencyValue : 0f);
			matPropertyBlock.SetFloat(Object3DMesh.matIdWorldWindEnabled, isWorldWindEnabled ? 1 : 0);
			matPropertyBlock.SetColor(Object3DMesh.matIdSelectionTintColor, parentMesh.SelectionTintColor);
			matPropertyBlock.SetFloat(Object3DMesh.matIdSelectionTintAmount, parentMesh.SelectionTintAmount);
			renderer.SetPropertyBlock(matPropertyBlock, materialIndex);
		}
	}

	public void SetSelectionTint(Color color, float amount)
	{
		if (!(renderer == null))
		{
			if (matPropertyBlock == null)
			{
				matPropertyBlock = new MaterialPropertyBlock();
			}
			renderer.GetPropertyBlock(matPropertyBlock, materialIndex);
			matPropertyBlock.SetColor(Object3DMesh.matIdSelectionTintColor, color);
			matPropertyBlock.SetFloat(Object3DMesh.matIdSelectionTintAmount, amount);
			renderer.SetPropertyBlock(matPropertyBlock, materialIndex);
		}
	}

	private void ForceSharedMaterials()
	{
		Material sharedMaterial = renderer.sharedMaterial;
		if (!(sharedMaterial == null))
		{
			Material matObject3DDeforming = LazySingletonSO<GlobalResources>.Instance.matObject3DDeforming;
			if (!(sharedMaterial == matObject3DDeforming) && sharedMaterial.name == matObject3DDeforming.name)
			{
				renderer.sharedMaterial = matObject3DDeforming;
				cachedSharedMaterials = null;
			}
		}
	}

	private void EnsureMeshNameFlagsCached()
	{
		if (!meshNameFlags.IsCached)
		{
			CacheMeshNameFlags();
		}
	}

	private void CacheMeshNameFlags()
	{
		meshNameFlags = MeshNameFlags.FromRenderer(renderer);
	}

	private Material[] GetSharedMaterials()
	{
		if (cachedSharedMaterials == null)
		{
			cachedSharedMaterials = renderer.sharedMaterials;
		}
		return cachedSharedMaterials;
	}

	private void EnsureInitialized()
	{
		if (matPropertyBlock == null)
		{
			matPropertyBlock = new MaterialPropertyBlock();
		}
	}

	private void ValidatePropertyBlock()
	{
		if (IsPropertyBlockTextureWasUnSet(Texture, isTextureSet) || IsPropertyBlockTextureWasUnSet(NormalMap, isNormalSet) || IsPropertyBlockTextureWasUnSet(Texture2, isTexture2Set))
		{
			matPropertyBlock = new MaterialPropertyBlock();
		}
		matPropertyBlock.Clear();
	}

	private void ClearPropertyBlockTexture(int propertyId)
	{
		matPropertyBlock.SetTexture(propertyId, Texture2D.blackTexture);
	}

	private bool TrySetPropertyBlockTexture(int propertyId, Texture2D texture2D)
	{
		if (texture2D != null)
		{
			matPropertyBlock.SetTexture(propertyId, texture2D);
		}
		return texture2D != null;
	}

	private bool IsPropertyBlockTextureWasUnSet(Texture2D texture2D, bool isSetFlag)
	{
		if (isSetFlag)
		{
			return texture2D == null;
		}
		return false;
	}

	private bool IsTreeFsTexture(string name)
	{
		return TreeFsTextureRegex.IsMatch(name);
	}

	private bool IsTreeFmNumericTexture(string name, out string digits)
	{
		Match match = TreeFmNumericTextureRegex.Match(name);
		digits = (match.Success ? match.Groups[1].Value : string.Empty);
		return match.Success;
	}

	private void ApplyAppropriateMaterial()
	{
		EnsureMeshNameFlagsCached();
		MeshNameFlags meshNameFlags = this.meshNameFlags;
		if (meshNameFlags.IsWaterSurfaceMesh)
		{
			return;
		}
		Material[] sharedMaterials = GetSharedMaterials();
		Material material = sharedMaterials[materialIndex];
		if (HasLutTexture)
		{
			Material matObject3DLUT = LazySingletonSO<GlobalResources>.Instance.matObject3DLUT;
			if (material != matObject3DLUT)
			{
				sharedMaterials[materialIndex] = matObject3DLUT;
				ApplyMaterials(sharedMaterials);
			}
		}
		else if (meshNameFlags.IsShadowMesh)
		{
			Material shadowMaterial = LazySingletonSO<GlobalResources>.Instance.shadowMaterial;
			if (material != shadowMaterial)
			{
				sharedMaterials[materialIndex] = shadowMaterial;
				ApplyMaterials(sharedMaterials);
			}
		}
		else if (meshNameFlags.IsGardenPlant)
		{
			Material matObject3DDeforming = LazySingletonSO<GlobalResources>.Instance.matObject3DDeforming;
			if (material != matObject3DDeforming)
			{
				sharedMaterials[materialIndex] = matObject3DDeforming;
				ApplyMaterials(sharedMaterials);
			}
		}
		else if (meshNameFlags.IsBlackoutMesh)
		{
			Material matBlackout = LazySingletonSO<GlobalResources>.Instance.matBlackout;
			if (material != matBlackout)
			{
				sharedMaterials[materialIndex] = matBlackout;
				ApplyMaterials(sharedMaterials);
			}
		}
		else if (meshNameFlags.IsWindClothMesh)
		{
			Material windClothMaterial = LazySingletonSO<GlobalResources>.Instance.windClothMaterial;
			if (material != windClothMaterial)
			{
				sharedMaterials[materialIndex] = windClothMaterial;
				ApplyMaterials(sharedMaterials);
			}
		}
		else if (meshNameFlags.IsTreeLeavesMesh)
		{
			Material material2;
			if (ignoreTransparencySet || parentMesh.transparencyValue.EqualsTo(0f))
			{
				material2 = LazySingletonSO<GlobalResources>.Instance.matObject3D;
				if (material == material2)
				{
					return;
				}
				LazySingletonSO<GlobalResources>.Instance.ReleaseTransparentMaterialFor(parentMesh);
			}
			else
			{
				LazySingletonSO<GlobalResources>.Instance.GetTransparentMaterialFor(parentMesh, LazySingletonSO<GlobalResources>.Instance.TransparentMaterialPlus1, out var material3, -0.01f);
				material2 = material3;
			}
			sharedMaterials[materialIndex] = material2;
			ApplyMaterials(sharedMaterials);
		}
		else if (meshNameFlags.IsTreeMesh)
		{
			Material material4;
			if (ignoreTransparencySet || parentMesh.transparencyValue.EqualsTo(0f))
			{
				material4 = LazySingletonSO<GlobalResources>.Instance.matObject3D;
				if (material == material4)
				{
					return;
				}
				LazySingletonSO<GlobalResources>.Instance.ReleaseTransparentMaterialFor(parentMesh);
			}
			else
			{
				LazySingletonSO<GlobalResources>.Instance.GetTransparentMaterialFor(parentMesh, LazySingletonSO<GlobalResources>.Instance.TransparentMaterial, out var material5);
				material4 = material5;
			}
			sharedMaterials[materialIndex] = material4;
			ApplyMaterials(sharedMaterials);
		}
		else
		{
			sharedMaterials[materialIndex] = LazySingletonSO<GlobalResources>.Instance.matObject3D;
			ApplyMaterials(sharedMaterials);
		}
	}

	private void ApplyMaterials(Material[] mats)
	{
		renderer.sharedMaterials = mats;
		cachedSharedMaterials = mats;
	}
}
