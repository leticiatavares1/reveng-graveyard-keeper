public class DisabledDropInteractionHandler : IInteractionHandler
{
	public void OnInteractionTargetEnter()
	{
	}

	public void OnInteractionTargetExit()
	{
	}

	public InteractionInfos GetInteractionInfos()
	{
		return new InteractionInfos();
	}

	public bool Interact()
	{
		return false;
	}

	public bool HasInteraction()
	{
		return false;
	}

	public bool Interact2()
	{
		return false;
	}

	public bool HasInteraction2()
	{
		return false;
	}
}
