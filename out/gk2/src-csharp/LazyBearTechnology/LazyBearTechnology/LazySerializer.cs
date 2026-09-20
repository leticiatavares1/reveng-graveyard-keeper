using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Sirenix.Serialization;
using UnityEngine;

namespace LazyBearTechnology;

public static class LazySerializer
{
	private enum FieldType
	{
		NullValue = 0,
		BoolTrue = 1,
		BoolFalse = 2,
		Int32 = 3,
		Int64 = 4,
		Single = 5,
		Double = 6,
		Byte = 7,
		Char = 8,
		String = 9,
		StringIndexer = 10,
		StringEmpty = 11,
		Json = 12,
		Vector2 = 13,
		Vector3 = 14,
		Quaternion = 15,
		Int32_0 = 16,
		Int32_1 = 17,
		Single_0 = 18,
		Single_1 = 19,
		Vector2_00 = 20,
		Vector2_11 = 21,
		Vector3_000 = 22,
		Vector3_111 = 23,
		Quaternion_0001 = 24,
		GenericList = 100,
		Array = 101,
		ByteArray = 102,
		LazySerialized = 250
	}

	public class Header
	{
		public int version = 1;

		public long sdataOffset;
	}

	private class SerializerData
	{
		public List<string> strings = new List<string>();
	}

	private const int VERSION = 1;

	private static readonly Type TYPE_SERIALIZED_FIELD = typeof(SerializeField);

	private static readonly Type TYPE_LAZY_SERIALIZE = typeof(LazySerialize);

	private static readonly Type TYPE_ODIN_SERIALIZE = typeof(OdinSerializeAttribute);

	private static readonly Type TYPE_LAZY_DONT_SERIALIZE = typeof(LazyDontSerialize);

	private static readonly Type TYPE_INTERFACE = typeof(ILazyCustomSerialize);

	private static readonly Type TYPE_COMPONENT = typeof(Component);

	private static readonly Type TYPE_MONOBEHAVIOUR = typeof(MonoBehaviour);

	private static Dictionary<Type, List<FieldInfo>> fieldsCache = new Dictionary<Type, List<FieldInfo>>();

	private static Dictionary<Type, Dictionary<int, FieldInfo>> fieldsHashesCache = new Dictionary<Type, Dictionary<int, FieldInfo>>();

	private static Dictionary<string, int> stringHashes = new Dictionary<string, int>();

	private static char[] buffer = new char[10240];

	private static StringBuilder sb = new StringBuilder();

	public static byte[] Serialize<T>(T o) where T : class
	{
		Stream stream;
		Stream stream2 = (stream = new MemoryStream());
		Serialize(o, stream);
		return ((MemoryStream)stream2).ToArray();
	}

