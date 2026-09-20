using Expressive.Exceptions;

namespace Expressive.Tokenisation;

internal class ParenthesisedTokenExtractor : ITokenExtractor
{
	private readonly char endingCharacter;

	private readonly char startingCharacter;

	public ParenthesisedTokenExtractor(char singleCharacter)
		: this(singleCharacter, singleCharacter)
	{
	}

	public ParenthesisedTokenExtractor(char startingCharacter, char endingCharacter)
	{
		this.startingCharacter = startingCharacter;
		this.endingCharacter = endingCharacter;
	}

	public Token ExtractToken(string expression, int currentIndex, Context context)
	{
		if (expression[currentIndex] != startingCharacter)
		{
			return null;
		}
		string @string = GetString(expression, currentIndex, endingCharacter);
		if (string.IsNullOrWhiteSpace(@string))
		{
			throw new MissingTokenException($"Missing closing token '{endingCharacter}'", endingCharacter);
		}
		return new Token(@string, currentIndex);
	}

	private static string GetString(string expression, int startIndex, char expectedEndingCharacter)
	{
		int num = startIndex;
		bool flag = false;
		char c = expression[num];
		bool flag2 = false;
		while (num < expression.Length && !flag)
		{
			if (num != startIndex && c == expectedEndingCharacter && !flag2)
			{
				flag = true;
			}
			flag2 = ((c == '\\' && !flag2) ? true : false);
			num++;
			if (num == expression.Length)
			{
				break;
			}
			c = expression[num];
		}
		if (!flag)
		{
			return null;
		}
		return expression.Substring(startIndex, num - startIndex);
	}
}
