using System.Collections.Generic;
using LazyBearTechnology;

public class PowerSourceInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (TryGetInsertableZombieOverhead(out var zombieItem) && HasNotOccupiedDockPoints())
		{
			DockPointData dockPointData = assignedWgo.Data.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter.OnlyZombie)[0];
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsCommon(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, dockPointData.GetPosFrom(assignedWgo.Data.Position), dockPointData.Direction);
			zombieWgoData.OccupyDockPoint(dockPointData, assignedWgo.Data.UniqueId);
			zombieWgoData.IsInteractable = false;
			assignedWgo.DrawWidgets();
			assignedWgo.Data.WorldZoneData.NotifyWgoDataChanged();
		}
		else if (MainGame.PlayerData.HasFreeOverheadSlot && HasOccupiedDockPoints())
		{
			List<DockPointData> dockPoints = assignedWgo.Data.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyOccupied, DockPointData.Filter.OnlyZombie);
			DockPointData dockPointData2 = dockPoints[dockPoints.Count - 1];
			SGuid occupiedBy = dockPointData2.OccupiedBy;
			ZombieWgoData zombie = MainGame.Instance.GameSave.zombieSystemData.GetZombie(occupiedBy);
			zombie.UnOccupyDockPoint(dockPointData2);
			zombie.IsInteractable = true;
			MainGame.ZombieSystemData.PutZombieFromGameSceneToStoreForPlayer(MainGame.PlayerData, zombie);
			assignedWgo.DrawWidgets();
			assignedWgo.Data.WorldZoneData.NotifyWgoDataChanged();
		}
		interactor.PlayerInteractionComponent.ResetInteractionState();
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (HasInsertableZombieOverhead() && HasNotOccupiedDockPoints())
		{
			return true;
		}
		if (MainGame.PlayerData.HasFreeOverheadSlot && HasOccupiedDockPoints())
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
		InteractionInfos interactionInfos2 = new InteractionInfos();
		if (HasInsertableZombieOverhead() && HasNotOccupiedDockPoints())
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_zombie", GameKey.Interaction)));
		}
		if (MainGame.PlayerData.HasFreeOverheadSlot && HasOccupiedDockPoints())
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
		}
		return interactionInfos2;
	}

	private bool HasOccupiedDockPoints()
	{
		return assignedWgo.Data.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyOccupied, DockPointData.Filter.OnlyZombie).Count > 0;
	}

	private bool HasNotOccupiedDockPoints()
	{
		return assignedWgo.Data.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter.OnlyZombie).Count > 0;
	}
}
