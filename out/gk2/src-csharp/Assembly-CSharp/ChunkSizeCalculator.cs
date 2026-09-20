using UnityEngine;
using UnityEngine.Rendering;

public static class ChunkSizeCalculator
{
	public static ChunkBoundsPair CalculateChunkBounds(GameObject obj)
	{
		Vector3 position = obj.transform.position;
		Bounds boundsWithShadows = new Bounds(position, Vector3.zero);
		bool boundsWithShadowsWereSet = false;
		Bounds boundsWithoutShadows = new Bounds(position, Vector3.zero);
		bool boundsWithoutShadowsWereSet = false;
		float shadowBoundExtends = 4f;
		if (obj.TryGetComponent<Renderer>(out var component))
		{
			EncapsulateRenderer(component);
		}
		if (obj.TryGetComponent<Light>(out var component2))
		{
			EncapsulateLight(component2);
		}
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			EncapsulateRenderer(componentsInChildren[i]);
		}
		Light[] componentsInChildren2 = obj.GetComponentsInChildren<Light>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			EncapsulateLight(componentsInChildren2[j]);
		}
		ChunkBoundsContributor[] componentsInChildren3 = obj.GetComponentsInChildren<ChunkBoundsContributor>(includeInactive: true);
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			EncapsulateContributor(componentsInChildren3[k]);
		}
		return new ChunkBoundsPair(boundsWithShadows, boundsWithoutShadows);
		void EncapsulateBounds(Bounds extraBounds)
		{
			EncapsulateWithShadowsBounds(extraBounds);
			EncapsulateWithoutShadowsBounds(extraBounds);
		}
		void EncapsulateContributor(ChunkBoundsContributor contributor)
		{
			Bounds worldBounds3;
			if (contributor.UseSeparateBounds)
			{
				if (contributor.TryGetBoundsWithShadows(out var worldBounds))
				{
					EncapsulateWithShadowsBounds(worldBounds);
				}
				if (contributor.TryGetBoundsWithoutShadows(out var worldBounds2))
				{
					EncapsulateWithoutShadowsBounds(worldBounds2);
				}
			}
			else if (contributor.TryGetBounds(out worldBounds3))
			{
				EncapsulateBounds(worldBounds3);
			}
		}
		void EncapsulateLight(Light lightComponent)
		{
			Vector3 lossyScale = lightComponent.transform.lossyScale;
			float num = lightComponent.range * Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
			EncapsulateBounds(new Bounds(lightComponent.transform.position, Vector3.one * num * 2f));
		}
		void EncapsulateRenderer(Renderer rendererComponent)
		{
			if (rendererComponent.enabled && rendererComponent.GetComponent<ParticleSystem>() == null)
			{
				Bounds bounds = rendererComponent.bounds;
				if (!boundsWithoutShadowsWereSet)
				{
					boundsWithoutShadows = bounds;
					boundsWithoutShadowsWereSet = true;
				}
				else
				{
					boundsWithoutShadows.Encapsulate(bounds);
				}
				if (rendererComponent.shadowCastingMode == ShadowCastingMode.On)
				{
					Vector3 vector = new Vector3(bounds.size.y * shadowBoundExtends, 0f, bounds.size.y * shadowBoundExtends);
					Vector3 size = bounds.size;
					if (vector.x > size.x || vector.z > size.z)
					{
						bounds.Expand(new Vector3(Mathf.Max(0f, vector.x - size.x), 0f, Mathf.Max(0f, vector.z - size.z)));
					}
				}
				if (!boundsWithShadowsWereSet)
				{
					boundsWithShadows = bounds;
					boundsWithShadowsWereSet = true;
				}
				else
				{
					boundsWithShadows.Encapsulate(bounds);
				}
			}
		}
		void EncapsulateWithShadowsBounds(Bounds extraBounds)
		{
			if (!boundsWithShadowsWereSet)
			{
				boundsWithShadows = extraBounds;
				boundsWithShadowsWereSet = true;
			}
			else
			{
				boundsWithShadows.Encapsulate(extraBounds);
			}
		}
		void EncapsulateWithoutShadowsBounds(Bounds extraBounds)
		{
			if (!boundsWithoutShadowsWereSet)
			{
				boundsWithoutShadows = extraBounds;
				boundsWithoutShadowsWereSet = true;
			}
			else
			{
				boundsWithoutShadows.Encapsulate(extraBounds);
			}
		}
	}
}
