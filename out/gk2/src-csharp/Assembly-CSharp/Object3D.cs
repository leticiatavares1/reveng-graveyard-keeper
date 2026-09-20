using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class Object3D : MonoBehaviour
{
	private const string TEXTURE_EXTENSION = ".png";

	private const string DEFAULT_HOR_SPRITE_NAME = "New Horizontal Sprite";

	private const string DEFAULT_VER_SPRITE_NAME = "New Vertical Sprite";

	public List<Object3DMeshData> modelsData = new List<Object3DMeshData>();

	public List<Object3DSpriteData> spritesData = new List<Object3DSpriteData>();

	[SerializeField]
	private List<Object3DMesh> object3DMeshes = new List<Object3DMesh>();

	[SerializeField]
	private List<HorizontalSprite> horizontalSprites = new List<HorizontalSprite>();

	[SerializeField]
	private List<VerticalSprite> verticalSprites = new List<VerticalSprite>();

	[SerializeField]
	private List<Collider> cachedColliders = new List<Collider>();

	private SpriteAtlas spriteAtlas;

	[SerializeField]
	[HideInInspector]
	private AssetReferenceAtlasedSprite spriteAtlasReference;

	[SerializeField]
	private List<Object3DMeshAnimation> meshAnimations = new List<Object3DMeshAnimation>();

	[SerializeField]
	[HideInInspector]
	private Animator animator;

	private bool isConstructorPart;

	private bool isVisible;

	private float transparencyValue;

	public SpriteAtlas SpriteAtlas => spriteAtlas;

	public float TransparencyValue
	{
		get
		{
			return transparencyValue;
		}
		set
		{
			transparencyValue = value;
			SetTransparency(value);
		}
	}

	public List<Object3DMesh> Object3DMeshes => object3DMeshes;

	public List<HorizontalSprite> HorizontalSprites => horizontalSprites;

	public List<Collider> CachedColliders => cachedColliders;

	private bool IsATree => object3DMeshes.Any((Object3DMesh mesh) => mesh?.IsATree ?? false);

	public void FixCollidersLossyScale()
	{
		foreach (Collider cachedCollider in cachedColliders)
		{
			if (!(cachedCollider == null) && cachedCollider.IsLossyScaleNegative() && cachedCollider is BoxCollider boxCol)
			{
				boxCol.FixBoxColliderLossyScale();
			}
		}
	}

	public void SetTreeHeightValue(float treeHeight)
	{
		if (!IsATree)
		{
			return;
		}
		foreach (Object3DMesh object3DMesh in object3DMeshes)
		{
			object3DMesh.treeHeight = treeHeight;
		}
	}

	public void SetSelectionTint(Color color, float amount)
	{
		for (int i = 0; i < object3DMeshes.Count; i++)
		{
			object3DMeshes[i]?.SetSelectionTint(color, amount);
		}
	}

	private void Awake()
	{
		SetAnimatableState(isAnimatable: false);
		FixCollidersLossyScale();
	}

	public void SetAnimatableState(bool isAnimatable)
	{
		foreach (Object3DMeshAnimation meshAnimation in meshAnimations)
		{
			meshAnimation.SetAnimatableState(isAnimatable);
		}
	}

	public void ResetToStaticState()
	{
		foreach (Object3DMeshAnimation meshAnimation in meshAnimations)
		{
			meshAnimation.ReturnToStaticState();
		}
	}

	private void SetTransparency(float transparencyValue)
	{
		foreach (Object3DMesh object3DMesh in Object3DMeshes)
		{
			object3DMesh.SetTransparency(transparencyValue);
		}
	}
}
