using LazyBearTechnology;
using UnityEngine;

public class FightersContainerInteractionHandler : WGOInteractionHandlerBase
{
	private CraftComponent assignedCraftComponent;

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (TryGetInsertableZombieOverhead(out var zombieItem))
		{
			Vector3 value = MainGame.PlayerData.position.Value;
			DockPointData nearestDockPointData = assignedWgo.Data.GetNearestDockPointData(value);
			if (nearestDockPointData == null)
			{
				return false;
			}
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsCommon(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, assignedWgo.Data.GetDockPointDataWorldPosition(nearestDockPointData), nearestDockPointData.Direction);
			zombieWgoData.AttachToFightersContainer();
			nearestDockPointData.Occupy(zombieWgoData.UniqueId);
			zombieWgoData.takenDockPointsParentSGuid = assignedWgo.Data.UniqueId;
			zombieWgoData.GameResStr.Set("fighters_flag", assignedWgo.Data.GameResStr.Get("fighters_flag"));
			GameScene.GetWgoViewGlobal(zombieWgoData.UniqueId).InitZombieFighter();
			assignedWgo.DrawWidgets();
			if (Vector3.Distance(assignedWgo.Data.GetDockPointDataWorldPosition(nearestDockPointData), interactor.PlayerData.position.Value) <= 1f)
			{
				interactor.TryTeleportPlayerToAnyFreePlace();
			}
			MainGame.Instance.GameSave.WorldData.GetWorldZoneDataById("town_guard_barracks")?.NotifyWgoDataChanged();
			return true;
		}
		return true;
	}

	public override bool Interact2(PlayerController interactor)
	{
		return false;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		HasInsertableZombieOverhead();
		return true;
	}

	public override bool HasInteraction2(PlayerController interactor)
	{
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
		if (HasInsertableZombieOverhead())
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_zombie", GameKey.Interaction)));
			return interactionInfos2;
		}
		return interactionInfos2;
	}

	protected override bool TryGetInsertableZombieOverhead(out Item zombieItem)
	{
		zombieItem = null;
		if (interactor == null || !assignedWgo.Data.Definition.canInsertZombie || assignedWgo.DockPoints == null || assignedWgo.DockPoints.Count == 0 || assignedWgo.Data.GetNearestDockPointData(MainGame.PlayerData.position.Value) == null)
		{
			return false;
		}
		return interactor.PlayerData.TryGetOverheadItem((Item item) => item.Definition.itemGroupIds.Contains("zombie"), out zombieItem);
	}
}
