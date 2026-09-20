using System.Collections.Generic;
using Expressive.Expressions;
using Expressive.Expressions.Binary.Multiplicative;

namespace Expressive.Operators.Multiplicative;

internal class ModulusOperator : OperatorBase
{
	public override IEnumerable<string> Tags => new string[2] { "%", "mod" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions, Context context)
	{
		return new ModulusExpression(expressions[0], expressions[1], context);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.Modulus;
	}
}
