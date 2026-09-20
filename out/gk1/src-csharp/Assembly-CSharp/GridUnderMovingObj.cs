using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

public static class GridUnderMovingObj
{
	public static Vector2 obj_size = Vector2.zero;

	public static IntVector2 obj_size_min = new IntVector2();

	public static IntVector2 obj_size_max = new IntVector2();

	public static bool UpdateObjSize(WorldGameObject obj)
	{
		return true;
	}

	private static bool DoesObjectStandsOnPoint(List<Collider2D> obj_colliders, Vector2 pos, int x, int y)
	{
		Vector2 vector = pos + new Vector2(x * 32, y * 32);
		Collider2D[] array = Physics2D.OverlapBoxAll(vector, BuildGrid.GRID_CHECK_BOX_SIZE, 0f, 1);
		Vector2 vector2 = pos;
		new GameObject("* test " + vector2.ToString()).transform.position = vector;
		bool result = false;
		Collider2D[] array2 = array;
		foreach (Collider2D item in array2)
		{
			if (obj_colliders.Contains(item))
			{
				result = true;
			}
		}
		return result;
	}

	public static void UpdateColor()
	{
	}
}
