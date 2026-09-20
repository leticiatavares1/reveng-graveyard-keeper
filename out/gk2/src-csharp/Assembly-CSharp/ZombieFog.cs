using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ZombieFog : MonoBehaviour
{
	private static readonly int idFogColor = Shader.PropertyToID("_FColor");

	private static readonly int idFogParticlesColor = Shader.PropertyToID("_Tint");

	private static readonly int idFogParticlesColor2 = Shader.PropertyToID("_TintColor");

	private static readonly int idFogParticlesColor3 = Shader.PropertyToID("_Color");

	public List<MeshRenderer> renderers = new List<MeshRenderer>();

	public bool autoFindParticles;

	public List<ParticleSystem> particles = new List<ParticleSystem>();

	private List<ParticleSystemRenderer> particleRenderers = new List<ParticleSystemRenderer>();

	public float fadeDuration = 1f;

	public bool initialState = true;

	private Tween tween;

	private float curAlpha;

	private readonly List<Color> cachedColorsForObjs = new List<Color>();

	private readonly List<Color> cachedColorsForParticles = new List<Color>();

	public void DoFade(bool isActive)
	{
		float num = (isActive ? 0f : 1f);
		curAlpha = num;
		float endValue = (isActive ? 1f : 0f);
		tween?.Kill();
		base.gameObject.SetActive(value: true);
		tween = DOTween.To(() => curAlpha, delegate(float x)
		{
			curAlpha = x;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			foreach (MeshRenderer renderer in renderers)
			{
				renderer.GetPropertyBlock(materialPropertyBlock);
				Color color = materialPropertyBlock.GetColor(idFogColor);
				color.a = x;
				materialPropertyBlock.SetColor(idFogColor, color);
				renderer.SetPropertyBlock(materialPropertyBlock);
			}
			foreach (ParticleSystemRenderer particleRenderer in particleRenderers)
			{
				particleRenderer.GetPropertyBlock(materialPropertyBlock);
				Color color2 = materialPropertyBlock.GetColor(idFogParticlesColor);
				color2.a = x;
				materialPropertyBlock.SetColor(idFogParticlesColor, color2);
				particleRenderer.SetPropertyBlock(materialPropertyBlock);
			}
		}, endValue, fadeDuration).OnComplete(delegate
		{
			if (!isActive)
			{
				base.gameObject.SetActive(value: false);
			}
			ResetColors();
		});
	}

	public void ResetActiveState()
	{
		base.gameObject.SetActive(initialState);
		ResetColors();
	}

	private void ResetColors()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		for (int i = 0; i < renderers.Count; i++)
		{
			MeshRenderer meshRenderer = renderers[i];
			meshRenderer.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetColor(idFogColor, cachedColorsForObjs[i]);
			meshRenderer.SetPropertyBlock(materialPropertyBlock);
		}
		for (int j = 0; j < particleRenderers.Count; j++)
		{
			ParticleSystemRenderer particleSystemRenderer = particleRenderers[j];
			particleSystemRenderer.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetColor(GetColorId(materialPropertyBlock), cachedColorsForParticles[j]);
			particleSystemRenderer.SetPropertyBlock(materialPropertyBlock);
		}
	}

	private void Awake()
	{
		if (autoFindParticles)
		{
			particles = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>(includeInactive: true));
		}
		foreach (ParticleSystem particle in particles)
		{
			if (particle.TryGetComponent<ParticleSystemRenderer>(out var component) && component.enabled)
			{
				particleRenderers.Add(component);
			}
		}
		foreach (MeshRenderer renderer in renderers)
		{
			cachedColorsForObjs.Add(renderer.sharedMaterial.GetColor(idFogColor));
		}
		foreach (ParticleSystemRenderer particleRenderer in particleRenderers)
		{
			cachedColorsForParticles.Add(GetColor(particleRenderer.sharedMaterial));
		}
	}

	private int GetColorId(MaterialPropertyBlock propertyBlock)
	{
		if (!propertyBlock.HasColor(idFogParticlesColor))
		{
			if (!propertyBlock.HasColor(idFogParticlesColor2))
			{
				return idFogParticlesColor3;
			}
			return idFogParticlesColor2;
		}
		return idFogParticlesColor;
	}

	private Color GetColor(Material material)
	{
		if (!material.HasColor(idFogParticlesColor))
		{
			if (!material.HasColor(idFogParticlesColor2))
			{
				return material.GetColor(idFogParticlesColor3);
			}
			return material.GetColor(idFogParticlesColor2);
		}
		return material.GetColor(idFogParticlesColor);
	}

	private void Start()
	{
		FightingLevel componentInParent = GetComponentInParent<FightingLevel>();
		if (componentInParent != null)
		{
			componentInParent.ZombieFogs.Add(this);
		}
	}

	private void OnDestroy()
	{
		FightingLevel componentInParent = GetComponentInParent<FightingLevel>();
		if ((bool)componentInParent)
		{
			componentInParent.ZombieFogs.Remove(this);
		}
	}
}
