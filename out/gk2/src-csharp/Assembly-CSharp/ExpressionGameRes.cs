using System;

[Serializable]
public class ExpressionGameRes : IAutoParsable
{
	public string name;

	public LazyExpression expression;

	public override string ToString()
	{
		return "[ExpressionGameRes: " + name + " " + expression.ToString() + "]";
	}
}
