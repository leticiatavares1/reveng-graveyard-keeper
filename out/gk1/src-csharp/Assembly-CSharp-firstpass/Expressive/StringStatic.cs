namespace Expressive;

public static class StringStatic
{
	public static bool IsNullOrWhiteSpace(this string value)
	{
		if (value == null)
		{
			return true;
		}
		return string.IsNullOrEmpty(value.Trim());
	}
}
