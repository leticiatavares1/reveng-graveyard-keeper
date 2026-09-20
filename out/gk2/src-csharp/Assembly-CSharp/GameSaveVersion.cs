using System;
using System.Globalization;

public readonly struct GameSaveVersion : IComparable<GameSaveVersion>, IEquatable<GameSaveVersion>
{
	public float Number { get; }

	public string Postfix { get; }

	public int NumberAsInt { get; }

	public bool HasPostfix => !string.IsNullOrEmpty(Postfix);

	public float ComparisonNumber => ToComparisonNumber(NumberAsInt, Postfix);

	public GameSaveVersion(float number, string postfix = null)
	{
		Number = number;
		Postfix = postfix ?? string.Empty;
		NumberAsInt = ToNumberAsInt(number);
	}

	private GameSaveVersion(float number, string postfix, int numberAsInt)
	{
		Number = number;
		Postfix = postfix ?? string.Empty;
		NumberAsInt = numberAsInt;
	}

	public static GameSaveVersion Parse(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			throw new ArgumentException("Version string is null or empty.", "version");
		}
		version = version.Trim();
		int num = 0;
		bool flag = false;
		while (num < version.Length)
		{
			char c = version[num];
			if (char.IsDigit(c))
			{
				num++;
				continue;
			}
			if (c != '.' || flag)
			{
				break;
			}
			flag = true;
			num++;
		}
		if (num == 0)
		{
			throw new ArgumentException("Can't parse version: " + version, "version");
		}
		string text = version.Substring(0, num);
		string postfix = ((num < version.Length) ? version.Substring(num) : string.Empty);
		if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			throw new ArgumentException("Can't parse version number: " + text, "version");
		}
		string text2 = text.Replace(".", string.Empty);
		if (!int.TryParse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2))
		{
			throw new ArgumentException("Can't parse version as int: " + text2, "version");
		}
		return new GameSaveVersion(result, postfix, result2);
	}

	public static bool TryParse(string version, out GameSaveVersion result)
	{
		try
		{
			result = Parse(version);
			return true;
		}
		catch (ArgumentException)
		{
			result = default(GameSaveVersion);
			return false;
		}
	}

	public static GameSaveVersion FromInt(int version)
	{
		string text = version.ToString(CultureInfo.InvariantCulture);
		return new GameSaveVersion(float.Parse((text.Length == 1) ? text : $"{text[0]}.{text.Substring(1)}", CultureInfo.InvariantCulture), string.Empty, version);
	}

	public int CompareTo(GameSaveVersion other)
	{
		int num = NumberAsInt.CompareTo(other.NumberAsInt);
		if (num != 0)
		{
			return num;
		}
		return string.CompareOrdinal(Postfix, other.Postfix);
	}

	public int CompareTo(float value)
	{
		SplitComparisonNumber(value, out var numberAsInt, out var postfix);
		int num = NumberAsInt.CompareTo(numberAsInt);
		if (num != 0)
		{
			return num;
		}
		if (string.IsNullOrEmpty(postfix))
		{
			return 0;
		}
		return string.CompareOrdinal(Postfix, postfix);
	}

	public bool Equals(GameSaveVersion other)
	{
		if (NumberAsInt == other.NumberAsInt)
		{
			return string.Equals(Postfix, other.Postfix, StringComparison.Ordinal);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GameSaveVersion other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (NumberAsInt * 397) ^ (Postfix?.GetHashCode() ?? 0);
	}

	public override string ToString()
	{
		string text = Number.ToString(CultureInfo.InvariantCulture);
		if (!HasPostfix)
		{
			return text;
		}
		return text + Postfix;
	}

	public static bool operator ==(GameSaveVersion left, GameSaveVersion right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GameSaveVersion left, GameSaveVersion right)
	{
		return !left.Equals(right);
	}

	public static bool operator <(GameSaveVersion left, GameSaveVersion right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(GameSaveVersion left, GameSaveVersion right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator <=(GameSaveVersion left, GameSaveVersion right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >=(GameSaveVersion left, GameSaveVersion right)
	{
		return left.CompareTo(right) >= 0;
	}

	public static bool operator ==(GameSaveVersion left, int right)
	{
		return left.NumberAsInt == right;
	}

	public static bool operator !=(GameSaveVersion left, int right)
	{
		return left.NumberAsInt != right;
	}

	public static bool operator <(GameSaveVersion left, int right)
	{
		return left.NumberAsInt < right;
	}

	public static bool operator >(GameSaveVersion left, int right)
	{
		return left.NumberAsInt > right;
	}

	public static bool operator <=(GameSaveVersion left, int right)
	{
		return left.NumberAsInt <= right;
	}

	public static bool operator >=(GameSaveVersion left, int right)
	{
		return left.NumberAsInt >= right;
	}

	public static bool operator ==(int left, GameSaveVersion right)
	{
		return left == right.NumberAsInt;
	}

	public static bool operator !=(int left, GameSaveVersion right)
	{
		return left != right.NumberAsInt;
	}

	public static bool operator <(int left, GameSaveVersion right)
	{
		return left < right.NumberAsInt;
	}

	public static bool operator >(int left, GameSaveVersion right)
	{
		return left > right.NumberAsInt;
	}

	public static bool operator <=(int left, GameSaveVersion right)
	{
		return left <= right.NumberAsInt;
	}

	public static bool operator >=(int left, GameSaveVersion right)
	{
		return left >= right.NumberAsInt;
	}

	public static bool operator ==(GameSaveVersion left, float right)
	{
		return left.CompareTo(right) == 0;
	}

	public static bool operator !=(GameSaveVersion left, float right)
	{
		return left.CompareTo(right) != 0;
	}

	public static bool operator <(GameSaveVersion left, float right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(GameSaveVersion left, float right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator <=(GameSaveVersion left, float right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >=(GameSaveVersion left, float right)
	{
		return left.CompareTo(right) >= 0;
	}

	public static bool operator ==(float left, GameSaveVersion right)
	{
		return right.CompareTo(left) == 0;
	}

	public static bool operator !=(float left, GameSaveVersion right)
	{
		return right.CompareTo(left) != 0;
	}

	public static bool operator <(float left, GameSaveVersion right)
	{
		return right.CompareTo(left) > 0;
	}

	public static bool operator >(float left, GameSaveVersion right)
	{
		return right.CompareTo(left) < 0;
	}

	public static bool operator <=(float left, GameSaveVersion right)
	{
		return right.CompareTo(left) >= 0;
	}

	public static bool operator >=(float left, GameSaveVersion right)
	{
		return right.CompareTo(left) <= 0;
	}

	private static int ToNumberAsInt(float number)
	{
		return int.Parse(number.ToString(CultureInfo.InvariantCulture).Replace(".", string.Empty), CultureInfo.InvariantCulture);
	}

	private static float ToComparisonNumber(int numberAsInt, string postfix)
	{
		if (string.IsNullOrEmpty(postfix) || postfix[0] != '.')
		{
			return numberAsInt;
		}
		if (float.TryParse("0" + postfix, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return (float)numberAsInt + result;
		}
		return numberAsInt;
	}

	private static void SplitComparisonNumber(float value, out int numberAsInt, out string postfix)
	{
		decimal num = decimal.Round((decimal)value, 4, MidpointRounding.AwayFromZero);
		numberAsInt = (int)num;
		decimal num2 = num - (decimal)numberAsInt;
		if (num2 == 0m)
		{
			postfix = string.Empty;
			return;
		}
		string text = num2.ToString(CultureInfo.InvariantCulture);
		postfix = (text.StartsWith("0.", StringComparison.Ordinal) ? text.Substring(1).TrimEnd('0') : ("." + text.TrimEnd('0')));
		if (postfix == ".")
		{
			postfix = string.Empty;
		}
	}
}
