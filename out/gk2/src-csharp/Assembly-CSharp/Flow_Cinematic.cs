using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("Cinematic", 0)]
[Category("Game/Cutscenes")]
[Color("8a8a8a")]
public class Flow_Cinematic : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private ValueInput<bool> enable;

	private ValueInput<bool> immediate;

	public override string name
	{
		get
		{
			if (!enable.value)
			{
				return "Disable Cinematic";
			}
			return "Enable Cinematic";
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeCinematic);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		enable = AddValueInput<bool>("enable");
		immediate = AddValueInput<bool>("immediate");
	}

	private void ChangeCinematic(Flow flow)
	{
		UICinematic uICinematic = LazyUI.Get<UICinematic>();
		if (enable.value)
		{
			uICinematic.EnableCinematic(delegate
			{
				onFinished.Call(flow);
			}, immediate.value);
		}
		else
		{
			uICinematic.DisableCinematic(delegate
			{
				onFinished.Call(flow);
			}, immediate.value);
		}
		@out.Call(flow);
	}
}
