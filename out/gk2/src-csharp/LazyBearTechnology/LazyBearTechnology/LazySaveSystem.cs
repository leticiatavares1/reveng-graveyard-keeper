using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

public class LazySaveSystem
{
	private Dictionary<Type, IDataSerializer> dataSaverDictionary;

	public LazySaveSystem(params (Type, IDataSerializer)[] dataSerializersArray)
	{
		dataSaverDictionary = new Dictionary<Type, IDataSerializer>();
		Array.ForEach(dataSerializersArray, delegate((Type, IDataSerializer) x)
		{
			dataSaverDictionary.Add(x.Item1, x.Item2);
		});
	}

	public bool Save<T>(T data, string directory, string saveFilename, Action callback) where T : class, ISerializableData, new()
	{
		if (!dataSaverDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new Exception($"{typeof(IDataSerializer)} for type {typeof(T)} not defined.");
		}
		return value.SerializeAndSave(data, directory, saveFilename, callback);
	}

	public void Load<T>(string directory, string saveFilename, Action<T> callback) where T : class, ISerializableData, new()
	{
		if (!dataSaverDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new Exception($"{typeof(IDataSerializer)} for type {typeof(T)} not defined.");
		}
		value.LoadAndDeserialize(directory, saveFilename, callback);
	}

	public void LoadAll<T>(string directory, Action<List<(T data, string fileName)>> callback) where T : class, ISerializableData, new()
	{
		if (!dataSaverDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new Exception($"{typeof(IDataSerializer)} for type {typeof(T)} not defined.");
		}
		value.LoadAndDeserializeAll(directory, callback);
	}

	public bool Remove<T>(string directory, string fileName, Action callback)
	{
		if (!dataSaverDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new Exception($"{typeof(IDataSerializer)} for type {typeof(T)} not defined.");
		}
		return value.Remove(directory, fileName, callback);
	}
}
