using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Has Flow Script", 0)]
[Category("Game/Script")]
[Color("ff5c5c")]
[Icon("FS", false, "")]
public class Flow_HasFlowScript : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<string> scriptName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			if (GlobalScriptsManager.HasFlowScript(scriptName.value))
			{
				yes.Call(flow);
			}
			else
			{
				no.Call(flow);
			}
		});
		yes = AddFlowOutput("yes".CapitalizeFirst());
		no = AddFlowOutput("no".CapitalizeFirst());
		scriptName = AddValueInput<string>("scriptName".CapitalizeFirst());
	}
}
