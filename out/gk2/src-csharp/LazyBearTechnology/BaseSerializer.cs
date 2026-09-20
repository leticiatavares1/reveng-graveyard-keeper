using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public abstract class BaseSerializer : IDataSerializer
{
	public virtual bool SerializeAndSave<T>(T data, string directory, string filename, Action callback) where T : class, ISerializableData, new()
	{
		Debug.LogError(string.Format("{0}: method {1} not implemented.", GetType(), "SerializeAndSave"));
		return true;
	}

	public virtual void LoadAndDeserialize<T>(string directory, string filename, Action<T> callback) where T : class, ISerializableData, new()
	{
		Debug.LogError(string.Format("{0}: method {1} not implemented.", GetType(), "LoadAndDeserialize"));
	}

	public virtual void LoadAndDeserializeAll<T>(string directory, Action<List<(T data, string fileName)>> callback) where T : class, ISerializableData, new()
	{
		Debug.LogError(string.Format("{0}: method {1} not implemented.", GetType(), "LoadAndDeserializeAll"));
	}

	public virtual bool Remove(string directory, string fileName, Action callback)
	{
		Debug.LogError(string.Format("{0}: method {1} not implemented.", GetType(), "Remove"));
		return true;
	}
}
