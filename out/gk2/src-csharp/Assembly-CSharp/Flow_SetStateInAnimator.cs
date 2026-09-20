using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Set State In Animator", 0)]
[Category("Game/Animation")]
public class Flow_SetStateInAnimator : GKCustomFlowNodeWithWgoData
{
	public enum AnimatorType
	{
		Wgo,
		GameObject,
		Player
	}

	[GatherPortsCallback]
	public AnimatorType animatorType;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GameObject> gameObject;

	private ValueInput<AnimationState> state;

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
		state = AddValueInput<AnimationState>("state".CapitalizeFirst());
	}

	private void SetTrigger(Flow flow)
	{
		switch (animatorType)
		{
		case AnimatorType.Wgo:
			Debug.LogError("Set state doesn't work for WGO (WgoData).");
			break;
		case AnimatorType.GameObject:
		{
			GameObject gameObject = ParamValueOrSelf(this.gameObject);
			if (gameObject != null)
			{
				Animator component = gameObject.GetComponent<Animator>();
				if (component != null)
				{
					component.SetInteger(AnimationComponentBase.idStateAnimator, (int)state.value);
				}
				else
				{
					Debug.LogError("[Flow_SetStateInAnimator]: Animator not found");
				}
			}
			else
			{
				Debug.LogError("[Flow_SetStateInAnimator]: GameObject is null");
			}
			break;
		}
		case AnimatorType.Player:
			MainGame.PlayerController.View.PlayerAnimation.SetState(state.value);
			break;
		}
		@out.Call(flow);
	}
}
