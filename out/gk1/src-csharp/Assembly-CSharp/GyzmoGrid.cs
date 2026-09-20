using UnityEngine;

public class GyzmoGrid : MonoBehaviour
{
	public bool enabled_grid = true;

	public float gridScale = 1f;

	public int minX = -20;

	public int minY = -35;

	public int maxX = 15;

	public int maxY = 15;

	public Vector3 gridOffset = Vector3.zero;

	public bool topDownGrid = true;

	public int gizmoMajorLines = 5;

	public Color gizmoLineColor = new Color(0.4f, 0.4f, 0.3f, 1f);

	public Color build_grid_color = new Color(0.4f, 0.4f, 0.3f, 0.3f);

	private void OnDrawGizmos()
	{
		if (!enabled_grid)
		{
			return;
		}
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Color color = new Color(gizmoLineColor.r, gizmoLineColor.g, gizmoLineColor.b, 0.25f * gizmoLineColor.a);
		Color color2 = Color.Lerp(Color.white, gizmoLineColor, 0.75f);
		for (int i = minX; i < maxX + 1; i++)
		{
			Gizmos.color = ((i % gizmoMajorLines == 0) ? gizmoLineColor : color);
			if (i == 0)
			{
				Gizmos.color = color2;
			}
			for (int j = 0; j < 3; j++)
			{
				float x = (float)i + (float)j * (1f / 3f);
				if (j != 0)
				{
					Gizmos.color = build_grid_color;
				}
				Vector3 vector = new Vector3(x, minY, 0f) * gridScale;
				Vector3 vector2 = new Vector3(x, maxY, 0f) * gridScale;
				if (topDownGrid)
				{
					vector = new Vector3(vector.x, 0f, vector.y);
					vector2 = new Vector3(vector2.x, 0f, vector2.y);
				}
				Gizmos.DrawLine(gridOffset + vector, gridOffset + vector2);
			}
		}
		for (int k = minY; k < maxY + 1; k++)
		{
			Gizmos.color = ((k % gizmoMajorLines == 0) ? gizmoLineColor : color);
			if (k == 0)
			{
				Gizmos.color = color2;
			}
			for (int l = 0; l < 3; l++)
			{
				float y = (float)k + (float)l * (1f / 3f);
				if (l != 0)
				{
					Gizmos.color = build_grid_color;
				}
				Vector3 vector3 = new Vector3(minX, y, 0f) * gridScale;
				Vector3 vector4 = new Vector3(maxX, y, 0f) * gridScale;
				if (topDownGrid)
				{
					vector3 = new Vector3(vector3.x, 0f, vector3.y);
					vector4 = new Vector3(vector4.x, 0f, vector4.y);
				}
				Gizmos.DrawLine(gridOffset + vector3, gridOffset + vector4);
			}
		}
	}
}
