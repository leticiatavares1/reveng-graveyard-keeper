using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using LinqTools;
using UnityEngine;

public static class CommandFieldAttributeHelper
{
	public static List<FieldInfo> GetCommandSerializedFields(Type type)
	{
		List<FieldInfo> list = new List<FieldInfo>();
		foreach (FieldInfo item in from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where f.GetCustomAttribute<CommandFieldAttribute>() != null
			select f)
		{
			Type fieldType = item.FieldType;
			if (!fieldType.IsPrimitive && !(fieldType == typeof(string)) && !(fieldType == typeof(Vector2)) && !(fieldType == typeof(Vector3)) && !fieldType.IsEnum && !typeof(IEnumerable).IsAssignableFrom(fieldType))
			{
				throw new Exception($"Unsupported type: {fieldType}");
			}
			list.Add(item);
		}
		return list;
	}

	public static int GetCommandFieldsSerializedSize<T>(T obj, List<FieldInfo> fieldInfos)
	{
		int num = 0;
		foreach (FieldInfo fieldInfo in fieldInfos)
		{
			Type fieldType = fieldInfo.FieldType;
			object value = fieldInfo.GetValue(obj);
			if (fieldType.IsPrimitive)
			{
				num += Marshal.SizeOf(fieldType);
				continue;
			}
			if (fieldType == typeof(string))
			{
				num += Encoding.UTF32.GetBytes((string)value).Length;
				continue;
			}
			if (fieldType == typeof(Vector2))
			{
				num += 8;
				continue;
			}
			if (fieldType == typeof(Vector3))
			{
				num += 12;
				continue;
			}
			if (fieldType.IsEnum)
			{
				Type underlyingType = Enum.GetUnderlyingType(fieldType);
				num += Marshal.SizeOf(underlyingType) * Enum.GetNames(fieldType).Length;
				continue;
			}
			if (typeof(IEnumerable).IsAssignableFrom(fieldType))
			{
				foreach (object item in (IEnumerable)value)
				{
					num += GetCommandFieldsSerializedSize(item, GetCommandSerializedFields(item.GetType()));
				}
				continue;
			}
			throw new Exception($"Unsupported type: {fieldType}, value: {value}");
		}
		return num;
	}
}
