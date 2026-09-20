using LazyBearTechnology;

public class FlagStandInteractionHandler : WGOInteractionHandlerBase
{
	public const string GAME_RES_STR_KEY = "flag_stand_sguid";

	private SGuid AssociatedFlagSGuid
	{
		get
		{
			if (string.IsNullOrEmpty(assignedWgo.Data.GameResStr.Get("flag_stand_sguid")))
			{
				return SGuid.Empty;
			}
			return SGuid.Parse(assignedWgo.Data.GameResStr.Get("flag_stand_sguid"));
		}
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		SGuid sGuid = interactor.attachedWgo?.Data.UniqueId;
		if (sGuid == null && !SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			return true;
		}
		if (sGuid != null && SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			return true;
		}
		if (sGuid != null && !SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			return true;
		}
		return false;
	}

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		SGuid sGuid = interactor.attachedWgo?.Data.UniqueId;
		FlagStandComponent componentInChildren = assignedWgo.GetComponentInChildren<FlagStandComponent>();
		if (sGuid == null && !SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(AssociatedFlagSGuid);
			if ((bool)wgoViewGlobal)
			{
				componentInChildren?.DetachFlag();
				interactor.AttachTheFlag(wgoViewGlobal);
				assignedWgo.Data.GameResStr.Set("flag_stand_sguid", string.Empty);
				AgentsGroupFlagController.SetInteractionLocked(wgoViewGlobal, isLocked: true);
				if (assignedWgo.Id.EndsWith("one_time"))
				{
					if (componentInChildren != null && LazySingleton<FightingGameController>.Instance.CurrentFightState != 0)
					{
						LazySingleton<FightingGameController>.Instance.FlagStandComponents.Remove(componentInChildren);
					}
					assignedWgo.RemoveWithData();
				}
			}
			assignedWgo.SetCustomBubblePoint(null);
			assignedWgo.DrawWidgets();
			return true;
		}
		if (sGuid != null && SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			Wgo wgoViewGlobal2 = GameScene.GetWgoViewGlobal(sGuid);
			if ((bool)wgoViewGlobal2)
			{
				interactor.RemoveTheFlag();
				if ((bool)componentInChildren)
				{
					componentInChildren.AttachFlag(wgoViewGlobal2);
					componentInChildren.TryToSyncFlagPosition();
				}
				else
				{
					wgoViewGlobal2.Data.Position = assignedWgo.transform.position;
				}
				assignedWgo.Data.GameResStr.Set("flag_stand_sguid", wgoViewGlobal2.Data.UniqueId.ToString());
				AgentsGroupFlagController.SetInteractionLocked(wgoViewGlobal2, isLocked: false);
				assignedWgo.SetCustomBubblePoint(wgoViewGlobal2.MainWgoPart.BubblePoint);
			}
			assignedWgo.DrawWidgets();
			return true;
		}
		if (sGuid != null && !SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			Wgo wgoViewGlobal3 = GameScene.GetWgoViewGlobal(AssociatedFlagSGuid);
			Wgo wgoViewGlobal4 = GameScene.GetWgoViewGlobal(sGuid);
			if ((bool)wgoViewGlobal3 && (bool)wgoViewGlobal4)
			{
				componentInChildren?.DetachFlag();
				interactor.RemoveTheFlag();
				if ((bool)componentInChildren)
				{
					componentInChildren.AttachFlag(wgoViewGlobal4);
					componentInChildren.TryToSyncFlagPosition();
				}
				else
				{
					wgoViewGlobal4.Data.Position = assignedWgo.transform.position;
				}
				assignedWgo.Data.GameResStr.Set("flag_stand_sguid", wgoViewGlobal4.Data.UniqueId.ToString());
				AgentsGroupFlagController.SetInteractionLocked(wgoViewGlobal4, isLocked: false);
				interactor.AttachTheFlag(wgoViewGlobal3);
				AgentsGroupFlagController.SetInteractionLocked(wgoViewGlobal3, isLocked: true);
				assignedWgo.SetCustomBubblePoint(wgoViewGlobal4.MainWgoPart.BubblePoint);
			}
			assignedWgo.DrawWidgets();
			return true;
		}
		return false;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		ApplyCustomBubblePoint();
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		SGuid sGuid = interactor?.attachedWgo?.Data?.UniqueId;
		if (sGuid == null && !SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			InteractionInfos interactionInfos2 = new InteractionInfos();
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take_flag", GameKey.Interaction)));
			interactionInfos2.Add(new InteractionInfo("", "icon-arrow_flag-take"));
			return interactionInfos2;
		}
		if (sGuid != null && SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			InteractionInfos interactionInfos3 = new InteractionInfos();
			interactionInfos3.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_flag", GameKey.Interaction)));
			interactionInfos3.Add(new InteractionInfo("", "icon-arrow_flag-insert"));
			return interactionInfos3;
		}
		if (sGuid != null && !SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			InteractionInfos interactionInfos4 = new InteractionInfos();
			interactionInfos4.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_flag", GameKey.Interaction)));
			interactionInfos4.Add(new InteractionInfo("", "icon-arrow_flag-insert"));
			return interactionInfos4;
		}
		return new InteractionInfos();
	}

	private void ApplyCustomBubblePoint()
	{
		if (!SGuid.IsNullOrEmpty(AssociatedFlagSGuid))
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(AssociatedFlagSGuid);
			if ((bool)wgoViewGlobal)
			{
				assignedWgo.SetCustomBubblePoint(wgoViewGlobal.MainWgoPart.BubblePoint);
			}
		}
		else
		{
			assignedWgo.SetCustomBubblePoint(null);
		}
	}
}
