using JetBrains.Annotations;

public class ConditionalDrawerContext
{
	private readonly BuildController buildController;

	public WgoPart WgoPart { get; }

	[CanBeNull]
	public WgoData WgoData => WgoPart?.Wgo?.Data;

	[CanBeNull]
	public Inventory Inventory => WgoData?.Inventory;

	[CanBeNull]
	public CraftComponent CraftComponent => WgoData?.CraftComponent;

	[CanBeNull]
	public ConveyorComponent ConveyorComponent
	{
		get
		{
			if (!(WgoData is ConveyorWgoData conveyorWgoData))
			{
				return null;
			}
			return conveyorWgoData.ConveyorComponent;
		}
	}

	[CanBeNull]
	private PlayerWorkComponent PlayerWorkComp => MainGame.PlayerController?.PlayerWorkComponent;

	[CanBeNull]
	private FightingAgent FightingAgent => WgoPart?.GetComponent<FightingAgent>();

	[CanBeNull]
	public ZombieWgoData ZombieWgoData => WgoData as ZombieWgoData;

	public bool IsInWork { get; private set; }

	public bool HasWorker { get; private set; }

	public bool IsCraftStarted { get; private set; }

	public bool IsCalledFromEvent { get; private set; }

	public bool IsFightingAgentActive { get; private set; }

	public bool IsBuildingModeActive { get; private set; }

	public bool IsZombieCareTakerOnStation { get; private set; }

	public ConditionalDrawerContext(WgoPart wgoPart, BuildController buildController)
	{
		WgoPart = wgoPart;
		this.buildController = buildController;
		UpdateCachedValues();
	}

	public void UpdateCachedValues(bool fromCalledEvent = false)
	{
		IsBuildingModeActive = buildController != null && buildController.IsBuildModeActive;
		IsInWork = false;
		if (WgoData == null)
		{
			HasWorker = false;
			IsCraftStarted = false;
			return;
		}
		CraftComponent craftComponent = CraftComponent;
		IsCraftStarted = craftComponent != null && craftComponent.Status == CraftComponentStatus.Started;
		IsCalledFromEvent = fromCalledEvent;
		HasWorker = WgoData.Worker != null && !WgoData.Worker.Id.IsEmpty;
		if (HasWorker)
		{
			bool flag = WgoPart.Wgo.InteractionHandler is WorkInteractionHandler;
			if (WgoData.Worker is PlayerController)
			{
				IsInWork = (flag || IsCraftStarted) && PlayerWorkComp != null && PlayerWorkComp.Wgo == WgoPart.Wgo && PlayerWorkComp.WorkInProgress && PlayerWorkComp.ToolComponent.IsActionActive;
			}
			else if (WgoData.Worker is ZombieWgoData zombieWgoData)
			{
				IsInWork = (flag || IsCraftStarted) && zombieWgoData.WorkerActivity is ZombieCraftActivity zombieCraftActivity && zombieCraftActivity.IsActive;
			}
		}
		IsFightingAgentActive = FightingAgent?.IsValid ?? false;
		ZombieWgoData.ZombieCaretakerState? zombieCaretakerState = ZombieWgoData?.CaretakerState;
		IsZombieCareTakerOnStation = zombieCaretakerState.HasValue && zombieCaretakerState.GetValueOrDefault() == ZombieWgoData.ZombieCaretakerState.OnStation;
	}
}
