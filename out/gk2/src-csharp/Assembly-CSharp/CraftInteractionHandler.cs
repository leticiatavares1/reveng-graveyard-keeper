using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class CraftInteractionHandler : WGOInteractionHandlerBase
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
		if (HasOneTimeCraftStarted())
		{
			return false;
		}
		if (assignedCraftComponent.HasCraftsByBalance && TryGetInsertableZombieOverhead(out var zombieItem))
		{
			DockPoint dockPoint = assignedWgo.TryGetDockPointForWorker(findNearest: true, interactor.transform.position);
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsCommon(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, dockPoint.transform.position, dockPoint.Direction);
			DockPointData dockPointData = assignedWgo.GetDockPointData(dockPoint);
			if (assignedWgo.Data.CraftableType == CraftableType.ConveyorWorkbench)
			{
				zombieWgoData.AttachToConveyorCraftWgoData(assignedWgo.Data.UniqueId, zombieItem, dockPointData);
			}
			else
			{
				zombieWgoData.AttachToCraftWgoData(assignedWgo.Data.UniqueId, zombieItem, dockPointData);
			}
			assignedWgo.Data.CraftComponent.UpdateCanContinueManualCraftState(Time.deltaTime);
			assignedWgo.DrawWidgets();
			if (Vector3.Distance(dockPoint.transform.position, interactor.PlayerData.position.Value) <= 1f)
			{
				interactor.TryTeleportPlayerToAnyFreePlace();
			}
			return true;
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
			if (craftDef.isFuelCraft)
			{
				UIFuelCraftWindow window = LazyUI.GetWindow<UIFuelCraftWindow>();
				UISingleCraftWindowData data = new UISingleCraftWindowData(assignedWgo.Data, craftDef, null, FormCraftElementAndStartCraft);
				window.Open(data, delegate
				{
					if (wasPlayerSetAsWorker)
					{
						assignedWgo.Data.ClearWorker();
					}
				});
			}
			else
			{
				UISingleCraftWindow window2 = LazyUI.GetWindow<UISingleCraftWindow>();
				UISingleCraftWindowData data2 = new UISingleCraftWindowData(assignedWgo.Data, craftDef, null, FormCraftElementAndStartCraft);
				window2.Open(data2, delegate
				{
					if (wasPlayerSetAsWorker)
					{
						assignedWgo.Data.ClearWorker();
					}
				});
			}
			return true;
		}
		UICraftWindow window3 = LazyUI.GetWindow<UICraftWindow>();
		UIBaseCraftWindowData data3 = new UIBaseCraftWindowData(assignedWgo, delegate(CraftElement ce)
		{
			OnCraftPressed(ce);
		}, delegate(CraftElement ce)
		{
			OnCraftPressed(ce, addToQueueTop: true);
		});
		window3.Open(data3, delegate
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
		if (HasOneTimeCraftStarted())
		{
			return false;
		}
		HasInsertableZombieOverhead();
		return true;
	}

	public override bool Interact2(PlayerController interactor)
	{
		if (base.Interact2(interactor))
		{
			return true;
		}
		if (assignedCraftComponent.IsDestroyingCraftActive)
		{
			return false;
		}
		if (HasOneTimeCraftStarted())
		{
			return false;
		}
		if (assignedWgo.Data.Definition.isAutoCrafter && assignedCraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
		{
			IWorker worker = assignedWgo.Data.Worker;
			bool flag = false;
			if (worker == null)
			{
				flag = true;
				assignedWgo.Data.TrySetWorker(interactor);
			}
			assignedCraftComponent.ContinueAutoCraft();
			List<Item> list = new List<Item>();
			list.AddRange(assignedWgo.Data.CraftableObjectCraftInventory.Data.RemoveAllItems());
			if (list.Count > 0)
			{
				foreach (Item item in list)
				{
					assignedWgo.Data.MakeDrop(item);
				}
			}
			assignedWgo.Data.DropStoredTechPoints();
			if (flag)
			{
				assignedWgo.Data.ClearWorker();
			}
			interactor.PlayerInteractionComponent.ResetInteractionState();
			return true;
		}
		if (assignedWgo.Data.Worker is ZombieWgoData zombieWgoData)
		{
			bool flag2 = false;
			string text = string.Empty;
			bool flag3 = zombieWgoData.CrafterCurrentOrder != null;
			for (int num = zombieWgoData.CrafterOrders.Count - 1; num >= 0; num--)
			{
				OrderBase orderBase = zombieWgoData.WorldZoneData.FindOrder(zombieWgoData.CrafterOrders[num]);
				if (orderBase != null)
				{
					if (orderBase.TryExecuteOrder(new PlayerOrderExecutor(), out var reasonIfNot))
					{
						zombieWgoData.WorldZoneData.RemoveOrder(orderBase.UniqueId);
						zombieWgoData.CrafterOnOrderExecuted(orderBase);
					}
					else
					{
						text = reasonIfNot;
						flag2 = true;
					}
				}
			}
			if (flag2)
			{
				Bubble.Talk(new PhraseData(isPlayer: true, null, text, null, null, SpeechBubbleType.Think));
			}
			interactor.PlayerInteractionComponent.ResetInteractionState();
			return true;
		}
		interactor.PlayerInteractionComponent.ResetInteractionState();
		return true;
	}

	public override bool HasInteraction2(PlayerController interactor)
	{
		assignedCraftComponent = assignedWgo.Data.CraftComponent;
		if (assignedCraftComponent.IsDestroyingCraftActive)
		{
			return true;
		}
		if (assignedWgo.Data.Definition.isAutoCrafter && assignedCraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
		{
			return true;
		}
		if (assignedWgo.Data.Worker is ZombieWgoData { CrafterCurrentOrder: not null })
		{
			return true;
		}
		if (HasOneTimeCraftStarted())
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
		WgoData data = assignedWgo.Data;
		InteractionInfos interactionInfos2 = new InteractionInfos();
		if (assignedCraftComponent.IsDestroyingCraftActive)
		{
			interactionInfos2.Add(GetInteractionInfoByUsingTool(isForCurrentCraft: true));
			return interactionInfos2;
		}
		if (HasOneTimeCraftStarted())
		{
			interactionInfos2.Add(GetInteractionInfoByUsingTool(isForCurrentCraft: true));
			return interactionInfos2;
		}
		if ((data.CraftComponent.HasCraftsByBalance || data.Definition.isAutoCrafter) && HasInsertableZombieOverhead() && !assignedCraftComponent.IsDestroyingCraftActive)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_zombie", GameKey.Interaction)));
			return interactionInfos2;
		}
		string hintId = ((data.id == "alchemy_flask_1_shed") ? "ui_place" : "hint_craft");
		if (data.Definition.isAutoCrafter)
		{
			if (data.CraftComponent.HasCraftsByBalance && data.CraftComponent.AvailableCrafts[0].isAuto)
			{
				interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon(hintId, GameKey.Interaction)));
				if (assignedWgo.Data.Definition.isAutoCrafter && assignedCraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
				{
					interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take_all", GameKey.Action)));
				}
			}
			else
			{
				interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon(hintId, GameKey.Interaction)));
			}
		}
		else
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon(hintId, GameKey.Interaction)));
		}
		if (assignedWgo.Data.Worker is ZombieWgoData { CrafterCurrentOrder: not null } zombieWgoData)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon(zombieWgoData.CrafterCurrentOrder.GetInteractionHint(), GameKey.Action)));
		}
		if ((data.CraftComponent.HasCraftsByBalance || data.Definition.isAutoCrafter) && data.CraftComponent.HasCraftsInQueue && !HasInsertedWorker() && !data.Definition.isAutoCrafter)
		{
			interactionInfos2.Add(GetInteractionInfoByUsingTool(isForCurrentCraft: true));
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
			if ((craftElement.Definition.IsMultipleCraftsDisabled && assignedCraftComponent.CraftElementsQueue.Find((CraftElementBase x) => x.CraftId == craftElement.CraftId) != null) || (assignedWgo.Data.Definition.conveyorType == ConveyorElementType.Workbench && assignedCraftComponent.CraftElementsQueue.Count > 0 && assignedCraftComponent.CraftElementsQueue.Find((CraftElementBase x) => x.CraftId == craftElement.CraftId) == null))
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

	private bool HasDroppableItem()
	{
		return assignedWgo.Data.CraftableObjectCraftInventory.Data.Inventory.Count > 0;
	}

	private bool HasInsertedWorker()
	{
		if (assignedWgo.Data.Worker != null)
		{
			return assignedWgo.Data.Worker != MainGame.PlayerController;
		}
		return false;
	}

	private bool HasOneTimeCraftStarted()
	{
		if (assignedWgo.Data.CraftComponent.IsStarted && assignedWgo.Data.CraftComponent.CurrentCraftElement?.Def is CraftDef { isOneTimeCraft: not false })
		{
			return true;
		}
		return false;
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
