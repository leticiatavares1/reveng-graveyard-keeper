using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Set Trigger To Animator", 0)]
[Category("Game/Animation")]
public class Flow_SetTriggerToAnimator : GKCustomFlowNodeWithWgoData
{
	public enum AnimatorType
	{
		Wgo,
		GameObject,
		Player
	}

	[GatherPortsCallback]
	public AnimatorType animatorType;

	[GatherPortsCallback]
	[ShowIf("animatorType", 0)]
	public bool setState;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GameObject> gameObject;

	private ValueInput<string> triggerName;

	private ValueInput<AnimationState> animationState;

	public override string name => "Set" + (setState ? " State In" : " Trigger To") + " Animator";

	protected override void RegisterPorts()
	{
		switch (animatorType)
		{
		case AnimatorType.Wgo:
			base.RegisterPorts();
			break;
		case AnimatorType.GameObject:
			gameObject = AddValueInput<GameObject>("gameObject".CapitalizeFirst());
			break;
		}
		@in = AddFlowInput("in".CapitalizeFirst(), SetTrigger);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (setState)
		{
			animationState = AddValueInput<AnimationState>("animationState".CapitalizeFirst());
		}
		else
		{
			triggerName = AddValueInput<string>("triggerName".CapitalizeFirst());
		}
	}

	private void SetTrigger(Flow flow)
	{
		switch (animatorType)
		{
		case AnimatorType.Wgo:
		{
			WgoData wgoData = GetWgoData();
			if (wgoData != null)
			{
				if (setState)
				{
					wgoData.SetStateToAnimator(animationState.value);
				}
				else
				{
					wgoData.SetTriggerToAnimator(triggerName.value);
				}
			}
			else
			{
				Debug.LogError("Flow_SetTriggerToAnimator: cannot trigger [" + triggerName.value + "] on null WGO");
			}
			break;
		}
		case AnimatorType.GameObject:
		{
			GameObject gameObject = ParamValueOrSelf(this.gameObject);
			if (gameObject != null)
			{
				Animator component = gameObject.GetComponent<Animator>();
				if (component != null)
				{
					component.SetTrigger(triggerName.value);
				}
				else
				{
					Debug.LogError("[Flow_SetTriggerToAnimator]: Animator not found");
				}
			}
			else
			{
				Debug.LogError("[Flow_SetTriggerToAnimator]: GameObject is null");
			}
			break;
		}
		case AnimatorType.Player:
			MainGame.PlayerController.View.PlayerAnimation.SetTrigger(triggerName.value);
			break;
		}
		@out.Call(flow);
	}
}
