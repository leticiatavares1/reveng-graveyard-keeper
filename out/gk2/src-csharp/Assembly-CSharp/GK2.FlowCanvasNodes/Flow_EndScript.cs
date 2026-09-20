using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("End Script", 0)]
[Category("Game/Script")]
[Icon("Assets/GFX/ParadoxNotionCustomIcons/RedCross.png", false, "")]
[Color("ff5c5c")]
public class Flow_EndScript : GKCustomFlowNode
{
	protected override void RegisterPorts()
	{
		AddFlowInput("In", delegate
		{
			TerminateScript();
		});
	}
}
