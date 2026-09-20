using System;
using System.Collections.Generic;
using System.Text;

namespace LazyBearTechnology;

public static class ArabicShaper
{
	private const char Tatweel = 'ـ';

	private const char Lam = 'ل';

	private static readonly Dictionary<char, char[]> Forms = new Dictionary<char, char[]>
	{
		{
			'ء',
			new char[4] { 'ﺀ', '\0', '\0', '\0' }
		},
		{
			'آ',
			new char[4] { 'ﺁ', 'ﺂ', '\0', '\0' }
		},
		{
			'أ',
			new char[4] { 'ﺃ', 'ﺄ', '\0', '\0' }
		},
		{
			'ؤ',
			new char[4] { 'ﺅ', 'ﺆ', '\0', '\0' }
		},
		{
			'إ',
			new char[4] { 'ﺇ', 'ﺈ', '\0', '\0' }
		},
		{
			'ئ',
			new char[4] { 'ﺉ', 'ﺊ', 'ﺋ', 'ﺌ' }
		},
		{
			'ا',
			new char[4] { 'ﺍ', 'ﺎ', '\0', '\0' }
		},
		{
			'ب',
			new char[4] { 'ﺏ', 'ﺐ', 'ﺑ', 'ﺒ' }
		},
		{
			'ة',
			new char[4] { 'ﺓ', 'ﺔ', '\0', '\0' }
		},
		{
			'ت',
			new char[4] { 'ﺕ', 'ﺖ', 'ﺗ', 'ﺘ' }
		},
		{
			'ث',
			new char[4] { 'ﺙ', 'ﺚ', 'ﺛ', 'ﺜ' }
		},
		{
			'ج',
			new char[4] { 'ﺝ', 'ﺞ', 'ﺟ', 'ﺠ' }
		},
		{
			'ح',
			new char[4] { 'ﺡ', 'ﺢ', 'ﺣ', 'ﺤ' }
		},
		{
			'خ',
			new char[4] { 'ﺥ', 'ﺦ', 'ﺧ', 'ﺨ' }
		},
		{
			'د',
			new char[4] { 'ﺩ', 'ﺪ', '\0', '\0' }
		},
		{
			'ذ',
			new char[4] { 'ﺫ', 'ﺬ', '\0', '\0' }
		},
		{
			'ر',
			new char[4] { 'ﺭ', 'ﺮ', '\0', '\0' }
		},
		{
			'ز',
			new char[4] { 'ﺯ', 'ﺰ', '\0', '\0' }
		},
		{
			'س',
			new char[4] { 'ﺱ', 'ﺲ', 'ﺳ', 'ﺴ' }
		},
		{
			'ش',
			new char[4] { 'ﺵ', 'ﺶ', 'ﺷ', 'ﺸ' }
		},
		{
			'ص',
			new char[4] { 'ﺹ', 'ﺺ', 'ﺻ', 'ﺼ' }
		},
		{
			'ض',
			new char[4] { 'ﺽ', 'ﺾ', 'ﺿ', 'ﻀ' }
		},
		{
			'ط',
			new char[4] { 'ﻁ', 'ﻂ', 'ﻃ', 'ﻄ' }
		},
		{
			'ظ',
			new char[4] { 'ﻅ', 'ﻆ', 'ﻇ', 'ﻈ' }
		},
		{
			'ع',
			new char[4] { 'ﻉ', 'ﻊ', 'ﻋ', 'ﻌ' }
		},
		{
			'غ',
			new char[4] { 'ﻍ', 'ﻎ', 'ﻏ', 'ﻐ' }
		},
		{
			'ف',
			new char[4] { 'ﻑ', 'ﻒ', 'ﻓ', 'ﻔ' }
		},
		{
			'ق',
			new char[4] { 'ﻕ', 'ﻖ', 'ﻗ', 'ﻘ' }
		},
		{
			'ك',
			new char[4] { 'ﻙ', 'ﻚ', 'ﻛ', 'ﻜ' }
		},
		{
			'ل',
			new char[4] { 'ﻝ', 'ﻞ', 'ﻟ', 'ﻠ' }
		},
		{
			'م',
			new char[4] { 'ﻡ', 'ﻢ', 'ﻣ', 'ﻤ' }
		},
		{
			'ن',
			new char[4] { 'ﻥ', 'ﻦ', 'ﻧ', 'ﻨ' }
		},
		{
			'ه',
			new char[4] { 'ﻩ', 'ﻪ', 'ﻫ', 'ﻬ' }
		},
		{
			'و',
			new char[4] { 'ﻭ', 'ﻮ', '\0', '\0' }
		},
		{
			'ى',
			new char[4] { 'ﻯ', 'ﻰ', '\0', '\0' }
		},
		{
			'ي',
			new char[4] { 'ﻱ', 'ﻲ', 'ﻳ', 'ﻴ' }
		}
	};

