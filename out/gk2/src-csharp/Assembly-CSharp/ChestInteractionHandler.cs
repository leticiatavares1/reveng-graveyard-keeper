using LazyBearTechnology;

public class ChestInteractionHandler : WGOInteractionHandlerBase
{
	private const string GARDEN_BAGS_STORAGE1_ID = "garden_bags_storage_1";

	private const string GARDEN_BAGS_STORAGE2_ID = "garden_bags_storage_2";

	private const string GARDEN_BAGS_STORAGE3_ID = "garden_bags_storage_3";

	private const string WINE_STORE_ID = "conveyor_wine_beer_pallet";

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		WorldZoneData currentWorldZoneData = interactor.PlayerData.CurrentWorldZoneData;
		UIBaseChestWindowData data = new UIBaseChestWindowData(interactor.PlayerData.inventory, (currentWorldZoneData == null) ? null : new MultiInventory(currentWorldZoneData, assignedWgo.Data), assignedWgo.Data);
		if (IsGardenBagsStorage(assignedWgo.Data.id))
		{
			LazyUI.GetWindow<UIConveyorVegetablesChestWindow>().Open(data);
		}
		else if (IsWineStorage(assignedWgo.Data.id))
		{
			LazyUI.GetWindow<UIConveyorWineChestWindow>().Open(data);
		}
		else if (assignedWgo.Data.Definition.conveyorType == ConveyorElementType.Chest || assignedWgo.Data.Definition.conveyorType == ConveyorElementType.ChestOut)
		{
			LazyUI.GetWindow<UIConveyorChestWindow>().Open(data);
		}
		else
		{
			LazyUI.GetWindow<UIChestWindow>().Open(data);
		}
		return true;
	}

	private static bool IsGardenBagsStorage(string wgoId)
	{
		if (!(wgoId == "garden_bags_storage_1") && !(wgoId == "garden_bags_storage_2"))
		{
			return wgoId == "garden_bags_storage_3";
		}
		return true;
	}

	private static bool IsWineStorage(string wgoId)
	{
		return wgoId == "conveyor_wine_beer_pallet";
	}

	public override bool HasInteraction(PlayerController interactor)
	{
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
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_open", GameKey.Interaction)));
	}
}
