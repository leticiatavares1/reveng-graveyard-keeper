using UnityEngine;

public class OptimizedCircleCollider2D : OptimizedCollider2D
{
	public Vector2 offset = Vector2.zero;

	public float radius = 1f;

	private CircleCollider2D _collider;

	private const bool DRAW_AS_POLYGONS = true;

	protected override void OnInit()
	{
		_collider = base.gameObject.AddComponent<CircleCollider2D>();
		_collider.offset = offset;
		_collider.radius = radius;
		_collider.isTrigger = is_trigger;
	}

	public override void OnDrawGizmosSelected()
	{
	}

	protected override Bounds CalculateLocalBounds()
	{
		return new Bounds(offset, Vector2.one * radius * 2f);
	}
}
