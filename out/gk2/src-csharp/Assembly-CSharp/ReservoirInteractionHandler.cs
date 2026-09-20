using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class ReservoirInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (MainGame.PlayerData.HasMultipleOverheadItems)
		{
			return false;
		}
		if (!interactor.PlayerData.toolBeltInventory.Data.HasItemsByItemType(ItemType.FishingRod))
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, "fishing_no_rod", null, null, SpeechBubbleType.Think));
			return true;
		}
		if (interactor.GetMasteryLevelForTalentBranch("talent_green") <= 0)
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, "fishing_no_mastery", null, null, SpeechBubbleType.Think));
			return true;
		}
		List<FishingDef> allForReservoir = FishingDef.GetAllForReservoir(assignedWgo.Id);
		allForReservoir.RemoveAll((FishingDef x) => assignedWgo.Data.GetGameResInt(x.fishId) == 0);
		if (allForReservoir.Count == 0)
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, "fishing_no_fish", null, null, SpeechBubbleType.Think));
			return true;
		}
		if (FishingDef.GetAvailableBaits(new List<Item>(MainGame.PlayerData.Inventory.GetItemsByType(ItemType.Bait))
		{
			new Item("no_bait")
		}, allForReservoir).Count == 0)
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, "fishing_no_bait", null, null, SpeechBubbleType.Think));
			return true;
		}
		if (!assignedWgo.Data.CraftComponent.IsStarted)
		{
			StartFishing(interactor.PlayerData.toolBeltInventory.Data.GetItemByType(ItemType.FishingRod).Definition);
		}
		return true;
	}

	private void StartFishing(ItemDef fishingRodDef)
	{
		DockPoint dockPoint = assignedWgo.TryGetDockPointForWorker();
		if (dockPoint == null)
		{
			Debug.LogError("[ReservoirInteractionHandler]: No dock point for fishing");
			return;
		}
		if (MainGame.PlayerData.HasOverheadItem)
		{
			MainGame.PlayerData.DropOverheadItem();
		}
		string currentGameSceneId = MainGame.PlayerData.currentGameSceneId;
		MainGame.PlayerController.MovementComponent.StartPath(dockPoint.transform.position, currentGameSceneId, currentGameSceneId, MovementType.Direct, 1.5f, "", delegate
		{
			UIFishingWindow fishingWindow = LazyUI.GetWindow<UIFishingWindow>();
			UIFishingWindowData windowData = new UIFishingWindowData(assignedWgo, fishingRodDef);
			if (!MainGame.PlayerData.interactedWithFishingReservoirOnce && MainGame.PlayerData.GetRes("fishing_tutorial_available") > 0f)
			{
				MainGame.PlayerData.interactedWithFishingReservoirOnce = true;
				UITutorialWindowData data = new UITutorialWindowData("tut_fishing_hdr");
				LazyUI.GetWindow<UITutorialWindow>().Open(data, delegate
				{
					fishingWindow.Open(windowData);
				});
			}
			else
			{
				fishingWindow.Open(windowData);
			}
		}, MainGame.PlayerController.PlayerLocalAreaMovement.Seeker);
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (MainGame.PlayerData.HasMultipleOverheadItems)
		{
			return false;
		}
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		InteractionInfo interactionInfoByUsingTool = GetInteractionInfoByUsingTool();
		if (!interactionInfoByUsingTool.isItemEquipped)
		{
			return new InteractionInfos(interactionInfoByUsingTool);
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("ui_submit_bait", GameKey.Interaction)));
	}
}
