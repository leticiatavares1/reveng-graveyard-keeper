using System;
using System.Collections.Generic;
using Expressive.Expressions;
using LinqTools;

namespace Expressive.Operators.Grouping;

internal class ParenthesisOpenOperator : IOperator
{
	public string[] Tags => new string[1] { "(" };

	public IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		return new ParenthesisedExpression(expressions[0] ?? expressions[1]);
	}

	public bool CanGetCaptiveTokens(Token previousToken, Token token, Queue<Token> remainingTokens)
	{
		Queue<Token> remainingTokens2 = new Queue<Token>(remainingTokens.ToArray());
		return GetCaptiveTokens(previousToken, token, remainingTokens2).Any();
	}

	public Token[] GetCaptiveTokens(Token previousToken, Token token, Queue<Token> remainingTokens)
	{
		IList<Token> list = new List<Token>();
		list.Add(token);
		int num = 1;
		while (remainingTokens.Any())
		{
			Token token2 = remainingTokens.Dequeue();
			list.Add(token2);
			if (string.Equals(token2.CurrentToken, "(", StringComparison.Ordinal))
			{
				num++;
			}
			else if (string.Equals(token2.CurrentToken, ")", StringComparison.Ordinal))
			{
				num--;
			}
			if (num <= 0)
			{
				break;
			}
		}
		return list.ToArray();
	}

	public Token[] GetInnerCaptiveTokens(Token[] allCaptiveTokens)
	{
		return allCaptiveTokens.Skip(1).Take(allCaptiveTokens.Length - 2).ToArray();
	}

	public OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.ParenthesisOpen;
	}
}
