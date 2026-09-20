using LazyBearTechnology;

public class TownPaletteInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (MainGame.PlayerData.TryGetOverheadItem((Item item) => assignedWgo.Data.Inventory.CanAddItemToInventory(item), out var item2))
		{
			MainGame.PlayerData.InsertOverheadItemTo(assignedWgo.Data, item2);
			assignedWgo.DrawWidgets();
		}
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (MainGame.PlayerData.TryGetOverheadItem((Item item) => assignedWgo.Data.Inventory.CanAddItemToInventory(item), out var _))
		{
			return true;
		}
		return false;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		if (MainGame.PlayerData.TryGetOverheadItem((Item item) => assignedWgo.Data.Inventory.CanAddItemToInventory(item), out var _))
		{
			return new InteractionInfos(new InteractionInfo(LocalizeHintWithActionIcon("hint_put", GameKey.Interaction)));
		}
		return new InteractionInfos(new InteractionInfo(""));
	}
}
