using System;
using System.Collections.Generic;
using UnityEngine;

public class GJLine
{
	private static UI2DSprite _pixel_prefab;

	private static Stack<UI2DSprite> _free = new Stack<UI2DSprite>();

	private static List<UI2DSprite> _used = new List<UI2DSprite>();

	private static GameObject _root;

	private static UI2DSprite DrawPixel(Transform parent, Vector2 coords)
	{
		UI2DSprite uI2DSprite = ((_free.Count != 0) ? _free.Pop() : UnityEngine.Object.Instantiate(_pixel_prefab));
		_used.Add(uI2DSprite);
		uI2DSprite.gameObject.SetActive(value: true);
		uI2DSprite.transform.parent = parent;
		uI2DSprite.transform.localScale = Vector3.one;
		uI2DSprite.transform.localPosition = coords;
		return uI2DSprite;
	}

	private static void RemovePixel(UI2DSprite p)
	{
		p.gameObject.SetActive(value: false);
		_used.Remove(p);
		_free.Push(p);
		p.transform.parent = _root.transform;
	}

	private static void PreCache(int n)
	{
		for (int i = 0; i < n; i++)
		{
			UI2DSprite uI2DSprite = UnityEngine.Object.Instantiate(_pixel_prefab);
			uI2DSprite.gameObject.SetActive(value: false);
			uI2DSprite.transform.parent = _root.transform;
			_free.Push(uI2DSprite);
		}
	}

	public static void Init(GameObject pixel_prefab)
	{
		_root = new GameObject("GJ Line Prefabs");
		_pixel_prefab = pixel_prefab.GetComponent<UI2DSprite>();
		PreCache(500);
	}

	private static void Swap(ref int v1, ref int v2)
	{
		int num = v1;
		v1 = v2;
		v2 = num;
	}

	public static GameObject DrawLine(Transform parent, int x0, int y0, int x1, int y1, Color color, int every_n_pixel = 1)
	{
		GameObject gameObject = new GameObject("Line");
		gameObject.transform.parent = parent;
		gameObject.transform.localScale = Vector3.one;
		bool flag = false;
		if (Math.Abs(x0 - x1) < Math.Abs(y0 - y1))
		{
			Swap(ref x0, ref y0);
			Swap(ref x1, ref y1);
			flag = true;
		}
		if (x0 > x1)
		{
			Swap(ref x0, ref x1);
			Swap(ref y0, ref y1);
		}
		int num = x1 - x0;
		float num2 = Math.Abs((float)(y1 - y0) / (float)num);
		float num3 = 0f;
		int num4 = y0;
		for (int i = x0; i <= x1; i++)
		{
			int num5 = (flag ? num4 : i);
			int num6 = (flag ? i : num4);
			if (every_n_pixel == 1 || num5 % every_n_pixel == 0)
			{
				DrawPixel(gameObject.transform, new Vector2(num5, num6)).color = color;
			}
			num3 += num2;
			if ((double)num3 > 0.5)
			{
				num4 += ((y1 > y0) ? 1 : (-1));
				num3 -= 1f;
			}
		}
		return gameObject;
	}

	public static void DrawLine(Texture2D tx, int x0, int y0, int x1, int y1, Color color, int every_n_pixel = 1)
	{
		bool flag = false;
		if (Math.Abs(x0 - x1) < Math.Abs(y0 - y1))
		{
			Swap(ref x0, ref y0);
			Swap(ref x1, ref y1);
			flag = true;
		}
		if (x0 > x1)
		{
			Swap(ref x0, ref x1);
			Swap(ref y0, ref y1);
		}
		int num = x1 - x0;
		float num2 = Math.Abs((float)(y1 - y0) / (float)num);
		float num3 = 0f;
		int num4 = y0;
		for (int i = x0; i <= x1; i++)
		{
			int num5 = (flag ? num4 : i);
			int y2 = (flag ? i : num4);
			if (every_n_pixel == 1 || num5 % every_n_pixel == 0)
			{
				tx.SetPixel(num5, y2, color);
			}
			num3 += num2;
			if ((double)num3 > 0.5)
			{
				num4 += ((y1 > y0) ? 1 : (-1));
				num3 -= 1f;
			}
		}
	}

	public static void RemoveLine(GameObject line)
	{
		UI2DSprite[] componentsInChildren = line.GetComponentsInChildren<UI2DSprite>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RemovePixel(componentsInChildren[i]);
		}
		UnityEngine.Object.Destroy(line);
	}

	public static List<GameObject> DrawLineGraph(UIWidget graph_area, List<Vector2> points, Color color, int every_n_pixel = 1)
	{
		List<GameObject> list = new List<GameObject>();
		int width = graph_area.width;
		int height = graph_area.height;
		int num = int.MinValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MaxValue;
		Transform transform = graph_area.transform;
		foreach (Vector2 point in points)
		{
			num = Math.Max(num, (int)point.x);
			num2 = Math.Max(num2, (int)point.y);
			num3 = Math.Min(num3, (int)point.x);
			num4 = Math.Min(num4, (int)point.y);
		}
		float num5 = ((float)num - (float)num3 * 1f) / (float)width;
		float num6 = ((float)num2 - (float)num4 * 1f) / (float)height;
		Vector2 vector = new Vector2(num3, num4);
		int num7 = height / 2;
		int num8 = width / 2;
		for (int i = 1; i < points.Count; i++)
		{
			Vector2 vector2 = points[i - 1] - vector;
			vector2 = new Vector2(vector2.x / num5 - (float)num8, vector2.y / num6 + (float)num7);
			Vector2 vector3 = points[i] - vector;
			vector3 = new Vector2(vector3.x / num5 - (float)num8, vector3.y / num6 + (float)num7);
			list.Add(DrawLine(transform, (int)vector2.x, (int)vector2.y, (int)vector3.x, (int)vector3.y, color, every_n_pixel));
		}
		return list;
	}

	public static Vector2 DrawLineGraph(Texture2D tx, List<Vector2> points, Color color, Vector2? min = null, Vector2? max = null, int every_n_pixel = 1)
	{
		int width = tx.width;
		int height = tx.height;
		int num = int.MinValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MaxValue;
		foreach (Vector2 point in points)
		{
			num = Math.Max(num, (int)point.x);
			num2 = Math.Max(num2, (int)point.y);
			num3 = Math.Min(num3, (int)point.x);
			num4 = Math.Min(num4, (int)point.y);
		}
		if (min.HasValue)
		{
			num3 = (int)min.Value.x;
			num4 = (int)min.Value.y;
		}
		if (max.HasValue)
		{
			num = (int)max.Value.x;
			num2 = (int)max.Value.y;
		}
		float num5 = ((float)num - (float)num3 * 1f) / (float)width;
		float num6 = ((float)num2 - (float)num4 * 1f) / (float)height;
		Vector2 vector = new Vector2(num3, num4);
		for (int i = 1; i < points.Count; i++)
		{
			Vector2 vector2 = points[i - 1] - vector;
			vector2 = new Vector2(Mathf.Max(0f, vector2.x / num5), Mathf.Max(0f, vector2.y / num6));
			Vector2 vector3 = points[i] - vector;
			vector3 = new Vector2(Mathf.Max(0f, vector3.x / num5), Mathf.Max(0f, vector3.y / num6));
			DrawLine(tx, (int)vector2.x, (int)vector2.y, (int)vector3.x, (int)vector3.y, color, every_n_pixel);
		}
		return new Vector2(num5, num6);
	}
}
