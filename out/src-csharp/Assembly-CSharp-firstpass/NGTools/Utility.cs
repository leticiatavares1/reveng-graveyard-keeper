using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NGTools;

public static class Utility
{
	public const BindingFlags ExposedBindingFlags = BindingFlags.Instance | BindingFlags.Public;

	public static ByteBuffer sharedBBuffer = new ByteBuffer(128);

	public static StringBuilder sharedBuffer = new StringBuilder(128);

	private static Dictionary<Assembly, Type[]> assemblyTypes = new Dictionary<Assembly, Type[]>();

	private static List<ICollectionModifier> cachedCollectionModifiers = new List<ICollectionModifier>();

	public static string ClipBoard
	{
		get
		{
			return GUIUtility.systemCopyBuffer;
		}
		set
		{
			GUIUtility.systemCopyBuffer = value;
		}
	}

	public static IEnumerable<Type> EachAssignableFrom(Type baseType, Func<Type, bool> match = null)
	{
		if (!assemblyTypes.TryGetValue(baseType.Assembly(), out var types))
		{
			types = baseType.Assembly().GetTypes();
			assemblyTypes[baseType.Assembly()] = types;
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (baseType.IsAssignableFrom(types[i]) && types[i] != baseType && (match == null || match(types[i])))
			{
				yield return types[i];
			}
		}
	}

	public static IEnumerable<Type> EachSubClassesOf(Type baseType, Func<Type, bool> match = null)
	{
		if (!assemblyTypes.TryGetValue(baseType.Assembly(), out var types))
		{
			types = baseType.Assembly().GetTypes();
			assemblyTypes[baseType.Assembly()] = types;
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i].IsSubclassOf(baseType) && (match == null || match(types[i])))
			{
				yield return types[i];
			}
		}
	}

	public static bool CanExposeTypeInInspector(Type type)
	{
		if (!type.IsInterface() && (type.IsPrimitive() || type == typeof(string) || type.IsEnum() || type == typeof(Rect) || type == typeof(Vector3) || type == typeof(Color) || typeof(UnityEngine.Object).IsAssignableFrom(type) || type == typeof(UnityEngine.Object) || type == typeof(Vector2) || type == typeof(Vector4) || type == typeof(Quaternion) || type == typeof(AnimationCurve)))
		{
			return true;
		}
		if (type.GetInterface(typeof(IList<>).Name) != null)
		{
			Type type2 = type.GetInterface(typeof(IList<>).Name).GetGenericArguments()[0];
			if (type2.GetInterface(typeof(IList<>).Name) == null)
			{
				return CanExposeTypeInInspector(type2);
			}
			return false;
		}
		if (typeof(IList).IsAssignableFrom(type))
		{
			return false;
		}
		if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
		{
			return false;
		}
		if (type != typeof(decimal) && (type.IsClass() || type.IsStruct()) && type.GetCustomAttributes(typeof(SerializableAttribute), inherit: true).Length != 0)
		{
			return true;
		}
		return false;
	}

	public static Type GetArraySubType(Type arrayType)
	{
		if (arrayType.IsArray)
		{
			return arrayType.GetElementType();
		}
		Type @interface = arrayType.GetInterface(typeof(IList<>).Name);
		if (@interface != null)
		{
			return @interface.GetGenericArguments()[0];
		}
		return null;
	}

	public static List<FieldInfo> GetFieldsHierarchyOrdered(Type t, Type stopType, BindingFlags flags)
	{
		Stack<Type> stack = new Stack<Type>();
		List<FieldInfo> list = new List<FieldInfo>();
		stack.Push(t);
		while (t.BaseType() != stopType)
		{
			stack.Push(t.BaseType());
			t = t.BaseType();
		}
		foreach (Type item in stack)
		{
			list.AddRange(item.GetFields(flags | BindingFlags.DeclaredOnly));
		}
		return list;
	}

	public static float RelativeAngle(Vector3 fwd, Vector3 targetDir, Vector3 upDir)
	{
		float num = Vector3.Angle(fwd, targetDir);
		if (AngleDirection(fwd, targetDir, upDir) == -1f)
		{
			return 0f - num;
		}
		return num;
	}

	public static float RelativeAngle(Vector2 fwd, Vector2 targetDir, Vector3 upDir)
	{
		float num = Vector2.Angle(fwd, targetDir);
		if (AngleDirection(fwd, targetDir, upDir) == -1f)
		{
			return 0f - num;
		}
		return num;
	}

	public static float AngleDirection(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		float num = Vector3.Dot(Vector3.Cross(fwd, targetDir), up);
		if (num > 0f)
		{
			return 1f;
		}
		if (num < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	public static float AngleDirection(Vector2 fwd, Vector2 targetDir, Vector3 up)
	{
		float num = Vector3.Dot(Vector3.Cross(new Vector3(fwd.x, 0f, fwd.y), new Vector3(targetDir.x, 0f, targetDir.y)), up);
		if (num > 0f)
		{
			return 1f;
		}
		if (num < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	public static bool IsUnityArray(this Type t)
	{
		if (!t.IsArray)
		{
			return typeof(IList).IsAssignableFrom(t);
		}
		return true;
	}

	public static bool IsStruct(this Type t)
	{
		if (t.IsValueType() && !t.IsPrimitive() && !t.IsEnum())
		{
			return t != typeof(decimal);
		}
		return false;
	}

	public static string GetShortAssemblyType(this Type t)
	{
		if (!t.Assembly().FullName.StartsWith("mscorlib"))
		{
			return t.FullName + "," + t.Assembly().FullName.Substring(0, t.Assembly().FullName.IndexOf(','));
		}
		return t.FullName;
	}

	public static IFieldModifier GetFieldInfo(Type type, string name)
	{
		FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null)
		{
			return new FieldModifier(field);
		}
		PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null)
		{
			return new PropertyModifier(property);
		}
		throw new MissingFieldException("Field or property \"" + name + "\" was not found in type \"" + type.Name + "\".");
	}

	public static ICollectionModifier GetCollectionModifier(object rawArray)
	{
		if (rawArray is Array array)
		{
			for (int i = 0; i < cachedCollectionModifiers.Count; i++)
			{
				if (cachedCollectionModifiers[i] is ArrayModifier arrayModifier)
				{
					arrayModifier.array = array;
					cachedCollectionModifiers.RemoveAt(i);
					return arrayModifier;
				}
			}
			return new ArrayModifier(array);
		}
		if (rawArray is IList list)
		{
			for (int j = 0; j < cachedCollectionModifiers.Count; j++)
			{
				if (cachedCollectionModifiers[j] is ListModifier listModifier)
				{
					listModifier.list = list;
					cachedCollectionModifiers.RemoveAt(j);
					return listModifier;
				}
			}
			return new ListModifier(list);
		}
		throw new Exception("Collection of type \"" + rawArray.GetType()?.ToString() + "\" is not supported.");
	}

	public static void ReturnCollectionModifier(ICollectionModifier modifier)
	{
		cachedCollectionModifiers.Add(modifier);
	}

	public static bool IsComponentEnableable(Component component)
	{
		Type type = component.GetType();
		if (component is Behaviour)
		{
			if (type.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null || type.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null || type.GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null || type.GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
			{
				return true;
			}
			return false;
		}
		if (type.GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
		{
			return true;
		}
		return false;
	}
}
