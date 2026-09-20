using GK2.FlowCanvasNodes;
using LazyBearTechnology;
using UnityEngine;

public class ZombieMineInteractionHandler : WGOInteractionHandlerBase
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
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsCommon(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, dockPoint.transform.position);
			assignedWgo.Data.TrySetWorker(zombieWgoData);
			zombieWgoData.IsInteractable = false;
			zombieWgoData.AttachToCraftWgoData(assignedWgo.Data.UniqueId, zombieItem);
			SetupZombieVisual(zombieWgoData);
			assignedWgo.Data.CraftComponent.UpdateCanContinueManualCraftState(Time.deltaTime);
			assignedWgo.DrawWidgets();
			if (assignedWgo.Data.CraftComponent.CurrentCraftElement == null)
			{
				CraftParamsData craftParamsData = new CraftParamsData(assignedWgo.Data.CraftComponent.AvailableCrafts[0].id, assignedWgo.Data);
				craftParamsData.customRes.Set("wait_for_zombie_at_mine", 1f);
				craftParamsData.customRes.Set("ignore_handle_output", 1f);
				assignedWgo.Data.CraftComponent.AddToQueue(new CraftElement(assignedWgo.Data.CraftComponent.AvailableCrafts[0].id, 1, craftParamsData));
			}
		}
		else if (assignedWgo.Data.Worker is ZombieWgoData zombieWgoData2 && MainGame.PlayerData.HasFreeOverheadSlot)
		{
			if (zombieWgoData2.GameResStr.Has("mine_point"))
			{
				MainGame.WorldData.GetWgoData("builder_mine").SetGameRes(zombieWgoData2.GameResStr.Get("mine_point"), 0);
				zombieWgoData2.GameResStr.Remove("mine_point");
				zombieWgoData2.FireEvent("mine_craft_end");
			}
			else if (zombieWgoData2.CaretakerPortableItem != null && !zombieWgoData2.CaretakerPortableItem.IsEmpty)
			{
				Flow_FinishZombieMineCraft.FinishCraft(zombieWgoData2.AttachedWgoData, startAnother: false);
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

	private void SetupZombieVisual(ZombieWgoData zombieData)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(zombieData.UniqueId);
		if (!(wgoViewGlobal == null))
		{
			int gameResInt = zombieData.GetGameResInt("zombie_head_id");
			int gameResInt2 = zombieData.GetGameResInt("zombie_body_id");
			string headLut = zombieData.GameResStr.Get("zombie_head_lut");
			int num;
			switch (gameResInt)
			{
			case 1050:
			case 1056:
				num = 1701;
				break;
			case 1052:
			case 1054:
				num = 1702;
				break;
			case 1058:
				num = 1703;
				break;
			default:
				num = 1700;
				break;
			}
			int head = num;
			SkinPresetGK2 presetForCustomizationData = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", gameResInt2, head, string.Empty, headLut);
			if (presetForCustomizationData != null && wgoViewGlobal.MainWgoPart.AnimationComponent is AnimationComponent animationComponent)
			{
				animationComponent.SetSkinPreset(presetForCustomizationData);
				animationComponent.ChangeSkinPreset(presetForCustomizationData);
			}
		}
	}
}
