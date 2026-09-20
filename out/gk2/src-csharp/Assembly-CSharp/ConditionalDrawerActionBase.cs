using System;

[Serializable]
public abstract class ConditionalDrawerActionBase : IConditionalDrawerAction
{
	public abstract void Execute(ConditionalDrawerContext context, bool conditionMet);

	public virtual void Reset(ConditionalDrawerContext context)
	{
	}
}
