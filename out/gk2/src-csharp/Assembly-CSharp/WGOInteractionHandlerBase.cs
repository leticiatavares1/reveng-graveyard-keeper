using LazyBearTechnology;

public abstract class WGOInteractionHandlerBase : IWGOInteractionHandler, IInteractionHandler
{
	protected Wgo assignedWgo;

	protected PlayerController interactor;

	protected string WorkHint => LocalizeHintWithActionIcon("hint_work", GameKey.Action);

	void IInteractionHandler.OnInteractionTargetEnter()
	{
		OnInteractionTargetEnter(null);
	}

	bool IInteractionHandler.Interact()
	{
		return Interact(null);
	}

	bool IInteractionHandler.Interact2()
	{
		return Interact2(null);
	}

	bool IInteractionHandler.HasInteraction()
	{
		return HasInteraction(interactor);
	}

	bool IInteractionHandler.HasInteraction2()
	{
		return HasInteraction2(interactor);
	}

	public virtual IWGOInteractionHandler Init(Wgo wgo)
	{
		assignedWgo = wgo;
		return this;
	}

	public virtual void OnInteractionTargetEnter(PlayerController interactor)
	{
		this.interactor = interactor;
		assignedWgo.DrawWidgets();
		assignedWgo.TryDrawNpcWidget();
		UIObjectBubbleManager.Instance.PutOnTopTargetId = assignedWgo.Data.UniqueId;
	}

	public virtual void OnInteractionTargetExit()
	{
		interactor = null;
		WgoBubbleDisplayHandler.Hide(assignedWgo);
		GUIElements.Instance.NpcWidget.Hide();
		assignedWgo.DrawWidgets();
		UIObjectBubbleManager.Instance.PutOnTopTargetId = SGuid.Empty;
	}

