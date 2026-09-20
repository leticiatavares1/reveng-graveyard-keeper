using UnityEngine;

public class OptimizedPolygonCollider2D : OptimizedCollider2D
{
	public Vector2 offset = Vector2.zero;

	public Vector2[] points;

	private PolygonCollider2D _collider;

	private Mesh _mesh;

	private const bool DRAW_AS_MESH = true;

	protected override void OnInit()
	{
		SpriteRenderer component = base.gameObject.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			Sprite sprite = component.sprite;
			component.sprite = null;
			AddPolygonCollider();
			component.sprite = sprite;
		}
		else
		{
			AddPolygonCollider();
		}
	}

	private void AddPolygonCollider()
	{
		_collider = base.gameObject.AddComponent<PolygonCollider2D>();
		_collider.offset = offset;
		_collider.isTrigger = is_trigger;
		_collider.points = points;
	}

	public override void OnDrawGizmosSelected()
	{
	}

	public void RebuildColliderMesh()
	{
		_mesh = GetGizmosMesh();
	}

	protected override Bounds CalculateLocalBounds()
	{
		Bounds result = default(Bounds);
		for (int i = 0; i < points.Length; i++)
		{
			Vector2 vector = points[i];
			if (i == 0)
			{
				result = new Bounds(vector + offset, Vector3.one * 0.001f);
			}
			else
			{
				result.Encapsulate(new Bounds(vector + offset, Vector3.one * 0.001f));
			}
		}
		return result;
	}

	private Mesh GetGizmosMesh()
	{
		Vector3[] array = new Vector3[points.Length * 2];
		for (int i = 0; i < points.Length; i++)
		{
			array[i * 2] = new Vector3(points[i].x, points[i].y, 0f);
			array[i * 2 + 1] = new Vector3(points[i].x + 0.0001f, points[i].y, 0f);
		}
		int num = points.Length;
		int[] array2 = new int[num * 3];
		for (int j = 0; j < num; j++)
		{
			array2[j * 3] = j * 2;
			array2[j * 3 + 1] = j * 2 + 1;
			array2[j * 3 + 2] = ((j != num - 1) ? (j * 2 + 2) : 0);
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array;
		mesh.triangles = array2;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}
}
