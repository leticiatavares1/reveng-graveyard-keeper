using FlowCanvas;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get Insanity", 0)]
[Category("Game/GameRes")]
public class Flow_GetInsanity : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool getMaxValue;

	private ValueOutput<float> gameResValueOut;

	public override string name => "Get " + (getMaxValue ? "Max" : "Current") + " Insanity";

	protected override void RegisterPorts()
	{
		gameResValueOut = AddValueOutput("value", () => getMaxValue ? PlayerInsanityGameResSystem.GetSystem().Max : base.PlayerData.GetRes("insanity"));
	}
}
