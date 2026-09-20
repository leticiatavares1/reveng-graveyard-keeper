using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LazyBearTechnology;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ConveyorPresetSerializer : BaseSerializer
{
	private readonly string fileExtension;

	private SerializationContext serializationContext;

	private DeserializationContext deserializationContext;

	public static string encryptKey;

	public ConveyorPresetSerializer(string fileExtension, SerializationContext serializationContext = null, DeserializationContext deserializationContext = null)
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
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
		string path = directory + filename + fileExtension;
		data.OnBeforeSerialize();
		byte[] array = Serialize(data);
		Debug.Log($"Serialized save length: {array.Length}");
		bool result = LazyAPI.LazyFile.WriteAllBytes(path, array);
		callback?.Invoke();
		return result;
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
		AsyncOperationHandle<TextAsset> asyncOperationHandle = Addressables.LoadAssetAsync<TextAsset>(directory + filename + fileExtension);
		asyncOperationHandle.WaitForCompletion();
		string text = directory + filename + fileExtension;
		if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogError("Save file not found: [" + text + "]");
			callback?.Invoke(null);
		}
		T val = null;
		try
		{
			byte[] bytes = asyncOperationHandle.Result.bytes;
			val = Deserialize<T>(bytes);
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

	public override void LoadAndDeserializeAll<T>(string directory, Action<List<(T data, string fileName)>> callback)
	{
		IList<TextAsset> list = Addressables.LoadAssetsAsync<TextAsset>("ConveyorPresets").WaitForCompletion();
		List<(T, string)> list2 = new List<(T, string)>();
		if (list == null || list.Count == 0)
		{
			Debug.LogWarning("No conveyor presets found by label [ConveyorPresets]");
			callback?.Invoke(list2);
			return;
		}
		foreach (TextAsset item in list)
		{
			try
			{
				byte[] bytes = item.bytes;
				T val = Deserialize<T>(bytes);
				if (val != null)
				{
					list2.Add((val, item.name));
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Error while reading save slot, file: {item.name}. Exception: {arg}");
			}
		}
		callback?.Invoke(list2);
	}
}
