using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Run Global Script", 0)]
[Category("Game/Script")]
[Color("ff5c5c")]
[Icon("FS", false, "")]
public class Flow_RunGlobalScript : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private ValueInput<string> scriptName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), RunScript);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		scriptName = AddValueInput<string>("scriptName".CapitalizeFirst());
	}

	private void RunScript(Flow flow)
	{
		GameScriptUtility.RunGlobalScript(scriptName.value, delegate
		{
			onFinished.Call(flow);
		});
		@out.Call(flow);
	}
}
