using System;

[Serializable]
public class HPAction : IAutoParsable
{
	public bool isValidAction;

	public int hp;

	public LazyExpression expression;

	public void EvaluateExpression(WgoData data)
	{
		expression.Evaluate(data);
	}
}
