using System;

[Serializable]
public class JobAtomDefinition : BalanceBaseObject
{
	public string action;

	public string target;

	public string location;

	public string job;

	public int time;

	public int anim;

	public BearCode code = new BearCode();

	public BearLogicExpression condition = new BearLogicExpression();
}
