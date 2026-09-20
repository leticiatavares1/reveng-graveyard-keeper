public interface IConditionalDrawerAction
{
	void Execute(ConditionalDrawerContext context, bool conditionMet);

	void Reset(ConditionalDrawerContext context);
}
