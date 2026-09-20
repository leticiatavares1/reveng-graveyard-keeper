using System.Collections.Generic;
using System.Linq;

namespace Expressive.Tokenisation;

internal sealed class Tokeniser
{
	private readonly Context context;

	private readonly IEnumerable<ITokenExtractor> tokenExtractors;

	public Tokeniser(Context context, IEnumerable<ITokenExtractor> tokenExtractors)
	{
		this.context = context;
		this.tokenExtractors = tokenExtractors;
	}

	internal IList<Token> Tokenise(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
		{
			return null;
		}
		int length = expression.Length;
		List<Token> list = new List<Token>();
		IList<char> list2 = null;
		int index;
		Token token;
		for (index = 0; index < length; index += token?.Length ?? 1)
		{
			token = tokenExtractors.Select((ITokenExtractor t) => t.ExtractToken(expression, index, context)).FirstOrDefault((Token t) => t != null);
			if (token != null)
			{
				CheckForUnrecognised(list2, list, index);
				list2 = null;
				list.Add(token);
				continue;
			}
			char c = expression[index];
			if (!char.IsWhiteSpace(c))
			{
				if (list2 == null)
				{
					list2 = new List<char>();
				}
				list2.Add(c);
			}
			else
			{
				CheckForUnrecognised(list2, list, index);
				list2 = null;
			}
		}
		CheckForUnrecognised(list2, list, index);
		return list;
	}

	private static void CheckForUnrecognised(IList<char> unrecognised, ICollection<Token> tokens, int index)
	{
		if (unrecognised != null)
		{
			string text = new string(unrecognised.ToArray());
			tokens.Add(new Token(text, index - text.Length));
		}
	}
}
