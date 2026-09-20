using System.Text;

namespace LazyBearTechnology;

public static class CjkWrapper
{
	private const string SmallKana = "ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヮヵヶ";

	private const string NoStartOther = "、。，．,.：；:;！？!?・）］｝〕〉》」』】〙〗”’ー〜～ヽヾゝゞ々〆〇〵\u309b\u309c";

	private const string NoEnd = "（［｛〔〈《「『【〘〖“‘";

	private static bool IsSmallKana(char c)
	{
		return "ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヮヵヶ".IndexOf(c) >= 0;
	}

	private static bool IsNoStartOther(char c)
	{
		return "、。，．,.：；:;！？!?・）］｝〕〉》」』】〙〗”’ー〜～ヽヾゝゞ々〆〇〵\u309b\u309c".IndexOf(c) >= 0;
	}

	private static bool IsNoEnd(char c)
	{
		return "（［｛〔〈《「『【〘〖“‘".IndexOf(c) >= 0;
	}

	public static string Wrap(string text, int maxWidth, SmallKanaKinsoku smallKana = SmallKanaKinsoku.Oidashi)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text ?? string.Empty;
		}
		if (maxWidth < 1)
		{
			maxWidth = 1;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(WrapParagraph(array[i], maxWidth, smallKana));
		}
		return stringBuilder.ToString();
	}

	private static string WrapParagraph(string text, int maxWidth, SmallKanaKinsoku smallKana)
	{
		if (text.Length == 0)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length + 8);
		int num = 0;
		int num2 = 0;
		foreach (char c in text)
		{
			int characterWidth = GetCharacterWidth(c);
			if (num2 + characterWidth <= maxWidth || stringBuilder.Length <= num)
			{
				stringBuilder.Append(c);
				num2 += characterWidth;
				continue;
			}
			if (IsNoStartOther(c) || (IsSmallKana(c) && smallKana == SmallKanaKinsoku.Oikomi))
			{
				stringBuilder.Append(c);
				num2 += characterWidth;
				continue;
			}
			int minCarry = ((IsSmallKana(c) && smallKana == SmallKanaKinsoku.Oidashi) ? 1 : 0);
			int num3 = ComputeCarry(stringBuilder, num, minCarry, smallKana);
			if (num3 < 0)
			{
				stringBuilder.Append(c);
				num2 += characterWidth;
				continue;
			}
			string text2 = ((num3 > 0) ? stringBuilder.ToString(stringBuilder.Length - num3, num3) : string.Empty);
			stringBuilder.Length -= num3;
			stringBuilder.Append('\n');
			num = stringBuilder.Length;
			stringBuilder.Append(text2);
			stringBuilder.Append(c);
			num2 = MeasureWidth(text2) + characterWidth;
		}
		return stringBuilder.ToString();
	}

	private static int ComputeCarry(StringBuilder sb, int lineStart, int minCarry, SmallKanaKinsoku smallKana)
	{
		int num = sb.Length - lineStart;
		int num2 = minCarry;
		if (num2 > num)
		{
			return -1;
		}
		bool flag = smallKana == SmallKanaKinsoku.Oidashi;
		bool flag2 = true;
		while (flag2)
		{
			flag2 = false;
			while (num2 < num)
			{
				char c = sb[sb.Length - num2 - 1];
				if (!IsNoEnd(c) && (!flag || !IsSmallKana(c)))
				{
					break;
				}
				num2++;
				flag2 = true;
			}
			while (flag && num2 > 0 && num2 < num && IsSmallKana(sb[sb.Length - num2]))
			{
				num2++;
				flag2 = true;
			}
		}
		if (num2 >= num)
		{
			return -1;
		}
		return num2;
	}

	private static int MeasureWidth(string s)
	{
		int num = 0;
		foreach (char c in s)
		{
			num += GetCharacterWidth(c);
		}
		return num;
	}

	private static int GetCharacterWidth(char c)
	{
		if ((c >= 'ᄀ' && c <= 'ᅟ') || (c >= '⺀' && c <= '〾') || (c >= 'ぁ' && c <= '㏿') || (c >= '㐀' && c <= '䶿') || (c >= '一' && c <= '鿿') || (c >= 'ꀀ' && c <= '\ua4cf') || (c >= '가' && c <= '힣') || (c >= '豈' && c <= '\ufaff') || (c >= '︰' && c <= '\ufe4f') || (c >= '\uff00' && c <= '｠') || (c >= '￠' && c <= '￦'))
		{
			return 2;
		}
		return 1;
	}
}
