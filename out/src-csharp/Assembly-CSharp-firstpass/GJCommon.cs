using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public class GJCommon
{
	public static bool DEBUG_MEMORY = false;

	public static string LOREM_IPSUM_SHORT = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

	public static string LOREM_IPSUM = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

	public static bool USE_ETC_TEXTURES = false;

	public static bool PVR_SUPPORTED = true;

	public static string DEFAULT_SHADER_NAME = "ex2D/Alpha Blended";

	public static string BASE_62 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public static string BASE_READABLE = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

	public static byte[] MergeByteArrays(byte[] b1, byte[] b2)
	{
		byte[] array = new byte[b1.Length + b2.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((i < b1.Length) ? b1[i] : b2[i - b1.Length]);
		}
		return array;
	}

	public static byte[] MergeByteArrays(byte[] b1, List<byte> b2)
	{
		byte[] array = new byte[b1.Length + b2.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((i < b1.Length) ? b1[i] : b2[i - b1.Length]);
		}
		return array;
	}

	public static byte[] ReadBinaryDataFromStream(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		if (binaryReader == null)
		{
			Debug.LogError("ReadBinaryDataFromStream(" + stream?.ToString() + ") error!");
			return null;
		}
		List<byte> list = new List<byte>();
		try
		{
			while (true)
			{
				byte item = binaryReader.ReadByte();
				list.Add(item);
			}
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
		binaryReader.Close();
		return list.ToArray();
	}

	public static byte[] ReadBinaryResource(string resource_name)
	{
		TextAsset textAsset = Resources.Load(resource_name) as TextAsset;
		if (textAsset == null)
		{
			Debug.LogError("Error ReadBinaryResource '" + resource_name + "' !");
			return null;
		}
		Stream stream = new MemoryStream(textAsset.bytes);
		byte[] result = ReadBinaryDataFromStream(stream as MemoryStream);
		stream.Close();
		return result;
	}

	public static string Base64Encode(byte[] b)
	{
		return Convert.ToBase64String(b).Replace("+", "-").Replace("/", "_")
			.Split("="[0])[0];
	}

	public static string Base64Encode(string s)
	{
		return Base64Encode(Encoding.UTF8.GetBytes(s));
	}

	public static byte[] Base64Decode(string s)
	{
		s = s.Replace("-", "+");
		s = s.Replace("_", "/");
		return Convert.FromBase64String(s);
	}

	public static int Checksum(byte[] b)
	{
		int num = 0;
		for (int i = 0; i < b.Length; i++)
		{
			num += b[i];
		}
		return num;
	}

	public static int CountTrailingNumbers(string s)
	{
		int num = 0;
		while (s.Substring(s.Length - 1)[0] >= "0"[0] && s.Substring(s.Length - 1)[0] <= "9"[0])
		{
			num++;
			s = s.Substring(0, s.Length - 1);
		}
		return num;
	}

	public static string FormatTime(int seconds, string days_suffix, string hours_suffix, string min_suffix, string sec_suffix)
	{
		int num = (int)Mathf.Floor(seconds / 86400);
		seconds -= num * 86400;
		int num2 = (int)Mathf.Floor(seconds / 3600);
		seconds -= num2 * 3600;
		int num3 = (int)Mathf.Floor(seconds / 60);
		seconds -= num3 * 60;
		int num4 = seconds;
		string text = num4.ToString() ?? "";
		string text2 = num3.ToString() ?? "";
		string text3 = num2.ToString() ?? "";
		string text4 = num.ToString() ?? "";
		if (min_suffix == ":" && num4 < 10)
		{
			text = "0" + text;
		}
		if (hours_suffix == ":" && num3 < 10)
		{
			text2 = "0" + text2;
		}
		if (num > 0)
		{
			return text4 + days_suffix + text3 + hours_suffix + text2;
		}
		if (num2 > 0)
		{
			return text3 + hours_suffix + text2 + min_suffix + text + sec_suffix;
		}
		if (num3 > 0)
		{
			return text2 + min_suffix + text + sec_suffix;
		}
		if (min_suffix == ":")
		{
			text = "00:" + text;
		}
		return text + sec_suffix;
	}

	public static string FormatInt(int i)
	{
		string text = i.ToString() ?? "";
		string text2 = "";
		int num = 0;
		while (true)
		{
			text2 = text[text.Length - 1] + text2;
			if (text.Length == 1)
			{
				break;
			}
			text = text.Substring(0, text.Length - 1);
			if (++num > 2)
			{
				text2 = " " + text2;
				num = 0;
			}
		}
		return text2;
	}

	public static List<Rect> FillRectWithSquares(List<int> available_squares, Vector2 rect)
	{
		List<Rect> list = new List<Rect>();
		int num = (int)rect.x;
		int capacity = (int)rect.y;
		List<List<bool>> list2 = new List<List<bool>>(num);
		for (int i = 0; i < num; i++)
		{
			list2[i] = new List<bool>(capacity);
		}
		for (int j = 0; (float)j < rect.x; j++)
		{
			for (int k = 0; (float)k < rect.y; k++)
			{
				list2[j][k] = false;
			}
		}
		List<int> list3 = new List<int>();
		list3.AddRange(available_squares);
		list3.Sort();
		for (int j = 0; (float)j < rect.x; j++)
		{
			for (int k = 0; (float)k < rect.y; k++)
			{
				if (list2[j][k])
				{
					continue;
				}
				int num2 = list3.Count;
				int num3 = 0;
				while (num2 > 0)
				{
					num2--;
					num3 = list3[num2];
					if ((float)(j + num3) <= rect.x && (float)(k + num3) <= rect.y)
					{
						bool flag = true;
						for (int l = 0; l < num3; l++)
						{
							for (int m = 0; m < num3; m++)
							{
								if (list2[j + l][k + m])
								{
									flag = false;
									break;
								}
							}
							if (!flag)
							{
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (num2 == 0)
					{
						Debug.LogWarning("FillRectWithSquares() couldn't fill rectangle correctly! (Maybe you don't have 1x1 square?)\nDidn't fill square at (" + j + ", " + k + ") of square " + rect.x + "x" + rect.y);
					}
				}
				int n = 0;
				int num4 = 0;
				try
				{
					for (n = 0; n < num3; n++)
					{
						for (num4 = 0; num4 < num3; num4++)
						{
							list2[j + n][k + num4] = true;
						}
					}
				}
				catch (Exception)
				{
					Debug.LogError("Map is out of range: " + (j + n) + ", " + (k + num4));
				}
				list.Add(new Rect(j, k, num3, num3));
			}
		}
		return list;
	}

	public static byte[] StringToByteArray(string s)
	{
		byte[] array = new byte[s.Length];
		for (int i = 0; i < s.Length; i++)
		{
			array[i] = (byte)s[i];
		}
		return array;
	}

	public static string MemoryToString(long mem)
	{
		if (mem < 1024)
		{
			return mem + " b";
		}
		if (mem < 1048576)
		{
			return Mathf.Round(mem / 1024) + " Kb";
		}
		return Mathf.Round(mem / 1048576) + " Mb";
	}

	public static UnityEngine.Object LoadResource(string res_name)
	{
		long num = Profiler.usedHeapSize;
		UnityEngine.Object @object = Resources.Load(res_name);
		if (@object == null)
		{
			Debug.LogWarning("Error loading resource \"" + res_name + "\" - resource not found!");
			return null;
		}
		if (DEBUG_MEMORY)
		{
			Debug.Log("Resource \"" + res_name + "\" loaded. Memory spent: " + MemoryToString(Profiler.usedHeapSize - num));
		}
		return @object;
	}

	public static UnityEngine.Object LoadResource(string res_name, Type res_class)
	{
		long num = Profiler.usedHeapSize;
		UnityEngine.Object @object = Resources.Load(res_name, res_class);
		if (@object == null)
		{
			Debug.LogWarning("Error loading resource \"" + res_name + "\" - resource not found!");
			return null;
		}
		if (DEBUG_MEMORY)
		{
			Debug.Log("Resource \"" + res_name + "\" loaded. Memory spent: " + MemoryToString(Profiler.usedHeapSize - num));
		}
		return @object;
	}

	public static bool ArrayContains(int[] ar, int item)
	{
		for (int num = ar.Length - 1; num >= 0; num--)
		{
			if (ar[num] == item)
			{
				return true;
			}
		}
		return false;
	}

	public static int ConvertStringDateToTimestamp(string s)
	{
		string[] array = s.Split("."[0]);
		if (array.Length != 3)
		{
			Debug.LogError("Unknown date format: " + s);
			return 0;
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return (int)(new DateTime(int.Parse(array[2]), int.Parse(array[1]), int.Parse(array[0]), 0, 0, 0, DateTimeKind.Utc) - dateTime).TotalSeconds;
	}

	public static string ToAnyBase(int n, string chars)
	{
		string text = "";
		int length = chars.Length;
		do
		{
			text = chars[n % length] + text;
			n = (int)Mathf.Floor((float)n * 1f / (float)length);
		}
		while (n > 0);
		return text;
	}

	public static int FromAnyBase(string s, string chars)
	{
		int num = 0;
		int length = chars.Length;
		for (int i = 0; i < s.Length; i++)
		{
			num = num * length + chars.IndexOf(s[i]);
		}
		return num;
	}

	public static string ToBase62(int n)
	{
		return ToAnyBase(n, BASE_62);
	}

	public static int FromBase62(string s)
	{
		return FromAnyBase(s, BASE_62);
	}

	public static int GetNowTimestamp()
	{
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return (int)(DateTime.UtcNow - dateTime).TotalSeconds;
	}

	public static int GetYearDay()
	{
		return DateTime.Now.DayOfYear;
	}

	private static string MD5Sum(string strToEncrypt)
	{
		byte[] bytes = new UTF8Encoding().GetBytes(strToEncrypt);
		byte[] array = new MD5CryptoServiceProvider().ComputeHash(bytes);
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, "0"[0]);
		}
		return text.PadLeft(32, "0"[0]);
	}

	public static string FixURLString(string s)
	{
		s = s.Replace("&quot;", "\"");
		s = s.Replace("&#xA;", "\n");
		return s;
	}

	public static string URLEncode(string s)
	{
		s = s.Replace(" ", "%20");
		s = s.Replace("?", "%3F");
		s = s.Replace("&", "%26");
		s = s.Replace("=", "%3D");
		s = s.Replace(",", "%2C");
		s = s.Replace("'", "%27");
		s = s.Replace("\"", "%22");
		s = s.Replace("$", "%24");
		s = s.Replace("\r", "%0D");
		s = s.Replace("\n", "%0A");
		s = s.Replace(":", "%2F");
		s = s.Replace("/", "%3A");
		return s;
	}
}
