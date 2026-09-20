using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Setting and applying WGO skin")]
[Name("Set WGO skin", 0)]
public class Flow_SetSkinToWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_skin_name = AddValueInput<string>("skin_name");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WGOParamOrSelf(in_wgo).ApplySkin(in_skin_name.value);
			flow_out.Call(f);
		});
	}
}
