using LazyBearTechnology;

public static class BinaryDataSerializer
{
	public static byte[] SerializeData<T>(T data) where T : class, ISerializableData, new()
	{
		data.OnBeforeSerialize();
		return LazySerializer.Serialize(data);
	}

	public static T DeserializeData<T>(byte[] byteData) where T : class, ISerializableData, new()
	{
		return LazySerializer.Deserialize<T>(byteData);
	}
}