	private static readonly Dictionary<char, char[]> LamAlef = new Dictionary<char, char[]>
	{
		{
			'آ',
			new char[2] { 'ﻵ', 'ﻶ' }
		},
		{
			'أ',
			new char[2] { 'ﻷ', 'ﻸ' }
		},
		{
			'إ',
			new char[2] { 'ﻹ', 'ﻺ' }
		},
		{
			'ا',
			new char[2] { 'ﻻ', 'ﻼ' }
		}
	};

	public static string ShapeForRender(string text, bool keepTashkeel = true)
	{
		string text2 = Shape(text);
		if (!keepTashkeel)
		{
			text2 = RemoveTashkeel(text2);
		}
		return FixDirectionalRuns(text2);
	}

	public static string RemoveTashkeel(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			if (IsTashkeel(text[i]))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		for (int j = 0; j < text.Length; j++)
		{
			if (!IsTashkeel(text[j]))
			{
				stringBuilder.Append(text[j]);
			}
		}
		return stringBuilder.ToString();
	}

	private static bool IsTashkeel(char c)
	{
		if (c < '\u064b' || c > '\u0652')
		{
			return c == '\u0670';
		}
		return true;
	}

	public static string Shape(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		bool flag = false;
		foreach (char c in text)
		{
			if (c >= 'ء' && c <= 'ي')
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		for (int j = 0; j < text.Length; j++)
		{
			char c2 = text[j];
			if (!Forms.TryGetValue(c2, out var value))
			{
				stringBuilder.Append(c2);
				continue;
			}
			bool flag2 = JoinsToFollowing(PrevSolid(text, j));
			if (c2 == 'ل')
			{
				int solidIndex;
				char key = NextSolid(text, j, out solidIndex);
				if (LamAlef.TryGetValue(key, out var value2))
				{
					stringBuilder.Append(flag2 ? value2[1] : value2[0]);
					for (int k = j + 1; k < solidIndex; k++)
					{
						stringBuilder.Append(text[k]);
					}
					j = solidIndex;
					continue;
				}
			}
			int solidIndex2;
			bool flag3 = value[3] != 0 && AcceptsJoinFromPreceding(NextSolid(text, j, out solidIndex2));
			char value3 = ((flag2 && flag3) ? value[3] : (flag2 ? ((value[1] != 0) ? value[1] : value[0]) : ((!flag3) ? value[0] : value[2])));
			stringBuilder.Append(value3);
		}
		return stringBuilder.ToString();
	}

	public static string FixDirectionalRuns(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		List<(int, int, char)> list = new List<(int, int, char)>(text.Length);
		int num = 0;
		while (num < text.Length)
		{
			char c = text[num];
			if (c == '<')
			{
				int num2 = FindTagEnd(text, num);
				if (num2 > 0)
				{
					list.Add((num, num2 - num + 1, 'N'));
					num = num2 + 1;
					continue;
				}
			}
			char item = ((c != '\n' && c != '\r') ? (((c < '٠' || c > '٩') && (c < '۰' || c > '۹')) ? (IsStrongRtl(c) ? 'R' : ((!char.IsLetter(c) && !char.IsDigit(c)) ? 'N' : 'L')) : 'L') : 'B');
			list.Add((num, 1, item));
			num++;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		int num3 = 0;
		while (num3 < list.Count)
		{
			int i;
			for (i = num3; i < list.Count && list[i].Item3 != 'B'; i++)
			{
			}
			AppendFixedLine(stringBuilder, text, list, num3, i);
			if (i >= list.Count)
			{
				break;
			}
			stringBuilder.Append(text, list[i].Item1, list[i].Item2);
			num3 = i + 1;
		}
		return stringBuilder.ToString();
	}

	private static void AppendFixedLine(StringBuilder sb, string text, List<(int start, int length, char cls)> units, int lineStart, int lineEnd)
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = lineStart; i < lineEnd; i++)
		{
			switch (units[i].cls)
			{
			case 'L':
				flag = true;
				break;
			case 'R':
				flag2 = true;
				break;
			}
		}
		if (flag && !flag2)
		{
			for (int num = lineEnd - 1; num >= lineStart; num--)
			{
				sb.Append(text, units[num].start, units[num].length);
			}
			return;
		}
		List<(int, int)> list = new List<(int, int)>();
		int num2 = lineStart;
		while (num2 < lineEnd)
		{
			if (units[num2].cls != 'L')
			{
				num2++;
				continue;
			}
			int num3 = num2;
			int j;
			for (j = num2 + 1; j < lineEnd && units[j].cls != 'R'; j++)
			{
				if (units[j].cls == 'L')
				{
					num3 = j;
				}
			}
			int num4 = num2;
			if (num4 > lineStart && units[num4 - 1].length == 1 && "+-#$".IndexOf(text[units[num4 - 1].start]) >= 0)
			{
				num4--;
			}
			if (num3 + 1 < lineEnd && units[num3 + 1].length == 1 && "%".IndexOf(text[units[num3 + 1].start]) >= 0)
			{
				num3++;
			}
			list.Add((num4, num3));
			num2 = j;
		}
		int num5 = 0;
		int num6 = lineStart;
		while (num6 < lineEnd)
		{
			if (num5 < list.Count && list[num5].Item1 == num6)
			{
				for (int num7 = list[num5].Item2; num7 >= list[num5].Item1; num7--)
				{
					sb.Append(text, units[num7].start, units[num7].length);
				}
				num6 = list[num5].Item2 + 1;
				num5++;
			}
			else
			{
				var (num8, num9, _) = units[num6];
				if (num9 == 1)
				{
					sb.Append(MirrorBracket(text[num8]));
				}
				else
				{
					sb.Append(text, num8, num9);
				}
				num6++;
			}
		}
	}

