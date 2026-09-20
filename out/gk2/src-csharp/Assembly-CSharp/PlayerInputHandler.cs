using LazyBearTechnology;

public class PlayerInputHandler
{
	private enum InteractionType
	{
		Interaction1,
		Interaction2
	}

	private PlayerController playerController;

	private PlayerInteractionComponent interactionComponent;

	private IWGOInteractionHandler curInteractionHandler;

	public MeleeMode meleeMode = MeleeMode.WithFocus;

	public RangedMode rangedMode = RangedMode.WithFocus;

	private PlayerData PlayerData => playerController.PlayerData;

	public PlayerInputHandler(PlayerController playerController)
	{
		this.playerController = playerController;
		interactionComponent = playerController.PlayerInteractionComponent;
	}

	private bool TryCallFireEvent(InteractionType interactionType)
	{
		bool flag = curInteractionHandler.HasInteraction() || curInteractionHandler.HasInteraction2();
		if (interactionComponent.WgoUnderInteraction.Data.Events.Count > 0)
		{
			if (!flag)
			{
				return interactionComponent.WgoUnderInteraction.Data.FireInteractionEvent();
			}
			switch (interactionType)
			{
			case InteractionType.Interaction1:
				if (curInteractionHandler.HasInteraction())
				{
					return interactionComponent.WgoUnderInteraction.Data.FireInteractionEvent();
				}
				break;
			case InteractionType.Interaction2:
				if (curInteractionHandler.HasInteraction2())
				{
					return interactionComponent.WgoUnderInteraction.Data.FireInteractionEvent();
				}
				break;
			}
		}
		return false;
	}

