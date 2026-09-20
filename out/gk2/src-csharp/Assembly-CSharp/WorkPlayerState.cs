using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class WorkPlayerState : SSMState
{
	private delegate void FixedUpdateInteractionHandler(float deltaTime);

	private static readonly FixedUpdateInteractionHandler skipInteractionHandler = SkipInteractionUpdate;

	private readonly FixedUpdateInteractionHandler updateInteractionHandler;

	private FixedUpdateInteractionHandler fixedUpdateInteractionHandler = skipInteractionHandler;

	private readonly PlayerWorkComponent playerWorkComponent;

	private Wgo targetWgo;

	private Wgo underInteractionWgo;

	private PlayerInputHandler playerInputHandler;

	public override bool CanEnter
	{
		get
		{
			if (IsActive)
			{
				return CanWork();
			}
			return false;
		}
	}

	public override bool IsActive
	{
		get
		{
			if (LazySingleton<FightingGameController>.Instance.CurrentFightState != FightState.ActiveFight && (LazyInput.GetKey(GameKey.Action) || playerWorkComponent.WorkIsTookControl))
			{
				return playerController.IsControlsEnabled;
			}
			return false;
		}
	}

	public WorkPlayerState(PlayerController playerController)
		: base(playerController)
	{
		playerWorkComponent = playerController.PlayerWorkComponent;
		playerInputHandler = playerController.PlayerInputHandler;
		updateInteractionHandler = UpdateInteraction;
	}

	public override void FixedUpdate()
	{
		fixedUpdateInteractionHandler(Time.fixedDeltaTime);
	}

	public override void Update()
	{
		base.Update();
		bool flag = false;
		if (playerWorkComponent.CanUpdateMagnetismDelayTimer)
		{
			playerWorkComponent.UpdateMagnetismDelay(Time.deltaTime);
			flag = true;
		}
		if (IsActive && !MainGame.IsGamePaused)
		{
			if (targetWgo != playerWorkComponent.Wgo && playerWorkComponent.Wgo != null)
			{
				if ((bool)targetWgo && !targetWgo.IsDespawning)
				{
					targetWgo.InteractionHandler.OnInteractionTargetExit();
				}
				targetWgo = playerWorkComponent.Wgo;
				targetWgo.InteractionHandler.OnInteractionTargetEnter(playerController);
			}
			fixedUpdateInteractionHandler = ((playerWorkComponent.IsActive && ((playerWorkComponent.WorkInProgress && playerWorkComponent.ToolComponent.IsActionActive) || playerWorkComponent.TryStartInteraction())) ? updateInteractionHandler : skipInteractionHandler);
		}
		else
		{
			fixedUpdateInteractionHandler = skipInteractionHandler;
			if (playerWorkComponent.WorkInProgress || playerWorkComponent.IsMoving)
			{
				playerWorkComponent.StopInteraction();
			}
		}
		if (IsActive && !flag)
		{
			playerInputHandler.UpdateHotBarInteraction();
		}
	}

	public override void OnEnter()
	{
		fixedUpdateInteractionHandler = skipInteractionHandler;
		PlayerData playerData = MainGame.PlayerData;
		if (playerData.HasOverheadItem && !playerData.HasMultipleOverheadItems)
		{
			playerData.DropOverheadItem();
		}
		underInteractionWgo = playerController.PlayerInteractionComponent.WgoUnderInteraction;
		if (playerWorkComponent.Wgo == null)
		{
			playerWorkComponent.FindWgoToWork(underInteractionWgo ? new List<Wgo> { underInteractionWgo } : null);
		}
		playerController.PlayerInteractionComponent.SetPauseState(PlayerInteractionPauseType.ByWork, isPaused: true);
		if ((bool)playerWorkComponent.Wgo)
		{
			targetWgo = playerWorkComponent.Wgo;
			targetWgo.InteractionHandler.OnInteractionTargetEnter(playerController);
		}
	}

	public override void OnExit()
	{
		fixedUpdateInteractionHandler = skipInteractionHandler;
		playerWorkComponent.StopInteraction();
		if ((bool)targetWgo)
		{
			if (playerController.PlayerInteractionComponent.WgoUnderInteraction != targetWgo)
			{
				targetWgo.InteractionHandler.OnInteractionTargetExit();
			}
			targetWgo = null;
		}
		playerController.PlayerInteractionComponent.SetPauseState(PlayerInteractionPauseType.ByWork, isPaused: false);
		underInteractionWgo = null;
		playerWorkComponent.SetMagnetismDelayState(isDelayed: false);
	}

	private bool CanWork()
	{
		Wgo wgoUnderInteraction = playerController.PlayerInteractionComponent.WgoUnderInteraction;
		Wgo wgo = playerWorkComponent.FindWgoToWorkNoAssign(wgoUnderInteraction ? new List<Wgo> { wgoUnderInteraction } : null);
		if (playerController.PlayerInteractionComponent.BigDropUnderInteraction != null && playerController.PlayerInteractionComponent.BigDropUnderInteraction.InteractionHandler.HasInteraction2())
		{
			return false;
		}
		if (playerController.PlayerData.HasMultipleOverheadItems)
		{
			return false;
		}
		if (playerController.PlayerData.HasOverheadItem && playerController.PlayerData.overheadItem.Definition.itemGroupIds.Contains("zombie"))
		{
			return false;
		}
		if (targetWgo != null && targetWgo.Data.Worker is PlayerController)
		{
			return true;
		}
		if (wgoUnderInteraction != null && wgoUnderInteraction != wgo)
		{
			return false;
		}
		if (wgo != null)
		{
			return true;
		}
		return false;
	}

	private void UpdateInteraction(float deltaTime)
	{
		playerWorkComponent.UpdateInteraction(deltaTime);
	}

	private static void SkipInteractionUpdate(float deltaTime)
	{
	}
}