	private static int FindTagEnd(string text, int open)
	{
		if (open + 1 >= text.Length)
		{
			return -1;
		}
		char c = text[open + 1];
		if (!char.IsLetter(c) && c != '/' && c != '#')
		{
			return -1;
		}
		int num = Math.Min(text.Length, open + 130);
		for (int i = open + 1; i < num; i++)
		{
			if (text[i] == '>')
			{
				return i;
			}
			if (text[i] == '<')
			{
				return -1;
			}
		}
		return -1;
	}

	private static bool IsStrongRtl(char c)
	{
		if ((c < '\u0590' || c > '\u08ff') && (c < 'יִ' || c > '﷿'))
		{
			if (c >= 'ﹰ')
			{
				return c <= '\ufeff';
			}
			return false;
		}
		return true;
	}

	private static char MirrorBracket(char c)
	{
		return c switch
		{
			'(' => ')', 
			')' => '(', 
			'[' => ']', 
			']' => '[', 
			'{' => '}', 
			'}' => '{', 
			_ => c, 
		};
	}

	private static bool IsTransparent(char c)
	{
		if ((c < '\u064b' || c > '\u065f') && c != '\u0670')
		{
			if (c >= '\u0610')
			{
				return c <= '\u061a';
			}
			return false;
		}
		return true;
	}

	private static char PrevSolid(string text, int index)
	{
		for (int num = index - 1; num >= 0; num--)
		{
			if (!IsTransparent(text[num]))
			{
				return text[num];
			}
		}
		return '\0';
	}

	private static char NextSolid(string text, int index, out int solidIndex)
	{
		for (int i = index + 1; i < text.Length; i++)
		{
			if (!IsTransparent(text[i]))
			{
				solidIndex = i;
				return text[i];
			}
		}
		solidIndex = text.Length;
		return '\0';
	}

	private static bool JoinsToFollowing(char c)
	{
		if (c != 'ـ')
		{
			if (Forms.TryGetValue(c, out var value))
			{
				return value[3] != '\0';
			}
			return false;
		}
		return true;
	}

	private static bool AcceptsJoinFromPreceding(char c)
	{
		if (c != 'ـ')
		{
			if (Forms.TryGetValue(c, out var value))
			{
				return value[1] != '\0';
			}
			return false;
		}
		return true;
	}
}
