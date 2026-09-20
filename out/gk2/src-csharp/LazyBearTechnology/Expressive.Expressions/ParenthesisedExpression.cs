using System.Collections.Generic;
using Expressive.Exceptions;

namespace Expressive.Expressions;

internal class ParenthesisedExpression : IExpression
{
	private readonly IExpression innerExpression;

	internal ParenthesisedExpression(IExpression innerExpression)
	{
		this.innerExpression = innerExpression;
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		if (innerExpression == null)
		{
			throw new MissingParticipantException("Missing contents inside ().");
		}
		return innerExpression.Evaluate(variables);
	}
}
