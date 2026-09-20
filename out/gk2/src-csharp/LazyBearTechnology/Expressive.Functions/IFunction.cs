using System.Collections.Generic;
using Expressive.Expressions;

namespace Expressive.Functions;

public interface IFunction
{
	IDictionary<string, object> Variables { get; set; }

	string Name { get; }

	object Evaluate(IExpression[] parameters, Context context);
}
