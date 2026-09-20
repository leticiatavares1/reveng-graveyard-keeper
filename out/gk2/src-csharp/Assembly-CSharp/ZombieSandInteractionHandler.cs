using GK2.FlowCanvasNodes;
using LazyBearTechnology;
using UnityEngine;

public class ZombieSandInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Worker == null && TryGetInsertableZombieOverhead(out var zombieItem))
		{
			GDPointData gDPointData = assignedWgo.Data.GetGDPointData("zombie_clay_sand_crafter_gd_point");
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsCommon(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, gDPointData.Position);
			assignedWgo.Data.TrySetWorker(zombieWgoData);
			zombieWgoData.IsInteractable = false;
			zombieWgoData.AttachToCraftWgoData(assignedWgo.Data.UniqueId, zombieItem);
			assignedWgo.Data.CraftComponent.UpdateCanContinueManualCraftState(Time.deltaTime);
			assignedWgo.DrawWidgets();
			if (assignedWgo.Data.CraftComponent.CurrentCraftElement == null)
			{
				CraftParamsData craftParamsData = new CraftParamsData(assignedWgo.Data.CraftComponent.AvailableCrafts[0].id, assignedWgo.Data);
				craftParamsData.customRes.Set("wait_for_zombie_at_sand", 1f);
				craftParamsData.customRes.Set("ignore_handle_output", 1f);
				assignedWgo.Data.CraftComponent.AddToQueue(new CraftElement(assignedWgo.Data.CraftComponent.AvailableCrafts[0].id, 1, craftParamsData));
			}
		}
		else if (assignedWgo.Data.Worker is ZombieWgoData zombieWgoData2 && MainGame.PlayerData.HasFreeOverheadSlot)
		{
			if (zombieWgoData2.GameResStr.Has("sand_point"))
			{
				MainGame.WorldData.GetWgoData("builder_clay_sand").SetGameRes(zombieWgoData2.GameResStr.Get("sand_point"), 0);
				zombieWgoData2.GameResStr.Remove("sand_point");
				zombieWgoData2.FireEvent("sand_craft_end");
			}
			else if (zombieWgoData2.CaretakerPortableItem != null && !zombieWgoData2.CaretakerPortableItem.IsEmpty)
			{
				Flow_FinishZombieSandCraft.FinishCraft(zombieWgoData2.AttachedWgoData, startAnother: false);
			}
			assignedWgo.Data.SetGameRes("stuff_disabled", 0);
			assignedWgo.Data.CraftComponent.Clear();
			zombieWgoData2.UnAttachFromWgoData();
			zombieWgoData2.IsInteractable = true;
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

	protected override bool TryGetInsertableZombieOverhead(out Item zombieItem)
	{
		zombieItem = null;
		if (interactor == null || !assignedWgo.Data.Definition.canInsertZombie || assignedWgo.Data.Worker != null)
		{
			return false;
		}
		return interactor.PlayerData.TryGetOverheadItem((Item item) => item.Definition.itemGroupIds.Contains("zombie"), out zombieItem);
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
