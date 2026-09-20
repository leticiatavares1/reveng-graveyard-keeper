using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Stop Craft", 0)]
[Category("Game Actions")]
[Icon("CubeArrowCube", false, "")]
[Description("If WGO is null, then self")]
public class Flow_StopCraft : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject != null)
			{
				worldGameObject.components.craft.CancelRemovalCraft();
				worldGameObject.components.craft.enabled = false;
			}
			flow_out.Call(f);
		});
	}
}
