using System.Collections.Generic;
using Expressive.Exceptions;

namespace Expressive.Expressions;

internal class ParenthesisedExpression : IExpression
{
	private readonly IExpression _innerExpression;

	internal ParenthesisedExpression(IExpression innerExpression)
	{
		_innerExpression = innerExpression;
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		if (_innerExpression == null)
		{
			throw new MissingParticipantException("Missing contents inside ().");
		}
		return _innerExpression.Evaluate(variables);
	}
}
