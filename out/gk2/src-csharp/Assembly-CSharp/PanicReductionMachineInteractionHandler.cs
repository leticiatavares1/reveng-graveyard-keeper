using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class PanicReductionMachineInteractionHandler : WGOInteractionHandlerBase
{
	protected CraftComponent assignedCraftComponent;

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedCraftComponent.IsDestroyingCraftActive)
		{
			return false;
		}
		if (MainGame.Instance.GameSave.environmentData.CurrentDayNumber != ConstDef.Get("day_gluttony").IntValue)
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, "pr_machine_available_tomorrow", null, null, SpeechBubbleType.Think));
			return false;
		}
		bool wasPlayerSetAsWorker = false;
		if (assignedWgo.Data.Worker == null)
		{
			assignedWgo.Data.TrySetWorker(interactor);
			wasPlayerSetAsWorker = true;
		}
		if (assignedCraftComponent.CraftsIn.Count == 1)
		{
			CraftDef craftDef = (CraftDef)assignedCraftComponent.CraftsIn[0];
			UISingleCraftWindow window = LazyUI.GetWindow<UISingleCraftWindow>();
			UISingleCraftWindowData data = new UISingleCraftWindowData(assignedWgo.Data, craftDef, null, FormCraftElementAndStartCraft);
			window.Open(data, delegate
			{
				if (wasPlayerSetAsWorker)
				{
					assignedWgo.Data.ClearWorker();
				}
			});
			return true;
		}
		UICraftWindow window2 = LazyUI.GetWindow<UICraftWindow>();
		UIBaseCraftWindowData data2 = new UIBaseCraftWindowData(assignedWgo, delegate(CraftElement ce)
		{
			OnCraftPressed(ce);
		}, delegate(CraftElement ce)
		{
			OnCraftPressed(ce, addToQueueTop: true);
		});
		window2.Open(data2, delegate
		{
			if (wasPlayerSetAsWorker)
			{
				assignedWgo.Data.ClearWorker();
			}
		});
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		assignedCraftComponent = assignedWgo.Data.CraftComponent;
		if (assignedCraftComponent.IsDestroyingCraftActive)
		{
			return false;
		}
		if (assignedCraftComponent.HasPreFinishUpdate)
		{
			return false;
		}
		if (assignedCraftComponent.IsStarted)
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
		WgoData data = assignedWgo.Data;
		InteractionInfos interactionInfos2 = new InteractionInfos();
		if (assignedCraftComponent.IsDestroyingCraftActive)
		{
			interactionInfos2.Add(GetInteractionInfoByUsingTool(isForCurrentCraft: true));
			return interactionInfos2;
		}
		if (data.CraftComponent.HasCraftsByBalance)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_craft", GameKey.Interaction)));
		}
		return interactionInfos2;
	}

	private void OnCraftPressed(CraftElement craftElement, bool addToQueueTop = false)
	{
		if (craftElement.Definition.skipQueue)
		{
			if (assignedCraftComponent.GetStartCraftStatus(craftElement) == CraftStatus.OK && craftElement.CanFinishCraft(assignedWgo.Data) == CraftStatus.OK)
			{
				assignedCraftComponent.ProcessInstantCraft(assignedWgo.Data, craftElement);
			}
		}
		else
		{
			if (craftElement.Definition.IsMultipleCraftsDisabled && assignedCraftComponent.CraftElementsQueue.Find((CraftElementBase x) => x.CraftId == craftElement.CraftId) != null)
			{
				return;
			}
			Debug.Log($"Adding Craft: id: {craftElement.CraftId}, count: {craftElement.Count}, addToQueue: {addToQueueTop}");
			if (assignedWgo.Data.Definition.isAutoCrafter && assignedCraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
			{
				assignedCraftComponent.AddToQueue(craftElement, addToQueueTop: false, 1);
				LazyUI.GetWindow<UICraftWindow>().Close();
				return;
			}
			assignedCraftComponent.AddToQueue(craftElement, addToQueueTop);
			if (addToQueueTop)
			{
				assignedCraftComponent.TryContinueFromQueue();
				LazyUI.GetWindow<UICraftWindow>().Close();
			}
		}
	}

	private void FormCraftElementAndStartCraft(CraftDef craftDefinition, List<NeedItemData> selectedNeedItems, CraftParamsData craftParams, int craftsCount = 1)
	{
		CraftElement craftElement = new CraftElement(craftDefinition.id, craftsCount, selectedNeedItems, craftParams);
		craftElement.DoBeforeStartCalculations(assignedWgo.Data);
		if (craftElement.Definition.isFuelCraft)
		{
			ItemDef data = GameBalance.Me.GetData<ItemDef>(craftElement.Definition.addItemsToWgoOnFinish.chanceOutputItems[0].id);
			int num = Mathf.FloorToInt((float)assignedWgo.Data.Inventory.Data.CanAddItemCountToInventory(data, 99999) / (float)craftElement.PreToWgoOnFinishItems[0].count);
			craftElement.Count = ((craftsCount > num) ? num : craftsCount);
		}
		OnCraftPressed(craftElement, addToQueueTop: true);
	}
}
