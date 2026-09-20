using LazyBearTechnology;

public class PrayerStandInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		SermonConfigDef data = GameBalance.Me.GetData<SermonConfigDef>(assignedWgo.Data.id);
		if (data == null)
		{
			return false;
		}
		if (MainGame.PlayerData.GetRes("sermon_ready") < 1f)
		{
			GlobalScriptsManager.FireEvent("System_Pray", "sermon_is_not_ready");
			return false;
		}
		WorldData worldData = MainGame.Instance.GameSave.WorldData;
		WorldZoneData worldZoneDataById = worldData.GetGameSceneDataById(interactor.PlayerData.currentGameSceneId).GetWorldZoneDataById(data.curWorldZoneId);
		WorldZoneData worldZoneDataById2 = worldData.GetGameSceneDataById(interactor.PlayerData.currentGameSceneId).GetWorldZoneDataById(data.attachedWorldZoneId);
		UIPrayWindowData data2 = new UIPrayWindowData(interactor.PlayerData, assignedWgo.Data, (int)worldZoneDataById2.GetTotalQuality(), (int)worldZoneDataById.GetTotalQuality(), MainGame.PlayerData.GetResInt("happiness"), data.rewardBoxId);
		LazyUI.GetWindow<UIPrayWindow>().Open(data2);
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (GameBalance.Me.GetData<SermonConfigDef>(assignedWgo.Data.id) == null)
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
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_pray", GameKey.Interaction)));
	}
}
