using System.IO;
using UnityEngine;

namespace LazyBearTechnology;

public class JsonDataController
{
	public static T Load<T>(string resourcePath)
	{
		StreamReader streamReader = new StreamReader(Application.persistentDataPath + resourcePath, detectEncodingFromByteOrderMarks: true);
		string json = streamReader.ReadToEnd();
		streamReader.Close();
		return JsonUtility.FromJson<T>(json);
	}

	public static T LoadOverwrite<T>(string resourcePath, T objectToOverwrite)
	{
		StreamReader streamReader = new StreamReader(Application.persistentDataPath + resourcePath, detectEncodingFromByteOrderMarks: true);
		string json = streamReader.ReadToEnd();
		streamReader.Close();
		JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
		return objectToOverwrite;
	}

	public static bool Save<T>(T data, string resourcePath)
	{
		string path = Application.persistentDataPath + resourcePath;
		string value = JsonUtility.ToJson(data, prettyPrint: true);
		StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.Write(value);
		streamWriter.Close();
		return true;
	}
}