	public static void Serialize<T>(T o, Stream stream) where T : class
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(0L);
		WriteHeader(binaryWriter, new Header());
		SerializerData sd = new SerializerData();
		SerializeInternal(o, binaryWriter, sd);
		WriteSerializerData(binaryWriter, sd);
		binaryWriter.Close();
	}

	public static T Deserialize<T>(byte[] data) where T : new()
	{
		return Deserialize<T>(new MemoryStream(data));
	}

	public static T Deserialize<T>(Stream stream) where T : new()
	{
		ClearCache();
		stream.Seek(0L, SeekOrigin.Begin);
		BinaryReader br = new BinaryReader(stream);
		SerializerData sd = ReadSerializerData(br);
		ReadHeader(br);
		return DeserializeInternal<T>(br, sd);
	}

	public static void DeserializeInto<T>(T obj, byte[] data)
	{
		ClearCache();
		BinaryReader br = new BinaryReader(new MemoryStream(data));
		SerializerData sd = ReadSerializerData(br);
		ReadHeader(br);
		DeserializeIntoInternal(obj.GetType(), obj, br, sd);
	}

	private static void SerializeInternal<T>(T o, BinaryWriter bw, SerializerData sd) where T : class
	{
		ClearCache();
		SerializeInternal(o.GetType(), o, bw, sd);
	}

	private static void ClearCache()
	{
		fieldsCache.Clear();
		fieldsHashesCache.Clear();
		stringHashes.Clear();
	}

	private static List<FieldInfo> GetSerializableFieldsForType(Type objType)
	{
		if (fieldsCache.TryGetValue(objType, out var value))
		{
			return value;
		}
		value = new List<FieldInfo>();
		FieldInfo[] fields = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (((fieldInfo.IsPublic && !fieldInfo.IsNotSerialized) || Attribute.IsDefined(fieldInfo, TYPE_SERIALIZED_FIELD, inherit: true) || Attribute.IsDefined(fieldInfo, TYPE_LAZY_SERIALIZE, inherit: true) || Attribute.IsDefined(fieldInfo, TYPE_ODIN_SERIALIZE, inherit: true)) && !Attribute.IsDefined(fieldInfo, TYPE_LAZY_DONT_SERIALIZE) && !objType.IsSubclassOf(TYPE_COMPONENT) && !objType.IsSubclassOf(TYPE_MONOBEHAVIOUR))
			{
				value.Add(fieldInfo);
			}
		}
		fieldsCache.Add(objType, value);
		return value;
	}

	private static Dictionary<int, FieldInfo> GetFieldsHashesForType(Type objType)
	{
		if (objType != null && fieldsHashesCache.TryGetValue(objType, out var value))
		{
			return value;
		}
		value = new Dictionary<int, FieldInfo>();
		if (objType == null)
		{
			return value;
		}
		foreach (FieldInfo item in GetSerializableFieldsForType(objType))
		{
			value.Add(item.Name.GetStableHashCode(), item);
		}
		fieldsHashesCache.Add(objType, value);
		return value;
	}

	private static void SerializeInternal(Type objType, object o, BinaryWriter bw, SerializerData sd)
	{
		if (o == null)
		{
			bw.Write(-1);
			return;
		}
		List<FieldInfo> serializableFieldsForType = GetSerializableFieldsForType(objType);
		if (TYPE_INTERFACE.IsAssignableFrom(objType))
		{
			objType.GetMethod("OnLazyPreSerialize").Invoke(o, null);
		}
		bw.Write(serializableFieldsForType.Count);
		foreach (FieldInfo item in serializableFieldsForType)
		{
			Type fieldType = item.FieldType;
			object value = item.GetValue(o);
			int stableHashCode = item.Name.GetStableHashCode();
			bw.Write(stableHashCode);
			if (!TrySerializeObject(fieldType, bw, value, sd))
			{
				if (fieldType.IsPrimitive)
				{
					Debug.LogError("Unsupported serialization type: " + fieldType.Name);
				}
				else
				{
					bw.Write((byte)250);
					SerializeInternal(fieldType, value, bw, sd);
				}
			}
			bw.Flush();
		}
	}

	private static bool TrySerializeObject(Type type, BinaryWriter bw, object value, SerializerData sd)
	{
		if (value == null)
		{
			bw.Write((byte)0);
			return true;
		}
		if (type == typeof(string))
		{
			string text = (string)value;
			if (text.Length == 0)
			{
				bw.Write((byte)11);
			}
			else if (text.Length > 30)
			{
				bw.Write((byte)9);
				WriteStringToStream(text, bw);
			}
			else
			{
				bw.Write((byte)10);
				WriteIndexedStringToStream(text, bw, sd);
			}
			return true;
		}
		if (type == typeof(int))
		{
			int num = (int)value;
			switch (num)
			{
			case 0:
				bw.Write((byte)16);
				break;
			case 1:
				bw.Write((byte)17);
				break;
			default:
				bw.Write((byte)3);
				bw.Write(num);
				break;
			}
			return true;
		}
		if (type == typeof(long))
		{
			bw.Write((byte)4);
			bw.Write((long)value);
			return true;
		}
		if (type == typeof(bool))
		{
			bw.Write((byte)(((bool)value) ? 1u : 2u));
			return true;
		}
		if (type == typeof(float))
		{
			float num2 = (float)value;
			if (num2 == 0f)
			{
				bw.Write((byte)18);
			}
			else if (num2 == 1f)
			{
				bw.Write((byte)19);
			}
			else
			{
				bw.Write((byte)5);
				bw.Write(num2);
			}
			return true;
		}
		if (type == typeof(double))
		{
			bw.Write((byte)6);
			bw.Write((double)value);
			return true;
		}
		if (type == typeof(byte))
		{
			bw.Write((byte)7);
			bw.Write((byte)value);
			return true;
		}
		if (type == typeof(char))
		{
			bw.Write((byte)8);
			bw.Write((char)value);
			return true;
		}
		if (type == typeof(Vector2))
		{
			Vector2 vector = (Vector2)value;
			if (vector.x == 0f && vector.y == 0f)
			{
				bw.Write((byte)20);
			}
			else if (vector.x == 1f && vector.y == 1f)
			{
				bw.Write((byte)21);
			}
			else
			{
				bw.Write((byte)13);
				bw.Write(vector.x);
				bw.Write(vector.y);
			}
			return true;
		}
		if (type == typeof(Vector3))
		{
			Vector3 vector2 = (Vector3)value;
			if (vector2.x == 0f && vector2.y == 0f && vector2.z == 0f)
			{
				bw.Write((byte)22);
			}
			else if (vector2.x == 1f && vector2.y == 1f && vector2.z == 1f)
			{
				bw.Write((byte)23);
			}
			else
			{
				bw.Write((byte)14);
				bw.Write(vector2.x);
				bw.Write(vector2.y);
				bw.Write(vector2.z);
			}
			return true;
		}
		if (type == typeof(Quaternion))
		{
			Quaternion quaternion = (Quaternion)value;
			if (quaternion.x == 0f && quaternion.y == 0f && quaternion.z == 0f && quaternion.w == 1f)
			{
				bw.Write((byte)24);
			}
			else
			{
				bw.Write((byte)15);
				bw.Write(quaternion.x);
				bw.Write(quaternion.y);
				bw.Write(quaternion.z);
				bw.Write(quaternion.w);
			}
			return true;
		}
		if (type == typeof(byte[]))
		{
			bw.Write((byte)102);
			byte[] array = (byte[])value;
			bw.Write(array.Length);
			bw.Write(array);
			return true;
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			Array array2 = value as Array;
			bw.Write((byte)101);
			bw.Write(array2.Length);
			for (int i = 0; i < array2.Length; i++)
			{
				object value2 = array2.GetValue(i);
				if (!TrySerializeObject(elementType, bw, value2, sd))
				{
					bw.Write((byte)250);
					SerializeInternal(elementType, value2, bw, sd);
				}
			}
			return true;
		}
		if (value is IList && type.IsGenericType)
		{
			IList list = value as IList;
			bw.Write((byte)100);
			bw.Write(list.Count);
			Type type2 = type.GetGenericArguments()[0];
			for (int j = 0; j < list.Count; j++)
			{
				object obj = list[j];
				if (!TrySerializeObject(type2, bw, obj, sd))
				{
					bw.Write((byte)250);
					SerializeInternal(type2, obj, bw, sd);
				}
			}
			return true;
		}
		return false;
	}

	private static T DeserializeInternal<T>(BinaryReader br, SerializerData sd) where T : new()
	{
		return (T)DeserializeInternal(typeof(T), br, sd);
	}

	private static object DeserializeInternal(Type objType, BinaryReader br, SerializerData sd)
	{
		object obj = ((objType == null) ? null : Activator.CreateInstance(objType));
		return DeserializeIntoInternal(objType, obj, br, sd);
	}

	private static object DeserializeIntoInternal(Type objType, object obj, BinaryReader br, SerializerData sd)
	{
		Dictionary<int, FieldInfo> fieldsHashesForType = GetFieldsHashesForType(objType);
		int num = br.ReadInt32();
		if (num == -1)
		{
			return null;
		}
		for (int i = 0; i < num; i++)
		{
			int key = br.ReadInt32();
			FieldInfo value = null;
			fieldsHashesForType.TryGetValue(key, out value);
			if (value == null)
			{
				Debug.LogWarning("Can't find a field to deserialize: " + key);
			}
			object obj2 = null;
			try
			{
				obj2 = DeserializeObject((value == null) ? null : value.FieldType, br, sd);
			}
			catch (MissingMethodException ex)
			{
				if (value != null)
				{
					Debug.LogError("Error deserializing object: " + value.FieldType?.ToString() + ", name = " + value.Name + ", ex: " + ex.Message);
				}
				throw;
			}
			if (value != null)
			{
				if (obj != null)
				{
					value.SetValue(obj, obj2);
				}
				continue;
			}
			Debug.LogError("Couldn't find a field to deserialize, obj_type = " + objType?.ToString() + ", field_type = " + ((obj2 == null) ? "null" : obj2.GetType().ToString()) + ", hash_size = " + fieldsHashesForType.Count);
		}
		if (objType != null && TYPE_INTERFACE.IsAssignableFrom(objType))
		{
			objType.GetMethod("OnLazyPostDeserialize").Invoke(obj, null);
		}
		return obj;
	}

	private static object DeserializeObject(Type objType, BinaryReader br, SerializerData sd)
	{
		FieldType fieldType = (FieldType)br.ReadByte();
		switch (fieldType)
		{
		case FieldType.NullValue:
			return null;
		case FieldType.String:
			return ReadStringFromStream(br);
		case FieldType.StringEmpty:
			return string.Empty;
		case FieldType.StringIndexer:
			return ReadIndexedStringFromStream(br, sd);
		case FieldType.Int32:
			return br.ReadInt32();
		case FieldType.Int32_0:
			return 0;
		case FieldType.Int32_1:
			return 1;
		case FieldType.Int64:
			return br.ReadInt64();
		case FieldType.Single:
			return br.ReadSingle();
		case FieldType.Single_0:
			return 0f;
		case FieldType.Single_1:
			return 1f;
		case FieldType.Double:
			return br.ReadDouble();
		case FieldType.Byte:
			return br.ReadByte();
		case FieldType.Char:
			return br.ReadChar();
		case FieldType.BoolTrue:
			return true;
		case FieldType.BoolFalse:
			return false;
		case FieldType.Vector2:
			return new Vector2(br.ReadSingle(), br.ReadSingle());
		case FieldType.Vector2_00:
			return Vector2.zero;
		case FieldType.Vector2_11:
			return Vector2.one;
		case FieldType.Vector3:
			return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		case FieldType.Vector3_000:
			return Vector3.zero;
		case FieldType.Vector3_111:
			return Vector3.one;
		case FieldType.Quaternion:
			return new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		case FieldType.Quaternion_0001:
			return new Quaternion(0f, 0f, 0f, 1f);
		case FieldType.ByteArray:
		{
			int count = br.ReadInt32();
			return br.ReadBytes(count);
		}
		case FieldType.Array:
		{
			int num2 = br.ReadInt32();
			Type type2 = ((objType == null) ? typeof(object) : objType.GetElementType());
			Array array = ((type2 == null) ? null : Array.CreateInstance(type2, num2));
			for (int j = 0; j < num2; j++)
			{
				object value2 = DeserializeObject(type2, br, sd);
				array?.SetValue(value2, j);
			}
			return array;
		}
		case FieldType.GenericList:
		{
			int num = br.ReadInt32();
			Type type = ((objType == null) ? typeof(object) : objType.GetGenericArguments()[0]);
			IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
			for (int i = 0; i < num; i++)
			{
				object value = DeserializeObject(type, br, sd);
				if (objType != null)
				{
					list.Add(value);
				}
			}
			return list;
		}
		case FieldType.LazySerialized:
			return DeserializeInternal(objType, br, sd);
		default:
			throw new NotImplementedException("Not implemented deserialization of a field type: " + fieldType);
		}
	}

	private static string ReadStringFromStream(BinaryReader br, bool encrypt = false)
	{
		int num = br.ReadInt32();
		if (num == -1)
		{
			return null;
		}
		int num2 = num;
		sb.Length = 0;
		while (num2 > 0)
		{
			int num3 = ((num2 <= buffer.Length) ? num2 : buffer.Length);
			num2 -= num3;
			long num4 = br.Read(buffer, 0, num3);
			if (encrypt)
			{
				for (int i = 0; i < num4; i++)
				{
					uint num5 = buffer[i];
					if (num5 <= 255 && num5 != 0 && num5 != 109)
					{
						num5 ^= 0x6Du;
						buffer[i] = (char)num5;
					}
				}
			}
			sb.Append(buffer, 0, num3);
			if (num4 < num3)
			{
				throw new EndOfStreamException();
			}
		}
		return sb.ToString();
	}

	private static string ReadIndexedStringFromStream(BinaryReader br, SerializerData sd)
	{
		int index = br.ReadInt32();
		return sd.strings[index];
	}

	private static void WriteIndexedStringToStream(string s, BinaryWriter bw, SerializerData sd)
	{
		int num = sd.strings.IndexOf(s);
		if (num == -1)
		{
			sd.strings.Add(s);
			num = sd.strings.Count - 1;
		}
		bw.Write(num);
	}

	private static void WriteStringToStream(string s, BinaryWriter bw, bool encrypt = false)
	{
		if (s == null)
		{
			bw.Write(-1);
			return;
		}
		char[] array = s.ToCharArray();
		bw.Write(array.Length);
		if (encrypt)
		{
			for (int i = 0; i < array.Length; i++)
			{
				uint num = array[i];
				if (num <= 255 && num != 0 && num != 109)
				{
					num ^= 0x6Du;
					array[i] = (char)num;
				}
			}
		}
		bw.Write(array);
	}

	private static Header ReadHeader(BinaryReader br)
	{
		Header result = new Header
		{
			sdataOffset = br.ReadInt64(),
			version = br.ReadInt32()
		};
		for (int i = 0; i < 15; i++)
		{
			br.ReadInt32();
		}
		return result;
	}

	private static void WriteHeader(BinaryWriter bw, Header header)
	{
		bw.Write(header.sdataOffset);
		bw.Write(header.version);
		for (int i = 0; i < 15; i++)
		{
			bw.Write(0);
		}
	}

	private static void WriteSerializerData(BinaryWriter bw, SerializerData sd)
	{
		long position = bw.BaseStream.Position;
		bw.BaseStream.Position = 0L;
		bw.Write(position);
		bw.BaseStream.Position = position;
		if (sd.strings == null)
		{
			bw.Write(0);
			return;
		}
		bw.Write(sd.strings.Count);
		foreach (string @string in sd.strings)
		{
			WriteStringToStream(@string, bw, encrypt: true);
		}
	}

	private static SerializerData ReadSerializerData(BinaryReader br)
	{
		long position = br.ReadInt64();
		long position2 = br.BaseStream.Position;
		br.BaseStream.Position = position;
		SerializerData serializerData = new SerializerData();
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string item = ReadStringFromStream(br, encrypt: true);
			serializerData.strings.Add(item);
		}
		br.BaseStream.Position = position2;
		return serializerData;
	}

	private static int GetStableHashCode(this string str)
	{
		if (stringHashes.TryGetValue(str, out var value))
		{
			return value;
		}
		int num = 5381;
		int num2 = num;
		for (int i = 0; i < str.Length && str[i] != 0; i += 2)
		{
			num = ((num << 5) + num) ^ str[i];
			if (i == str.Length - 1 || str[i + 1] == '\0')
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ str[i + 1];
		}
		value = num + num2 * 1566083941;
		stringHashes.Add(str, value);
		return value;
	}
}
