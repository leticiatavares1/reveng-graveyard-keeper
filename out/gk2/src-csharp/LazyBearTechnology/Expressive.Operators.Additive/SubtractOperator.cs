using System;
using System.Collections.Generic;
using System.Linq;
using Expressive.Expressions;
using Expressive.Expressions.Binary.Additive;
using Expressive.Expressions.Unary.Additive;

namespace Expressive.Operators.Additive;

internal class SubtractOperator : OperatorBase
{
	public override IEnumerable<string> Tags => new string[2] { "-", "−" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions, Context context)
	{
		if (IsUnary(previousToken))
		{
			return new MinusExpression(expressions[0] ?? expressions[1]);
		}
		return new SubtractExpression(expressions[0], expressions[1], context);
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
		if (!IsUnary(previousToken))
		{
			return OperatorPrecedence.Subtract;
		}
		return OperatorPrecedence.UnaryMinus;
	}

	private static bool IsUnary(Token previousToken)
	{
		if (!string.IsNullOrEmpty(previousToken?.CurrentToken) && !string.Equals(previousToken.CurrentToken, "(", StringComparison.Ordinal))
		{
			return previousToken.CurrentToken.IsArithmeticOperator();
		}
		return true;
	}
}
