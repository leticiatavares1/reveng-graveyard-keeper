using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Fire Event On Global Script", 0)]
[Category("Game/Script")]
[Color("ff5c5c")]
[Icon("FS", false, "")]
public class Flow_FireEventOnGlobalScript : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> scriptName;

	private ValueInput<string> eventName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in", delegate(Flow flow)
		{
			GlobalScriptsManager.FireEvent(scriptName.value, eventName.value);
			@out.Call(flow);
		});
		@out = AddFlowOutput("out".CapitalizeFirst());
		scriptName = AddValueInput<string>("scriptName");
		eventName = AddValueInput<string>("eventName");
	}
}
