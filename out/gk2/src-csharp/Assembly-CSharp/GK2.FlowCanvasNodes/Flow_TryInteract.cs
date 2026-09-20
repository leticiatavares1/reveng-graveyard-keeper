using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Try Interact", 0)]
[Category("Game/Player")]
[Description("Attempts to perform the primary interaction with the object currently under the player's focus.")]
[Color("313c8f")]
public class Flow_TryInteract : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onSuccess;

	private FlowOutput onFail;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), TryInteract);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onSuccess = AddFlowOutput("onSuccess".CapitalizeFirst());
		onFail = AddFlowOutput("onFail".CapitalizeFirst());
	}

	private void TryInteract(Flow flow)
	{
		PlayerController playerController = MainGame.PlayerController;
		PlayerInteractionComponent playerInteractionComponent = playerController.PlayerInteractionComponent;
		bool flag = false;
		DropView dropView = playerInteractionComponent.BigDropUnderInteraction;
		if (dropView == null)
		{
			if (playerInteractionComponent.TryGetComponent<BoxCollider>(out var component))
			{
				component.enabled = true;
			}
			dropView = playerInteractionComponent.TryGetClosestInteractionTarget();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
		if (dropView != null && dropView.InteractionHandler.HasInteraction())
		{
			dropView.InteractionHandler.Interact();
			flag = true;
		}
		else if (playerInteractionComponent.WgoUnderInteraction != null)
		{
			IWGOInteractionHandler interactionHandler = playerInteractionComponent.WgoUnderInteraction.InteractionHandler;
			WgoData data = playerInteractionComponent.WgoUnderInteraction.Data;
			if (data.Events.Count > 0)
			{
				if (interactionHandler.HasInteraction() || interactionHandler.HasInteraction2())
				{
					if (interactionHandler.HasInteraction())
					{
						flag = data.FireInteractionEvent();
					}
				}
				else
				{
					flag = data.FireInteractionEvent();
				}
			}
			if (!flag && interactionHandler.HasInteraction())
			{
				flag = interactionHandler.Interact(playerController);
			}
		}
		@out.Call(flow);
		if (flag)
		{
			onSuccess.Call(flow);
		}
		else
		{
			onFail.Call(flow);
		}
	}
}
