using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Destroy WGO", 0)]
[Category("Game Actions")]
[Icon("Cross", false, "")]
[Description("If WGO is null, then self")]
public class Flow_DestroyWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (!(worldGameObject == null))
			{
				worldGameObject.DestroyMe();
				flow_out.Call(f);
			}
		});
	}
}
