using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Cursor Visibility", 0)]
[Category("Game/UI")]
[Description("Shows or hides the mouse cursor using CursorController.")]
[Color("313c8f")]
public class Flow_SetCursorVisibility : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool hide = true;

	private FlowInput @in;

	private FlowOutput @out;

	public override string name
	{
		get
		{
			if (!hide)
			{
				return "Show Cursor";
			}
			return "Hide Cursor";
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetVisibility);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void SetVisibility(Flow flow)
	{
		CursorController.ChangeCursorVisibleState(!hide);
		@out.Call(flow);
	}
}
