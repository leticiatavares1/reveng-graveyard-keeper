using LazyBearTechnology;

public class BigDropInteractionHandler : IInteractionHandler
{
	protected DropView drop;

	public BigDropInteractionHandler Init(DropView drop)
	{
		this.drop = drop;
		return this;
	}

	public virtual void OnInteractionTargetEnter()
	{
		UIObjectBubbleManager.Instance.PutOnTopTargetId = drop.Data.UniqueId;
	}

	public virtual void OnInteractionTargetExit()
	{
		UIObjectBubbleManager.Instance.PutOnTopTargetId = SGuid.Empty;
	}

	public virtual InteractionInfos GetInteractionInfos()
	{
		return new InteractionInfos(new InteractionInfo(string.Empty));
	}

	public virtual bool Interact()
	{
		MainGame.Instance.dropSystem.RemoveDrop(drop.Data, drop.Data.WorldId);
		MainGame.PlayerData.AddOverheadItem(drop.Data.Item);
		return true;
	}

	public virtual bool HasInteraction()
	{
		return true;
	}

	public virtual bool Interact2()
	{
		return false;
	}

	public virtual bool HasInteraction2()
	{
		return false;
	}

	protected string LocalizeHintWithActionIcon(string hintId, GameKey gameKey)
	{
		return ControllerIconLibrary.GetIconId(gameKey) + LLBase.L(hintId);
	}
}
