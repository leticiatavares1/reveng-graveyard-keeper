using LazyBearTechnology;
using UnityEngine;

public class ConstructorPart : MonoBehaviour, IChunkableObject
{
	[HideInInspector]
	public ConstructorPartChildData constructorPartChildData = new ConstructorPartChildData();

	private bool isVisible;

	public bool HasChildPath
	{
		get
		{
			if (constructorPartChildData != null)
			{
				return !string.IsNullOrEmpty(constructorPartChildData.pathToObject);
			}
			return false;
		}
	}

	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	public bool IgnoreChunkVisibility => false;

	public void SetLutTexture(Texture2D texture)
	{
		constructorPartChildData.lut = texture;
		if (!TryGetComponent<Object3D>(out var component))
		{
			component = GetComponentInChildren<Object3D>();
			if (component == null)
			{
				return;
			}
		}
		foreach (Object3DMesh object3DMesh in component.Object3DMeshes)
		{
			object3DMesh.SetLutTexture(texture);
		}
	}

	public BurstableBounds GetChunkableData()
	{
		return constructorPartChildData.chunkBounds.GetBounds();
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		this.DrawChunkGizmos();
	}

	public void UpdateChunkVisibility(bool isVisible)
	{
		if (this.isVisible == isVisible)
		{
			return;
		}
		this.isVisible = isVisible;
		if (isVisible)
		{
			constructorPartChildData.view = ConstructorPartPool.Get(constructorPartChildData.pathToObject);
			if (constructorPartChildData.view == null)
			{
				this.isVisible = false;
				return;
			}
			constructorPartChildData.view.transform.SetParent(base.transform);
			constructorPartChildData.view.transform.localScale = constructorPartChildData.localScale;
			constructorPartChildData.view.transform.rotation = constructorPartChildData.rotation;
			constructorPartChildData.view.transform.localPosition = constructorPartChildData.localPosition;
			if (!(constructorPartChildData.lut != null))
			{
				return;
			}
			if (!TryGetComponent<Object3D>(out var component))
			{
				component = GetComponentInChildren<Object3D>();
				if (component == null)
				{
					return;
				}
			}
			{
				foreach (Object3DMesh object3DMesh in component.Object3DMeshes)
				{
					object3DMesh.SetLutTexture(constructorPartChildData.lut);
				}
				return;
			}
		}
		if (constructorPartChildData.view != null)
		{
			ConstructorPartPool.Release(constructorPartChildData.pathToObject, constructorPartChildData.view);
			constructorPartChildData.view = null;
		}
	}
}
