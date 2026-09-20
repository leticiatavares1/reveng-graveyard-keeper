using System.Globalization;

namespace Expressive.Tokenisation;

internal class NumericTokenExtractor : ITokenExtractor
{
	private class Location
	{
		public int End { get; }

		public int Length => End - Start;

		public int Start { get; }

		public Location(int start, int end)
		{
			Start = start;
			End = end;
		}
	}

	public Token ExtractToken(string expression, int currentIndex, Context context)
	{
		if (!IsValidStart(expression[currentIndex], context, NumberStyles.Any))
		{
			return null;
		}
		Location numberLocation = GetNumberLocation(expression, currentIndex, context, NumberStyles.Any);
		return new Token(expression.Substring(numberLocation.Start, numberLocation.Length), currentIndex);
	}

	private static Location GetNumberLocation(string expression, int startIndex, Context context, NumberStyles numberStyles)
	{
		int num = startIndex;
		char character = expression[num];
		int length = expression.Length;
		Location exponentialLocation;
		while (IsAllowableCharacter(character, expression, num, startIndex, length, context, ref numberStyles, out exponentialLocation) && num < length)
		{
			if (exponentialLocation != null)
			{
				return new Location(startIndex, exponentialLocation.End);
			}
			num++;
			if (num == expression.Length)
			{
				break;
			}
			character = expression[num];
		}
		return new Location(startIndex, num);
	}

	private static bool IsAllowableCharacter(char character, string expression, int index, int startIndex, int expressionLength, Context context, ref NumberStyles numberStyles, out Location exponentialLocation)
	{
		exponentialLocation = null;
		if (char.IsDigit(character) || (IsValidStart(character, context, numberStyles) && index == startIndex))
		{
			return true;
		}
		if (!numberStyles.HasFlag(NumberStyles.AllowExponent))
		{
			return false;
		}
		if ((character == 'e' || character == 'E') && char.IsDigit(expression[index - 1]) && index + 1 < expressionLength && IsValidStart(expression[index + 1], context, NumberStyles.Integer))
		{
			exponentialLocation = GetNumberLocation(expression, index + 1, context, NumberStyles.Integer);
			if (exponentialLocation != null)
			{
				return true;
			}
		}
		if (numberStyles.HasFlag(NumberStyles.AllowDecimalPoint) && character == context.DecimalSeparator)
		{
			numberStyles &= ~NumberStyles.AllowDecimalPoint;
			return true;
		}
		return false;
	}

	private static bool IsSignCharacter(char character)
	{
		if (character != '-' && character != '−')
		{
			return character == '+';
		}
		return true;
	}

	private static bool IsValidStart(char character, Context context, NumberStyles numberStyles)
	{
		if (!char.IsDigit(character) && (!numberStyles.HasFlag(NumberStyles.AllowLeadingSign) || !IsSignCharacter(character)))
		{
			if (numberStyles.HasFlag(NumberStyles.AllowDecimalPoint))
			{
				return character == context.DecimalSeparator;
			}
			return false;
		}
		return true;
	}
}
