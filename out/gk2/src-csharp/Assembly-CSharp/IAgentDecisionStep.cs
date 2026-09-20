public interface IAgentDecisionStep<TContext>
{
	MobCommand TryCreateCommand(TContext context);
}
