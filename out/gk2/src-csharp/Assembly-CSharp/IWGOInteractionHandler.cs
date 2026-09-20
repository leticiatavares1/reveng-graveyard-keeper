public interface IWGOInteractionHandler : IInteractionHandler
{
	IWGOInteractionHandler Init(Wgo wgo);

	void OnInteractionTargetEnter(PlayerController interactor);

	bool Interact(PlayerController interactor);

	bool HasInteraction(PlayerController interactor);

	bool Interact2(PlayerController interactor);

	bool HasInteraction2(PlayerController interactor);

	ItemType GetRequiredInteractionToolType();
}