	public virtual bool Interact(PlayerController interactor)
	{
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.Interaction, assignedWgo.Id ?? "");
		return false;
	}

	public virtual bool Interact2(PlayerController interactor)
	{
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.Interaction, assignedWgo.Id ?? "");
		return false;
	}

	public virtual ItemType GetRequiredInteractionToolType()
	{
		if (assignedWgo.Data.CraftComponent.CurrentCraftElement != null)
		{
			CraftDef craftDef = assignedWgo.Data.CraftComponent.CurrentCraftElement.Def as CraftDef;
			if (craftDef != null)
			{
				if (assignedWgo.Data.CraftComponent.HasPreFinishUpdate)
				{
					foreach (CraftElementBase item in assignedWgo.Data.CraftComponent.CraftElementsQueue)
					{
						if (assignedWgo.Data.CraftComponent.GetStartCraftStatus(item) == CraftStatus.OK)
						{
							craftDef = (CraftDef)item.Def;
							break;
						}
					}
				}
				if (craftDef.customItemTypeAction != 0 && !craftDef.isAuto)
				{
					return craftDef.customItemTypeAction;
				}
			}
		}
		return assignedWgo.Data.Definition.toolAction.actionableTool;
	}

	public InteractionInfos GetInteractionInfos()
	{
		InteractionInfos interactionInfos = FormInteractionInfo();
		foreach (InteractionInfo item in interactionInfos.list)
		{
			item.text = (string.IsNullOrEmpty(item.text) ? item.text : FixSpace(item.text));
		}
		return interactionInfos;
	}

	public virtual bool HasInteraction(PlayerController interactor)
	{
		return false;
	}

	public virtual bool HasInteraction2(PlayerController interactor)
	{
		return false;
	}

	protected virtual InteractionInfos FormInteractionInfo()
	{
		if (assignedWgo.Data.CraftComponent.CurrentCraftElement is CraftElement craftElement && craftElement.Definition.isObjDestroyCraft)
		{
			return new InteractionInfos(GetInteractionInfoByUsingTool());
		}
		bool isControlsEnabledForInteractionHints = MainGame.PlayerController.IsControlsEnabledForInteractionHints;
		if (assignedWgo.Data.Events.Count > 0 && isControlsEnabledForInteractionHints)
		{
			return new InteractionInfos(GetInteractionInfoByEvent(assignedWgo.Data.PeekFirstAddedEvent()));
		}
		if (MainGame.Instance.GameSave.questSystemData.WgoHasReadyToFinishQuest(assignedWgo.Id))
		{
			return new InteractionInfos(GetInteractionInfoByQuestStatus());
		}
		return new InteractionInfos();
	}

	protected bool TryGetCustomInteractionStr(out string hint)
	{
		hint = string.Empty;
		bool flag = !string.IsNullOrEmpty(assignedWgo.Data.Definition.customInteraction.hint);
		if (flag && assignedWgo.Data.Definition.customInteraction.IsInteractable(assignedWgo.Data))
		{
			hint = LocalizeHintWithActionIcon(assignedWgo.Data.Definition.customInteraction.hint, GameKey.Interaction);
			return true;
		}
		bool flag2 = !string.IsNullOrEmpty(assignedWgo.Data.Definition.customInteraction2.hint);
		if (flag2 && assignedWgo.Data.Definition.customInteraction2.IsInteractable(assignedWgo.Data))
		{
			hint = LocalizeHintWithActionIcon(assignedWgo.Data.Definition.customInteraction2.hint, GameKey.Interaction);
			return true;
		}
		if (flag)
		{
			hint = LocalizeHintWithActionIcon(assignedWgo.Data.Definition.customInteraction.hint, GameKey.Interaction);
			return true;
		}
		if (flag2)
		{
			hint = LocalizeHintWithActionIcon(assignedWgo.Data.Definition.customInteraction2.hint, GameKey.Interaction);
			return true;
		}
		return false;
	}

	protected string LocalizeHintWithActionIcon(string hintId, GameKey gameKey)
	{
		return ControllerIconLibrary.GetIconId(gameKey) + LLBase.L(hintId);
	}

	protected InteractionInfo GetInteractionInfoByUsingTool(bool isForCurrentCraft = false)
	{
		if (MainGame.PlayerData != null && MainGame.PlayerData.HasMultipleOverheadItems)
		{
			return new InteractionInfo();
		}
		if (interactor == null)
		{
			return new InteractionInfo(WorkHint);
		}
		TalentDef dataOrNull = GameBalance.Me.GetDataOrNull<TalentDef>(assignedWgo.Data.Definition.talent);
		ItemType itemType = GetRequiredInteractionToolType();
		if (itemType == ItemType.None || itemType == ItemType.Hand)
		{
			if (dataOrNull == null)
			{
				return new InteractionInfo(WorkHint);
			}
			itemType = ItemType.Hand;
		}
		bool isItemEquipped = !interactor.PlayerData.toolBeltInventory.GetItemByType(itemType).IsEmpty;
		int masteryLock = assignedWgo.Data.Definition.MasteryLock;
		bool flag = false;
		if (isForCurrentCraft)
		{
			CraftDefBase def = assignedWgo.Data.CraftComponent.CurrentCraftElement.Def;
			masteryLock = ((def.isStarCraft || def.isAutopsyCraft) ? 1 : def.talentLock);
			flag = dataOrNull == null || assignedWgo.Data.GetGameResInt("seed_mastery_lock") > 0 || MainGame.PlayerController.GetMasteryLevelForTalentBranch(dataOrNull.id, def) >= masteryLock;
			return new InteractionInfo(ControllerIconLibrary.GetIconId(GameKey.Action), itemType, isItemEquipped, dataOrNull, masteryLock, flag);
		}
		flag = dataOrNull == null || assignedWgo.Data.GetGameResInt("seed_mastery_lock") > 0 || MainGame.PlayerController.GetMasteryLevelForTalentBranch(dataOrNull.id) >= masteryLock;
		return new InteractionInfo(ControllerIconLibrary.GetIconId(GameKey.Action), itemType, isItemEquipped, dataOrNull, masteryLock, flag);
	}

	protected InteractionInfo GetInteractionInfoByEvent(InteractionEvent interactionEvent)
	{
		if (interactor != null)
		{
			GameKey key = ((assignedWgo.Data.Definition.interactionType == WGODef.InteractionType.Work) ? GameKey.Action : GameKey.Interaction);
			if (assignedWgo.Data.Definition.interactionType == WGODef.InteractionType.Work)
			{
				TalentDef dataOrNull = GameBalance.Me.GetDataOrNull<TalentDef>(assignedWgo.Data.Definition.talent);
				ItemType requiredInteractionToolType = GetRequiredInteractionToolType();
				if (requiredInteractionToolType == ItemType.None || requiredInteractionToolType == ItemType.Hand)
				{
					return new InteractionInfo(ControllerIconLibrary.GetIconId(key), interactionEvent.CustomIcon);
				}
				bool isItemEquipped = !interactor.PlayerData.toolBeltInventory.GetItemByType(requiredInteractionToolType).IsEmpty;
				int masteryLock = assignedWgo.Data.Definition.MasteryLock;
				bool isEnoughMastery = dataOrNull == null || assignedWgo.Data.GetGameResInt("seed_mastery_lock") > 0 || MainGame.PlayerController.GetMasteryLevelForTalentBranch(dataOrNull.id) >= masteryLock;
				return new InteractionInfo(ControllerIconLibrary.GetIconId(key), interactionEvent.CustomIcon, requiredInteractionToolType, isItemEquipped, dataOrNull, masteryLock, isEnoughMastery);
			}
			return new InteractionInfo(ControllerIconLibrary.GetIconId(key), interactionEvent.CustomIcon);
		}
		return new InteractionInfo
		{
			customIconId = interactionEvent.CustomIcon
		};
	}

	protected InteractionInfo GetInteractionInfoByQuestStatus()
	{
		if (interactor != null)
		{
			return new InteractionInfo(ControllerIconLibrary.GetIconId(GameKey.Interaction), "icon_speech_bubble");
		}
		return new InteractionInfo
		{
			customIconId = "icon_speech_bubble"
		};
	}

	protected bool HasInsertableZombieOverhead()
	{
		Item zombieItem;
		return TryGetInsertableZombieOverhead(out zombieItem);
	}

	protected virtual bool TryGetInsertableZombieOverhead(out Item zombieItem)
	{
		zombieItem = null;
		if (interactor == null || assignedWgo?.Data?.Definition == null || !assignedWgo.Data.Definition.canInsertZombie || assignedWgo.Data.Worker != null || assignedWgo.DockPoints == null || assignedWgo.DockPoints.Count == 0)
		{
			return false;
		}
		return interactor.PlayerData.TryGetOverheadItem((Item item) => item.Definition.itemGroupIds.Contains("zombie"), out zombieItem);
	}

	private string FixSpace(string s)
	{
		s = s.Replace(" ", LL.GetSpace());
		s = s.Replace("sprite" + LL.GetSpace(), "sprite ");
		return s;
	}
}
