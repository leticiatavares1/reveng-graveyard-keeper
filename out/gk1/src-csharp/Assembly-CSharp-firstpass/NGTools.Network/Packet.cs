using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NGTools.Network;

public abstract class Packet
{
	private static Dictionary<Type, int> cachedPacketId = new Dictionary<Type, int>();

	private static Dictionary<Type, FieldInfo[]> cachedPacketFields = new Dictionary<Type, FieldInfo[]>();

	public readonly int packetId;

	public readonly bool isBatchable;

	protected Packet()
	{
		if (!cachedPacketId.TryGetValue(GetType(), out packetId))
		{
			PacketLinkToAttribute[] array = GetType().GetCustomAttributes(typeof(PacketLinkToAttribute), inherit: true) as PacketLinkToAttribute[];
			if (array.Length != 1)
			{
				throw new MissingComponentException("Missing attribute PacketLinkToAttribute on " + ToString());
			}
			packetId = array[0].packetId;
			isBatchable = array[0].isBatchable;
			cachedPacketId.Add(GetType(), packetId);
		}
	}

	protected Packet(ByteBuffer buffer)
		: this()
	{
		try
		{
			In(buffer);
		}
		catch (Exception exception)
		{
			InternalNGDebug.LogFileException(GetType().ToString(), exception);
			throw;
		}
	}

