using System;
using System.Collections.Generic;
using System.Linq;
using Expressive.Exceptions;
using Expressive.Expressions;

namespace Expressive.Functions;

public abstract class FunctionBase : IFunction
{
	public IDictionary<string, object> Variables { get; set; }

	public abstract string Name { get; }

	public abstract object Evaluate(IExpression[] parameters, Context context);

	protected void ValidateParameterCount(IExpression[] parameters, int expectedCount, int minimumCount)
	{
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		if (expectedCount == 0 && (parameters.Any() || parameters.Length != expectedCount))
		{
			throw new ParameterCountMismatchException(Name + "() does not take any arguments");
		}
		if (expectedCount > 0 && (!parameters.Any() || parameters.Length != expectedCount))
		{
			throw new ParameterCountMismatchException($"{Name}() takes only {expectedCount} argument(s)");
		}
		if (minimumCount > 0 && (!parameters.Any() || parameters.Length < minimumCount))
		{
			throw new ParameterCountMismatchException($"{Name}() expects at least {minimumCount} argument(s)");
		}
	}
}
