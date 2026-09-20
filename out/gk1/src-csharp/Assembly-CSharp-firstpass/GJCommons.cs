using System;
using System.Collections.Generic;
using UnityEngine;

public static class GJCommons
{
	public delegate void VoidDelegate();

	public delegate bool BoolDelegate();

	public class GJCommonsHelper : MonoBehaviour
	{
		public Vector3 main_camera_pos = Vector3.zero;

		public Vector2 main_camera_pos2 = Vector2.zero;

		public void Update()
		{
			main_camera_pos = main_camera_tf.position;
			main_camera_pos2 = main_camera_pos;
		}
	}

	private static long initial_ticks;

	private static float ticks_in_seconds;

	public static int current_timestamp;

	private static float _paused_time;

	private static float _pause_shift;

	private static GJCommonsHelper _helper;

	private static bool _helper_set;

	private static Transform _main_camera_tf;

	private static bool _main_camera_tf_set;

	public static Transform main_camera_tf
	{
		get
		{
			if (!_main_camera_tf_set)
			{
				_main_camera_tf_set = true;
				_main_camera_tf = Camera.main.transform;
			}
			return _main_camera_tf;
		}
	}

	private static GJCommonsHelper helper
	{
		get
		{
			if (_helper_set)
			{
				return _helper;
			}
			_helper = UnityEngine.Object.FindObjectOfType<GJCommonsHelper>();
			if (_helper != null)
			{
				_helper_set = true;
				return _helper;
			}
			GameObject gameObject = new GameObject("* GJCommonsHelper");
			gameObject.transform.parent = main_camera_tf;
			_helper = gameObject.AddComponent<GJCommonsHelper>();
			_helper_set = true;
			return _helper;
		}
	}

	public static Vector3 main_camera_pos => helper.main_camera_pos;

	public static Vector2 main_camera_pos_v2 => helper.main_camera_pos2;

	public static string ListToString<T>(List<T> list)
	{
		string text = "[List of <" + typeof(T)?.ToString() + ">: ";
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += list[i];
		}
		return text + "]";
	}

	public static string GetRomeNumber(int n, string one_char = "I")
	{
		return n switch
		{
			1 => one_char, 
			2 => one_char + one_char, 
			3 => one_char + one_char + one_char, 
			4 => one_char + "V", 
			5 => "V", 
			6 => "V" + one_char, 
			7 => "V" + one_char + one_char, 
			8 => one_char + one_char + "X", 
			9 => one_char + "X", 
			10 => "X", 
			_ => n.ToString(), 
		};
	}

	public static string GetTimeDebug()
	{
		return "<color=#006b70>[" + Time.time.ToString("f3") + "]</color> ";
	}

	public static string FormatNumber(int n)
	{
		if (n < 1000)
		{
			return n.ToString();
		}
		if (n < 1000000)
		{
			return $"'{n:### ###}'";
		}
		return $"'{n:### ### ###}'";
	}

	public static float GetTicksInSeconds()
	{
		if (initial_ticks == 0L)
		{
			initial_ticks = DateTime.Now.Ticks;
		}
		return (float)(DateTime.Now.Ticks - initial_ticks) / 10000000f - _pause_shift;
	}

	public static int GetDateTicksInSeconds()
	{
		DateTime dateTime = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
		return (int)(DateTime.UtcNow - dateTime).TotalSeconds;
	}

	public static bool BracketValidator(string s)
	{
		int num = 0;
		int num2 = 0;
		foreach (char num3 in s)
		{
			if (num3 == '(')
			{
				num++;
			}
			if (num3 == ')')
			{
				num2++;
			}
		}
		return num == num2;
	}

	public static float RoundFloatXX(float a)
	{
		return Mathf.Round(a * 10f) / 10f;
	}

	public static void OnApplicationPaused()
	{
		_paused_time = GetTicksInSeconds();
	}

	public static void OnApplicationUnpaused()
	{
		GetTicksInSeconds();
	}

	public static bool IsStringOnlyNumbers(ref string s)
	{
		if (s.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (c < '0' || c > '9')
			{
				return false;
			}
		}
		return true;
	}

	public static string RemoveLineDashes(string s)
	{
		return s.Replace("- ", "").Replace("-\n", "");
	}

	private static string MyEscape(string a)
	{
		return WWW.EscapeURL(a).Replace("+", "%20");
	}

	public static void SendEmail(string addr, string subject, string body)
	{
		Application.OpenURL("mailto:" + addr + "?subject=" + MyEscape(subject) + "&body=" + MyEscape(body));
	}

	public static float GetDialonalSize()
	{
		if (Screen.dpi == 0f)
		{
			return 10f;
		}
		float result = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / Screen.dpi;
		Debug.Log("Screen.dpi = " + Screen.dpi + ", diagonal = " + result);
		return result;
	}

	public static void SetLayerRecursively(this GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform item in obj.transform)
		{
			if (!(item == null))
			{
				item.gameObject.SetLayerRecursively(layer);
			}
		}
	}

	public static GameObject GameObjectHardFind(string str)
	{
		GameObject gameObject = null;
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>();
		foreach (Transform transform in array)
		{
			if (transform.parent == null)
			{
				gameObject = GameObjectHardFind(transform.gameObject, str, 0);
				if (gameObject != null)
				{
					break;
				}
			}
		}
		return gameObject;
	}

	public static GameObject GameObjectHardFind(string str, string parent)
	{
		GameObject gameObject = GameObjectHardFind(parent);
		if (gameObject == null)
		{
			return null;
		}
		return GameObjectHardFind(gameObject, str, 0);
	}

	private static GameObject GameObjectHardFind(GameObject item, string str, int index)
	{
		if (item.name == str)
		{
			return item;
		}
		if (index < item.transform.childCount)
		{
			GameObject gameObject = GameObjectHardFind(item.transform.GetChild(index).gameObject, str, 0);
			if (gameObject == null)
			{
				return GameObjectHardFind(item, str, ++index);
			}
			return gameObject;
		}
		return null;
	}

	public static void Destroy(UnityEngine.Object o)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(o);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(o);
		}
	}
}
