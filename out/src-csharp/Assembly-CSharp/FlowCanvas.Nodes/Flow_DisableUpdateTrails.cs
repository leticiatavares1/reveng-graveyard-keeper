using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Disable Update Trails", 0)]
[Category("Game Actions")]
[Description("Works for main character only")]
public class Flow_DisableUpdateTrails : MyFlowNode
{
	private ValueInput<bool> enable_par;

	public override string name => (enable_par.value ? "Enable" : "Disable") + " Update Trails";

	protected override void RegisterPorts()
	{
		FlowOutput @out = AddFlowOutput("Out");
		enable_par = AddValueInput<bool>("enable?");
		AddFlowInput("In", delegate(Flow flow)
		{
			MainGame.me.player_component.Trail.UpdateTrails = enable_par.value;
			@out.Call(flow);
		});
	}
}
