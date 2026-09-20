using LazyBearTechnology;

public class StationInteractionHandler : WGOInteractionHandlerBase
{
	private CraftComponent assignedCraftComponent;

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Worker == null)
		{
			if (TryGetInsertableZombieOverhead(out var zombieItem))
			{
				GDPointData gDPointData = assignedWgo.Data.GetGDPointData("zombie_porter_station_gd_point");
				ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsAssistant(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, gDPointData.Position, gDPointData.Direction);
				zombieWgoData.AttachToStationWgoData(zombie: zombieWgoData.ZombieItem, uniqueId: assignedWgo.Data.UniqueId);
			}
			else
			{
				Bubble.Talk(new PhraseData(isPlayer: true, null, "zombie_supplier_station_no_overhead", null, null, SpeechBubbleType.Think));
			}
		}
		interactor.PlayerInteractionComponent.ResetInteractionState();
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (assignedWgo.Data.Worker == null)
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
		InteractionInfos interactionInfos2 = new InteractionInfos();
		if (assignedWgo.Data.Worker == null)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_zombie", GameKey.Interaction)));
		}
		return interactionInfos2;
	}
}
