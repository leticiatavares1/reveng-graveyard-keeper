using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Puff effect on WGO")]
[Category("Game Actions")]
[Name("Puff FX", 0)]
public class Flow_DrawPuffFx : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject != null)
			{
				worldGameObject.DrawPuffFX();
				flow_out.Call(f);
			}
		});
	}
}
