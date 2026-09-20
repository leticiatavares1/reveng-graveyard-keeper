using System;
using System.Linq;

namespace Expressive.Tokenisation;

internal class ValueTokenExtractor : ITokenExtractor
{
	private readonly string value;

	public ValueTokenExtractor(string value)
	{
		this.value = value ?? throw new ArgumentNullException("value");
	}

	public Token ExtractToken(string expression, int currentIndex, Context context)
	{
		char c = expression[currentIndex];
		int length = expression.Length;
		char c2 = value.First();
		if (string.Equals(c.ToString(), c2.ToString(), context.ParsingStringComparison) && CanExtractValue(expression, length, currentIndex, value, context))
		{
			return new Token(value, currentIndex);
		}
		return null;
	}

	private static bool CanExtractValue(string expression, int expressionLength, int index, string expectedValue, Context context)
	{
		return string.Equals(expectedValue, ExtractValue(expression, expressionLength, index, expectedValue, context), context.ParsingStringComparison);
	}

	private static string ExtractValue(string expression, int expressionLength, int index, string expectedValue, Context context)
	{
		string result = null;
		int length = expectedValue.Length;
		if (expressionLength >= index + length)
		{
			string text = expression.Substring(index, length);
			bool flag = true;
			if (expressionLength > index + length)
			{
				flag = !char.IsLetterOrDigit(expression[index + length]) || string.Equals(expectedValue, ','.ToString(), context.ParsingStringComparison);
			}
			if (string.Equals(text, expectedValue, context.ParsingStringComparison) && flag)
			{
				result = text;
			}
		}
		return result;
	}
}
