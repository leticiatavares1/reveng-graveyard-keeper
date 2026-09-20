using LazyBearTechnology;
using UnityEngine;

[DisallowMultipleComponent]
public class ChunkableObjectComponent : MonoBehaviour, IChunkableObject
{
	[SerializeField]
	protected ChunkBoundsPair bounds;

	[SerializeField]
	[HideInInspector]
	private Vector3 initialBoundsPosition;

	protected bool isVisible = true;

	private bool chunkBoundsCalculated;

	protected BurstableBounds chunkBounds;

	private bool customVisibilityDisabled;

	public bool CustomVisibilityDisabled
	{
		get
		{
			return customVisibilityDisabled;
		}
		set
		{
			if (customVisibilityDisabled != value)
			{
				customVisibilityDisabled = value;
				ApplyVisibility();
			}
		}
	}

	protected bool ShouldBeActive
	{
		get
		{
			if (isVisible)
			{
				return !customVisibilityDisabled;
			}
			return false;
		}
	}

	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	public virtual void CalculateChunkBounds()
	{
		initialBoundsPosition = base.transform.position;
		bounds = ChunkSizeCalculator.CalculateChunkBounds(base.gameObject);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		this.DrawChunkGizmos();
	}

	public BurstableBounds GetChunkableData()
	{
		if (!chunkBoundsCalculated)
		{
			chunkBounds = new BurstableBounds(bounds.GetBounds().center + base.transform.position - initialBoundsPosition, bounds.GetBounds().size);
			chunkBoundsCalculated = true;
			isVisible = base.gameObject.activeSelf;
		}
		return chunkBounds;
	}

	public virtual void UpdateChunkVisibility(bool isVisible)
	{
		this.isVisible = isVisible;
		ApplyVisibility();
	}

	protected virtual void ApplyVisibility()
	{
		bool shouldBeActive = ShouldBeActive;
		if (base.gameObject.activeSelf != shouldBeActive)
		{
			base.gameObject.SetActive(shouldBeActive);
		}
	}
}
