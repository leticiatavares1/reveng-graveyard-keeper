using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Last Sermon Equals", 0)]
[Category("Game/Sermon")]
[Color("70f1ff")]
public class Flow_LastSermonEquals : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<string> sermonId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			if (MainGame.PlayerData.currentSermon.id == sermonId.value)
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
		sermonId = AddValueInput<string>("sermonId".CapitalizeFirst());
	}
}
