using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Set GameRes to WGO", 0)]
public class Flow_SetGameResToWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<GameRes> in_res = AddValueInput<GameRes>("GameRes");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			for (int i = 0; i < in_res.value.Types.Count; i++)
			{
				in_wgo.value.data.SetParam(in_res.value.Types[i], in_res.value.Get(in_res.value.Types[i]));
			}
			flow_out.Call(f);
		});
	}
}
