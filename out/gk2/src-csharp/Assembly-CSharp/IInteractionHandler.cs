public interface IInteractionHandler
{
	void OnInteractionTargetEnter();

	void OnInteractionTargetExit();

	InteractionInfos GetInteractionInfos();

	bool Interact();

	bool HasInteraction();

	bool Interact2();

	bool HasInteraction2();
}
