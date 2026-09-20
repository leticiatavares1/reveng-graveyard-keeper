using UnityEngine;

public class OptimizedPointCollider2D : OptimizedCollider2D
{
	public Vector2 offset = Vector2.zero;

	private CircleCollider2D _collider;

	protected override void OnInit()
	{
		_collider = base.gameObject.AddComponent<CircleCollider2D>();
		_collider.offset = offset;
		_collider.radius = 0.01f;
		_collider.isTrigger = is_trigger;
	}

	public override void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		float x = base.transform.lossyScale.x;
		Gizmos.DrawWireSphere(base.transform.position + (Vector3)offset * x, 0.01f * x);
	}

	protected override Bounds CalculateLocalBounds()
	{
		return new Bounds(offset, Vector3.one * 0.01f);
	}
}
