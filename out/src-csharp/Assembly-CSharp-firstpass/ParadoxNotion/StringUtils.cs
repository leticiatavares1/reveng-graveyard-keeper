using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ParadoxNotion;

public static class StringUtils
{
	private static Dictionary<string, string> splitCaseCache = new Dictionary<string, string>(StringComparer.Ordinal);

	public static string SplitCamelCase(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		if (splitCaseCache.TryGetValue(s, out var value))
		{
			return value;
		}
		value = s.Replace("_", " ");
		value = char.ToUpper(value[0]) + value.Substring(1);
		value = Regex.Replace(value, "(?<=[a-z])([A-Z])", " $1").Trim();
		return splitCaseCache[s] = value;
	}

	public static string CapLength(this string s, int max)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		string text = s.Substring(0, Mathf.Min(s.Length, max));
		if (text.Length < s.Length)
		{
			text += "...";
		}
		return text;
	}

	public static string GetCapitals(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		string text = "";
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (char.IsUpper(c))
			{
				text += c;
			}
		}
		return text.Trim();
	}

	public static string GetAlphabetLetter(int index)
	{
		if (index < 0)
		{
			return null;
		}
		string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		if (index >= text.Length)
		{
			return index.ToString();
		}
		return text[index].ToString();
	}

	public static string GetStringWithin(this string input, string from, string to)
	{
		return new Regex($"{from}(.*?){to}").Match(input).Groups[1].ToString();
	}

	public static string ToStringAdvanced(this object o)
	{
		if (o == null || o.Equals(null))
		{
			return "NULL";
		}
		if (o is string)
		{
			return $"\"{(string)o}\"";
		}
		if (o is UnityEngine.Object)
		{
			return (o as UnityEngine.Object).name;
		}
		Type type = o.GetType();
		if (type.RTIsSubclassOf(typeof(Enum)) && type.RTIsDefined<FlagsAttribute>(inherited: true))
		{
			string text = "";
			int num = 0;
			Array values = Enum.GetValues(type);
			foreach (object item in values)
			{
				if ((Convert.ToInt32(item) & Convert.ToInt32(o)) == Convert.ToInt32(item))
				{
					num++;
					text = ((!(text == "")) ? "Mixed..." : item.ToString());
				}
			}
			if (num == 0)
			{
				return "Nothing";
			}
			if (num == values.Length)
			{
				return "Everything";
			}
			return text;
		}
		return o.ToString();
	}
}
