using UnityEngine;

[ExecuteInEditMode]
public class WaterFloatingObject : MonoBehaviour
{
	public float amplitude = 0.04f;

	public float waveFrequency = 1f;

	public float waveSpeed = 1f;

	private float startPhase;

	private static Vector2 Floor(Vector2 v)
	{
		return new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
	}

	private static Vector3 Floor(Vector3 v)
	{
		return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
	}

	private static float Frac(float x)
	{
		return x - Mathf.Floor(x);
	}

	private static Vector3 Frac(Vector3 v)
	{
		return new Vector3(Frac(v.x), Frac(v.y), Frac(v.z));
	}

	private static Vector3 Abs(Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	private static Vector3 Max(Vector3 v, float val)
	{
		return new Vector3(Mathf.Max(v.x, val), Mathf.Max(v.y, val), Mathf.Max(v.z, val));
	}

	private static Vector3 Mod289(Vector3 x)
	{
		return x - Floor(x * 0.0034602077f) * 289f;
	}

	private static Vector2 Mod289(Vector2 x)
	{
		return x - Floor(x * 0.0034602077f) * 289f;
	}

	private static Vector3 Permute(Vector3 x)
	{
		return Mod289(Vector3.Scale(x * 34f + Vector3.one, x));
	}

	public static float Snoise(Vector2 v)
	{
		Vector4 vector = new Vector4(0.21132487f, 0.36602542f, -0.57735026f, 1f / 41f);
		Vector2 vector2 = Floor(v + Vector2.Dot(v, new Vector2(vector.y, vector.y)) * Vector2.one);
		Vector2 vector3 = v - vector2 + Vector2.Dot(vector2, new Vector2(vector.x, vector.x)) * Vector2.one;
		Vector2 vector4 = ((vector3.x > vector3.y) ? new Vector2(1f, 0f) : new Vector2(0f, 1f));
		Vector4 vector5 = new Vector4(vector3.x, vector3.y, vector3.x, vector3.y) + new Vector4(vector.x, vector.x, vector.z, vector.z);
		vector5.x -= vector4.x;
		vector5.y -= vector4.y;
		vector2 = Mod289(vector2);
		Vector3 a = Permute(Permute(new Vector3(vector2.y + 0f, vector2.y + vector4.y, vector2.y + 1f) + Vector3.one * vector2.x + new Vector3(0f, vector4.x, 1f)));
		float num = Vector2.Dot(vector3, vector3);
		Vector2 vector6 = new Vector2(vector5.x, vector5.y);
		Vector2 vector7 = new Vector2(vector5.z, vector5.w);
		float num2 = Vector2.Dot(vector6, vector6);
		float num3 = Vector2.Dot(vector7, vector7);
		Vector3 vector8 = Max(new Vector3(0.5f - num, 0.5f - num2, 0.5f - num3), 0f);
		vector8 = Vector3.Scale(vector8, vector8);
		vector8 = Vector3.Scale(vector8, vector8);
		Vector3 b = new Vector3(vector.w, vector.w, vector.w);
		Vector3 vector9 = 2f * Frac(Vector3.Scale(a, b)) - Vector3.one;
		Vector3 vector10 = Abs(vector9) - new Vector3(0.5f, 0.5f, 0.5f);
		Vector3 vector11 = Floor(vector9 + new Vector3(0.5f, 0.5f, 0.5f));
		Vector3 vector12 = vector9 - vector11;
		vector8 = Vector3.Scale(vector8, Vector3.one * 1.7928429f - 0.85373473f * (Vector3.Scale(vector12, vector12) + Vector3.Scale(vector10, vector10)));
		float x = vector12.x * vector3.x + vector10.x * vector3.y;
		float y = vector12.y * vector5.x + vector10.y * vector5.y;
		float z = vector12.z * vector5.z + vector10.z * vector5.w;
		Vector3 rhs = new Vector3(x, y, z);
		return 130f * Vector3.Dot(vector8, rhs);
	}

	private void Awake()
	{
		startPhase = Random.Range(0f, amplitude);
	}

	private void Update()
	{
		float num = Snoise(new Vector2(base.transform.position.x, base.transform.position.z) * waveFrequency) + startPhase;
		base.transform.localPosition = amplitude * new Vector3(0f, Mathf.Sin(Time.time * waveSpeed + num), 0f);
	}
}
