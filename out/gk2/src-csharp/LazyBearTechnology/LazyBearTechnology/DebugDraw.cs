using UnityEngine;

namespace LazyBearTechnology;

public static class DebugDraw
{
	public static void DrawCross(Vector3 p, float size, Color color, float duration = 1f, bool xyPlane = true)
	{
		float num = (xyPlane ? 1 : 0);
		float num2 = 1f - num;
		Debug.DrawLine(p + new Vector3(0f - size, 0f), p + new Vector3(size, 0f), color, duration);
		Debug.DrawLine(p + new Vector3(0f, (0f - size) * num, (0f - size) * num2), p + new Vector3(0f, size * num, size * num2), color, duration);
	}

	public static void DrawSquare(Vector3 center, float size, Color color, float duration = 1f, bool xyPlane = true)
	{
		float num = size * 0.5f;
		float num2 = (xyPlane ? 1 : 0);
		float num3 = 1f - num2;
		Vector3 vector = center + new Vector3(0f - num, (0f - num) * num2, (0f - num) * num3);
		Vector3 vector2 = center + new Vector3(0f - num, num * num2, num * num3);
		Vector3 vector3 = center + new Vector3(num, num * num2, num * num3);
		Vector3 vector4 = center + new Vector3(num, (0f - num) * num2, (0f - num) * num3);
		Debug.DrawLine(vector, vector2, color, duration);
		Debug.DrawLine(vector2, vector3, color, duration);
		Debug.DrawLine(vector3, vector4, color, duration);
		Debug.DrawLine(vector4, vector, color, duration);
	}

	public static void DrawBox(Vector3 center, Vector3 halfExtends, Color color, float duration = 1f)
	{
		Vector3 vector = center + new Vector3(halfExtends.x, halfExtends.y, halfExtends.z);
		Vector3 vector2 = center + new Vector3(0f - halfExtends.x, halfExtends.y, halfExtends.z);
		Vector3 vector3 = center + new Vector3(halfExtends.x, halfExtends.y, 0f - halfExtends.z);
		Vector3 end = center + new Vector3(0f - halfExtends.x, halfExtends.y, 0f - halfExtends.z);
		Vector3 vector4 = center + new Vector3(halfExtends.x, 0f - halfExtends.y, halfExtends.z);
		Vector3 vector5 = center + new Vector3(0f - halfExtends.x, 0f - halfExtends.y, halfExtends.z);
		Vector3 start = center + new Vector3(halfExtends.x, 0f - halfExtends.y, 0f - halfExtends.z);
		Vector3 vector6 = center + new Vector3(0f - halfExtends.x, 0f - halfExtends.y, 0f - halfExtends.z);
		Debug.DrawLine(vector, vector2, color, duration);
		Debug.DrawLine(vector2, end, color, duration);
		Debug.DrawLine(vector3, end, color, duration);
		Debug.DrawLine(vector3, vector, color, duration);
		Debug.DrawLine(vector4, vector5, color, duration);
		Debug.DrawLine(vector5, vector6, color, duration);
		Debug.DrawLine(start, vector6, color, duration);
		Debug.DrawLine(start, vector4, color, duration);
		Debug.DrawLine(vector4, vector, color, duration);
		Debug.DrawLine(vector5, vector2, color, duration);
		Debug.DrawLine(start, vector3, color, duration);
		Debug.DrawLine(vector6, end, color, duration);
	}
}
