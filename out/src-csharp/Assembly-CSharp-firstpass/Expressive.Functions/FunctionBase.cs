using System.Collections.Generic;
using Expressive.Exceptions;
using Expressive.Expressions;
using LinqTools;

namespace Expressive.Functions;

internal abstract class FunctionBase : IFunction
{
	public IDictionary<string, object> Variables { get; set; }

	public abstract string Name { get; }

	public abstract object Evaluate(IExpression[] parameters);

	protected bool ValidateParameterCount(IExpression[] parameters, int expectedCount, int minimumCount)
	{
		if (expectedCount != -1 && (parameters == null || !parameters.Any() || parameters.Length != expectedCount))
		{
			throw new ParameterCountMismatchException(Name + "() takes only " + expectedCount + " argument(s)");
		}
		if (minimumCount > 0 && (parameters == null || !parameters.Any() || parameters.Length < minimumCount))
		{
			throw new ParameterCountMismatchException(Name + "() expects at least " + minimumCount + " argument(s)");
		}
		return true;
	}
}
