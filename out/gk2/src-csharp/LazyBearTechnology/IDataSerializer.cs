using System;
using System.Collections.Generic;
using LazyBearTechnology;

public interface IDataSerializer
{
	bool SerializeAndSave<T>(T data, string directory, string filename, Action callback) where T : class, ISerializableData, new();

	void LoadAndDeserialize<T>(string directory, string filename, Action<T> callback) where T : class, ISerializableData, new();

	void LoadAndDeserializeAll<T>(string directory, Action<List<(T data, string fileName)>> callback) where T : class, ISerializableData, new();

	bool Remove(string directory, string fileName, Action callback);
}
