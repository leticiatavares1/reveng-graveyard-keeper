using UnityEngine;

[DefaultExecutionOrder(9000)]
public abstract class OptimizedCollider2D : MonoBehaviour
{
	public bool is_trigger = true;

	public bool always_visible = true;

	private bool _initialized;

	private Bounds _local_bounds;

	private bool _bounds_inited;

	public Bounds local_bounds
	{
		get
		{
			if (!_bounds_inited)
			{
				_bounds_inited = true;
				_local_bounds = CalculateLocalBounds();
			}
			return _local_bounds;
		}
	}

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			OnInit();
		}
	}

	protected abstract void OnInit();

	protected abstract Bounds CalculateLocalBounds();

	public void OnDrawGizmos()
	{
		if (always_visible)
		{
			OnDrawGizmosSelected();
		}
	}

	public virtual void OnDrawGizmosSelected()
	{
	}
}
