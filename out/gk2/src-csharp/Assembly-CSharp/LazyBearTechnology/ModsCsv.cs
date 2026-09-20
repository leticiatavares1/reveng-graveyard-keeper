using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LazyBearTechnology;

public static class ModsCsv
{
	private static readonly Encoding Utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

	public static void WriteKeyValueFile(string path, IEnumerable<KeyValuePair<string, string>> rows)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("key,value\r\n");
		foreach (KeyValuePair<string, string> row in rows)
		{
			stringBuilder.Append(Escape(row.Key));
			stringBuilder.Append(',');
			stringBuilder.Append(Escape(row.Value ?? string.Empty));
			stringBuilder.Append("\r\n");
		}
		File.WriteAllText(path, stringBuilder.ToString(), Utf8Bom);
	}

	public static Dictionary<string, string> ReadKeyValueFile(string path)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!File.Exists(path))
		{
			return dictionary;
		}
		List<string[]> list = Parse(File.ReadAllText(path));
		for (int i = 0; i < list.Count; i++)
		{
			string[] array = list[i];
			if (array.Length >= 2)
			{
				string text = array[0];
				string text2 = array[1];
				if ((i != 0 || !string.Equals(text, "key", StringComparison.OrdinalIgnoreCase) || !string.Equals(text2, "value", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(text))
				{
					dictionary[text] = text2 ?? string.Empty;
				}
			}
		}
		return dictionary;
	}

	private static string Escape(string value)
	{
		if (value == null)
		{
			return "\"\"";
		}
		if (value.IndexOfAny(new char[4] { ',', '"', '\n', '\r' }) < 0)
		{
			return value;
		}
		return "\"" + value.Replace("\"", "\"\"") + "\"";
	}

	private static List<string[]> Parse(string text)
	{
		List<string[]> list = new List<string[]>();
		List<string> list2 = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (flag)
			{
				if (c == '"')
				{
					if (i + 1 < text.Length && text[i + 1] == '"')
					{
						stringBuilder.Append('"');
						i++;
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					stringBuilder.Append(c);
				}
				continue;
			}
			switch (c)
			{
			case '"':
				flag = true;
				break;
			case ',':
				list2.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
				break;
			case '\n':
				list2.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
				if (list2.Count > 1 || (list2.Count == 1 && !string.IsNullOrEmpty(list2[0])))
				{
					list.Add(list2.ToArray());
				}
				list2.Clear();
				break;
			default:
				stringBuilder.Append(c);
				break;
			case '\r':
				break;
			}
		}
		if (flag || stringBuilder.Length > 0 || list2.Count > 0)
		{
			list2.Add(stringBuilder.ToString());
			if (list2.Count > 1 || (list2.Count == 1 && !string.IsNullOrEmpty(list2[0])))
			{
				list.Add(list2.ToArray());
			}
		}
		return list;
	}
}
