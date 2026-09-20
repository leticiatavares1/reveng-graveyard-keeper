using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(MeshRenderer))]
public class Object3DOptimizedPart : MonoBehaviour
{
	public bool isShadow;

	[SerializeField]
	private SpriteAtlas sourceAtlas;

	[SerializeField]
	private Texture2D lutTexture;

	private static readonly int matIdMainTexture = Shader.PropertyToID("_MainTex");

	private static readonly int matIdLutTexture = Shader.PropertyToID("_ReplaceLUT");

	private Texture2D cachedAtlasTexture;

	private static readonly Dictionary<string, Texture2D> atlasTextureCache = new Dictionary<string, Texture2D>();

	private MaterialPropertyBlock mpb;

	private Renderer cachedRenderer;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStaticCaches()
	{
		atlasTextureCache.Clear();
	}

	public static void ClearAtlasTextureCache()
	{
		atlasTextureCache.Clear();
	}

	public void SetSourceAtlas(SpriteAtlas atlas)
	{
		sourceAtlas = atlas;
		cachedAtlasTexture = null;
	}

	public void SetLutTexture(Texture2D texture)
	{
		lutTexture = texture;
	}

	public void ApplyPropertyBlock()
	{
		if (cachedRenderer == null)
		{
			cachedRenderer = GetComponent<Renderer>();
		}
		if (cachedRenderer == null)
		{
			return;
		}
		if (!cachedAtlasTexture)
		{
			cachedAtlasTexture = ResolveAtlasTexture(sourceAtlas);
		}
		if ((bool)cachedAtlasTexture)
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			cachedRenderer.GetPropertyBlock(mpb);
			mpb.SetTexture(matIdMainTexture, cachedAtlasTexture);
			if (lutTexture != null)
			{
				mpb.SetTexture(matIdLutTexture, lutTexture);
			}
			cachedRenderer.SetPropertyBlock(mpb);
		}
	}

	private void Awake()
	{
		ApplyPropertyBlock();
	}

	private static Texture2D ResolveAtlasTexture(SpriteAtlas atlas)
	{
		if (atlas == null || atlas.spriteCount == 0)
		{
			return null;
		}
		string key = atlas.tag;
		if (atlasTextureCache.TryGetValue(key, out var value))
		{
			if ((bool)value)
			{
				return value;
			}
			atlasTextureCache.Remove(key);
		}
		Sprite[] array = new Sprite[atlas.spriteCount];
		atlas.GetSprites(array);
		Sprite[] array2 = array;
		foreach (Sprite sprite in array2)
		{
			if (sprite != null && (bool)sprite.texture)
			{
				atlasTextureCache[key] = sprite.texture;
				return sprite.texture;
			}
		}
		return null;
	}
}
