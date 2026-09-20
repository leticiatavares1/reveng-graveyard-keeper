using System;
using Expressive.Expressions;

namespace Expressive.Functions.String;

internal class PadRightFunction : FunctionBase
{
	public override string Name => "PadRight";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, 3, 3);
		object obj = parameters[0].Evaluate(base.Variables);
		object obj2 = parameters[1].Evaluate(base.Variables);
		if (obj == null || obj2 == null)
		{
			return null;
		}
		string text = null;
		text = ((!(obj is string)) ? obj.ToString() : ((string)obj));
		int totalWidth = Convert.ToInt32(obj2);
		object obj3 = parameters[2].Evaluate(base.Variables);
		char paddingChar = ' ';
		if (obj3 is char)
		{
			paddingChar = (char)obj3;
		}
		else if (obj3 is string)
		{
			paddingChar = ((string)obj3)[0];
		}
		return text.PadRight(totalWidth, paddingChar);
	}
}