	public virtual void Out(ByteBuffer buffer)
	{
		FieldInfo[] fields = GetFields(GetType());
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].FieldType == typeof(string))
			{
				buffer.AppendUnicodeString((string)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType.IsEnum())
			{
				buffer.Append((int)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(bool))
			{
				buffer.Append((bool)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(byte))
			{
				buffer.Append((byte)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(sbyte))
			{
				buffer.Append((sbyte)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(char))
			{
				buffer.Append((char)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(float))
			{
				buffer.Append((float)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(double))
			{
				buffer.Append((double)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(short))
			{
				buffer.Append((short)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(int))
			{
				buffer.Append((int)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(long))
			{
				buffer.Append((long)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(ushort))
			{
				buffer.Append((ushort)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(uint))
			{
				buffer.Append((uint)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(ulong))
			{
				buffer.Append((ulong)fields[i].GetValue(this));
				continue;
			}
			if (fields[i].FieldType == typeof(Vector2))
			{
				Vector2 vector = (Vector2)fields[i].GetValue(this);
				buffer.Append(vector.x);
				buffer.Append(vector.y);
				continue;
			}
			if (fields[i].FieldType == typeof(Vector3))
			{
				Vector3 vector2 = (Vector3)fields[i].GetValue(this);
				buffer.Append(vector2.x);
				buffer.Append(vector2.y);
				buffer.Append(vector2.z);
				continue;
			}
			if (fields[i].FieldType == typeof(Vector4))
			{
				Vector4 vector3 = (Vector4)fields[i].GetValue(this);
				buffer.Append(vector3.x);
				buffer.Append(vector3.y);
				buffer.Append(vector3.z);
				buffer.Append(vector3.w);
				continue;
			}
			if (fields[i].FieldType == typeof(Rect))
			{
				Rect rect = (Rect)fields[i].GetValue(this);
				buffer.Append(rect.x);
				buffer.Append(rect.y);
				buffer.Append(rect.width);
				buffer.Append(rect.height);
				continue;
			}
			if (fields[i].FieldType == typeof(Quaternion))
			{
				Quaternion quaternion = (Quaternion)fields[i].GetValue(this);
				buffer.Append(quaternion.x);
				buffer.Append(quaternion.y);
				buffer.Append(quaternion.z);
				buffer.Append(quaternion.w);
				continue;
			}
			if (fields[i].FieldType == typeof(Type))
			{
				buffer.AppendUnicodeString(((Type)fields[i].GetValue(this)).GetShortAssemblyType());
				continue;
			}
			if (fields[i].FieldType == typeof(byte[]))
			{
				byte[] array = (byte[])fields[i].GetValue(this);
				buffer.Append(array.Length);
				buffer.Append(array);
				continue;
			}
			if (fields[i].FieldType.IsUnityArray())
			{
				ICollectionModifier collectionModifier = Utility.GetCollectionModifier(fields[i].GetValue(this));
				buffer.Append(collectionModifier.Size);
				AppendArrayToBuffer(collectionModifier, Utility.GetArraySubType(fields[i].FieldType), buffer);
				continue;
			}
			throw new NotSupportedException("Type \"" + fields[i].FieldType?.ToString() + "\" is not supported.");
		}
	}

	public virtual void In(ByteBuffer buffer)
	{
		FieldInfo[] fields = GetFields(GetType());
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].FieldType == typeof(int))
			{
				fields[i].SetValue(this, buffer.ReadInt32());
				continue;
			}
			if (fields[i].FieldType == typeof(string))
			{
				fields[i].SetValue(this, buffer.ReadUnicodeString());
				continue;
			}
			if (fields[i].FieldType == typeof(float))
			{
				fields[i].SetValue(this, buffer.ReadSingle());
				continue;
			}
			if (fields[i].FieldType.IsEnum())
			{
				fields[i].SetValue(this, buffer.ReadInt32());
				continue;
			}
			if (fields[i].FieldType == typeof(bool))
			{
				fields[i].SetValue(this, buffer.ReadBoolean());
				continue;
			}
			if (fields[i].FieldType == typeof(byte))
			{
				fields[i].SetValue(this, buffer.ReadByte());
				continue;
			}
			if (fields[i].FieldType == typeof(sbyte))
			{
				fields[i].SetValue(this, buffer.ReadSByte());
				continue;
			}
			if (fields[i].FieldType == typeof(char))
			{
				fields[i].SetValue(this, buffer.ReadChar());
				continue;
			}
			if (fields[i].FieldType == typeof(double))
			{
				fields[i].SetValue(this, buffer.ReadDouble());
				continue;
			}
			if (fields[i].FieldType == typeof(short))
			{
				fields[i].SetValue(this, buffer.ReadInt16());
				continue;
			}
			if (fields[i].FieldType == typeof(long))
			{
				fields[i].SetValue(this, buffer.ReadInt64());
				continue;
			}
			if (fields[i].FieldType == typeof(ushort))
			{
				fields[i].SetValue(this, buffer.ReadUInt16());
				continue;
			}
			if (fields[i].FieldType == typeof(uint))
			{
				fields[i].SetValue(this, buffer.ReadUInt32());
				continue;
			}
			if (fields[i].FieldType == typeof(ulong))
			{
				fields[i].SetValue(this, buffer.ReadUInt64());
				continue;
			}
			if (fields[i].FieldType == typeof(Vector2))
			{
				fields[i].SetValue(this, new Vector2(buffer.ReadSingle(), buffer.ReadSingle()));
				continue;
			}
			if (fields[i].FieldType == typeof(Vector3))
			{
				fields[i].SetValue(this, new Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle()));
				continue;
			}
			if (fields[i].FieldType == typeof(Vector4))
			{
				fields[i].SetValue(this, new Vector4(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle()));
				continue;
			}
			if (fields[i].FieldType == typeof(Rect))
			{
				fields[i].SetValue(this, new Rect(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle()));
				continue;
			}
			if (fields[i].FieldType == typeof(Quaternion))
			{
				fields[i].SetValue(this, new Quaternion(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle()));
				continue;
			}
			if (fields[i].FieldType == typeof(Type))
			{
				fields[i].SetValue(this, Type.GetType(buffer.ReadUnicodeString()));
				continue;
			}
			if (fields[i].FieldType == typeof(byte[]))
			{
				fields[i].SetValue(this, buffer.ReadBytes(buffer.ReadInt32()));
				continue;
			}
			if (fields[i].FieldType.IsUnityArray())
			{
				object obj;
				if (fields[i].FieldType.IsArray)
				{
					obj = Array.CreateInstance(fields[i].FieldType.GetElementType(), buffer.ReadInt32());
				}
				else
				{
					int num = buffer.ReadInt32();
					obj = Activator.CreateInstance(fields[i].FieldType, num);
					IList list = (IList)obj;
					object value = ((!Utility.GetArraySubType(fields[i].FieldType).IsValueType()) ? null : Activator.CreateInstance(Utility.GetArraySubType(fields[i].FieldType)));
					for (int j = 0; j < num; j++)
					{
						list.Add(value);
					}
				}
				ICollectionModifier collectionModifier = Utility.GetCollectionModifier(obj);
				ReadArrayFromBuffer(collectionModifier, Utility.GetArraySubType(fields[i].FieldType), buffer);
				fields[i].SetValue(this, obj);
				continue;
			}
			throw new NotSupportedException("Type \"" + fields[i].FieldType?.ToString() + "\" is not supported.");
		}
	}

	public virtual void OnGUI(IUnityData unityData)
	{
		GUILayout.Label(GetType().Name);
	}

	public virtual bool AggregateInto(Packet x)
	{
		return false;
	}

	private void AppendArrayToBuffer(ICollectionModifier array, Type type, ByteBuffer buffer)
	{
		if (type == typeof(string))
		{
			for (int i = 0; i < array.Size; i++)
			{
				buffer.AppendUnicodeString((string)array.Get(i));
			}
		}
		else if (type.IsEnum())
		{
			for (int j = 0; j < array.Size; j++)
			{
				buffer.Append((int)array.Get(j));
			}
		}
		else if (type == typeof(bool))
		{
			for (int k = 0; k < array.Size; k++)
			{
				buffer.Append((bool)array.Get(k));
			}
		}
		else if (type == typeof(byte))
		{
			for (int l = 0; l < array.Size; l++)
			{
				buffer.Append((byte)array.Get(l));
			}
		}
		else if (type == typeof(sbyte))
		{
			for (int m = 0; m < array.Size; m++)
			{
				buffer.Append((sbyte)array.Get(m));
			}
		}
		else if (type == typeof(char))
		{
			for (int n = 0; n < array.Size; n++)
			{
				buffer.Append((char)array.Get(n));
			}
		}
		else if (type == typeof(float))
		{
			for (int num = 0; num < array.Size; num++)
			{
				buffer.Append((float)array.Get(num));
			}
		}
		else if (type == typeof(double))
		{
			for (int num2 = 0; num2 < array.Size; num2++)
			{
				buffer.Append((double)array.Get(num2));
			}
		}
		else if (type == typeof(short))
		{
			for (int num3 = 0; num3 < array.Size; num3++)
			{
				buffer.Append((short)array.Get(num3));
			}
		}
		else if (type == typeof(int))
		{
			for (int num4 = 0; num4 < array.Size; num4++)
			{
				buffer.Append((int)array.Get(num4));
			}
		}
		else if (type == typeof(long))
		{
			for (int num5 = 0; num5 < array.Size; num5++)
			{
				buffer.Append((long)array.Get(num5));
			}
		}
		else if (type == typeof(ushort))
		{
			for (int num6 = 0; num6 < array.Size; num6++)
			{
				buffer.Append((ushort)array.Get(num6));
			}
		}
		else if (type == typeof(uint))
		{
			for (int num7 = 0; num7 < array.Size; num7++)
			{
				buffer.Append((uint)array.Get(num7));
			}
		}
		else if (type == typeof(ulong))
		{
			for (int num8 = 0; num8 < array.Size; num8++)
			{
				buffer.Append((ulong)array.Get(num8));
			}
		}
	}

	private void ReadArrayFromBuffer(ICollectionModifier array, Type type, ByteBuffer buffer)
	{
		if (type == typeof(string))
		{
			for (int i = 0; i < array.Size; i++)
			{
				array.Set(i, buffer.ReadUnicodeString());
			}
		}
		else if (type.IsEnum())
		{
			for (int j = 0; j < array.Size; j++)
			{
				array.Set(j, buffer.ReadInt32());
			}
		}
		else if (type == typeof(bool))
		{
			for (int k = 0; k < array.Size; k++)
			{
				array.Set(k, buffer.ReadBoolean());
			}
		}
		else if (type == typeof(byte))
		{
			for (int l = 0; l < array.Size; l++)
			{
				array.Set(l, buffer.ReadByte());
			}
		}
		else if (type == typeof(sbyte))
		{
			for (int m = 0; m < array.Size; m++)
			{
				array.Set(m, buffer.ReadSByte());
			}
		}
		else if (type == typeof(char))
		{
			for (int n = 0; n < array.Size; n++)
			{
				array.Set(n, buffer.ReadChar());
			}
		}
		else if (type == typeof(float))
		{
			for (int num = 0; num < array.Size; num++)
			{
				array.Set(num, buffer.ReadSingle());
			}
		}
		else if (type == typeof(double))
		{
			for (int num2 = 0; num2 < array.Size; num2++)
			{
				array.Set(num2, buffer.ReadDouble());
			}
		}
		else if (type == typeof(short))
		{
			for (int num3 = 0; num3 < array.Size; num3++)
			{
				array.Set(num3, buffer.ReadInt16());
			}
		}
		else if (type == typeof(int))
		{
			for (int num4 = 0; num4 < array.Size; num4++)
			{
				array.Set(num4, buffer.ReadInt32());
			}
		}
		else if (type == typeof(long))
		{
			for (int num5 = 0; num5 < array.Size; num5++)
			{
				array.Set(num5, buffer.ReadInt64());
			}
		}
		else if (type == typeof(ushort))
		{
			for (int num6 = 0; num6 < array.Size; num6++)
			{
				array.Set(num6, buffer.ReadUInt16());
			}
		}
		else if (type == typeof(uint))
		{
			for (int num7 = 0; num7 < array.Size; num7++)
			{
				array.Set(num7, buffer.ReadUInt32());
			}
		}
		else if (type == typeof(ulong))
		{
			for (int num8 = 0; num8 < array.Size; num8++)
			{
				array.Set(num8, buffer.ReadUInt64());
			}
		}
	}

	private FieldInfo[] GetFields(Type type)
	{
		if (!cachedPacketFields.TryGetValue(type, out var value))
		{
			List<FieldInfo> list = new List<FieldInfo>(type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public));
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsDefined(typeof(StripFromNetworkAttribute), inherit: false))
				{
					list.RemoveAt(i);
					i--;
				}
			}
			value = list.ToArray();
			cachedPacketFields.Add(type, value);
		}
		return value;
	}
}