	public void UpdateInput()
	{
		if (LazyInput.GetKeyDown(GameKey.Interaction))
		{
			if (interactionComponent.BigDropUnderInteraction != null && interactionComponent.BigDropUnderInteraction.InteractionHandler.HasInteraction())
			{
				interactionComponent.BigDropUnderInteraction.InteractionHandler.Interact();
				return;
			}
			if (interactionComponent.WgoUnderInteraction != null)
			{
				curInteractionHandler = interactionComponent.WgoUnderInteraction.InteractionHandler;
				if (TryCallFireEvent(InteractionType.Interaction1) || (curInteractionHandler.HasInteraction() && curInteractionHandler.Interact(playerController)))
				{
					return;
				}
			}
			if (PlayerData.HasOverheadItem)
			{
				PlayerData.DropOverheadItem();
			}
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Action))
		{
			if (interactionComponent.BigDropUnderInteraction != null && interactionComponent.BigDropUnderInteraction.InteractionHandler.HasInteraction2())
			{
				interactionComponent.BigDropUnderInteraction.InteractionHandler.Interact2();
				return;
			}
			if (interactionComponent.WgoUnderInteraction != null)
			{
				curInteractionHandler = interactionComponent.WgoUnderInteraction.InteractionHandler;
				if (TryCallFireEvent(InteractionType.Interaction2))
				{
					return;
				}
				if (curInteractionHandler.HasInteraction2())
				{
					curInteractionHandler.Interact2(playerController);
					return;
				}
			}
		}
		if (LazyInput.GetKeyDown(GameKey.InGameMenu))
		{
			LazyUI.GetWindow<UIGamePauseWindow>().Open(null);
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.CharacterWindow))
		{
			OpenCharacterWindow(PlayerData.lastOpenedPage);
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Inventory))
		{
			OpenCharacterWindow();
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.TechTree) && !MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.TechTree))
		{
			OpenCharacterWindow(CharacterWindowData.CharPage.TechTree);
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.QuestTree) && !MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.QuestTree))
		{
			OpenCharacterWindow(CharacterWindowData.CharPage.QuestTree);
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Map) && !MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.Map))
		{
			OpenCharacterWindow(CharacterWindowData.CharPage.Map);
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Inspirations) && (!MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.Inspiration) || HasPendingInspirationTutorial()))
		{
			OpenCharacterWindow(CharacterWindowData.CharPage.Inspiration);
			return;
		}
		if (playerController.IsControlsEnabled && playerController.AttackComponent.HasEquippedWeapon && !playerController.AttackComponent.IsRangedWeapon)
		{
			if (LazyInput.GetKey(GameKey.AttackFocus) && meleeMode == MeleeMode.WithFocus)
			{
				playerController.Ssm.ForceEnterState<AttackSwordFocusedPlayerState>();
			}
			if (LazyInput.GetKeyDown(GameKey.Attack) && !(playerController.Ssm.CurState is AttackSwordFocusedPlayerState) && MainGame.PlayerData.staminaSystem.CanPerformAttack())
			{
				switch (meleeMode)
				{
				case MeleeMode.WithStop:
					playerController.Ssm.ForceEnterState<AttackSwordPlayerState>();
					break;
				case MeleeMode.Continuous:
					playerController.Ssm.ForceEnterState<AttackSwordContinuousPlayerState>();
					break;
				case MeleeMode.WithMove:
				case MeleeMode.WithFocus:
					playerController.Ssm.ForceEnterState<AttackSwordDefaultPlayerState>();
					break;
				}
				return;
			}
		}
		if (playerController.IsControlsEnabled && playerController.AttackComponent.HasEquippedWeapon && playerController.AttackComponent.IsRangedWeapon)
		{
			if (LazyInput.GetKey(GameKey.AttackFocus) && rangedMode == RangedMode.WithFocus && MainGame.PlayerData.staminaSystem.CanPerformAttack())
			{
				playerController.Ssm.ForceEnterState<AttackBowFocusedPlayerState>();
			}
			if (LazyInput.GetKeyDown(GameKey.Attack) && !(playerController.Ssm.CurState is AttackBowFocusedPlayerState) && MainGame.PlayerData.staminaSystem.CanPerformAttack())
			{
				switch (rangedMode)
				{
				case RangedMode.Default:
				case RangedMode.WithFocus:
					playerController.Ssm.ForceEnterState<AttackBowDefaultPlayerState>();
					break;
				case RangedMode.WithAim:
					playerController.Ssm.ForceEnterState<AttackBowAutoPlayerState>();
					break;
				}
			}
		}
		UpdateHotBarInteraction();
	}

	public void UpdateInputInteractionOnly()
	{
		if (LazyInput.GetKeyDown(GameKey.Interaction))
		{
			if (interactionComponent.BigDropUnderInteraction != null && interactionComponent.BigDropUnderInteraction.InteractionHandler.HasInteraction())
			{
				interactionComponent.BigDropUnderInteraction.InteractionHandler.Interact();
				return;
			}
			if (interactionComponent.WgoUnderInteraction != null)
			{
				curInteractionHandler = interactionComponent.WgoUnderInteraction.InteractionHandler;
				if (curInteractionHandler.HasInteraction())
				{
					curInteractionHandler.Interact(playerController);
					return;
				}
			}
			if (PlayerData.HasOverheadItem)
			{
				PlayerData.DropOverheadItem();
			}
		}
		else
		{
			UpdateHotBarInteraction();
		}
	}

	public bool UpdateHotBarInteraction()
	{
		if (LazyInput.GetKeyDown(GameKey.UseHotBarItem1))
		{
			PlayerData.TryUseHotBarItem(PlayerData.pinnedItems[0]);
			return true;
		}
		if (LazyInput.GetKeyDown(GameKey.UseHotBarItem2))
		{
			PlayerData.TryUseHotBarItem(PlayerData.pinnedItems[1]);
			return true;
		}
		if (LazyInput.GetKeyDown(GameKey.UseHotBarItem3))
		{
			PlayerData.TryUseHotBarItem(PlayerData.pinnedItems[2]);
			return true;
		}
		if (LazyInput.GetKeyDown(GameKey.UseHotBarItem4))
		{
			PlayerData.TryUseHotBarItem(PlayerData.pinnedItems[3]);
			return true;
		}
		return false;
	}

	private void OpenCharacterWindow(CharacterWindowData.CharPage page = CharacterWindowData.CharPage.Main)
	{
		TalentSystemData talentSystemData = MainGame.Instance.GameSave.talentSystemData;
		if (!MainGame.PlayerData.sawInspirationTutorialOnce && talentSystemData.TryGetTalentIdWithTwoZeroFaithInspirationsToBuy(out var talentId))
		{
			MainGame.Instance.GameSave.knowledgeSystem.TryUnlockInspirationTab();
			LazyUI.GetWindow<CharacterWindow>().Open(new CharacterWindowData(MainGame.Instance.GameSave, CharacterWindowData.CharPage.Inspiration, talentId));
		}
		else
		{
			LazyUI.GetWindow<CharacterWindow>().Open(new CharacterWindowData(MainGame.Instance.GameSave, page));
		}
	}

	private bool HasPendingInspirationTutorial()
	{
		if (!MainGame.PlayerData.sawInspirationTutorialOnce)
		{
			return MainGame.Instance.GameSave.talentSystemData.HasTwoZeroFaithInspirationsToBuyInSameBranch();
		}
		return false;
	}
}
