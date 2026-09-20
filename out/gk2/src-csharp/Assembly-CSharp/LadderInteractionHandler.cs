using LazyBearTechnology;
using UnityEngine;

public class LadderInteractionHandler : WGOInteractionHandlerBase
{
	public const string BUSY_GAMERES_KEY = "ladder_busy";

	private Ladder ladder;

	public override IWGOInteractionHandler Init(Wgo wgo)
	{
		ladder = wgo.GetComponentInChildren<Ladder>();
		if (!ladder)
		{
			Debug.LogError("Ladder not found");
		}
		return base.Init(wgo);
	}

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		assignedWgo.Data.SetGameRes("ladder_busy", 1);
		interactor.Ssm.ForceEnterState<LadderPlayerState>();
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (!ladder)
		{
			return false;
		}
		Wgo wgo = assignedWgo;
		if (wgo.Data.GetGameResInt("ladder_busy") != 0 || wgo.Data.Definition.interactionType != WGODef.InteractionType.Ladder || !interactor.LadderClimbController.CanUse)
		{
			return false;
		}
		return true;
	}

	public override void OnInteractionTargetEnter(PlayerController interactor)
	{
		if ((bool)ladder)
		{
			LadderEdgePart nearestLadderPart = ladder.GetNearestLadderPart(interactor.transform.position);
			Transform bubblePointToDisplay = nearestLadderPart.BubblePointToDisplay;
			if (bubblePointToDisplay != null)
			{
				assignedWgo.SetCustomBubblePoint(bubblePointToDisplay);
			}
			interactor.LadderClimbController.LadderUnderInteraction = nearestLadderPart.Ladder;
			base.OnInteractionTargetEnter(interactor);
		}
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_climb", GameKey.Interaction)));
	}
}
