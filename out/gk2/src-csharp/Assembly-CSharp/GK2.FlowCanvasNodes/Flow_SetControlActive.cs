using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Control Active", 0)]
[Category("Game/Cutscenes")]
[Color("8a8a8a")]
public class Flow_SetControlActive : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool isAffectCinematic;

	[GatherPortsCallback]
	[ShowIf("isAffectCinematic", 1)]
	public bool isImmediateCinematic;

	private FlowInput @in;

	private FlowOutput @out;

	protected FlowOutput onFinish;

	private ValueInput<bool> isEnable;

	public override string name => (isEnable.value ? "Control Enable" : "Control Disable") + (isAffectCinematic ? " \n/w Cinematic" : "");

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetControl);
		@out = AddFlowOutput("out".CapitalizeFirst());
		isEnable = AddValueInput<bool>("isEnable".CapitalizeFirst());
		if (isAffectCinematic)
		{
			onFinish = AddFlowOutput("onFinish".CapitalizeFirst());
		}
	}

	private void SetControl(Flow flow)
	{
		if (isAffectCinematic)
		{
			UICinematic uICinematic = LazyUI.Get<UICinematic>();
			if (isEnable.value)
			{
				uICinematic.DisableCinematic(delegate
				{
					MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFlow, isEnable.value);
					GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerChangeControlByFlow, isEnable.value.ToString());
					onFinish.Call(flow);
				}, isImmediateCinematic);
				@out.Call(flow);
			}
			else
			{
				MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFlow, isEnable.value);
				uICinematic.EnableCinematic(delegate
				{
					onFinish.Call(flow);
				}, isImmediateCinematic);
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerChangeControlByFlow, isEnable.value.ToString());
				@out.Call(flow);
			}
		}
		else
		{
			MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFlow, isEnable.value);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerChangeControlByFlow, isEnable.value.ToString());
			@out.Call(flow);
		}
	}
}
