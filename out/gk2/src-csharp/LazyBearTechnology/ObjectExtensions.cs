using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

public static class ObjectExtensions
{
	public static T DeepCloneByBinaryFormatter<T>(this T obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(memoryStream, obj);
		memoryStream.Position = 0L;
		return (T)binaryFormatter.Deserialize(memoryStream);
	}

	public static T DeepClone<T>(this T original)
	{
		if (original == null)
		{
			return default(T);
		}
		Type type = original.GetType();
		if (type.IsValueType || type == typeof(string))
		{
			return original;
		}
		if (type.IsArray)
		{
			Type type2 = Type.GetType(type.FullName.Replace("[]", string.Empty));
			Array array = original as Array;
			Array array2 = Array.CreateInstance(type2, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				array2.SetValue(array.GetValue(i).DeepClone(), i);
			}
			return (T)Convert.ChangeType(array2, original.GetType());
		}
		if (type.IsClass)
		{
			T val = (T)Activator.CreateInstance(original.GetType());
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				object value = fieldInfo.GetValue(original);
				if (value != null)
				{
					fieldInfo.SetValue(val, value.DeepClone());
				}
			}
			return val;
		}
		throw new ArgumentException("DeepClone: Unknown type [" + type.ToString() + "].");
	}
}
