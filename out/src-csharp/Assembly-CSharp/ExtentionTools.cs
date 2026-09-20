using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ExtentionTools
{
	public static bool EqualsTo(this float a, float b, float epsilon = 1E-05f)
	{
		return Mathf.Abs(a - b) < epsilon;
	}

	public static bool EqualsOrMore(this float a, float b, float epsilon = 1E-05f)
	{
		if (Mathf.Abs(a - b) < epsilon)
		{
			return true;
		}
		return a > b;
	}

	public static bool EqualsTo(this Vector2 a, Vector2 b, float epsilon = 1E-05f)
	{
		if (a.x.EqualsTo(b.x, epsilon))
		{
			return a.y.EqualsTo(b.y, epsilon);
		}
		return false;
	}

	public static Vector2 Round(this Vector2 vec, float value)
	{
		if (value.EqualsTo(0f))
		{
			return vec;
		}
		vec.x = Mathf.Round(vec.x / value) * value;
		vec.y = Mathf.Round(vec.y / value) * value;
		return vec;
	}

	public static Vector3 Round(this Vector3 vec, float value = 1f)
	{
		if (value.EqualsTo(0f))
		{
			return vec;
		}
		return new Vector3(Mathf.Round(vec.x / value) * value, Mathf.Round(vec.y / value) * value, Mathf.Round(vec.z / value) * value);
	}

	public static void RemoveNulls<T>(this List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == null)
			{
				list.RemoveAt(i);
				i--;
			}
		}
	}

	public static void RemoveUnityNulls<T>(this List<T> list) where T : UnityEngine.Object
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (!(list[i] != null))
			{
				list.RemoveAt(i);
				i--;
			}
		}
	}

	public static T Copy<T>(this T source, Transform parent = null, bool activate = true, string name = "") where T : MonoBehaviour
	{
		if (source == null)
		{
			Debug.LogError("Null prefab");
			return null;
		}
		T val = UnityEngine.Object.Instantiate(source);
		val.gameObject.transform.SetParent(parent ?? source.gameObject.transform.parent, worldPositionStays: false);
		val.gameObject.SetActive(activate);
		if (!string.IsNullOrEmpty(name))
		{
			val.name = name;
		}
		return val;
	}

	public static GameObject Copy(this GameObject source, Transform parent = null, bool activate = true, string name = "")
	{
		if (source == null)
		{
			Debug.LogError("Null prefab");
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(source);
		gameObject.transform.SetParent(parent ?? source.transform.parent, worldPositionStays: false);
		gameObject.SetActive(activate);
		if (!string.IsNullOrEmpty(name))
		{
			gameObject.name = name;
		}
		return gameObject;
	}

	public static void Activate<T>(this T behaviour) where T : MonoBehaviour
	{
		if (behaviour != null)
		{
			behaviour.gameObject.SetActive(value: true);
		}
	}

	public static void Deactivate<T>(this T behaviour) where T : MonoBehaviour
	{
		if (behaviour != null)
		{
			behaviour.gameObject.SetActive(value: false);
		}
	}

	public static void Activate(this GameObject obj)
	{
		if (obj != null)
		{
			obj.SetActive(value: true);
		}
	}

	public static void Deactivate(this GameObject obj)
	{
		if (obj != null)
		{
			obj.SetActive(value: false);
		}
	}

	public static void SetActive(this MonoBehaviour behaviour, bool active)
	{
		if ((bool)behaviour)
		{
			behaviour.gameObject.SetActive(active);
		}
	}

	public static bool HasParam(this Animator animator, string param_name)
	{
		AnimatorControllerParameter[] parameters = animator.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == param_name)
			{
				return true;
			}
		}
		return false;
	}

	public static string ColorizeText(this string text, Color color)
	{
		return "[c][" + color.ToHex() + "]" + text + "[-][/c]";
	}

	public static string ToHex(this Color color, bool alpha = false)
	{
		Color32 color2 = color;
		string text = color2.r.ToString("X2") + color2.g.ToString("X2") + color2.b.ToString("X2");
		if (alpha)
		{
			text += color2.a.ToString("X2");
		}
		return text;
	}

	public static void TryInvoke(this GJCommons.VoidDelegate callback)
	{
		callback?.Invoke();
	}

	public static void TryInvoke(this Action callback)
	{
		callback?.Invoke();
	}

	public static void TryInvoke<T>(this Action<T> callback, T obj)
	{
		callback?.Invoke(obj);
	}

	public static void TryExecute(this EventDelegate event_delegate)
	{
		event_delegate?.Execute();
	}

	public static void TrySetActive(this GameObject go, bool active)
	{
		if (go != null)
		{
			go.SetActive(active);
		}
	}

	public static void RoundCamPos(this Transform tf, Camera cam, Vector3 offset = default(Vector3), float pixel_k = 1f)
	{
		tf.localPosition = offset;
		tf.position = tf.position.ReturnRoundedPos(cam, cam, zero_z: false, pixel_k);
	}

	public static void SetGUIPosToWorldPos(this Transform tf, Vector3 position, Camera world_cam, Camera gui_cam, Vector2 shift, bool halfres_magic = true)
	{
		Vector3 vector = position.ReturnRoundedPos(world_cam, gui_cam, zero_z: true, 1f, halfres_magic);
		tf.position = vector + (Vector3)shift;
	}

	public static void SetGUIPosToWorldPos(this Transform tf, Vector3 position, Camera world_cam, Camera gui_cam)
	{
		tf.SetGUIPosToWorldPos(position, world_cam, gui_cam, Vector2.zero);
	}

	public static Vector3 ReturnRoundedPos(this Vector3 position, Camera world_cam, Camera obj_cam, bool zero_z = false, float pixel_k = 1f, bool halfres_magic = false)
	{
		float z = (zero_z ? 0f : position.z);
		position = world_cam.WorldToScreenPoint(position);
		position.x = Mathf.Round(position.x / pixel_k) * pixel_k;
		position.y = Mathf.Round(position.y / pixel_k) * pixel_k;
		position = obj_cam.ScreenToWorldPoint(position);
		position.z = z;
		return position;
	}

	public static T Cache<T>(this MonoBehaviour beh, out T cached, out bool flag, bool deep) where T : Component
	{
		cached = null;
		T[] components = beh.GetComponents<T>();
		foreach (T val in components)
		{
			if (!(val == null))
			{
				cached = val;
				break;
			}
		}
		if (cached == null && deep)
		{
			components = beh.GetComponentsInChildren<T>(includeInactive: true);
			foreach (T val2 in components)
			{
				if (!(val2 == null))
				{
					cached = val2;
					break;
				}
			}
		}
		flag = cached != null;
		return cached;
	}

	public static T AddComponentNotDuplicate<T>(this GameObject obj) where T : Component
	{
		T component = obj.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		return obj.AddComponent<T>();
	}

	public static void DestroyComponentIfExists<T>(this GameObject obj) where T : MonoBehaviour
	{
		if (obj.GetComponent<T>() != null)
		{
			obj.Destroy<T>();
		}
	}

	public static void Destroy(this GameObject go, float delay = 0f)
	{
		if (go == null)
		{
			Debug.LogError("Can't destroy null obj");
		}
		else if (!Application.isPlaying)
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
		else if (delay > 0f)
		{
			UnityEngine.Object.Destroy(go, delay);
		}
		else
		{
			UnityEngine.Object.Destroy(go);
		}
	}

	public static void Destroy<T>(this GameObject go, float delay = 0f) where T : MonoBehaviour
	{
		if (go == null)
		{
			Debug.LogError("Can't destroy null obj");
		}
		else
		{
			go.GetComponent<T>().DestroyComponent();
		}
	}

	public static void DestroyComponent(this MonoBehaviour component, float delay = 0f)
	{
		if (component == null)
		{
			Debug.LogError("Can't destroy null component");
		}
		else if (!Application.isPlaying)
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
		else if (delay > 0f)
		{
			UnityEngine.Object.Destroy(component, delay);
		}
		else
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	public static void DestroyGO<T>(this T component, float delay = 0f) where T : MonoBehaviour
	{
		if (component == null)
		{
			Debug.LogError("Can't destroy null component");
		}
		else
		{
			component.gameObject.Destroy(delay);
		}
	}

	public static void SetXY(this Transform tf, Vector2 pos)
	{
		Vector3 position = new Vector3(pos.x, pos.y, tf.position.z);
		tf.position = position;
	}

	public static GameObject GetFirstParent(this GameObject go)
	{
		GameObject gameObject = go;
		while (gameObject.transform.parent != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
		}
		return gameObject;
	}

	public static void MakeStatic(this Rigidbody2D body)
	{
		if (body.bodyType != RigidbodyType2D.Static)
		{
			body.bodyType = RigidbodyType2D.Static;
		}
	}

	public static void MakeDynamic(this Rigidbody2D body)
	{
		if (body.bodyType != 0)
		{
			body.bodyType = RigidbodyType2D.Dynamic;
		}
	}

	public static T RandomElement<T>(this List<T> list, bool remove_element = false)
	{
		if (list == null || list.Count == 0)
		{
			return default(T);
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		T result = list[index];
		if (remove_element)
		{
			list.RemoveAt(index);
		}
		return result;
	}

	public static T LastElement<T>(this List<T> list)
	{
		if (list == null || list.Count == 0)
		{
			return default(T);
		}
		return list[list.Count - 1];
	}

	public static string JoinToString<T>(this List<T> list, string separator = ",")
	{
		if (list == null || list.Count == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T item in list)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(item);
		}
		return stringBuilder.ToString();
	}

	public static bool PolygonContainsPoint(Vector2[] polygon, Vector2 point)
	{
		int num = polygon.Length;
		int num2 = 0;
		bool flag = false;
		float x = point.x;
		float y = point.y;
		Vector2 vector = polygon[num - 1];
		float x2 = vector.x;
		float y2 = vector.y;
		while (num2 < num)
		{
			float num3 = x2;
			float num4 = y2;
			Vector2 vector2 = polygon[num2++];
			x2 = vector2.x;
			y2 = vector2.y;
			flag ^= ((y2 > y) ^ (num4 > y)) && x - x2 < (y - y2) * (num3 - x2) / (num4 - y2);
		}
		return flag;
	}

	public static void SetAlpha(this Color c, float a)
	{
		c.a = a;
	}

	public static void SetX(this Vector3 v, float x)
	{
		v.x = x;
	}

	public static void SetY(this Vector3 v, float y)
	{
		v.y = y;
	}

	public static string ConcatWithSeparator(this string s, string ss, string separator = "\n")
	{
		string text = s;
		if (!string.IsNullOrEmpty(s))
		{
			text += separator;
		}
		return text + ss;
	}
}
