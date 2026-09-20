using System;
using System.IO;
using System.Text;
using LazyBearTechnology;
using UnityEngine;

public class BinaryFileSerializer : BaseSerializer
{
	private readonly string fileExtension;

	public static string encryptKey;

	public BinaryFileSerializer(string fileExtension)
	{
		this.fileExtension = fileExtension;
	}

	public override bool SerializeAndSave<T>(T data, string directory, string filename, Action callback)
	{
		bool result = true;
		if (LazyAPI.Platform.IsGuest())
		{
			Debug.LogError("Save Error: Save is not allowed for guests.");
			callback?.Invoke();
			return false;
		}
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
		string text = directory + filename + fileExtension;
		data.OnBeforeSerialize();
		byte[] array = LazySerializer.Serialize(data);
		if (!string.IsNullOrEmpty(encryptKey))
		{
			array = Encrypt(array);
		}
		Debug.Log($"Serialized save length: {array.Length}");
		if (LazyAPI.LazyFile.IsSupportingBackupSaves)
		{
			try
			{
				File.WriteAllBytes(text + ".new", array);
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
			result = LazyAPI.LazyFile.WriteAllBytes(text, array);
		}
		callback?.Invoke();
		return result;
	}

	public override void LoadAndDeserialize<T>(string directory, string filename, Action<T> callback)
	{
		string text = directory + filename + fileExtension;
		if (!LazyAPI.LazyFile.Exists(text))
		{
			Debug.LogError("Save file not found: [" + text + "]");
			callback?.Invoke(null);
		}
		T val = null;
		try
		{
			byte[] array = LazyAPI.LazyFile.ReadAllBytes(text);
			if (!string.IsNullOrEmpty(encryptKey))
			{
				array = Decrypt(array);
			}
			val = LazySerializer.Deserialize<T>(array);
			val.OnAfterSerialize();
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error while reading save file {text}. Ex: {arg}");
			callback?.Invoke(null);
			return;
		}
		callback?.Invoke(val);
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

	private byte[] Encrypt(byte[] bytes)
	{
		try
		{
			byte[] bytes2 = Encoding.UTF8.GetBytes(encryptKey);
			byte[] array = new byte[bytes.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (byte)(bytes[i] ^ bytes2[i % bytes2.Length]);
			}
			return array;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't encrypt data, exception:[{arg}]");
			return bytes;
		}
	}

	private byte[] Decrypt(byte[] bytes)
	{
		try
		{
			byte[] array = new byte[bytes.Length];
			byte[] bytes2 = Encoding.UTF8.GetBytes(encryptKey);
			for (int i = 0; i < bytes.Length; i++)
			{
				array[i] = (byte)(bytes[i] ^ bytes2[i % bytes2.Length]);
			}
			return array;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't decrypt data, exception:[{arg}]");
			return bytes;
		}
	}
}
