using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class FilePlayerPrefs
{
	private static readonly Hashtable PlayerPrefsHashtable;

	private static bool _hashTableChanged;

	private static string _serializedOutput;

	private static readonly string SerializedInput;

	private const string ParametersSeperator = ";";

	private const string KeyValueSeperator = ":";

	private static readonly string FileName;

	static FilePlayerPrefs()
	{
		PlayerPrefsHashtable = new Hashtable();
		_serializedOutput = "";
		SerializedInput = "";
		FileName = Application.persistentDataPath + "/MAPlayerPrefs.txt";
		if (File.Exists(FileName))
		{
			StreamReader streamReader = new StreamReader(FileName);
			SerializedInput = streamReader.ReadLine();
			Deserialize();
			streamReader.Close();
		}
	}

	public static bool HasKey(string key)
	{
		return PlayerPrefsHashtable.ContainsKey(key);
	}

	public static void SetString(string key, string value)
	{
		if (!PlayerPrefsHashtable.ContainsKey(key))
		{
			PlayerPrefsHashtable.Add(key, value);
		}
		else
		{
			PlayerPrefsHashtable[key] = value;
		}
		_hashTableChanged = true;
	}

	public static void SetInt(string key, int value)
	{
		if (!PlayerPrefsHashtable.ContainsKey(key))
		{
			PlayerPrefsHashtable.Add(key, value);
		}
		else
		{
			PlayerPrefsHashtable[key] = value;
		}
		_hashTableChanged = true;
	}

	public static void SetFloat(string key, float value)
	{
		if (!PlayerPrefsHashtable.ContainsKey(key))
		{
			PlayerPrefsHashtable.Add(key, value);
		}
		else
		{
			PlayerPrefsHashtable[key] = value;
		}
		_hashTableChanged = true;
	}

	public static void SetBool(string key, bool value)
	{
		if (!PlayerPrefsHashtable.ContainsKey(key))
		{
			PlayerPrefsHashtable.Add(key, value);
		}
		else
		{
			PlayerPrefsHashtable[key] = value;
		}
		_hashTableChanged = true;
	}

	public static string GetString(string key)
	{
		if (PlayerPrefsHashtable.ContainsKey(key))
		{
			return PlayerPrefsHashtable[key].ToString();
		}
		return null;
	}

	public static string GetString(string key, string defaultValue)
	{
		if (PlayerPrefsHashtable.ContainsKey(key))
		{
			return PlayerPrefsHashtable[key].ToString();
		}
		PlayerPrefsHashtable.Add(key, defaultValue);
		_hashTableChanged = true;
		return defaultValue;
	}

	public static int GetInt(string key)
	{
		if (!PlayerPrefsHashtable.ContainsKey(key))
		{
			return 0;
		}
		object obj = PlayerPrefsHashtable[key];
		if (obj is int)
		{
			return (int)obj;
		}
		int num = int.Parse(obj.ToString());
		PlayerPrefsHashtable[key] = num;
		obj = num;
		return (int)obj;
	}

	public static int GetInt(string key, int defaultValue)
	{
		if (PlayerPrefsHashtable.ContainsKey(key))
		{
			return (int)PlayerPrefsHashtable[key];
		}
		PlayerPrefsHashtable.Add(key, defaultValue);
		_hashTableChanged = true;
		return defaultValue;
	}

	public static float GetFloat(string key)
	{
		if (!PlayerPrefsHashtable.ContainsKey(key))
		{
			return 0f;
		}
		object obj = PlayerPrefsHashtable[key];
		if (obj is float)
		{
			return (float)obj;
		}
		float num = float.Parse(obj.ToString(), CultureInfo.InvariantCulture);
		PlayerPrefsHashtable[key] = num;
		obj = num;
		return (float)obj;
	}

	public static float GetFloat(string key, float defaultValue)
	{
		if (PlayerPrefsHashtable.ContainsKey(key))
		{
			return (float)PlayerPrefsHashtable[key];
		}
		PlayerPrefsHashtable.Add(key, defaultValue);
		_hashTableChanged = true;
		return defaultValue;
	}

	public static bool GetBool(string key)
	{
		if (PlayerPrefsHashtable.ContainsKey(key))
		{
			return (bool)PlayerPrefsHashtable[key];
		}
		return false;
	}

	public static bool GetBool(string key, bool defaultValue)
	{
		if (PlayerPrefsHashtable.ContainsKey(key))
		{
			return (bool)PlayerPrefsHashtable[key];
		}
		PlayerPrefsHashtable.Add(key, defaultValue);
		_hashTableChanged = true;
		return defaultValue;
	}

	public static void DeleteKey(string key)
	{
		PlayerPrefsHashtable.Remove(key);
	}

	public static void DeleteAll()
	{
		PlayerPrefsHashtable.Clear();
	}

	public static void Flush()
	{
		if (_hashTableChanged)
		{
			Serialize();
			StreamWriter streamWriter = File.CreateText(FileName);
			if (streamWriter == null)
			{
				Debug.LogWarning("PlayerPrefs::Flush() opening file for writing failed: " + FileName);
			}
			streamWriter.WriteLine(_serializedOutput);
			streamWriter.Close();
			_serializedOutput = "";
		}
	}

	private static void Serialize()
	{
		IDictionaryEnumerator enumerator = PlayerPrefsHashtable.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (_serializedOutput != "")
			{
				_serializedOutput += " ; ";
			}
			_serializedOutput = _serializedOutput + EscapeNonSeperators(enumerator.Key.ToString()) + " : " + EscapeNonSeperators(enumerator.Value.ToString()) + " : " + enumerator.Value.GetType();
		}
	}

	private static void Deserialize()
	{
		string[] array = SerializedInput.Split(new string[1] { " ; " }, StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new string[1] { " : " }, StringSplitOptions.None);
			PlayerPrefsHashtable.Add(DeEscapeNonSeperators(array2[0]), GetTypeValue(array2[2], DeEscapeNonSeperators(array2[1])));
			if (array2.Length > 3)
			{
				Debug.LogWarning("PlayerPrefs::Deserialize() parameterContent has " + array2.Length + " elements");
			}
		}
	}

	private static string EscapeNonSeperators(string inputToEscape)
	{
		inputToEscape = inputToEscape.Replace(":", "\\:");
		inputToEscape = inputToEscape.Replace(";", "\\;");
		return inputToEscape;
	}

	private static string DeEscapeNonSeperators(string inputToDeEscape)
	{
		inputToDeEscape = inputToDeEscape.Replace("\\:", ":");
		inputToDeEscape = inputToDeEscape.Replace("\\;", ";");
		return inputToDeEscape;
	}

	public static object GetTypeValue(string typeName, string value)
	{
		switch (typeName)
		{
		case "System.String":
			return value;
		case "System.Int32":
			return Convert.ToInt32(value);
		case "System.Boolean":
			return Convert.ToBoolean(value);
		case "System.Single":
			return Convert.ToSingle(value);
		default:
			Debug.Log("Unsupported type: " + typeName);
			return null;
		}
	}
}
