using UnityEngine;
using UnityEngine.Rendering;

public abstract class GenericSprite : CachedSpriteRenderer
{
	[SerializeField]
	protected bool castShadows;

	[SerializeField]
	protected bool invisibleSprite;

	[SerializeField]
	protected int layer;

	[SerializeField]
	[HideInInspector]
	protected int prevLayer;

	public int Layer
	{
		get
		{
			return layer;
		}
		set
		{
		}
	}

	protected virtual void Awake()
	{
		ApplyMaterial();
	}

	protected virtual void ApplyMaterial()
	{
		if (invisibleSprite)
		{
			base.SpriteRenderer.receiveShadows = false;
			base.SpriteRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
		}
		else
		{
			base.SpriteRenderer.receiveShadows = true;
			base.SpriteRenderer.shadowCastingMode = (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
		}
	}
}
