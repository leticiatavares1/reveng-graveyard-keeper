using System.Collections.Generic;
using Expressive.Expressions;

namespace Expressive.Operators;

public interface IOperator
{
	IEnumerable<string> Tags { get; }

	IExpression BuildExpression(Token previousToken, IExpression[] expressions, Context context);

	bool CanGetCaptiveTokens(Token previousToken, Token token, Queue<Token> remainingTokens);

	Token[] GetCaptiveTokens(Token previousToken, Token token, Queue<Token> remainingTokens);

	Token[] GetInnerCaptiveTokens(Token[] allCaptiveTokens);

	OperatorPrecedence GetPrecedence(Token previousToken);
}
