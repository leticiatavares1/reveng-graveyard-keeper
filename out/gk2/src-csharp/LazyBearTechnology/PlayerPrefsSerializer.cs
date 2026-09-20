using System;
using LazyBearTechnology;
using UnityEngine;

public class PlayerPrefsSerializer : BaseSerializer
{
	public override bool SerializeAndSave<T>(T data, string directory, string filename, Action callback)
	{
		data.OnBeforeSerialize();
		string value = JsonUtility.ToJson(data);
		LazyAPI.PlayerPrefs.SetString(filename, value);
		LazyAPI.PlayerPrefs.Save();
		callback?.Invoke();
		return true;
	}

	public override void LoadAndDeserialize<T>(string directory, string filename, Action<T> callback)
	{
		string @string = LazyAPI.PlayerPrefs.GetString(filename);
		if (string.IsNullOrEmpty(@string))
		{
			callback?.Invoke(null);
			return;
		}
		T val = JsonUtility.FromJson<T>(@string);
		val.OnAfterSerialize();
		callback?.Invoke(val);
	}
}
