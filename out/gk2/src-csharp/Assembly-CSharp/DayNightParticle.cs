using UnityEngine;

[ExecuteAlways]
public class DayNightParticle : DayNightLightBase
{
	[SerializeField]
	private ParticleSystemRenderer psRenderer;

	private MaterialPropertyBlock propertyBlock;

	private Material appliedMaterial;

	private int deferredReapplyFrames;

	private void Awake()
	{
		psRenderer = GetComponent<ParticleSystemRenderer>();
		ApplyLightMode(mode);
	}

	private void OnEnable()
	{
		if (psRenderer == null)
		{
			psRenderer = GetComponent<ParticleSystemRenderer>();
		}
		deferredReapplyFrames = 2;
		ApplyLightMode(mode);
		appliedMaterial = ((psRenderer != null) ? psRenderer.sharedMaterial : null);
	}

	private void LateUpdate()
	{
		if (psRenderer == null)
		{
			return;
		}
		Material sharedMaterial = psRenderer.sharedMaterial;
		if (deferredReapplyFrames > 0 || !(sharedMaterial == appliedMaterial))
		{
			if (deferredReapplyFrames > 0)
			{
				deferredReapplyFrames--;
			}
			ApplyLightMode(mode);
			appliedMaterial = sharedMaterial;
		}
	}

	protected override void ApplyLightMode(LightMode mode)
	{
		base.mode = mode;
		if (psRenderer == null)
		{
			psRenderer = GetComponent<ParticleSystemRenderer>();
		}
		if (!(psRenderer == null))
		{
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			psRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetInteger(DayNightLightBase.idLightMode, (int)mode);
			psRenderer.SetPropertyBlock(propertyBlock);
			Material material = (Application.isPlaying ? psRenderer.material : psRenderer.sharedMaterial);
			if (material != null && !material.IsKeywordEnabled(DayNightLightBase.idSunLightDependencyKeyword))
			{
				material.EnableKeyword(DayNightLightBase.idSunLightDependencyKeyword);
			}
		}
	}
}
