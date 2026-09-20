using LazyBearTechnology;

public class ZombieDropInteractionHandler : BigDropInteractionHandler
{
	public override InteractionInfos GetInteractionInfos()
	{
		InteractionInfos interactionInfos = new InteractionInfos();
		if (HasInteraction())
		{
			interactionInfos.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
		}
		if (HasInteraction2())
		{
			interactionInfos.Add(new InteractionInfo(LocalizeHintWithActionIcon("action_inspect", GameKey.Action)));
		}
		return interactionInfos;
	}

	public override bool HasInteraction()
	{
		return base.HasInteraction();
	}

	public override bool Interact()
	{
		LinkItemToZombieData();
		return base.Interact();
	}

	public override bool HasInteraction2()
	{
		return TryGetZombie() != null;
	}

	public override bool Interact2()
	{
		if (TryGetZombie() == null)
		{
			return false;
		}
		LinkItemToZombieData();
		UIZombieWorkerWindowData data = new UIZombieWorkerWindowData(drop.Data);
		LazyUI.GetWindow<UIZombieWorkerWindow>().Open(data);
		return true;
	}

	public override void OnInteractionTargetEnter()
	{
		UIObjectBubbleManager.Instance.Display(drop);
	}

	public override void OnInteractionTargetExit()
	{
		UIObjectBubbleManager.Instance.Hide(drop);
	}

	private ZombieWgoData TryGetZombie()
	{
		return MainGame.ZombieSystemData.GetZombie(drop.Data.Item.UniqueId);
	}

	private void LinkItemToZombieData()
	{
		TryGetZombie()?.SetZombieItem(drop.Data.Item);
	}
}
