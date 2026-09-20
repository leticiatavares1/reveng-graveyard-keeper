using LazyBearTechnology;

public class EmbalmInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.CraftComponent.IsStarted)
		{
			return false;
		}
		if (HasInsertableOverheadBodyItem())
		{
			InsertOverheadItem();
			return true;
		}
		LinkZombieItem();
		bool wasPlayerSetAsWorker = false;
		if (assignedWgo.Data.Worker == null)
		{
			assignedWgo.Data.TrySetWorker(interactor);
			wasPlayerSetAsWorker = true;
		}
		UIEmbalmWindow window = LazyUI.GetWindow<UIEmbalmWindow>();
		UIEmbalmWindowData data = new UIEmbalmWindowData(assignedWgo.Data);
		window.Open(data, delegate
		{
			if (wasPlayerSetAsWorker)
			{
				assignedWgo.Data.ClearWorker();
			}
		});
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (assignedWgo.Data.CraftComponent.IsStarted)
		{
			return false;
		}
		base.HasInteraction(interactor);
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		if (assignedWgo.Data.CraftComponent.IsStarted)
		{
			return new InteractionInfos(GetInteractionInfoByUsingTool(isForCurrentCraft: true));
		}
		if (HasInsertableOverheadBodyItem())
		{
			return new InteractionInfos(new InteractionInfo(LocalizeHintWithActionIcon("hint_place_body", GameKey.Interaction)));
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("action_inspect", GameKey.Interaction)));
	}

	private bool HasInsertableOverheadBodyItem()
	{
		if (HasBodyItemInside())
		{
			return false;
		}
		Item item2;
		return MainGame.Instance.GameSave.playerData.TryGetOverheadItem((Item item) => !item.HasItemsByItemType(ItemType.Demon) && item.Definition.itemGroupIds.Contains("body"), out item2);
	}

	private bool HasBodyItemInside()
	{
		foreach (Item item in assignedWgo.Data.Inventory.Data.Inventory)
		{
			if (item.Definition.itemGroupIds.Contains("body"))
			{
				return true;
			}
		}
		return false;
	}

	private void InsertOverheadItem()
	{
		if (MainGame.Instance.GameSave.playerData.TryGetOverheadItem((Item item) => !item.HasItemsByItemType(ItemType.Demon) && item.Definition.itemGroupIds.Contains("body"), out var item2))
		{
			assignedWgo.Data.Inventory.AddItemToInventory(item2);
			MainGame.Instance.GameSave.playerData.RemoveOverheadItem(item2);
			assignedWgo.DrawWidgets();
		}
	}

	private void LinkZombieItem()
	{
		foreach (Item item in assignedWgo.Data.Inventory.Data.Inventory)
		{
			if (item.Definition.itemGroupIds.Contains("body") && item.Definition.itemGroupIds.Contains("zombie"))
			{
				MainGame.ZombieSystemData.GetZombie(item.UniqueId).SetZombieItem(item);
				break;
			}
		}
	}
}
