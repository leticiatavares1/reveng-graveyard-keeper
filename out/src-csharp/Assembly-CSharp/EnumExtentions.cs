using System;

public static class EnumExtentions
{
	public static bool Is<T>(this T value, T flags) where T : struct
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("The type parameter T must be an enum type.");
		}
		return ((int)(object)value & (int)(object)flags) != 0;
	}

	public static bool IsNot<T>(this T value, T flags) where T : struct
	{
		return !value.Is(flags);
	}
}
