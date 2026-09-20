using System;
using System.Collections.Generic;
using System.IO;
using LazyBearTechnology;
using UnityEngine;

public class JsonFileSerializer : BaseSerializer
{
	private readonly string fileExtension;

	public JsonFileSerializer(string fileExtension)
	{
		this.fileExtension = fileExtension;
	}

	public override bool SerializeAndSave<T>(T data, string directory, string filename, Action callback)
	{
		bool result = true;
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
		data.OnBeforeSerialize();
		string text = directory + filename + fileExtension;
		if (LazyAPI.LazyFile.IsSupportingBackupSaves)
		{
			try
			{
				LazyAPI.LazyFile.WriteAllText(text + ".new", JsonUtility.ToJson(data));
				if (File.Exists(GetBackupPath(directory, filename, 3)))
				{
					File.Delete(GetBackupPath(directory, filename, 3));
				}
				if (File.Exists(GetBackupPath(directory, filename, 2)))
				{
					File.Move(GetBackupPath(directory, filename, 2), GetBackupPath(directory, filename, 3));
				}
				if (File.Exists(GetBackupPath(directory, filename, 1)))
				{
					File.Move(GetBackupPath(directory, filename, 1), GetBackupPath(directory, filename, 2));
				}
				if (File.Exists(text))
				{
					File.Move(text, GetBackupPath(directory, filename, 1));
				}
				File.Move(text + ".new", text);
			}
			catch (Exception arg)
			{
				Debug.LogError($"Error saving file: {arg}");
				result = false;
			}
		}
		else
		{
			result = LazyAPI.LazyFile.WriteAllText(text, JsonUtility.ToJson(data));
		}
		callback?.Invoke();
		return result;
	}

	public override void LoadAndDeserialize<T>(string directory, string filename, Action<T> callback)
	{
		T val = JsonUtility.FromJson<T>(LazyAPI.LazyFile.ReadAllText(directory + filename + fileExtension));
		val.OnAfterSerialize();
		callback?.Invoke(val);
	}

	public override void LoadAndDeserializeAll<T>(string directory, Action<List<(T data, string fileName)>> callback)
	{
		List<(T, string)> list = new List<(T, string)>();
		string[] files = LazyAPI.LazyFile.GetFiles(directory, fileExtension, SearchOption.TopDirectoryOnly);
		foreach (string text in files)
		{
			try
			{
				T val = JsonUtility.FromJson<T>(LazyAPI.LazyFile.ReadAllText(text));
				if (val != null)
				{
					val.OnAfterSerialize();
					list.Add((val, text.Remove(0, directory.Length)));
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Error while reading save slot, file: {text}. Exception: {arg}");
			}
		}
		callback?.Invoke(list);
	}

	public override bool Remove(string directory, string fileName, Action callback)
	{
		bool result = LazyAPI.LazyFile.Delete(directory + fileName + fileExtension);
		callback?.Invoke();
		return result;
	}

	private string GetBackupPath(string directory, string filename, int backupIndex)
	{
		return $"{directory}{filename}_backup_{backupIndex}{fileExtension}";
	}
}
