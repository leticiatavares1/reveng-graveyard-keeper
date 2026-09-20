using System.Text;

public static class StringExtensions
{
	public static string ConcatWithSeparator(this string str, string strToAppend, string separator = "\n")
	{
		StringBuilder stringBuilder = new StringBuilder(str);
		if (!string.IsNullOrEmpty(str))
		{
			stringBuilder.Append(separator);
		}
		return stringBuilder.Append(strToAppend).ToString();
	}
}
