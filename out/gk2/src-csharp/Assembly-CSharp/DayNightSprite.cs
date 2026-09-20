using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteAlways]
public class DayNightSprite : DayNightLightBase
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		ApplyLightMode(mode);
	}

	protected override void ApplyLightMode(LightMode mode)
	{
		base.mode = mode;
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		if (!(spriteRenderer == null))
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			spriteRenderer.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetInteger(DayNightLightBase.idLightMode, (int)mode);
			spriteRenderer.SetPropertyBlock(materialPropertyBlock);
			LightFaker lightFaker = GetComponent<LightFaker>();
			if (lightFaker == null)
			{
				lightFaker = GetComponentInParent<LightFaker>();
			}
			if (lightFaker != null && lightFaker.mode != mode)
			{
				lightFaker.ApplyLightModeInt((int)mode);
			}
		}
	}
}
