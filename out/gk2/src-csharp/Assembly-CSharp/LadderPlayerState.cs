using LazyBearTechnology;

public class LadderPlayerState : SSMState
{
	private LadderClimbController ladderClimbController;

	private Wgo ladderWgo;

	public override bool CanEnter => CanUse();

	public override bool IsActive => ladderClimbController.IsClimbActive;

	public LadderPlayerState(PlayerController playerController, LadderClimbController ladderClimbController)
		: base(playerController)
	{
		this.ladderClimbController = ladderClimbController;
	}

	public override void OnEnter()
	{
		playerController.SetControlTakenType(TakenControlType.ByLadder, isEnabled: false);
		playerController.PhysicalBody.SetNonKinematicFlag(PlayerDynamicType.ByLadder, isDynamic: false);
		playerController.PlayerInteractionComponent.SetPauseState(PlayerInteractionPauseType.ByLadder, isPaused: true);
		playerController.PhysicalBody.enabled = false;
		ladderClimbController.StartClimb(0f, () => LazyInput.GetKey(GameKey.Up) || LazyInput.GetKey(GameKey.DpadUp), () => LazyInput.GetKey(GameKey.Down) || LazyInput.GetKey(GameKey.DpadDown), ladderClimbController.LadderUnderInteraction, playerController.PhysicalBody.Rb);
		ladderWgo = ladderClimbController.LadderUnderUse.GetComponentInParent<Wgo>();
	}

	public override void Update()
	{
		if (!ladderClimbController.AutoLeaveLadderEnabled && ladderClimbController.CanLeaveLadder && LazyInput.GetKeyDown(GameKey.Interaction) && IsActive)
		{
			ladderClimbController.StopClimb();
		}
	}

	public override void OnExit()
	{
		playerController.PhysicalBody.SetNonKinematicFlag(PlayerDynamicType.ByLadder, isDynamic: true);
		playerController.PhysicalBody.enabled = true;
		playerController.SetControlTakenType(TakenControlType.ByLadder, isEnabled: true);
		if ((bool)ladderWgo)
		{
			ladderWgo.Data.SetGameRes("ladder_busy", 0);
			ladderWgo = null;
		}
		playerController.PlayerInteractionComponent.SetPauseState(PlayerInteractionPauseType.ByLadder, isPaused: false);
		playerController.PlayerInteractionComponent.ResetInteractionState();
	}

	private bool CanUse()
	{
		return false;
	}
}
