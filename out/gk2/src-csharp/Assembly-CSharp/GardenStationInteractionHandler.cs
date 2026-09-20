using LazyBearTechnology;

public class GardenStationInteractionHandler : WGOInteractionHandlerBase
{
	private CraftComponent assignedCraftComponent;

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Worker == null && TryGetInsertableZombieOverhead(out var zombieItem))
		{
			GDPointData gDPointData = assignedWgo.Data.GetGDPointData("zombie_garden_crafter_gd_point");
			ZombieWgoData zombieWgoData = MainGame.ZombieSystemData.PutZombieFromStoreToGameSceneAsGardener(interactor.PlayerData, zombieItem, interactor.PlayerData.currentGameSceneId, gDPointData.Position, gDPointData.Direction);
			Item zombieItem2 = zombieWgoData.ZombieItem;
			zombieWgoData.AttachToGardenStationWgoData(assignedWgo.Data.UniqueId, zombieItem2);
			SetupZombieVisual(zombieWgoData);
		}
		interactor.PlayerInteractionComponent.ResetInteractionState();
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (assignedWgo.Data.Worker == null && HasInsertableZombieOverhead())
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
		if (assignedWgo.Data.Worker == null && HasInsertableZombieOverhead())
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_zombie", GameKey.Interaction)));
		}
		return interactionInfos2;
	}

	private void SetupZombieVisual(ZombieWgoData zombieData)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(zombieData.UniqueId);
		if (!(wgoViewGlobal == null))
		{
			int gameResInt = zombieData.GetGameResInt("zombie_head_id");
			int gameResInt2 = zombieData.GetGameResInt("zombie_body_id");
			string headLut = zombieData.GameResStr.Get("zombie_head_lut");
			int num;
			switch (gameResInt)
			{
			case 1050:
			case 1056:
				num = 1801;
				break;
			case 1052:
			case 1054:
				num = 1802;
				break;
			case 1058:
				num = 1803;
				break;
			default:
				num = 1800;
				break;
			}
			int head = num;
			SkinPresetGK2 presetForCustomizationData = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", gameResInt2, head, string.Empty, headLut);
			if (presetForCustomizationData != null && wgoViewGlobal.MainWgoPart.AnimationComponent is AnimationComponent animationComponent)
			{
				animationComponent.SetSkinPreset(presetForCustomizationData);
				animationComponent.ChangeSkinPreset(presetForCustomizationData);
			}
		}
	}
}
