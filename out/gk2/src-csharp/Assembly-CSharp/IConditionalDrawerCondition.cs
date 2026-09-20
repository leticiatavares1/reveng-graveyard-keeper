public interface IConditionalDrawerCondition
{
	ConditionalEventType EventType { get; }

	bool Evaluate(ConditionalDrawerContext context);

	bool IsValid(ConditionalDrawerContext context);
}
