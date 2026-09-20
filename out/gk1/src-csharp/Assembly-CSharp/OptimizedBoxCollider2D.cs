using UnityEngine;

public class OptimizedBoxCollider2D : OptimizedCollider2D
{
	public Vector2 offset = Vector2.zero;

	public Vector2 size = Vector2.one;

	private BoxCollider2D _collider;

	protected override void OnInit()
	{
		_collider = base.gameObject.AddComponent<BoxCollider2D>();
		_collider.offset = offset;
		_collider.size = size;
		_collider.isTrigger = is_trigger;
	}

	public override void OnDrawGizmosSelected()
	{
	}

	protected override Bounds CalculateLocalBounds()
	{
		return new Bounds(offset, size);
	}
}
