using LazyBearTechnology;
using UnityEngine;

public class ZombieCarrierInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Worker == null && TryGetInsertableZombieOverhead(out var zombieItem))
		{
			DockPoint dockPoint = assignedWgo.TryGetDockPointForWorker(findNearest: true, interactor.transform.position);
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsCommon(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, dockPoint.transform.position, dockPoint.Direction);
			zombieWgoData.AttachToCraftWgoData(dockPointData: assignedWgo.GetDockPointData(dockPoint), uniqueId: assignedWgo.Data.UniqueId, zombie: zombieItem);
			assignedWgo.Data.CraftComponent.UpdateCanContinueManualCraftState(Time.deltaTime);
			assignedWgo.DrawWidgets();
			if (assignedWgo.Data.CraftComponent.CurrentCraftElement == null)
			{
				CraftParamsData craftParamsData = new CraftParamsData(assignedWgo.Data.CraftComponent.AvailableCrafts[0].id, assignedWgo.Data);
				craftParamsData.customRes.Set("auto_start_same_craft_after_pickup", 1f);
				craftParamsData.customRes.Set("do_not_check_multiinventory_space", 1f);
				assignedWgo.Data.CraftComponent.TryStartCraft(new CraftElementBase(assignedWgo.Data.CraftComponent.AvailableCrafts[0].id, 1, craftParamsData));
			}
			zombieWgoData.CrafterStopCraftActivity();
			zombieWgoData.CrafterStartCraftActivity(resetTicks: false);
		}
		else if (assignedWgo.Data.Worker is ZombieWgoData zombieWgoData2 && MainGame.PlayerData.HasFreeOverheadSlot)
		{
			zombieWgoData2.UnAttachFromWgoData();
			assignedWgo.Data.WorldZoneData.NotifyWgoDataChanged();
			MainGame.ZombieSystemData.PutZombieFromGameSceneToStoreForPlayer(MainGame.PlayerData, zombieWgoData2);
		}
		interactor.PlayerInteractionComponent.ResetInteractionState();
		return true;
	}

	public override bool Interact2(PlayerController interactor)
	{
		if (assignedWgo.Data.Worker is ZombieWgoData zombieWgoData)
		{
			UIZombieWorkerWindowData data = new UIZombieWorkerWindowData(zombieWgoData);
			LazyUI.GetWindow<UIZombieWorkerWindow>().Open(data);
			return true;
		}
		return false;
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
		return assignedWgo.Data.Worker != null;
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
		else if (assignedWgo.Data.Worker != null)
		{
			if (MainGame.PlayerData.HasFreeOverheadSlot)
			{
				interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
			}
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("action_inspect", GameKey.Action)));
		}
		return interactionInfos2;
	}
}
