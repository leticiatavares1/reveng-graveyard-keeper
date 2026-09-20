using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("ff0000")]
[Category("Game General")]
[Name("End Script", 0)]
[Icon("RedCross", false, "")]
public class Flow_EndScript : MyFlowNode
{
	protected override void RegisterPorts()
	{
		AddFlowInput("In", delegate
		{
			if (base.cfs != null)
			{
				base.cfs.TerminateMe();
			}
		});
	}
}
