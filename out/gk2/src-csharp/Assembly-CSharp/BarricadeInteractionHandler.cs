using LazyBearTechnology;

public class BarricadeInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		FlagPlacementPoint componentInChildren = assignedWgo.GetComponentInChildren<FlagPlacementPoint>();
		if ((bool)componentInChildren)
		{
			if (!componentInChildren.FlagWgo && (bool)interactor.attachedWgo)
			{
				componentInChildren.FlagWgo = interactor.RemoveTheFlag();
				AgentsGroupFlagController componentInChildren2 = componentInChildren.FlagWgo.GetComponentInChildren<AgentsGroupFlagController>();
				componentInChildren2.AttachedSGuid = assignedWgo.Data.UniqueId;
				componentInChildren2.IsSetAtPoint = true;
				componentInChildren2.AgentsController.TryRetargetAgents();
				componentInChildren.FlagWgo.Data.Position = componentInChildren.transform.position;
				AgentsGroupFlagController.SetInteractionLocked(componentInChildren.FlagWgo, isLocked: true);
				assignedWgo.DrawWidgets();
				return true;
			}
			if ((bool)componentInChildren.FlagWgo && !interactor.attachedWgo)
			{
				Wgo flagWgo = componentInChildren.FlagWgo;
				componentInChildren.FlagWgo = null;
				interactor.AttachTheFlag(flagWgo);
				AgentsGroupFlagController componentInChildren3 = interactor.attachedWgo.GetComponentInChildren<AgentsGroupFlagController>();
				componentInChildren3.AttachedSGuid = null;
				componentInChildren3.IsSetAtPoint = false;
				foreach (FightingAgent agent in componentInChildren3.AgentsController.Agents)
				{
					if (agent.IsExecutingCommand)
					{
						agent.StopCommandExecution(reportAlsoAsCompletion: true);
					}
				}
				componentInChildren3.AgentsController.TryRetargetAgents();
				foreach (FightingAgent agent2 in componentInChildren3.AgentsController.Agents)
				{
					WgoData wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(agent2.Wgo.Data.takenDockPointsParentSGuid);
					if (wgoData != null)
					{
						wgoData.MainWgoPartData.GetOccupiedDockPointBy(agent2.Wgo.Data.UniqueId)?.UnOccupy();
						agent2.Wgo.Data.takenDockPointsParentSGuid = SGuid.Empty;
					}
				}
				AgentsGroupFlagController.SetInteractionLocked(flagWgo, isLocked: false);
				assignedWgo.DrawWidgets();
				return true;
			}
			if ((bool)componentInChildren.FlagWgo && (bool)interactor.attachedWgo)
			{
				Wgo flagWgo2 = componentInChildren.FlagWgo;
				Wgo wgo2 = (componentInChildren.FlagWgo = interactor.RemoveTheFlag());
				AgentsGroupFlagController componentInChildren4 = wgo2.GetComponentInChildren<AgentsGroupFlagController>();
				componentInChildren4.AttachedSGuid = assignedWgo.Data.UniqueId;
				componentInChildren4.IsSetAtPoint = true;
				componentInChildren4.AgentsController.TryRetargetAgents();
				wgo2.Data.Position = componentInChildren.transform.position;
				AgentsGroupFlagController.SetInteractionLocked(wgo2, isLocked: true);
				interactor.AttachTheFlag(flagWgo2);
				AgentsGroupFlagController componentInChildren5 = interactor.attachedWgo.GetComponentInChildren<AgentsGroupFlagController>();
				componentInChildren5.AttachedSGuid = null;
				componentInChildren5.IsSetAtPoint = false;
				foreach (FightingAgent agent3 in componentInChildren5.AgentsController.Agents)
				{
					if (agent3.IsExecutingCommand)
					{
						agent3.StopCommandExecution(reportAlsoAsCompletion: true);
					}
				}
				componentInChildren5.AgentsController.TryRetargetAgents();
				foreach (FightingAgent agent4 in componentInChildren5.AgentsController.Agents)
				{
					WgoData wgoData2 = MainGame.Instance.GameSave.WorldData.GetWgoData(agent4.Wgo.Data.takenDockPointsParentSGuid);
					if (wgoData2 != null)
					{
						wgoData2.MainWgoPartData.GetOccupiedDockPointBy(agent4.Wgo.Data.UniqueId)?.UnOccupy();
						agent4.Wgo.Data.takenDockPointsParentSGuid = SGuid.Empty;
					}
				}
				AgentsGroupFlagController.SetInteractionLocked(flagWgo2, isLocked: false);
				assignedWgo.DrawWidgets();
				return true;
			}
		}
		return false;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		FlagPlacementPoint componentInChildren = assignedWgo.GetComponentInChildren<FlagPlacementPoint>();
		if ((bool)componentInChildren)
		{
			bool flag = interactor.attachedWgo;
			if ((bool)componentInChildren.FlagWgo)
			{
				return true;
			}
			if (flag)
			{
				return true;
			}
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
		FlagPlacementPoint componentInChildren = assignedWgo.GetComponentInChildren<FlagPlacementPoint>();
		bool flag = interactor.attachedWgo;
		if ((bool)componentInChildren)
		{
			if (!componentInChildren.FlagWgo && flag)
			{
				interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_flag", GameKey.Interaction)));
				interactionInfos2.Add(new InteractionInfo("", "icon-arrow_flag-insert"));
			}
			else if ((bool)componentInChildren.FlagWgo && !flag)
			{
				interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take_flag", GameKey.Interaction)));
				interactionInfos2.Add(new InteractionInfo("", "icon-arrow_flag-take"));
			}
			else if ((bool)componentInChildren.FlagWgo && flag)
			{
				interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_flag", GameKey.Interaction)));
				interactionInfos2.Add(new InteractionInfo("", "icon-arrow_flag-insert"));
			}
		}
		return interactionInfos2;
	}
}
