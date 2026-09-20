using System;
using System.Collections.Generic;

namespace Expressive.Tokenisation;

internal class KeywordTokenExtractor : ITokenExtractor
{
	private readonly IEnumerable<string> keywords;

	public KeywordTokenExtractor(IEnumerable<string> keywords)
	{
		if (keywords == null)
		{
			throw new ArgumentNullException("keywords");
		}
		this.keywords = keywords;
	}

	public Token ExtractToken(string expression, int currentIndex, Context context)
	{
		int length = expression.Length;
		foreach (string keyword in keywords)
		{
			string text = expression.Substring(currentIndex, Math.Min(keyword.Length, length - currentIndex));
			if (string.Equals(text, keyword, context.ParsingStringComparison))
			{
				return new Token(text, currentIndex);
			}
		}
		return null;
	}
}
