using System.Collections.Generic;
using LazyBearTechnology;

public class ZombieInteractionHandler : WGOInteractionHandlerBase
{
	public override bool HasInteraction(PlayerController interactor)
	{
		if (assignedWgo.Data is ZombieWgoData { AttachedWgoData: not null } zombieWgoData && (zombieWgoData.AttachedWgoData.id == "sawmill_wood_crafter" || zombieWgoData.AttachedWgoData.id == "mine_ore_coal_crafter"))
		{
			return false;
		}
		return true;
	}

	public override bool HasInteraction2(PlayerController interactor)
	{
		if (assignedWgo.Data is ZombieWgoData { AttachedWgoData: not null } zombieWgoData && (zombieWgoData.AttachedWgoData.id == "sawmill_wood_crafter" || zombieWgoData.AttachedWgoData.id == "mine_ore_coal_crafter"))
		{
			return false;
		}
		return true;
	}

	public override bool Interact(PlayerController interactor)
	{
		if (assignedWgo.Data is ZombieWgoData { AttachedWgoData: var attachedWgoData } zombieWgoData)
		{
			if (attachedWgoData != null)
			{
				if (attachedWgoData.CraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
				{
					attachedWgoData.CraftComponent.ContinueAutoCraft();
					List<Item> list = new List<Item>();
					list.AddRange(attachedWgoData.CraftableObjectCraftInventory.Data.RemoveAllItems());
					if (list.Count > 0)
					{
						foreach (Item item in list)
						{
							MainGame.Instance.dropSystem.DropItem(item, attachedWgoData.WorldId, attachedWgoData.Position);
						}
					}
					attachedWgoData.DropStoredTechPoints();
				}
				zombieWgoData.UnAttachFromWgoData();
			}
			if (!SGuid.IsNullOrEmpty(zombieWgoData.takenDockPointsParentSGuid))
			{
				MainGame.Instance.GameSave.WorldData.GetWgoData(zombieWgoData.takenDockPointsParentSGuid)?.MainWgoPartData.GetOccupiedDockPointBy(zombieWgoData.UniqueId)?.UnOccupy();
				zombieWgoData.takenDockPointsParentSGuid = null;
			}
			assignedWgo.Data.WorldZoneData.NotifyWgoDataChanged();
			MainGame.ZombieSystemData.PutZombieFromGameSceneToStoreForPlayer(MainGame.PlayerData, zombieWgoData);
			return true;
		}
		return false;
	}

	public override bool Interact2(PlayerController interactor)
	{
		if (assignedWgo.Data is ZombieWgoData zombieWgoData)
		{
			UIZombieWorkerWindowData data = new UIZombieWorkerWindowData(zombieWgoData);
			LazyUI.GetWindow<UIZombieWorkerWindow>().Open(data);
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
		if (assignedWgo.Data is ZombieWgoData)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("action_inspect", GameKey.Action)));
		}
		return interactionInfos2;
	}
}
