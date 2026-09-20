using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace LazyBearTechnology;

public static class Common
{
	public static bool debugMemory = false;

	public static string loremIpsumShort = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

	public static string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

	public static bool useEtcTextures = false;

	public static bool pvrSupported = true;

	public static string defaultShaderName = "ex2D/Alpha Blended";

	public static string base62 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public static string baseReadable = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

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

	public static byte[] ReadBinaryDataFromStream(MemoryStream stream)
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
		catch (Exception)
		{
		}
		binaryReader.Close();
		return list.ToArray();
	}

	public static byte[] ReadBinaryResource(string resourceName)
	{
		TextAsset textAsset = Resources.Load(resourceName) as TextAsset;
		if (textAsset == null)
		{
			Debug.LogError("Error ReadBinaryResource '" + resourceName + "' !");
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

	public static byte[] Base64Decode(string s)
	{
		s = s.Replace("-", "+");
		s = s.Replace("_", "/");
		return Convert.FromBase64String(s);
	}

	public static string Base64Encode(string s)
	{
		return Base64Encode(Encoding.UTF8.GetBytes(s));
	}

	public static int Checksum(byte[] b)
	{
		int num = 0;
		foreach (byte b2 in b)
		{
			num += b2;
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

	public static string FormatTime(int seconds, string daysSuffix, string hoursSuffix, string minSuffix, string secSuffix)
	{
		int num = (int)Mathf.Floor((float)seconds / 86400f);
		seconds -= num * 86400;
		int num2 = (int)Mathf.Floor((float)seconds / 3600f);
		seconds -= num2 * 3600;
		int num3 = (int)Mathf.Floor((float)seconds / 60f);
		seconds -= num3 * 60;
		int num4 = seconds;
		string text = num4.ToString() ?? "";
		string text2 = num3.ToString() ?? "";
		string text3 = num2.ToString() ?? "";
		string text4 = num.ToString() ?? "";
		if (minSuffix == ":" && num4 < 10)
		{
			text = "0" + text;
		}
		if (hoursSuffix == ":" && num3 < 10)
		{
			text2 = "0" + text2;
		}
		if (num > 0)
		{
			return text4 + daysSuffix + text3 + hoursSuffix + text2;
		}
		if (num2 > 0)
		{
			return text3 + hoursSuffix + text2 + minSuffix + text + secSuffix;
		}
		if (num3 > 0)
		{
			return text2 + minSuffix + text + secSuffix;
		}
		if (minSuffix == ":")
		{
			text = "00:" + text;
		}
		return text + secSuffix;
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

	public static List<Rect> FillRectWithSquares(List<int> availableSquares, Vector2 rect)
	{
		List<Rect> list = new List<Rect>();
		bool[,] array = new bool[(int)rect.x, (int)rect.y];
		for (int i = 0; (float)i < rect.x; i++)
		{
			for (int j = 0; (float)j < rect.y; j++)
			{
				array[i, j] = false;
			}
		}
		List<int> list2 = new List<int>();
		list2.AddRange(availableSquares);
		list2.Sort();
		for (int i = 0; (float)i < rect.x; i++)
		{
			for (int j = 0; (float)j < rect.y; j++)
			{
				if (array[i, j])
				{
					continue;
				}
				int num = list2.Count;
				int num2 = 0;
				int k = 0;
				int l = 0;
				while (num > 0)
				{
					num--;
					num2 = list2[num];
					if ((float)(i + num2) <= rect.x && (float)(j + num2) <= rect.y)
					{
						bool flag = true;
						for (k = 0; k < num2; k++)
						{
							for (l = 0; l < num2; l++)
							{
								if (array[i + k, j + l])
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
					if (num == 0)
					{
						Debug.LogWarning("FillRectWithSquares() couldn't fill rectangle correctly! (Maybe you don't have 1x1 square?)\nDidn't fill square at (" + i + ", " + j + ") of square " + rect.x + "x" + rect.y);
					}
				}
				try
				{
					for (k = 0; k < num2; k++)
					{
						for (l = 0; l < num2; l++)
						{
							array[i + k, j + l] = true;
						}
					}
				}
				catch (Exception)
				{
					Debug.LogError("Map is out of range: " + (i + k) + ", " + (j + l));
				}
				list.Add(new Rect(i, j, num2, num2));
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

	public static string MemoryToString(long memory)
	{
		if (memory < 1024)
		{
			return memory + " b";
		}
		if (memory < 1048576)
		{
			return Mathf.Round(memory / 1024) + " Kb";
		}
		return Mathf.Round(memory / 1048576) + " Mb";
	}

	public static object LoadResource(string name)
	{
		long usedHeapSizeLong = Profiler.usedHeapSizeLong;
		object obj = Resources.Load(name);
		if (obj == null)
		{
			Debug.LogWarning("Error loading resource \"" + name + "\" - resource not found!");
			return null;
		}
		if (debugMemory)
		{
			Debug.Log("Resource \"" + name + "\" loaded. Memory spent: " + MemoryToString(Profiler.usedHeapSizeLong - usedHeapSizeLong));
		}
		return obj;
	}

	public static object LoadResource(string name, object objectClass)
	{
		long usedHeapSizeLong = Profiler.usedHeapSizeLong;
		object obj = Resources.Load(name, objectClass.GetType());
		if (obj == null)
		{
			Debug.LogWarning("Error loading resource \"" + name + "\" - resource not found!");
			return null;
		}
		if (debugMemory)
		{
			Debug.Log("Resource \"" + name + "\" loaded. Memory spent: " + MemoryToString(Profiler.usedHeapSizeLong - usedHeapSizeLong));
		}
		return obj;
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
		return ToAnyBase(n, base62);
	}

	public static int FromBase62(string s)
	{
		return FromAnyBase(s, base62);
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

	private static string URLEncode(string s)
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
