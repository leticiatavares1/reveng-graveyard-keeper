using System;
using System.Collections.Generic;
using Expressive.Expressions;
using LinqTools;

namespace Expressive.Operators.Additive;

internal class SubtractOperator : OperatorBase
{
	public override string[] Tags => new string[2] { "-", "−" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		if (IsUnary(previousToken))
		{
			return new UnaryExpression(UnaryExpressionType.Minus, expressions[0] ?? expressions[1]);
		}
		return new BinaryExpression(BinaryExpressionType.Subtract, expressions[0], expressions[1]);
	}

	public override bool CanGetCaptiveTokens(Token previousToken, Token token, Queue<Token> remainingTokens)
	{
		Queue<Token> remainingTokens2 = new Queue<Token>(remainingTokens.ToArray());
		return GetCaptiveTokens(previousToken, token, remainingTokens2).Any();
	}

	public override Token[] GetInnerCaptiveTokens(Token[] allCaptiveTokens)
	{
		return allCaptiveTokens.Skip(1).ToArray();
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		if (IsUnary(previousToken))
		{
			return OperatorPrecedence.UnaryPlus;
		}
		return OperatorPrecedence.Add;
	}

	private bool IsUnary(Token previousToken)
	{
		if (!string.IsNullOrEmpty(previousToken?.CurrentToken) && !string.Equals(previousToken.CurrentToken, "(", StringComparison.Ordinal))
		{
			return previousToken.CurrentToken.IsArithmeticOperator();
		}
		return true;
	}
}
