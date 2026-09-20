using LazyBearTechnology;

public class PorterStationInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Worker == null && TryGetInsertableZombieOverhead(out var zombieItem))
		{
			GDPointData gDPointData = assignedWgo.Data.GetGDPointData("zombie_porter_station_gd_point");
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsCommon(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, gDPointData.Position);
			assignedWgo.Data.TrySetWorker(zombieWgoData);
			zombieWgoData.AttachToPorterStation(assignedWgo.Data.UniqueId, zombieItem);
			zombieWgoData.direction.Value = Direction.Down.ConvertToVector2XZ();
		}
		else if (assignedWgo.Data.Worker is ZombieWgoData zombieWgoData2 && MainGame.PlayerData.HasFreeOverheadSlot)
		{
			zombieWgoData2.UnAttachFromWgoData(dropPorterInventoryNearPlayer: true);
			MainGame.ZombieSystemData.PutZombieFromGameSceneToStoreForPlayer(MainGame.PlayerData, zombieWgoData2);
		}
		interactor.PlayerInteractionComponent.ResetInteractionState();
		return true;
	}

	public override bool Interact2(PlayerController interactor)
	{
		LazyUI.GetWindow<UIPorterStationWindow>().Open(new UIPorterStationWindowData(assignedWgo.Data));
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (assignedWgo.Data.Worker == null && HasInsertableZombieOverhead())
		{
			return true;
		}
		if (assignedWgo.Data.Worker != null && MainGame.PlayerData.HasFreeOverheadSlot)
		{
			return true;
		}
		return false;
	}

	public override bool HasInteraction2(PlayerController interactor)
	{
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		InteractionInfos interactionInfos2 = new InteractionInfos();
		if (assignedWgo.Data.Worker == null && HasInsertableZombieOverhead())
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_zombie", GameKey.Interaction)));
		}
		else if (assignedWgo.Data.Worker != null && MainGame.PlayerData.HasFreeOverheadSlot)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
		}
		interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_open", GameKey.Action)));
		return interactionInfos2;
	}
}
