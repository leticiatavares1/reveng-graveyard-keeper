using System;

public static class ClassExtensions
{
	public static T InvokeMethod<T>(this T obj, Action<T> action)
	{
		action(obj);
		return obj;
	}
}
