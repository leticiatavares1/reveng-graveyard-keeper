using System;
using System.IO;
using System.Text;
using LazyBearTechnology;
using Sirenix.Serialization;
using UnityEngine;

public class OdinBinaryFileSerializer : BaseSerializer
{
	private readonly string fileExtension;

	private SerializationContext serializationContext;

	private DeserializationContext deserializationContext;

	public static string encryptKey;

	public OdinBinaryFileSerializer(string fileExtension, SerializationContext serializationContext = null, DeserializationContext deserializationContext = null)
	{
		this.fileExtension = fileExtension;
		this.serializationContext = serializationContext;
		this.deserializationContext = deserializationContext;
	}

	public override bool SerializeAndSave<T>(T data, string directory, string filename, Action callback)
	{
		if (LazyAPI.Platform.IsGuest())
		{
			Debug.LogError("Save Error: Save is not allowed for guests.");
			callback?.Invoke();
			return false;
		}
		data.OnBeforeSerialize();
		byte[] array = Serialize(data);
		Debug.Log($"Serialized save length: {array.Length}");
		bool result = SaveBytes(directory, filename, array);
		callback?.Invoke();
		return result;
	}

	public bool SaveBytes(string directory, string filename, byte[] bytes)
	{
		if (LazyAPI.Platform.IsGuest())
		{
			Debug.LogError("Save Error: Save is not allowed for guests.");
			return false;
		}
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
		string text = directory + filename + fileExtension;
		if (LazyAPI.LazyFile.IsSupportingBackupSaves)
		{
			try
			{
				File.WriteAllBytes(text + ".new", bytes);
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
				return true;
			}
			catch (Exception arg)
			{
				Debug.LogError($"Error saving file: {arg}");
				return false;
			}
		}
		return LazyAPI.LazyFile.WriteAllBytes(text, bytes);
	}

	public byte[] Serialize<T>(T data)
	{
		try
		{
			byte[] array = SerializationUtility.SerializeValue(data, DataFormat.Binary, serializationContext);
			if (!string.IsNullOrEmpty(encryptKey))
			{
				array = Encrypt(array);
			}
			return array;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error during odin serialization:[{arg}]");
			return Array.Empty<byte>();
		}
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
			byte[] byteData = LazyAPI.LazyFile.ReadAllBytes(text);
			val = Deserialize<T>(byteData);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error while reading save file {text}. Ex: {arg}");
			callback?.Invoke(null);
			return;
		}
		callback?.Invoke(val);
	}

	public T Deserialize<T>(byte[] byteData) where T : class, ISerializableData, new()
	{
		if (!string.IsNullOrEmpty(encryptKey))
		{
			byteData = Decrypt(byteData);
		}
		T val = SerializationUtility.DeserializeValue<T>(byteData, DataFormat.Binary, deserializationContext);
		val.OnAfterSerialize();
		return val;
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
