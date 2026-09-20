using System.Text;

public static class GarbagelessStrings
{
	private static StringBuilder _sb = new StringBuilder();

	public static void IntToCharsWithLeadingZeros(int v, ref char[] c, int sign_chars, int start_pos)
	{
		int num = start_pos + sign_chars - 1;
		for (int num2 = sign_chars; num2 > 0; num2--)
		{
			c[num] = (char)(48 + v % 10);
			v /= 10;
			num--;
		}
	}

	public static void StringToChars(ref string s, ref char[] c)
	{
		int length = s.Length;
		for (int i = 0; i < length; i++)
		{
			c[i] = s[i];
		}
		c[length] = '\0';
	}

	public static string CharsToString(ref char[] chars)
	{
		_sb.Length = 0;
		for (int i = 0; i < chars.Length; i++)
		{
			char c = chars[i];
			if (c == '\0')
			{
				break;
			}
			_sb.Append(c);
		}
		return _sb.ToString();
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
