using System.Globalization;

public static class NumericExtensions
{
	public static int ToInt32Invariant(this string value, int defaultValue = 0)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return defaultValue;
		}
		if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public static string ToInvariantCultureString(this int value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public static string ToInvariantCultureString(this float value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
