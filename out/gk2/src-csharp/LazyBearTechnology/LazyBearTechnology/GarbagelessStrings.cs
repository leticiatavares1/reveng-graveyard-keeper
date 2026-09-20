using System.Text;

namespace LazyBearTechnology;

public static class GarbagelessStrings
{
	private static StringBuilder stringBuilder = new StringBuilder();

	public static void IntToCharsWithLeadingZeros(int value, ref char[] chars, int signChars, int startPos)
	{
		int num = startPos + signChars - 1;
		for (int num2 = signChars; num2 > 0; num2--)
		{
			chars[num] = (char)(48 + value % 10);
			value /= 10;
			num--;
		}
	}

	public static void StringToChars(ref string text, ref char[] chars)
	{
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			chars[i] = text[i];
		}
		chars[length] = '\0';
	}

	public static string CharsToString(ref char[] chars)
	{
		stringBuilder.Length = 0;
		for (int i = 0; i < chars.Length; i++)
		{
			char c = chars[i];
			if (c == '\0')
			{
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	public static int GetHashCode(ref char[] chars)
	{
		int num = 5381;
		int num2 = num;
		for (int i = 0; i < chars.Length && chars[i] != 0; i += 2)
		{
			num = ((num << 5) + num) ^ chars[i];
			if (i == chars.Length - 1 || chars[i + 1] == '\0')
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ chars[i + 1];
		}
		return num + num2 * 1566083941;
	}
}
