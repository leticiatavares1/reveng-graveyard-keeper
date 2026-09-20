public class BuildPlayerState : SSMState
{
	public override bool CanEnter => IsActive;

	public override bool IsActive
	{
		get
		{
			if (BuildController.Instance != null)
			{
				return BuildController.Instance.IsBuildModeActive;
			}
			return false;
		}
	}

	public BuildPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		playerController.PhysicalBody.SetNonKinematicFlag(PlayerDynamicType.ByBuilding, isDynamic: false);
		playerController.PlayerInteractionComponent.SetPauseState(PlayerInteractionPauseType.ByBuild, isPaused: true);
	}

	public override void OnExit()
	{
		playerController.PhysicalBody.SetNonKinematicFlag(PlayerDynamicType.ByBuilding, isDynamic: true);
		playerController.PlayerInteractionComponent.SetPauseState(PlayerInteractionPauseType.ByBuild, isPaused: false);
	}
}
