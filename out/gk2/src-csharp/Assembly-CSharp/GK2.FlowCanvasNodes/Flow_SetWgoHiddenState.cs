using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set WGO Hidden State", 0)]
[Category("Game/Environment")]
[Color("f47dff")]
public class Flow_SetWgoHiddenState : GKCustomFlowNodeWithWgoData
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
				return "Show Wgo";
			}
			return "Hide Wgo";
		}
	}

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), SetHiddenState);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void SetHiddenState(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			wgoData.IsHidden = hide;
		}
		@out.Call(flow);
	}
}
