using System;
using System.Reflection;
using UnityEngine;

namespace LazyBearTechnology;

public abstract class Enumeration
{
	public int value;

	protected Enumeration(int value)
	{
		this.value = value;
	}

	public static T GetByStaticFieldName<T>(string name) where T : Enumeration
	{
		try
		{
			return (T)typeof(T).GetField(name, BindingFlags.Static | BindingFlags.Public).GetValue(null);
		}
		catch (Exception)
		{
			Debug.LogError("Can't get static field for class:[T] input name:[" + name + "]");
			return null;
		}
	}

	public static string GetNameOfStaticField<T>(int value) where T : Enumeration
	{
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			if (((T)fields[i].GetValue(null)).value == value)
			{
				return fields[i].Name;
			}
		}
		return string.Empty;
	}

	public override int GetHashCode()
	{
		return -1584136870 + value.GetHashCode();
	}

	public static bool operator ==(Enumeration key1, Enumeration key2)
	{
		if ((object)key1 == null || (object)key2 == null)
		{
			return false;
		}
		return key1.value == key2.value;
	}

	public static bool operator !=(Enumeration key1, Enumeration key2)
	{
		if ((object)key1 == null || (object)key2 == null)
		{
			return false;
		}
		return key1.value != key2.value;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Enumeration enumeration)
		{
			return value == enumeration.value;
		}
		return false;
	}
}
