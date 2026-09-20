using System;

[Serializable]
public class ExpressionRes
{
	public string name;

	public SmartExpression expression;

	public override string ToString()
	{
		return "[ExpressionGameRes: " + name + " " + expression.ToString() + "]";
	}
}
